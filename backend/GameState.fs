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

let applyItemEffects (state: GameState) (event: ItemEvent) items log =
  items
    |> List.fold
      (fun (state, log) item ->
       let itemEffects = item.effect state item event
       let log = if List.isEmpty itemEffects then log else log @ [ItemTriggered item]
       List.fold
         (fun (state, log) itemEffect ->
          match itemEffect with
            | ExtraScore (han, fu) ->
              (addBaseScore state (han, fu)), (log @ [EarnedExtraScore (han, fu, ScoreReason.ItemEffect item)])
            | AddHonba n ->
              { state with honbaLeft = state.honbaLeft + n }, (log @ [EarnedExtraHonba n])
            | AddGold n ->
              { state with gold = state.gold + n }, (log @ [EarnedGold n])
            | UpdateItemState itemState ->
              let newItems = state.items |> List.map (fun x -> if x.id = item.id then {x with state = itemState} else x)
              { state with items = newItems },
              (log @ [UpdatedItemState ({item with state = itemState}, itemState)])
            | SelfDestruct ->
              {state with items = state.items |> List.filter (fun x -> x.id <> item.id)},
              (log @ [ItemDestroyed item])
            | DiscloseNMoreDora n ->
              let doraToFlip = min (5 - Array.length state.dora) n
              if doraToFlip <= 0 then
                state, log
              else
                let newDoras = Array.sub state.doraPile 0 doraToFlip
                let events = RevealedDora (Array.toList <| newDoras)
                { state with
                    dora = Array.append state.dora newDoras
                    doraPile = Array.skip doraToFlip state.doraPile }, (log @ [events])
            | ShufflePile scope ->
              match scope with
              | DiscardToDrawPile ->
                let newPile = Array.copy state.discardPile
                state.rng.Shuffle(newPile)
                { state with pile = newPile; discardPile = [||] }, (log @ [ShuffledPile scope])
              | Everything
              | UnrevaledTilesOnly ->
                state, log
            | _ -> state, log)
         (state, log)
         itemEffects)
       (state, log)

let createGameState (rng: Random) : GameState =
    let mutable pile = List.toArray allTiles
    rng.Shuffle(pile)

    let mutable currentIdx = 0
    let take n =
        let res = Array.sub pile currentIdx n
        currentIdx <- currentIdx + n
        res

    // take 13
    let handTiles = [|Tile 1; Tile 1; Tile 1; Tile 1; Tile 2; Tile 2; Tile 2; Tile 2; Tile 3; Tile 3; Tile 3; Tile 3; Tile 4;|]
    let firstTsumo = take 1 |> Array.head
    let hand = Hand (tileArrayToHand handTiles, firstTsumo, [])

    // Rinshan has 4 tiles for up to 4 kans.
    let rinshang = take 4
    // Dora indicators (1 open, 4 hidden for kans)
    let dora = take 1
    let doraPile = take 9 // Including ura dora

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
        honbaLeft = Config.tsumoPerRound
        isRinshanKaihouApplicable = false
        isTenhouApplicable = true 
        // items = Yaku.yakuItems
        items = []
        currentScore = 0I
        goalScore = Config.initialGoalScore
        gold = 0
        baseScore = (0, 0)
        phase = GameStarting
        shopItems = []
    }

let canDiscard (t: Tile) (state: GameState) = state.hand.IsDiscardValid(t) && state.pile.Length >= 1
let canKan (t: Tile) (state: GameState) = state.hand.IsKanValid(t) && state.rinshang.Length > 0
let canAnyKan (state: GameState) = [1..9] |> List.exists (fun v -> canKan (Tile v) state)
let isPileEmpty (state: GameState) = Array.isEmpty state.pile

let isWrapAroundEnabled (state: GameState) =
    state.items
    |> List.exists (fun item -> 
        item.effect state item Parsing 
        |> List.contains AllowWrapAroundShuntsu)

let canTsumo (state: GameState) =
    parseHand (isWrapAroundEnabled state) state.hand |> List.isEmpty |> not

let canBuy (item: Item) (state: GameState) = state.gold >= item.cost && state.items.Length < Config.maxItems

let shufflePile (state: GameState) =
  let mutable pile = List.toArray allTiles
  state.rng.Shuffle(pile)

  let mutable currentIdx = 0
  let take n =
    let res = Array.sub pile currentIdx n
    currentIdx <- currentIdx + n
    res

  // let handTiles = take 13
  let handTiles = [|Tile 1; Tile 1; Tile 1; Tile 1; Tile 2; Tile 2; Tile 2; Tile 2; Tile 3; Tile 3; Tile 3; Tile 3; Tile 4;|]
  let firstTsumo = take 1 |> Array.head
  let hand = Hand (tileArrayToHand handTiles, firstTsumo, [])

  let rinshang = take 4
  let dora = take 1 
  let doraPile = take 9

  let remainingPile = Array.sub pile currentIdx (pile.Length - currentIdx)

  { state with
      hand = hand
      pile = remainingPile
      discardPile = [||]
      dora = dora
      doraPile = doraPile
      rinshang = rinshang
      isRinshanKaihouApplicable = false
      isTenhouApplicable = true }

let transitionTo (state: GameState) (phase: GamePhase) log =
  match phase with
    | InGame ->
      let state = shufflePile state
      let log = [ShuffledPile Everything; RevealedDora (Array.toList <| state.dora)] @ log
      let (state, log) = applyItemEffects state Honba state.items log
      
      { state with
          honbaLeft = state.honbaLeft - 1;
          phase = InGame;
          baseScore = (0, 0) }, log
    | Shop ->
      let (state, log) = applyItemEffects state OnRoundEnd state.items log
      let shopItems = chooseShopItems state.rng Config.numberOfShopItems allItems
      let earnedGold = calculateGoldsEarned state.honbaLeft
      
      { state with
          gold = state.gold + earnedGold;
          round = state.round + 1;
          goalScore = Config.nextGoalScore state.goalScore;
          honbaLeft = Config.tsumoPerRound;
          phase = Shop;
          shopItems = shopItems }, log @ [ShopEntered; EarnedGold earnedGold; PresentedItem shopItems]

    | ScorePresentation ->
      let (han, fu) = state.baseScore
      let score = score han fu
      let state =
        { state with
            currentScore = state.currentScore + score; }
      let log = ([ScoringStarted] @ log @ [Scored (han, fu, score)])

      { state with phase = phase }, log

    | GameOver ->
      { state with phase = GameOver }, log @ [GameOverEvent]
    | _ -> { state with phase = phase }, log
    
let update (state: GameState) (input: PlayerInput) =
  match state.phase with
    | GameStarting ->
      match input with
        | Start ->
          transitionTo state InGame [GameStarted]
        | _ ->
          state, []
    | InGame ->
      match input with
        | Discard t when state.hand.IsDiscardValid(t) && state.pile.Length >= 1 ->
          let newTsumo = Array.head state.pile
          
          let newPile = Array.skip 1 state.pile
          let newHand = state.hand.Discard t newTsumo

          let nextState =
            { state with
                hand = newHand
                pile = newPile
                discardPile = Array.append state.discardPile [| t |]
                isRinshanKaihouApplicable = false
                isTenhouApplicable = false } 

          applyItemEffects nextState (OnDiscard t) nextState.items [TileDiscarded t; TileDrawn newTsumo]

        | DeclareKan t when canKan t state ->
          let newTsumo = Array.head state.rinshang
          
          let newRinshang = Array.skip 1 state.rinshang
          let newHand = state.hand.Kan t newTsumo
          
          let nextState =
            { state with
                hand = newHand
                rinshang = newRinshang
                isRinshanKaihouApplicable = true
                isTenhouApplicable = false }

          let (state, log) = applyItemEffects nextState (OnKan t) nextState.items [Kan t ; RinshangDrawn newTsumo]

          state, log

        | DeclareTsumo when canTsumo state ->
          // Transition to the score presentation
          let (state, log) = applyItemEffects state OnTsumo state.items []

          let everyParsingResult = everyParsing (isWrapAroundEnabled state) state.hand
          let nDora = calculateDora state.hand (Array.toList state.dora)

          let (newState, log) =
            everyParsingResult
              |> List.map (fun (parsedHand, machi, tsumo) ->
                           let (state1, log) = applyItemEffects state (OnYakuCalc (parsedHand, machi, tsumo, state.hand)) state.items log
                           ({state1 with baseScore = (fst state1.baseScore + nDora, fu parsedHand machi tsumo + snd state1.baseScore)}, log @ [EarnedExtraScore (nDora, 0, Dora); EarnedExtraScore (0, fu parsedHand machi tsumo, BaseFu)]))
              |> List.maxBy (fun (state, _) ->
                             let (han, fu) = state.baseScore
                             score han fu)
          transitionTo newState ScorePresentation log

        | ConfirmPileEmpty ->
          let (state, log) = applyItemEffects state WhenPileEmpty state.items []
          if state.pile.Length > 0 then
            state, log
          else
            transitionTo state ScorePresentation log

        | _ -> state, []

      | ScorePresentation ->
        match input with
          | ConfirmScore when state.currentScore >= state.goalScore ->
            transitionTo state Shop [] 

          | ConfirmScore when state.honbaLeft > 0 ->
            transitionTo state InGame []

          | ConfirmScore ->
            transitionTo state GameOver []

          | _ -> state, []
      | Shop ->
        match input with
          | Buy item when not (state.shopItems |> List.exists (fun x -> x.name = item.name)) ->
            state, [ShopItemNotFound]

          | Buy item when state.items.Length >= Config.maxItems ->
            state, [InventoryFull]

          | Buy item when state.gold >= item.cost ->
            let state = { state with
                            items = item :: state.items
                            gold = state.gold - item.cost
                            shopItems = state.shopItems |> List.filter (fun x -> x.name <> item.name) }
            let log = [Bought item; PresentedItem state.shopItems]

            applyItemEffects state WhenObtained [item] log

          | Buy item ->
            state, [NotEnoughGold]

          | Sell item ->
            let newItems = List.filter (fun x -> x.id <> item.id) state.items
            let returnedItem = { item with id = Guid.NewGuid() }
            let state = { state with
                            items = newItems
                            gold = state.gold + discount item.cost
                            shopItems = returnedItem :: state.shopItems }
            let log = [Sold item; PresentedItem state.shopItems]

            applyItemEffects state WhenSold [item] log

          | ExitShop ->
            transitionTo state InGame []

          | _ -> state, []
      | _ -> state, []
