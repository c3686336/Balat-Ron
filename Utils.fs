module Utils

open Types

let inline RoundUpTo (n: ^T) (d: ^T) =
  let r = n % d
  if r = LanguagePrimitives.GenericZero<^T> then n else n + (d - r)

let allTiles = List.map (fun x -> [Tile x; Tile x; Tile x; Tile x]) [1..9] |> List.concat
