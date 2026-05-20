open System
open System.Numerics
open Types
open Utils
open Evaluator
open Fu
open Items
open GameState

type Options =
    { items: string list
      samples: int
      seed: int
      doraCount: int
      discards: int
      pileRemaining: int
      goal: bigint
      attempts: int
      clearTrials: int
      rinshan: bool
      tenhou: bool
      showHelp: bool }

type SampleResult =
    { score: bigint
      han: int
      fu: int
      triggered: Set<string> }

let defaultOptions =
    { items = []
      samples = 100000
      seed = 1
      doraCount = 1
      discards = 0
      pileRemaining = 30
      goal = Config.initialGoalScore
      attempts = Config.tsumoPerRound
      clearTrials = 100000
      rinshan = false
      tenhou = false
      showHelp = false }

let usage =
    """
Balat-Ron balance simulator

Usage:
  dotnet run --project tools/Balat-Ron.Balance -- --items "Pinfu,Iipeikou" --samples 100000

Options:
  --items "A,B"       Comma-separated item names. Names are case-insensitive.
  --samples N         Random 14-tile samples to draw. Default: 100000.
  --seed N            RNG seed. Default: 1.
  --dora N            Number of revealed dora indicators. Default: 1.
  --discards N        Discard-pile size for items like Patience. Default: 0.
  --pile N            Draw-pile size for items like Deep Wall. Default: 30.
  --goal N            Goal score for clear-probability estimates. Default: initial goal.
  --attempts N        Max scoring attempts to estimate. Default: configured tsumo count.
  --clear-trials N    Monte Carlo trials for clear estimates. Default: 100000.
  --rinshan           Simulate winning after kan.
  --tenhou            Simulate winning by first draw.
  --list-items        Print available item names and exit.
"""

let rec parseArgs opts args =
    match args with
    | [] -> Choice1Of2 opts
    | "--help" :: _ | "-h" :: _ -> Choice1Of2 { opts with showHelp = true }
    | "--list-items" :: _ -> Choice2Of2 "list-items"
    | "--items" :: value :: rest ->
        let names =
            value.Split(',', StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
            |> Array.toList
        parseArgs { opts with items = names } rest
    | "--samples" :: value :: rest ->
        match Int32.TryParse value with
        | true, n when n > 0 -> parseArgs { opts with samples = n } rest
        | _ -> failwith "--samples must be a positive integer."
    | "--seed" :: value :: rest ->
        match Int32.TryParse value with
        | true, n -> parseArgs { opts with seed = n } rest
        | _ -> failwith "--seed must be an integer."
    | "--dora" :: value :: rest ->
        match Int32.TryParse value with
        | true, n when n >= 0 -> parseArgs { opts with doraCount = n } rest
        | _ -> failwith "--dora must be a non-negative integer."
    | "--discards" :: value :: rest ->
        match Int32.TryParse value with
        | true, n when n >= 0 -> parseArgs { opts with discards = n } rest
        | _ -> failwith "--discards must be a non-negative integer."
    | "--pile" :: value :: rest ->
        match Int32.TryParse value with
        | true, n when n >= 0 -> parseArgs { opts with pileRemaining = n } rest
        | _ -> failwith "--pile must be a non-negative integer."
    | "--goal" :: value :: rest ->
        match BigInteger.TryParse value with
        | true, n when n > 0I -> parseArgs { opts with goal = n } rest
        | _ -> failwith "--goal must be a positive integer."
    | "--attempts" :: value :: rest ->
        match Int32.TryParse value with
        | true, n when n > 0 -> parseArgs { opts with attempts = n } rest
        | _ -> failwith "--attempts must be a positive integer."
    | "--clear-trials" :: value :: rest ->
        match Int32.TryParse value with
        | true, n when n > 0 -> parseArgs { opts with clearTrials = n } rest
        | _ -> failwith "--clear-trials must be a positive integer."
    | "--rinshan" :: rest -> parseArgs { opts with rinshan = true } rest
    | "--tenhou" :: rest -> parseArgs { opts with tenhou = true } rest
    | arg :: _ -> failwith $"Unknown argument: {arg}"

let normalize (s: string) = s.Trim().ToLowerInvariant()

let itemByName =
    allItems
    |> List.map (fun item -> normalize item.name, item)
    |> Map.ofList

let resolveItems names =
    names
    |> List.map (fun name ->
        match Map.tryFind (normalize name) itemByName with
        | Some item -> { item with id = Guid.NewGuid() }
        | None ->
            let known = allItems |> List.map (fun item -> item.name) |> String.concat ", "
            failwith $"Unknown item '{name}'. Known items: {known}")

let mkHandFromDraw (draw: Tile array) =
    let handTiles = draw |> Array.take 13
    let tsumo = draw[13]
    Hand (tileArrayToHand handTiles, tsumo, [])

let sampleWithoutReplacement (rng: Random) count (pool: Tile array) =
    let arr = Array.copy pool
    for i in 0 .. count - 1 do
        let j = rng.Next(i, arr.Length)
        let tmp = arr[i]
        arr[i] <- arr[j]
        arr[j] <- tmp
    arr |> Array.take count

let syntheticDiscardPile n =
    Array.init n (fun i -> Tile ((i % 9) + 1))

let syntheticPile n =
    Array.init n (fun i -> Tile (((i + 3) % 9) + 1))

let scoreHand (template: GameState) (items: Item list) (hand: Hand) =
    let wrapAround = isWrapAroundEnabled { template with items = items; hand = hand }
    let parses = everyParsing wrapAround hand
    if List.isEmpty parses then
        None
    else
        parses
        |> List.map (fun (parsedHand, machi, tsumo) ->
            let state = { template with hand = hand; items = items; baseScore = (0, 0) }
            let stateAfterItems, log = applyItemEffects state (OnYakuCalc (parsedHand, machi, tsumo, hand)) items []
            let dora = calculateDora hand (Array.toList state.dora)
            let han = fst stateAfterItems.baseScore + dora
            let fuVal = snd stateAfterItems.baseScore + fu parsedHand machi tsumo
            let triggered =
                log
                |> List.choose (function
                    | EarnedExtraScore (_, _, ScoreReason.ItemEffect item) -> Some item.name
                    | ItemTriggered item -> Some item.name
                    | _ -> None)
                |> Set.ofList
            { score = score han fuVal; han = han; fu = fuVal; triggered = triggered })
        |> List.maxBy (fun result -> result.score)
        |> Some

let averageBigInt (values: bigint seq) =
    let mutable count = 0I
    let mutable total = 0I
    for value in values do
        count <- count + 1I
        total <- total + value
    if count = 0I then 0.0 else float total / float count

let percentile p (values: bigint array) =
    if Array.isEmpty values then 0I
    else
        Array.sortInPlace values
        let index = int (Math.Clamp(p, 0.0, 1.0) * float (values.Length - 1))
        values[index]

let variance values =
    let arr = values |> Seq.map float |> Seq.toArray
    if arr.Length = 0 then 0.0, 0.0
    else
        let mean = Array.average arr
        let var = arr |> Array.averageBy (fun x -> let d = x - mean in d * d)
        mean, var

let clearProbabilities (rng: Random) (goal: bigint) maxAttempts trials (scores: bigint array) =
    if Array.isEmpty scores then
        [| for attempts in 1 .. maxAttempts -> attempts, 0.0 |]
    else
        [| for attempts in 1 .. maxAttempts ->
            let mutable clears = 0
            for _ in 1 .. trials do
                let mutable total = 0I
                for _ in 1 .. attempts do
                    total <- total + scores[rng.Next(scores.Length)]
                if total >= goal then
                    clears <- clears + 1
            attempts, float clears / float trials |]

let runSimulation opts =
    let rng = Random(opts.seed)
    let selectedItems = resolveItems opts.items
    let template =
        let baseState = createGameState (Random(0))
        { baseState with
            items = selectedItems
            dora = sampleWithoutReplacement rng (min opts.doraCount allTiles.Length) (List.toArray allTiles)
            discardPile = syntheticDiscardPile opts.discards
            pile = syntheticPile opts.pileRemaining
            isRinshanKaihouApplicable = opts.rinshan
            isTenhouApplicable = opts.tenhou }

    let pool = List.toArray allTiles
    let mutable wins = 0
    let mutable totalScoreIncludingMisses = 0I
    let scores = ResizeArray<bigint>()
    let hanCounts = ResizeArray<int>()
    let fuCounts = ResizeArray<int>()
    let triggerCounts = selectedItems |> List.map (fun item -> item.name, 0) |> Map.ofList |> ref

    for _ in 1 .. opts.samples do
        let hand = sampleWithoutReplacement rng 14 pool |> mkHandFromDraw
        match scoreHand template selectedItems hand with
        | None -> ()
        | Some result ->
            wins <- wins + 1
            totalScoreIncludingMisses <- totalScoreIncludingMisses + result.score
            scores.Add(result.score)
            hanCounts.Add(result.han)
            fuCounts.Add(result.fu)
            for name in result.triggered do
                if Map.containsKey name triggerCounts.Value then
                    triggerCounts.Value <- triggerCounts.Value |> Map.change name (Option.map ((+) 1))

    let scoreArray = scores.ToArray()
    let winRate = float wins / float opts.samples
    let expectedPerSample = float totalScoreIncludingMisses / float opts.samples
    let expectedOnWin = averageBigInt scoreArray
    let avgHan = if wins = 0 then 0.0 else hanCounts |> Seq.averageBy float
    let avgFu = if wins = 0 then 0.0 else fuCounts |> Seq.averageBy float
    let _, scoreVariance = variance scoreArray
    let scoreStdDev = sqrt scoreVariance
    let scoreCv = if expectedOnWin = 0.0 then 0.0 else scoreStdDev / expectedOnWin
    let clearRates = clearProbabilities (Random(opts.seed + 7919)) opts.goal opts.attempts opts.clearTrials scoreArray

    printfn "Items: %s" (if List.isEmpty selectedItems then "(none)" else selectedItems |> List.map (fun item -> item.name) |> String.concat ", ")
    printfn "Samples: %d" opts.samples
    printfn "Winning samples: %d (%.2f%%)" wins (winRate * 100.0)
    printfn "Expected score per random 14-tile sample: %.1f" expectedPerSample
    printfn "Expected score conditional on winning: %.1f" expectedOnWin
    printfn "Score variance/stddev on winning hands: %.1f / %.1f (CV %.2f)" scoreVariance scoreStdDev scoreCv
    printfn "Average best han/fu on winning hands: %.2f han / %.2f fu" avgHan avgFu
    printfn "Score percentiles on winning hands: p50=%A p75=%A p90=%A p99=%A" (percentile 0.50 scoreArray) (percentile 0.75 scoreArray) (percentile 0.90 scoreArray) (percentile 0.99 scoreArray)
    printfn "\nClear probability for goal %A by scoring attempts:" opts.goal
    for attempts, rate in clearRates do
        printfn "  %d attempt(s): %.2f%%" attempts (rate * 100.0)

    if not (List.isEmpty selectedItems) then
        printfn "\nSelected item trigger rates on winning hands:"
        for item in selectedItems do
            let count = triggerCounts.Value |> Map.tryFind item.name |> Option.defaultValue 0
            let rate = if wins = 0 then 0.0 else float count / float wins * 100.0
            printfn "  %-24s %7.2f%%" item.name rate

[<EntryPoint>]
let main argv =
    try
        match parseArgs defaultOptions (Array.toList argv) with
        | Choice2Of2 "list-items" ->
            allItems
            |> List.sortBy (fun item -> item.name)
            |> List.iter (fun item -> printfn "%s [%A] %dG - %s" item.name item.rarity item.cost item.description)
            0
        | Choice2Of2 _ -> 0
        | Choice1Of2 opts when opts.showHelp ->
            printfn "%s" usage
            0
        | Choice1Of2 opts ->
            runSimulation opts
            0
    with ex ->
        eprintfn "%s" ex.Message
        eprintfn "%s" usage
        1
