module EvaluatorTests

open Xunit
open Evaluator
open Types

let ParseHand (handArray: int array, kantsu: Kantsu list) =
    match handArray |> Array.tryFindIndex (fun x -> x > 0) with
    | Some firstTile ->
        let updatedArray = Array.updateAt firstTile (handArray[firstTile] - 1) handArray
        parseHand false (Hand (updatedArray, Tile firstTile, kantsu))
    | None ->
        parseHand false (Hand (handArray, Tile 1, kantsu))

[<Fact>]
let ``TryParse with standard winning hand`` () =
    let handArray = [|0; 3; 1; 1; 2; 1; 1; 1; 1; 3|]
    let result = ParseHand (handArray, [])
    
    Assert.Single(result) |> ignore
    let (ParsedHand (kantsu, shuntsu, kotsu, toitsu)) = 
        match result.Head with
        | NormalHand n -> n
        | _ -> failwith "Expected NormalHand"
    
    Assert.Empty(kantsu)
    Assert.Equal(3, shuntsu.Length)
    Assert.Equal(1, kotsu.Length)
    Assert.Equal(Toitsu (Tile 9), toitsu)

    // Expected shuntsu are 2-3-4, 4-5-6, 7-8-9
    // Due to the order of parsing (recursive), let's just check they are present
    let expectedShuntsu = [ Shuntsu (Tile 7, Tile 8, Tile 9)
                            Shuntsu (Tile 4, Tile 5, Tile 6)
                            Shuntsu (Tile 2, Tile 3, Tile 4) ]
    
    Assert.Equivalent(expectedShuntsu, shuntsu)
    
    // Expected kotsu is 1-1-1
    Assert.Equal<seq<Kotsu>>([Kotsu (Tile 1)], kotsu)


[<Fact>]
let ``TryParse with multiple groups of same tile`` () =
    let handArray = [|0; 4; 1; 1; 1; 1; 1; 1; 1; 3|]
    let result = ParseHand (handArray, [])
    
    Assert.Single(result) |> ignore
    let (ParsedHand (kantsu, shuntsu, kotsu, toitsu)) = 
        match result.Head with
        | NormalHand n -> n
        | _ -> failwith "Expected NormalHand"
    
    Assert.Empty(kantsu)
    Assert.Equal(3, shuntsu.Length)
    Assert.Equal(1, kotsu.Length)
    Assert.Equal(Toitsu (Tile 9), toitsu)

    let expectedShuntsu = [ Shuntsu (Tile 7, Tile 8, Tile 9)
                            Shuntsu (Tile 4, Tile 5, Tile 6)
                            Shuntsu (Tile 1, Tile 2, Tile 3) ]
    Assert.Equivalent(expectedShuntsu, shuntsu)
    Assert.Equal<seq<Kotsu>>([Kotsu (Tile 1)], kotsu)

[<Fact>]
let ``TryParse with unparsable hand returns empty list`` () =
    // 17 tiles, invalid hand
    let handArray = [|0; 3; 1; 4; 2; 1; 1; 1; 1; 3|]
    let result = ParseHand (handArray, [])
    
    Assert.Empty(result)

[<Fact>]
let ``TryParse with 9-kotsu and 1-toitsu`` () =
    let handArray = [|0; 3; 1; 1; 1; 1; 1; 1; 1; 4|]
    let result = ParseHand (handArray, [])
    
    Assert.Single(result) |> ignore
    let (ParsedHand (kantsu, shuntsu, kotsu, toitsu)) = 
        match result.Head with
        | NormalHand n -> n
        | _ -> failwith "Expected NormalHand"
    
    Assert.Empty(kantsu)
    Assert.Equal(3, shuntsu.Length)
    Assert.Equal(1, kotsu.Length)
    Assert.Equal(Toitsu (Tile 1), toitsu)

    let expectedShuntsu = [ Shuntsu (Tile 7, Tile 8, Tile 9)
                            Shuntsu (Tile 4, Tile 5, Tile 6)
                            Shuntsu (Tile 1, Tile 2, Tile 3) ]
    Assert.Equivalent(expectedShuntsu, shuntsu)
    Assert.Equal<seq<Kotsu>>([Kotsu (Tile 9)], kotsu)

[<Fact>]
let ``TryParse with ambiguous hand returning multiple valid parses`` () =
    // Chuuren Poutou structure is often ambiguous, but let's do a simple one:
    // 1-1-1-2-2-2-3-3-3-4-4-4-5-5
    // Can be 123 123 123 444 55
    // Can be 111 222 333 444 55
    let handArray = [|0; 3; 3; 3; 3; 2; 0; 0; 0; 0|]
    let result = ParseHand (handArray, [])
    
    // There are multiple ways to parse this hand
    Assert.True(result.Length > 1)
[<Fact>]
let ``TryParse with one kantsu`` () =
    // Kantsu of 9 (4 tiles removed from main hand)
    // Hand has 10 tiles left, needing 3 sets and 1 pair
    let handArray = [|0; 3; 1; 1; 2; 1; 1; 2; 0; 0|] // 1-1-1, 2-3-4, 4-5-6, 7-7
    let kantsu = [Kantsu (Tile 9)]
    let result = ParseHand (handArray, kantsu)
    
    Assert.Single(result) |> ignore
    let (ParsedHand (k, shuntsu, kotsu, toitsu)) = 
        match result.Head with
        | NormalHand n -> n
        | _ -> failwith "Expected NormalHand"
    
    Assert.Single(k) |> ignore
    Assert.Equal(Kantsu (Tile 9), k.Head)
    Assert.Equal(2, shuntsu.Length)
    Assert.Equal(1, kotsu.Length)
    Assert.Equal(Toitsu (Tile 7), toitsu)
[<Fact>]
let ``TryParse with empty hand returns empty`` () =
    let handArray = Array.zeroCreate 10
    let result = ParseHand (handArray, [])
    Assert.Empty(result)
[<Fact>]
let ``TryParse with Ryanpeikou shape returns 3 valid parses`` () =
    // 1-1, 2-2, 3-3, 4-4, 5-5, 6-6, 7-7
    // Pairs: 11, 44, 77. Shuntsu: 234 234 567 567 etc.
    let handArray = [|0; 2; 2; 2; 2; 2; 2; 2; 0; 0|]
    let result = ParseHand (handArray, [])
    
    // Since this is 7 pairs, it also parses as Chitoitsu now, so there are 4 results total
    Assert.Equal(4, result.Length)

    // Ensure they have different Toitsu
    let toitsuList = result |> List.choose (function NormalHand (ParsedHand (_, _, _, t)) -> Some t | _ -> None)
    Assert.Contains(Toitsu (Tile 1), toitsuList)
    Assert.Contains(Toitsu (Tile 4), toitsuList)
    Assert.Contains(Toitsu (Tile 7), toitsuList)

[<Fact>]
let ``TryParse with 3-2-2-2-2-3 shape returns 2 parses`` () =
    // 111, 22, 33, 44, 55, 666
    let handArray = [|0; 3; 2; 2; 2; 2; 3; 0; 0; 0|]
    let result = ParseHand (handArray, [])
    
    Assert.Equal(2, result.Length)
    
    let toitsuList = result |> List.choose (function NormalHand (ParsedHand (_, _, _, t)) -> Some t | _ -> None)
    Assert.Contains(Toitsu (Tile 2), toitsuList)
    Assert.Contains(Toitsu (Tile 5), toitsuList)

[<Fact>]
let ``TryParse with 3-4-4-3 shape returns 4 parses`` () =
    // 111, 2222, 3333, 444
    let handArray = [|0; 3; 4; 4; 3; 0; 0; 0; 0; 0|]
    let result = ParseHand (handArray, [])
    
    Assert.Equal(4, result.Length)
    
    let toitsuList = result |> List.choose (function NormalHand (ParsedHand (_, _, _, t)) -> Some t | _ -> None) |> List.distinct
    Assert.Equal(2, toitsuList.Length)
    Assert.Contains(Toitsu (Tile 1), toitsuList)
    Assert.Contains(Toitsu (Tile 4), toitsuList)
[<Fact>]
let ``Parse Chitoitsu standard case returns exactly 1 result`` () =
    // Hand: 11, 22, 44, 55, 77, 88, 99
    let handArray = [|0; 2; 2; 0; 2; 2; 0; 2; 2; 2|]
    let result = ParseHand (handArray, [])
    
    Assert.Single(result) |> ignore
    
    match result.Head with
    | Chitoitsu (ParsedChitoitsu tiles) ->
        Assert.Equal(7, tiles.Length)
        let expectedTiles = [Tile 1; Tile 2; Tile 4; Tile 5; Tile 7; Tile 8; Tile 9]
        Assert.Equal<seq<Tile>>(expectedTiles, tiles)
    | _ -> Assert.Fail("Expected Chitoitsu")

[<Fact>]
let ``Parse Chitoitsu rejects hands with 4 identical tiles (if no other pairs make up 7 pairs)`` () =
    // Hand: 11, 22, 33, 4444, 55 (Not a valid Chitoitsu in standard rules, Balat-Ron rule check)
    let handArray = [|0; 2; 2; 2; 4; 2; 0; 0; 0; 0|]
    let result = ParseHand (handArray, [])
    
    let isChitoitsu = result |> List.exists (function Chitoitsu _ -> true | _ -> false)
    Assert.False(isChitoitsu)

[<Fact>]
let ``Parse Chitoitsu with kantsu ignores Chitoitsu`` () =
    // Even if you have 10 tiles that are pairs, if there is a kantsu, you can't have 7 pairs
    let handArray = [|0; 2; 2; 2; 2; 2; 0; 0; 0; 0|] // 10 tiles (5 pairs)
    let kantsu = [Kantsu (Tile 9)] // 4 tiles
    let result = ParseHand (handArray, kantsu)
    
    let isChitoitsu = result |> List.exists (function Chitoitsu _ -> true | _ -> false)
    Assert.False(isChitoitsu)
