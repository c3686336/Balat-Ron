open Types
open Config
open Utils
open System
open GameState
open Items

let renderLog log =
    for event in log do
        match event with
        | GameStarted -> ()
        | TileDiscarded t -> printfn $"Discarded {t}"
        | TileDrawn t -> printfn $"Drew {t}"
        | RinshangDrawn t -> printfn $"Rinshan draw: {t}"
        | Kan t -> printfn $"Kan: {t}"
        | PileEmpty -> printfn "Pile empty!"
        | Scored (han, fu, score) -> printfn $"Scored: {han} han, {fu} fu = {score} points"
        | EarnedExtraScore (han, fu, reason) ->
            let label = match reason with | Dora -> "Dora" | BaseFu -> "Base fu" | ScoreReason.ItemEffect item -> item.name
            printfn $"+{han} han, +{fu} fu ({label})"
        | EarnedExtraHonba _ -> ()
        | EarnedGold n -> printfn $"+{n} gold"
        | ShopEntered -> printfn "\n--- Shop ---"
        | PresentedItem items ->
            printfn "Available to buy:"
            if items.IsEmpty then printfn "  (None)"
            else
                for i in 0 .. items.Length - 1 do
                    let x = items.[i]
                    printfn $"  {i}. {x.name} ({x.cost}G) - {x.description}"
        | Bought item -> printfn $"Bought {item.name}."
        | Sold item ->
            let v = discount item.cost
            printfn $"Sold {item.name} for {v}G."
        | NotEnoughGold -> printfn "Not enough gold."
        | InventoryFull -> printfn "Inventory full."
        | ShopItemNotFound -> printfn "Item not available."
        | ItemAlreadyOwned -> printfn "You already own that item."
        | ShuffledPile s -> printfn $"Pile shuffled ({s})"
        | RevealedDora tiles ->
            let doraStr = tiles |> List.map string |> String.concat ""
            printfn $"Dora: {doraStr}"
        | NextRound r -> printfn $"Round {r}!"
        | GameOverEvent -> printfn "Game over."
        | ItemTriggered i -> printfn $"{i.name} triggered"
        | _ -> ()

let rec askInGame (state: GameState): PlayerInput =
    printfn $"  Pile: {state.pile.Length} remaining"
    if canTsumo state then printfn "  >> Tsumo available! <<"
    printfn "  [1-9] discard  [k<n>] kan  [t] tsumo  [?] wait"
    match CliInput.TryParse(Console.ReadLine()) with
    | Some(CliInput.Tsumo) when canTsumo state -> DeclareTsumo
    | Some(CliInput.Kan t) when canKan t state -> DeclareKan(t)
    | Some(CliInput.Discard t) when canDiscard t state -> Discard(t)
    | Some _ when isPileEmpty state -> ConfirmPileEmpty
    | _ -> askInGame state

let rec askShop (state: GameState): PlayerInput =
    printfn $"Gold: {state.gold} | Items: {state.items.Length}/{state.maxItems}"
    printfn "\nYour items (sell):"
    if state.items.IsEmpty then printfn "  (None)"
    else
        for i in 0 .. state.items.Length - 1 do
            let x = state.items.[i]
            printfn $"  s{i}. Sell {x.name} (+{discount x.cost}G)"
    printfn "  [<n>] buy  [s<n>] sell  [q] exit"

    match Console.ReadLine().Trim() with
    | "q" -> ExitShop
    | s when s.StartsWith("s") ->
        match Int32.TryParse(s.Substring(1)) with
        | (true, idx) when 0 <= idx && idx < state.items.Length -> Sell state.items.[idx]
        | _ -> printfn "Invalid item."; askShop state
    | s ->
        match Int32.TryParse(s) with
        | (true, idx) when 0 <= idx && idx < state.shopItems.Length ->
            Buy { state.shopItems.[idx] with id = Guid.NewGuid() }
        | _ -> printfn "Invalid."; askShop state

let step state input =
    let (s, log) = update state input
    renderLog log
    (s, log)

[<EntryPoint>]
let main argv =
    let rng =
        if Array.length argv = 2 then Random(int argv[1])
        else Random(Environment.TickCount)

    let mutable state = createGameState rng

    while state.phase <> GameOver do
        match state.phase with
        | InGame ->
            let r = state.round
            let g = state.goalScore
            let h = state.honbaLeft
            printfn $"\n=== Round {r} | Goal {g} | Honba {h} ==="

            let itemDisplay =
                if state.items.IsEmpty then "(none)"
                else state.items |> List.mapi (fun i x -> $"[{i}] {x.name}") |> String.concat " "
            printfn $"Items: {itemDisplay}"
            printfn $"{state}"

            state <- step state (askInGame state) |> fst

        | ScorePresentation ->
            let s = state.currentScore
            let g = state.goalScore
            printfn $"\n>>> Tsumo! Score: {s} / Goal: {g} <<<"
            state <- step state ConfirmScore |> fst

        | Shop ->
            let mutable inShop = true
            while inShop && state.phase = Shop do
                let (newState, log) = step state (askShop state)
                state <- newState
                let isTransaction =
                    log |> List.exists (function Bought _ | Sold _ | NotEnoughGold | InventoryFull | ShopItemNotFound | ItemAlreadyOwned -> true | _ -> false)
                if not isTransaction && state.phase = Shop then inShop <- false

        | _ -> ()

    0
