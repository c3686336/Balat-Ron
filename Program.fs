open Types
open Utils
open Evaluator
open System

let hand = Hand ([|0;0;0;0;1;0;0;0;0;0|], Tile 4, [Kantsu <| Tile 2; Kantsu <| Tile 3; Kantsu <| Tile 6; Kantsu <| Tile 8])

printfn $"{hand}"

let (Some (han, fu, score, names)) = CalculateScore hand  [Tile 1; Tile 1; Tile 1; Tile 1; Tile 7; Tile 5; Tile 5; Tile 5; Tile 5; Tile 7] 2 ["Riichi"; "Kaitei"; "Ippatsu"]

List.map (fun x -> printfn $"{x}") names |> ignore
printfn $"{han} {fu} {score}\n"

let hand2 = Hand ([|0;1;1;2;3;2;1;1;0;2|], Tile 9, [])

printfn $"{hand2}"
let (Some (han2, fu2, score2, names2)) = CalculateScore hand2  [] 0 []

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

  let mutable hand: Hand = Hand (TileArrayToHand (Array.take 13 pile), Array.head pile, [])
  pile <- Array.skip 14 pile

  let mutable isTenhouApplicable = true
  let mutable isRinShanApplicable = false
  let mutable didTsumo = false

  while not didTsumo do
    let maybeScore = CalculateScore hand [] 0 []

    printfn $"{hand}"
    
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
            | Kan(t) when hand.IsKanVaild(t) -> Kan(t)
            | Discard(t) when hand.IsDiscardValid(t) -> Discard(t)
            | _ -> Ask ()
        | None -> Ask ()

    let action = Ask ()

    match action with
      | Tsumo -> didTsumo <- true
      | Kan(t) ->
        hand <- hand.Kan t (Array.head pile)
        pile <- Array.skip 1 pile
      | Discard(t) ->
        hand <- hand.Discard t (Array.head pile)
        pile <- Array.skip 1 pile

  printfn $"{hand}"

  let maybeScore = CalculateScore hand [] 0 []

  match maybeScore with
    | Some (han, fu, score, _) -> printfn $"{x}"

  0
