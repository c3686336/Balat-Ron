open Types
open Utils
open Evaluator
open System
open GameState
open Items

let emptyState hand dora items = {
    rng = Random(); hand = hand; pile = [||]; discardPile = [||]; doraPile = [||]; dora = dora;
    rinshang = [||]; round = 1; tsumoLeft = 0; isRinshanKaihouApplicable = false;
    isTenhouApplicable = false; items = items; currentScore = 0; goalScore = 0; gold = 0; itemsLeft = []
}

let hand = Hand ([|0;0;0;0;1;0;0;0;0;0|], Tile 4, [Kantsu <| Tile 2; Kantsu <| Tile 3; Kantsu <| Tile 6; Kantsu <| Tile 8])
let dora1 = [|Tile 1; Tile 1; Tile 1; Tile 1; Tile 7; Tile 5; Tile 5; Tile 5; Tile 5; Tile 7|]
let items1 = allItems @ [
  { name = "Riichi"; description = "Grants +1 Yaku (score multiplier) if you declare readiness to win before your final draw."; triggers = [YakuTrigger]; condition = Always; effect = [ItemEffect.Yaku 1u]; cost = 50 }
  { name = "Kaitei"; description = "Grants +1 Yaku (score multiplier) if you win on the very last tile drawn in the round."; triggers = [YakuTrigger]; condition = Always; effect = [ItemEffect.Yaku 1u]; cost = 50 }
  { name = "Ippatsu"; description = "Grants +1 Yaku (score multiplier) if you win within the first turn after declaring readiness."; triggers = [YakuTrigger]; condition = Always; effect = [ItemEffect.Yaku 1u]; cost = 50 }
]

let dummyState1 = emptyState hand dora1 items1

printfn $"{hand}"

let (Some (han, fuVal, scoreVal, names)) = calculateScore dummyState1

List.map (fun x -> printfn $"{x}") names |> ignore
printfn $"{han} {fuVal} {scoreVal}\n"

let hand2 = Hand ([|0;1;1;2;3;2;1;1;0;2|], Tile 9, [])
let dummyState2 = emptyState hand2 [||] allItems 

printfn $"{hand2}"
let (Some (han2, fu2, score2, names2)) = calculateScore dummyState2

List.map (fun x -> printfn $"{x}") names2 |> ignore
printfn $"{han2} {fu2} {score2}\n{names}"

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
        let maybeScore = calculateScore gameState

    
        printfn $"{gameState}"
        
        match maybeScore with
          | Some (_) ->
            printfn "Tsumo available"
            // List.map (fun x -> printfn $"{x}") names |> ignore
            // printfn $"{han} {fu} {score}"
          | None ->
            ignore ()
  
        printfn $"{Array.length gameState.pile} tiles remaining" 
    
        let rec Ask () =
          printfn "1-9 to discard, kn with kan n or t to shout tsumo."
          let choice = Console.ReadLine ()
          match PlayerInput.TryParse(choice) with
            | Some(x) ->
              match x with
                | Tsumo -> if maybeScore <> None then Tsumo else Ask ()
                | Kan(t) when gameState.hand.IsKanValid(t) -> Kan(t)
                | _ when isPileEmpty gameState -> EmptyPile
                | Discard(t) when gameState.hand.IsDiscardValid(t) -> Discard(t)
                | _ -> Ask ()
            | None -> Ask ()
    
        let action = Ask ()
    
        match action with
          | Tsumo ->
            didTsumo <- true
          | Kan(t) ->
            gameState <- Option.get (GameState.kan t gameState)
          | Discard(t) ->
            gameState <- Option.get (GameState.discard t gameState)
          | EmptyPile ->
            pileEmpty <- true
    
      if didTsumo then
        printfn $"{gameState}"
      
        let maybeScore = calculateScore gameState
      
        match maybeScore with
          | Some (han, fuVal, scoreVal, names) ->
            let namesStr = names |> List.map (fun x -> x.ToString()) |> String.concat "\n"
            printfn $"{namesStr}\n{han}판 {fuVal}부 {scoreVal}점"
            gameState <- nextTsumoWithScore gameState scoreVal
          | None ->
            ()
      else if pileEmpty then
        printfn "Pile empty!"
        printfn $"{gameState.tsumoLeft}"
        gameState <- nextTsumoWithScore gameState 0I
        printfn $"{gameState.tsumoLeft}"
  
      printfn $"Total score: {gameState.currentScore} / Goal score: {gameState.goalScore}"
      printfn $"{gameState.tsumoLeft} tsumo left"
      printfn "----------"

    if gameState.currentScore >= gameState.goalScore then
      printfn "Round clear!"
      let (additionalGolds, newGameState) = nextRound gameState
      gameState <- newGameState

      printfn $"Earned {additionalGolds} golds. Total {gameState.gold} golds"

      // Shop phase
      let items = chooseRandom rng 3 gameState.itemsLeft

      items |> List.mapi (fun i x -> $"{i}. {x}\n") |> String.concat "" |> printf "%s"

      let rec Ask () =
        printfn "0-2 to chose the item to buy. s to Skip"

        match Console.ReadLine().Trim() with
          | "s" -> None
          | s ->
            match Int32.TryParse(s) with
              | (true, x) when 0 <= x && x <= 2 && gameState.gold >= items.[x].cost ->
                Some (items.[x]) 
              | _ -> Ask()

      match Ask () with
        | None ->
          printfn "No item was bought"
        | Some(item) ->
          printfn $"Bought {item.name}"
          gameState <- buyItem gameState item

      printfn $"Golds: {gameState.gold}"
      
    else
      printfn "Game Over"
      isGameOver <- true

  0
