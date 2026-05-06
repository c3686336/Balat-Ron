open Types
open Utils
open Evaluator
open System

let hand = Hand ([|0;0;0;0;1;0;0;0;0;0|], Tile 4, [Kantsu <| Tile 2; Kantsu <| Tile 3; Kantsu <| Tile 6; Kantsu <| Tile 8])

printfn $"{hand}"

let (Some (han, fu, score, names)) = CalculateScore hand  [Tile 1; Tile 1; Tile 1; Tile 1; Tile 7; Tile 5; Tile 5; Tile 5; Tile 5; Tile 7] 0

printfn $"{han} {fu} {score}\n"
List.map (fun x -> printfn $"{x}") names

let hand2 = Hand ([|0;2;2;2;2;2;2;1;0;0|], Tile 7, [])

printfn $"{hand2}"
let (Some (han2, fu2, score2, names2)) = CalculateScore hand2  [] 0

printfn $"{han2} {fu2} {score2}\n{names}"
List.map (fun x -> printfn $"{x}") names2

[<EntryPoint>]
let main argv =
  let rng =
    if Array.length argv = 2 then
      Random(int argv[1])
    else
      Random(Environment.TickCount)


  let mutable pile: Tile array = List.toArray allTiles
  rng.Shuffle pile

  let mutable hand: Hand = Hand (TileArrayToHand (Array.take 13 pile), Array.head pile, [])
  pile <- Array.truncate 14 pile

  printfn $"{hand}"

  0
