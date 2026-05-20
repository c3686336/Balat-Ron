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
    | Mythical -> 5.0

let chooseShopItems (rng: Random) count (items: Item list) =
    let rec gather acc pool needed =
        if needed = 0 || List.isEmpty pool then acc
        else
            let groups = pool |> List.groupBy (fun i -> i.rarity)
            let totalWeight = groups |> List.sumBy (fun (r, _) -> rarityWeight r)
            
            if totalWeight <= 0.0 then acc
            else
                let target = rng.NextDouble() * totalWeight
                let mutable current = 0.0
                let mutable chosenRarity = fst groups.Head
                
                for (r, _) in groups do
                    let w = rarityWeight r
                    if target >= current && target < current + w then
                        chosenRarity <- r
                    current <- current + w
                    
                let available = groups |> List.find (fun (r, _) -> r = chosenRarity) |> snd
                let picked = { available.[rng.Next(available.Length)] with id = Guid.NewGuid() }
                
                // Remove the picked item from the local pool so it doesn't appear twice in the exact same shop
                let newPool = pool |> List.filter (fun i -> i.name <> picked.name)
                
                gather (picked :: acc) newPool (needed - 1)
                
    gather [] items count
