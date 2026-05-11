module Utils

open Types
open System

let inline roundUpTo (n: ^T) (d: ^T) =
  let r = n % d
  if r = LanguagePrimitives.GenericZero<^T> then n else n + (d - r)

let allTiles = List.map (fun x -> [Tile x; Tile x; Tile x; Tile x]) [1..9] |> List.concat

let tileArrayToHand tileArray: ArrayHand =
  Array.map (fun x -> Array.filter (fun (Tile y) -> x = y) tileArray |> Array.length) [|0..9|]

let chooseRandom (rng: Random) count list =
    list
    |> List.sortBy (fun _ -> rng.Next())
    |> List.truncate count

let rarityWeight (r: Rarity) =
    match r with
    | Common -> 60.0
    | Uncommon -> 50.0
    | Rare -> 30.0
    | Legendary -> 10.0

let chooseShopItems (rng: Random) count (items: Item list) =
    items
    |> List.sortByDescending (fun i -> Math.Pow(rng.NextDouble(), 1.0 / rarityWeight i.rarity))
    |> List.truncate count
