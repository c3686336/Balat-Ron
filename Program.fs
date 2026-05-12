open Types
open Config
open Utils
open Evaluator
open System
open GameState
open Items
open Yaku

// let emptyState hand dora items = {
//     rng = Random(); hand = hand; pile = [||]; discardPile = [||]; doraPile = [||]; dora = dora;
//     rinshang = [||]; round = 1; tsumoLeft = 0; isRinshanKaihouApplicable = false;
//     isTenhouApplicable = false; items = items; currentScore = 0; goalScore = 0; gold = 0; itemsLeft = []; baseScore = (0, 0)
// }

// let hand = Hand ([|0;0;0;0;1;0;0;0;0;0|], Tile 4, [Kantsu <| Tile 2; Kantsu <| Tile 3; Kantsu <| Tile 6; Kantsu <| Tile 8])
// let dora1 = [|Tile 1; Tile 1; Tile 1; Tile 1; Tile 7; Tile 5; Tile 5; Tile 5; Tile 5; Tile 7|]
// let items1 = allItems @ [
//   { id = Guid.NewGuid(); name = "Riichi";
//     description = "Grants +1 Yaku (score multiplier) if you declare readiness to win before your final draw.";
//     rarity = Common;
//     effect = (fun _ _ e -> match e with | OnYakuCalc _ -> [ItemEffect.ExtraScore (1, 0)] | _ -> []);
//     cost = 50;
//     state = Nothing }
//   { id = Guid.NewGuid(); name = "Kaitei";
//     description = "Grants +1 Yaku (score multiplier) if you win on the very last tile drawn in the round.";
//     rarity = Common;
//     effect = (fun _ _ e -> match e with | OnYakuCalc _ -> [ItemEffect.ExtraScore (1, 0)] | _ -> []);
//     cost = 50;
//     state = Nothing }
//   { id = Guid.NewGuid(); name = "Ippatsu";
//     description = "Grants +1 Yaku (score multiplier) if you win within the first turn after declaring readiness.";
//     rarity = Common;
//     effect = (fun _ _ e -> match e with | OnYakuCalc _ -> [ItemEffect.ExtraScore (1, 0)] | _ -> []);
//     cost = 50;
//    state = Nothing}
//   ]

// let dummyState1 = emptyState hand dora1 items1

// printfn $"{hand}"

// let (Some (han, fuVal, scoreVal, names)) = calculateScoreFromCanonical dummyState1

// names |> List.iter (fun x -> printfn $"{x}")
// printfn $"{han} {fuVal} {scoreVal}\n"

// let hand2 = Hand ([|0;1;1;2;3;2;1;1;0;2|], Tile 9, [])
// let dummyState2 = emptyState hand2 [||] allItems 

// printfn $"{hand2}"
// let (Some (han2, fu2, score2, names2)) = calculateScoreFromCanonical dummyState2

// names2 |> List.iter (fun x -> printfn $"{x}")
// printfn $"{han2} {fu2} {score2}\n{names}"

let ParseHand (handArray: int array, kantsu: Kantsu list) =
    match handArray |> Array.tryFindIndex (fun x -> x > 0) with
    | Some firstTile ->
        let updatedArray = Array.updateAt firstTile (handArray[firstTile] - 1) handArray
        parseHand (Hand (updatedArray, Tile firstTile, kantsu))
    | None ->
        parseHand (Hand (handArray, Tile 1, kantsu))

let handArray = [|0; 0; 2; 2; 2; 2; 2; 2; 0; 2|]
let result = ParseHand (handArray, [])

List.map (fun x -> printfn $"{x}") result

let machi = parseMachi result.[1] (Tile 7)
List.map (fun x -> printfn $"{x}") machi 
iipeikoup result.[0] machi.[0] (Tile 7) |> printfn "%A"

[<EntryPoint>]
let main argv =
  printfn "----------"
  
  let rng =
    if Array.length argv = 2 then
      Random(int argv[1])
    else
      Random(Environment.TickCount)

  let mutable gameState = GameState.createGameState rng
  let mutable isGameOver = false

  while not isGameOver do
    printfn $"Goal score: {gameState.goalScore}"
    printfn $"Tsumo left: {gameState.tsumoLeft}"


    while gameState.tsumoLeft <> 0 && gameState.currentScore < gameState.goalScore do
      gameState.items |> List.mapi (fun i x -> $"{i}. {x}\n") |> String.concat "" |> printfn "Items:\n%s----"
      
      let mutable didTsumo = false
      let mutable pileEmpty = false
    
      while not (didTsumo || pileEmpty) do
        let isDone = isComplete gameState

    
        printfn $"{gameState}"
        
        match isDone with
          | true ->
            printfn "Tsumo available"
            // names |> List.iter (fun x -> printfn $"{x}")
            // printfn $"{han} {fu} {score}"
          | false ->
            ignore ()
  
        printfn $"{Array.length gameState.pile} tiles remaining" 
    
        let rec Ask () =
          printfn "1-9 to discard, kn with kan n or t to shout tsumo."
          let choice = Console.ReadLine ()
          match PlayerInput.TryParse(choice) with
            | Some(x) ->
              match x with
                | Tsumo -> if isDone then Tsumo else Ask ()
                | Kan(t) when gameState.hand.IsKanValid(t) -> Kan(t)
                | _ when isPileEmpty gameState -> EmptyPile
                | Discard(t) when gameState.hand.IsDiscardValid(t) -> Discard(t)
                | _ -> Ask ()
            | None -> Ask ()
    
        let action = Ask ()
    
        match action with
          | Tsumo ->
            gameState <- fst (processItems gameState OnTsumo gameState.items)
            didTsumo <- true
          | Kan(t) ->
            gameState <- Option.get (GameState.kan t gameState) |> fst
          | Discard(t) ->
            gameState <- Option.get (GameState.discard t gameState) |> fst
          | EmptyPile ->
            let ((newGameState, newPileEmpty), _) = confirmEmptyPile gameState
            gameState <- newGameState
            pileEmpty <- newPileEmpty
    
      if didTsumo then
        printfn $"{gameState}"
        gameState <- declareTsumo gameState |> fst

      else if pileEmpty then
        printfn "Pile empty!"
  
      printfn $"Total score: {gameState.currentScore} / Goal score: {gameState.goalScore}"
      printfn $"{gameState.tsumoLeft} tsumo left"
      printfn "----------"

    if gameState.currentScore >= gameState.goalScore then
      printfn "Round clear!"
      let ((additionalGolds, newGameState), _) = nextRound gameState
      gameState <- newGameState

      printfn $"Earned {additionalGolds} golds. Total {gameState.gold} golds"

      // Shop phase
      let mutable shopItems = chooseShopItems rng Config.numberOfShopItems allItems
      let mutable inShop = true

      while inShop do
        printfn "\n--- Shop ---"
        printfn $"Gold: {gameState.gold} | Items: {gameState.items.Length}/{Config.maxItems}"
        printfn "Available to Buy:"
        if shopItems.Length = 0 then printfn "  (None)"
        else shopItems |> List.iteri (fun i x -> printfn $"  {i}. {x.name} ({x.cost}G) - {x.description}")
        
        printfn "\nYour Items (Sell):"
        if gameState.items.Length = 0 then printfn "  (None)"
        else gameState.items |> List.iteri (fun i x -> printfn $"  s{i}. Sell {x.name} (+{discount x.cost}G)")
        
        printfn "\nEnter item number to buy, 's' followed by number to sell (e.g. s0), or 'q' to finish shopping."
        match Console.ReadLine().Trim() with
        | "q" -> inShop <- false
        | s when s.StartsWith("s") ->
          match Int32.TryParse(s.Substring(1)) with
          | (true, idx) when 0 <= idx && idx < gameState.items.Length ->
            let itemToSell = gameState.items.[idx]
            gameState <- sellItem gameState itemToSell
            shopItems <- itemToSell :: shopItems
            printfn $"Sold {itemToSell.name} for {discount itemToSell.cost}G."
          | _ -> printfn "Invalid item to sell."
        | s ->
          match Int32.TryParse(s) with
          | (true, idx) when 0 <= idx && idx < shopItems.Length ->
            let itemToBuy = { shopItems.[idx] with id = Guid.NewGuid() }
            if gameState.items.Length >= Config.maxItems then
              printfn "You cannot hold any more items. Sell an item first."
            elif gameState.gold < itemToBuy.cost then
              printfn "Not enough gold!"
            else
              gameState <- buyItem gameState itemToBuy |> fst
              shopItems <- shopItems |> List.removeAt idx
              printfn $"Bought {itemToBuy.name}."
          | _ -> printfn "Invalid input."
      
    else
      printfn "Game Over"
      isGameOver <- true

  0
