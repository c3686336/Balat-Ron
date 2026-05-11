module GameState

open Config
open Types
open Utils
open System
open Evaluator
open Items

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
    }

let discard (t: Tile) (state: GameState) : GameState option =
    if state.hand.IsDiscardValid(t) then
        if state.pile.Length > 0 then
            let newTsumo = Array.head state.pile
            let newPile = Array.skip 1 state.pile
            let newHand = state.hand.Discard t newTsumo
            Some { state with
                    hand = newHand
                    pile = newPile
                    discardPile = Array.append state.discardPile [| t |]
                    tsumoLeft = state.tsumoLeft
                    isRinshanKaihouApplicable = false 
                    isTenhouApplicable = false}
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

            Some { state with
                    hand = newHand
                    rinshang = newRinshang
                    dora = newDora
                    doraPile = newDoraPile
                    isRinshanKaihouApplicable = true
                    isTenhouApplicable = true}
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
  }

let nextRound (state: GameState) =
  let additionalGolds = Config.calculateGoldsEarned state.tsumoLeft
  
  (additionalGolds, {
     (resetPile state) with
       tsumoLeft = Config.tsumoPerRound
       currentScore = 0I
       round = state.round + 1
       goalScore = Config.nextGoalScore state.goalScore
       gold = state.gold + additionalGolds 
   })

let buyItem (state: GameState) (item: Item) =
  let newItemsLeft = List.filter (fun x -> x.name <> item.name) state.itemsLeft

  {
    state with
      itemsLeft = newItemsLeft
      items = item :: state.items
      gold = state.gold - item.cost
  }
