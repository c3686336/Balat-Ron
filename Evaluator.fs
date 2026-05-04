module Evaluator

open FSharp.Collections

type Tile = int
type Kantsu = Tile
type Shuntsu = Tile * Tile * Tile
type Kotsu = Tile
type Toitsu = Tile
type ListHand = (int * int) list // Tile's name and count
type ArrayHand = int array
type Hand = ArrayHand * Kantsu list
type ParsedHand = Kantsu list * Shuntsu list * Kotsu list * Toitsu

let TrimHead = List.skipWhile (snd >> (=) 0) 

let TryParseNTile n hand =
  match hand with
    | (t, c) :: xs when c >= n ->
      (t, c - n) :: xs
        |> TrimHead
        |> fun x -> t, x
        |> Some
    | _ -> None

let TryParseToitsu = TryParseNTile 2
let TryParseKotsu = TryParseNTile 3

let TryParseShuntsu hand =
  match hand with
    | (a, a1) :: (b, a2) :: (c, a3) :: xs when a1 > 0 && a2 > 0 && a3 > 0 && b = a + 1 && c = a + 2 ->
      (a, a1 - 1) :: (b, a2 - 1) :: (c, a3 - 1) :: xs
        |> TrimHead
        |> fun x -> (a, b, c), x
        |> Some
    | _ -> None

let rec TryParseHeadlessHandAsMuch (hand: ListHand) (parsedHand: ParsedHand) =
  match hand with
    | [] -> [parsedHand]
    | hand ->
      let (parsedKantsu, parsedShuntsu, parsedKotsu, parsedToitsu) = parsedHand
      let shuntsu = TryParseShuntsu hand
      let kotsu = TryParseKotsu hand
      
      let shuntsuParsingResult =
        shuntsu
        |> Option.map (fun (newShuntsu, hand) -> TryParseHeadlessHandAsMuch hand (parsedKantsu, newShuntsu::parsedShuntsu, parsedKotsu, parsedToitsu))
        |> Option.defaultValue []
      let kotsuParsingResult =
        kotsu
        |> Option.map (fun (newKotsu, hand) -> TryParseHeadlessHandAsMuch hand (parsedKantsu, parsedShuntsu, newKotsu::parsedKotsu, parsedToitsu))
        |> Option.defaultValue []

      shuntsuParsingResult @ kotsuParsingResult

let ArraytoList (hand: ArrayHand) =
  List.fold (fun acc elem -> if hand[elem] <> 0 then (elem, hand[elem])::acc else acc) [] [1..9] |> List.rev

let TryParseHeadlessHand (hand: ArrayHand) (toitsu) (kantsu: Kantsu list) =
  let result = TryParseHeadlessHandAsMuch (ArraytoList hand) (kantsu, [], [], toitsu)
  List.filter (fun (kan: Kantsu list, shun: Shuntsu list, ko: Kotsu list, toi: Toitsu) -> kan.Length + shun.Length + ko.Length = 4) result

let TryParse ((hand, kantsu): Hand) =
  List.fold (fun acc elem ->
             if hand[elem] >= 2 then
               (TryParseHeadlessHand (Array.updateAt elem (hand[elem] - 2) hand) elem kantsu)::acc
             else
               acc) [] [1..9]
    |> List.filter ((<>) [])
