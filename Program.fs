open Types
open Utils
open Evaluator
open System

let testHand (name: string) (arr: int array) =
    let result = ParseHand (arr, [])
    printfn $"=== %s{name} ==="
    printfn $"Found %d{result.Length} parsings."
    for r in result do
        printfn $"%O{r}"
    printfn ""

testHand "Ryanpeikou wait" [|0; 2; 2; 2; 2; 2; 2; 2; 0; 0|]
testHand "Iipeikou with kotsu" [|0; 3; 2; 2; 2; 2; 3; 0; 0; 0|]
testHand "Overlapping shuntsu 1" [|0; 1; 2; 3; 2; 1; 0; 0; 0; 5|] // 14 tiles. wait 1+2+3+2+1+5 = 14
testHand "Overlapping shuntsu 2" [|0; 3; 4; 4; 3; 0; 0; 0; 0; 0|]

let hand = Hand ([|0;0;0;0;2;0;0;0;0;0|], [Kantsu <| Tile 2; Kantsu <| Tile 3; Kantsu <| Tile 6; Kantsu <| Tile 8])

let (han, fu, score) = CalculateScore hand (Tile 4) [Tile 1; Tile 1; Tile 1; Tile 1; Tile 7; Tile 5; Tile 5; Tile 5; Tile 5; Tile 7] 0

printfn $"{han} {fu} {score}"

[<EntryPoint>]
let main argv =
  let rng =
    if Array.length argv = 2 then
      Random(int argv[1])
    else
      Random(Environment.TickCount)


  let mutable pile: Tile array = List.toArray allTiles
  rng.Shuffle pile

  let mutable handArray: ArrayHand = TileArrayToHand (Array.take 13 pile)
  pile <- Array.truncate 13 pile

  let mutable tsumoTile: Tile = Array.head pile
  pile <- Array.truncate 1 pile

  0
