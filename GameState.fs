module GameState

open Config
open Types
open Utils
open System
open Evaluator
open Items
open Fu

let addBaseScore (state: GameState) (han, fu) =
  {state with baseScore = (fst state.baseScore + han, snd state.baseScore + fu)}

let addTsumoCount (state: GameState) amount =
  {state with tsumoLeft = state.tsumoLeft + amount}

let processItems (state: GameState) (event: Event) items =
  items
    |> List.fold
      (fun (state, effects) item ->
       let itemEffects = item.effect state item event
       let newState =
         List.fold
           (fun state effect ->
            match effect with
            | ExtraScore (x, y) -> addBaseScore state (x, y)
            | AddTsumo n -> addTsumoCount state n
            | SubtractTargetScore x -> {state with goalScore = state.goalScore - x}
            | ModifyPile p -> {state with pile = p}
            | ModifyGameState s -> s
            | AddGold n -> {state with gold = state.gold + n}
            | UpdateItemState itemState ->
              let newItems = state.items |> List.map (fun x -> if x.id = item.id then {x with state = itemState} else x)
              {state with items = newItems}
            | SelfDestruct ->
              {state with items = state.items |> List.filter (fun x -> x.id <> item.id)}
            | PrintName ->
              printfn "%s" item.name
              state
            | PrintStr s ->
              printfn "%s" s
              state
            | AllowWrapAroundShuntsu -> state
            | DiscloseNMoreDora n ->
              let doraToFlip = min (5 - Array.length state.dora) n
              { state with
                  dora = Array.append (Array.sub state.doraPile 0 doraToFlip) state.dora
                  doraPile = Array.skip doraToFlip state.doraPile }
            | DiscloseUraDora -> state)
           state itemEffects
       (newState, Map.add event ((item, itemEffects) :: (Map.tryFind event effects |> Option.defaultValue [])) effects))
      (state, Map.empty)

let createGameState (rng: Random) : GameState =
    let mutable pile = List.toArray allTiles
    rng.Shuffle(pile)

    let mutable currentIdx = 0
    let take n =
        let res = Array.sub pile currentIdx n
        currentIdx <- currentIdx + n
        res

    let handTiles = take 13
    let firstTsumo = take 1 |> Array.head
    let hand = Hand (tileArrayToHand handTiles, firstTsumo, [])

    // Rinshan has 4 tiles for up to 4 kans.
    let rinshang = take 4
    // Dora indicators (1 open, 4 hidden for kans)
    let dora = take 1
    let doraPile = take 7 // Including ura dora

    let remainingPile = Array.sub pile currentIdx (pile.Length - currentIdx)

    {
        rng = rng
        hand = hand
        pile = remainingPile
        discardPile = [||]
        doraPile = doraPile
        dora = dora
        rinshang = rinshang
        round = 1
        tsumoLeft = Config.tsumoPerRound
        isRinshanKaihouApplicable = false
        isTenhouApplicable = true 
        // items = Yaku.yakuItems
        items = []
        currentScore = 0I
        goalScore = Config.initialGoalScore
        gold = 0
        baseScore = (0, 0)
    }

let discard (t: Tile) (state: GameState) : (GameState * ItemEffects) option =
    if state.hand.IsDiscardValid(t) then
        if state.pile.Length > 0 then
            let newTsumo = Array.head state.pile
            let newPile = Array.skip 1 state.pile
            let newHand = state.hand.Discard t newTsumo
            let nextState =
                { state with
                    hand = newHand
                    pile = newPile
                    discardPile = Array.append state.discardPile [| t |]
                    tsumoLeft = state.tsumoLeft
                    isRinshanKaihouApplicable = false 
                    isTenhouApplicable = false }
            Some (processItems nextState (OnDiscard t) nextState.items)
        else
            None
    else
        None

let kan (t: Tile) (state: GameState) : (GameState * ItemEffects) option =
    if state.hand.IsKanValid(t) then
        if state.rinshang.Length > 0 then
            let newTsumo = Array.head state.rinshang
            let newRinshang = Array.skip 1 state.rinshang
            let newHand = state.hand.Kan t newTsumo

            let nextState =
                { state with
                    hand = newHand
                    rinshang = newRinshang
                    isRinshanKaihouApplicable = true
                    isTenhouApplicable = false }
            Some (processItems nextState (OnKan t) nextState.items)
        else
            None
    else
        None

let isPileEmpty (state: GameState) = Array.isEmpty state.pile

let resetPile (state: GameState) =
  let mutable pile = List.toArray allTiles
  state.rng.Shuffle(pile)

  let mutable currentIdx = 0
  let take n =
    let res = Array.sub pile currentIdx n
    currentIdx <- currentIdx + n
    res

  let handTiles = take 13
  let firstTsumo = take 1 |> Array.head
  let hand = Hand (tileArrayToHand handTiles, firstTsumo, [])

  // Rinshan has 4 tiles for up to 4 kans.
  let rinshang = take 4
  // Dora indicators (1 open, 4 hidden for kans)
  let dora = take 1 
  let doraPile = take 9 // Including ura dora

  let remainingPile = Array.sub pile currentIdx (pile.Length - currentIdx)

  processItems {
    state with
      hand = hand
      pile = remainingPile
      discardPile = [||]
      dora = dora
      rinshang = rinshang
      isRinshanKaihouApplicable = false
      isTenhouApplicable = true
  } PileReset state.items

let getWrapAround (effects: ItemEffects) =
    effects.TryFind(Parsing)
    |> Option.defaultValue []
    |> List.collect snd
    |> List.exists (function AllowWrapAroundShuntsu -> true | _ -> false)

let declareTsumo (state: GameState) =
  let (_, parseEffects) = processItems state Parsing state.items
  let wrapAround = getWrapAround parseEffects
  let everyParsingResult = everyParsing wrapAround state.hand
  let nDora = calculateDora state.hand (Array.toList state.dora)

  let (_, parse, yakuEffects) =
    everyParsingResult
      |> List.map (fun parse ->
                   let (a, b, c) = parse
                   let (newState, effects) = processItems state (OnYakuCalc (a, b, c, state.hand)) state.items
                   let fu = fu a b c
                   (score (fst newState.baseScore + nDora) (snd newState.baseScore + fu), parse, effects))
      |> List.maxBy (fun (s, _, _) -> s)

  let (a, b, c) = parse
  let (calculation, scoreEffects) = processItems state (OnScoreCalc (a, b, c, state.hand)) state.items
  let fu = fu a b c
  let (han, yakuFu) = calculation.baseScore
  let finalScore = score (han + nDora) (fu + yakuFu)
  printfn $"도라 {nDora}"
  printfn $"{han + nDora}판 {yakuFu + fu}부 {finalScore}점"

  let (resetState, resetEffects) = resetPile calculation
  let mergedEffects = [yakuEffects; scoreEffects; resetEffects] |> List.reduce (fun a b -> Map.fold (fun m k v -> Map.add k (v @ (Map.tryFind k m |> Option.defaultValue [])) m) a b)

  ({
    resetState with
      tsumoLeft = state.tsumoLeft - 1
      currentScore = state.currentScore + finalScore
      baseScore = (0, 0)
  }, mergedEffects)

let confirmEmptyPile (state: GameState) =
  let (newState, effects) = processItems state WhenPileEmpty state.items
  
  if isPileEmpty newState then
    let (resetState, resetEffects) = resetPile state
    let mergedEffects = Map.fold (fun m k v -> Map.add k (v @ (Map.tryFind k m |> Option.defaultValue [])) m) resetEffects effects
    (({ resetState with
         tsumoLeft = state.tsumoLeft - 1
         currentScore = state.currentScore
         baseScore = (0, 0)
     }, true), mergedEffects)
  else
    ((newState, false), effects)

let isComplete (state: GameState) =
    let (_, effects) = processItems state Parsing state.items
    parseHand (getWrapAround effects) state.hand |> List.isEmpty |> not

let nextRound (state: GameState) =
  let (stateAfterEnd, effects) = processItems state OnRoundEnd state.items
  let additionalGolds = Config.calculateGoldsEarned stateAfterEnd.tsumoLeft
  let (resetState, resetEffects) = resetPile stateAfterEnd
  let mergedEffects = Map.fold (fun m k v -> Map.add k (v @ (Map.tryFind k m |> Option.defaultValue [])) m) resetEffects effects

  ((additionalGolds, {
     resetState with
       tsumoLeft = Config.tsumoPerRound
       currentScore = 0I
       round = stateAfterEnd.round + 1
       goalScore = Config.nextGoalScore stateAfterEnd.goalScore
       gold = stateAfterEnd.gold + additionalGolds 
       baseScore = (0, 0)
   }), mergedEffects)

let buyItem (state: GameState) (item: Item) =
  let nextState = {
    state with
      items = item :: state.items
      gold = state.gold - item.cost
  }
  processItems nextState WhenObtained [item]

let sellItem (state: GameState) (item: Item) =
  let newItems = List.filter (fun x -> x.id <> item.id) state.items
  {
    state with
      items = newItems
      gold = state.gold + discount item.cost
  }

let nextHonba (state: GameState) =
  processItems state Honba state.items
