module GameState

open Config
open Types
open Utils
open System
open Evaluator
open Items

let addBaseScore (state: GameState) (han, fu) =
  {state with baseScore = (fst state.baseScore + han, snd state.baseScore + fu)}

let addTsumoCount (state: GameState) amount =
  {state with tsumoLeft = state.tsumoLeft + amount}

let processItems (state: GameState) (trigger: ItemTrigger) items =
  let parsedHand = parseHand state.hand
  let (Hand (_, tsumo, _)) = state.hand
  let machi = List.map (fun x -> parseMachi x tsumo |> List.map (fun y -> (x, y)))  parsedHand |> List.concat

  items
    |> List.filter (fun x ->
                    if not (List.contains trigger x.triggers) then false
                    else
                        if machi.IsEmpty then
                            evaluateCondition x.condition state state.hand None None tsumo
                        else
                            List.exists (fun (p, m) -> evaluateCondition x.condition state state.hand (Some p) (Some m) tsumo) machi)
    |> List.fold
      (fun state item ->
       List.fold
         (fun state effect ->
          match effect with
          | ExtraScore (x, y) -> addBaseScore state (x, y)
          | Yaku han -> addBaseScore state (int han, 0)
          | AddTsumo n -> addTsumoCount state n
          | SubtractTargetScore x -> {state with goalScore = state.goalScore - x}
          | ModifyPile f -> {state with pile = f state.pile}
          | ModifyGameState f -> f state
          | AddGold n -> {state with gold = state.gold + n})
         state item.effect)
       state

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
        itemsLeft = allItems
        baseScore = (0, 0)
    }

let discard (t: Tile) (state: GameState) : GameState option =
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
            Some (processItems nextState OnDiscard nextState.items)
        else
            None // No tiles left to draw
    else
        None

let kan (t: Tile) (state: GameState) : GameState option =
    if state.hand.IsKanValid(t) then
        if state.rinshang.Length > 0 then
            let newTsumo = Array.head state.rinshang
            let newRinshang = Array.skip 1 state.rinshang
            let newHand = state.hand.Kan t newTsumo

            let newDora, newDoraPile =
                if state.doraPile.Length > 0 then
                    (Array.append state.dora [| Array.head state.doraPile |], Array.skip 1 state.doraPile)
                else
                    (state.dora, state.doraPile)

            let nextState =
                { state with
                    hand = newHand
                    rinshang = newRinshang
                    dora = newDora
                    doraPile = newDoraPile
                    isRinshanKaihouApplicable = true
                    isTenhouApplicable = true }
            Some (processItems nextState OnKan nextState.items)
        else
            None
    else
        None

let isPileEmpty (state: GameState) = Array.isEmpty state.pile

let declareTsumo (state: GameState) =
    calculateScore state

let isComplete (state: GameState) =
    calculateScore state |> Option.isSome

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

  {
    state with
      hand = hand
      pile = remainingPile
      discardPile = [||]
      dora = dora
      rinshang = rinshang
      isRinshanKaihouApplicable = false
      isTenhouApplicable = true
  }

let nextTsumoWithScore (state: GameState) (score: bigint) =
  {
    (resetPile state) with
      tsumoLeft = state.tsumoLeft - 1
      currentScore = state.currentScore + score
      baseScore = (0, 0)
  }

let nextRound (state: GameState) =
  let stateAfterEnd = processItems state OnRoundEnd state.items
  let additionalGolds = Config.calculateGoldsEarned stateAfterEnd.tsumoLeft
  
  (additionalGolds, {
     (resetPile stateAfterEnd) with
       tsumoLeft = Config.tsumoPerRound
       currentScore = 0I
       round = stateAfterEnd.round + 1
       goalScore = Config.nextGoalScore stateAfterEnd.goalScore
       gold = stateAfterEnd.gold + additionalGolds 
       baseScore = (0, 0)
   })

let buyItem (state: GameState) (item: Item) =
  let newItemsLeft = List.filter (fun x -> x.name <> item.name) state.itemsLeft

  let nextState = {
    state with
      itemsLeft = newItemsLeft
      items = item :: state.items
      gold = state.gold - item.cost
  }
  processItems nextState WhenObtained [item]

let sellItem (state: GameState) (item: Item) =
  let newItems = List.filter (fun x -> x.name <> item.name) state.items
  {
    state with
      items = newItems
      itemsLeft = item :: state.itemsLeft
      gold = state.gold + item.cost
  }
