module Utils

open Types

let inline RoundUpTo (n: ^T) (d: ^T) =
  let r = n % d
  if r = LanguagePrimitives.GenericZero<^T> then n else n + (d - r)

let allTiles = List.map (fun x -> [Tile x; Tile x; Tile x; Tile x]) [1..9] |> List.concat

let TileArrayToHand tileArray: ArrayHand =
  Array.map (fun x -> Array.filter (fun (Tile y) -> x = y) tileArray |> Array.length) [|0..9|]
