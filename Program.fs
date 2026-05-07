open Types
open Utils
open Evaluator
open System

let emptyState hand dora items = {
    rng = Random(); hand = hand; pile = [||]; discardPile = [||]; doraPile = [||]; dora = dora;
    rinshang = [||]; round = 1; tsumoLeft = 0; isRinshanKaihouApplicable = false;
    isTenhouApplicable = false; items = items
}

let hand = Hand ([|0;0;0;0;1;0;0;0;0;0|], Tile 4, [Kantsu <| Tile 2; Kantsu <| Tile 3; Kantsu <| Tile 6; Kantsu <| Tile 8])
let dora1 = [|Tile 1; Tile 1; Tile 1; Tile 1; Tile 7; Tile 5; Tile 5; Tile 5; Tile 5; Tile 7|]
let items1 = Yaku.yakuItems @ [
  ("Riichi", "", [YakuTrigger], Always, fun () -> [ItemEffect.Yaku 1u])
  ("Kaitei", "", [YakuTrigger], Always, fun () -> [ItemEffect.Yaku 1u])
  ("Ippatsu", "", [YakuTrigger], Always, fun () -> [ItemEffect.Yaku 1u])
]

let dummyState1 = emptyState hand dora1 items1

printfn $"{hand}"

let (Some (han, fuVal, scoreVal, names)) = calculateScore dummyState1

List.map (fun x -> printfn $"{x}") names |> ignore
printfn $"{han} {fuVal} {scoreVal}\n"

let hand2 = Hand ([|0;1;1;2;3;2;1;1;0;2|], Tile 9, [])
let dummyState2 = emptyState hand2 [||] Yaku.yakuItems

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


  let mutable pile: Tile array = List.toArray allTiles
  rng.Shuffle pile

  let mutable gameState = GameState.createGameState rng

  let mutable didTsumo = false

  while not didTsumo do
    let maybeScore = calculateScore gameState

    printfn $"{gameState.hand}"
    
    match maybeScore with
      | Some (_) ->
        printfn "Tsumo available"
        // List.map (fun x -> printfn $"{x}") names |> ignore
        // printfn $"{han} {fu} {score}"
      | None ->
        ignore ()

    let rec Ask () =
      printfn "1-9 to discard, kn with kan n or t to shout tsumo"
      let choice = Console.ReadLine ()
      match PlayerInput.TryParse(choice) with
        | Some(x) ->
          match x with
            | Tsumo -> if maybeScore <> None then Tsumo else Ask ()
            | Kan(t) when gameState.hand.IsKanValid(t) -> Kan(t)
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

  printfn $"{gameState.hand}"

  let maybeScore = calculateScore gameState

  match maybeScore with
    | Some (han, fuVal, scoreVal, _) -> printfn $"{han}판 {fuVal}부 {scoreVal}점"
    | None -> ()

  0
