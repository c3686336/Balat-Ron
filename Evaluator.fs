module Evaluator

open FSharp.Collections
open Types;

let FilterZero = List.filter (snd >> (<>) 0) 

let TryParseNTile (n: int) (hand: ListHand): (Tile * ListHand) option =
  match hand with
    | ListHand ((t, c) :: xs) when c >= n ->
      (t, c - n) :: xs
        |> FilterZero
        |> fun x -> t, ListHand x
        |> Some
    | _ -> None

let TryParseToitsu hand: (Toitsu * ListHand) option = TryParseNTile 2 hand |> Option.map (fun (a, b) -> (Toitsu a, b))

let TryParseKotsu hand : (Kotsu * ListHand) option = TryParseNTile 3 hand |> Option.map (fun (a, b) -> (Kotsu a, b))

let TryParseShuntsu (hand: ListHand): (Shuntsu * ListHand) option =
  match hand with
    | ListHand ((Tile a, a1) :: (Tile b, a2) :: (Tile c, a3) :: xs) when a1 > 0 && a2 > 0 && a3 > 0 && b = a + 1 && c = a + 2 ->
      (Tile a, a1 - 1) :: (Tile b, a2 - 1) :: (Tile c, a3 - 1) :: xs
        |> FilterZero
        |> fun x -> Shuntsu (Tile a, Tile b, Tile c), ListHand x
        |> Some
    | _ -> None

let rec TryParseHeadlessHandAsMuch (hand: ListHand) (parsedHand: ParsedNormalHand): ParsedNormalHand list =
  match hand with
    | ListHand [] -> [parsedHand]
    | hand ->
      // Order of Shuntsu and Kotsu shoudn't matter
      let (ParsedHand (parsedKantsu, parsedShuntsu, parsedKotsu, parsedToitsu)) = parsedHand
      let shuntsu = TryParseShuntsu hand
      let kotsu = TryParseKotsu hand
      
      let shuntsuParsingResult =
        shuntsu
        |> Option.map (fun (newShuntsu, hand) -> TryParseHeadlessHandAsMuch hand (ParsedHand (parsedKantsu, newShuntsu::parsedShuntsu, parsedKotsu, parsedToitsu)))
        |> Option.defaultValue []
      let kotsuParsingResult =
        kotsu
        |> Option.map (fun (newKotsu: Kotsu, hand) -> TryParseHeadlessHandAsMuch hand (ParsedHand (parsedKantsu, parsedShuntsu, newKotsu::parsedKotsu, parsedToitsu)))
        |> Option.defaultValue []

      shuntsuParsingResult @ kotsuParsingResult |> List.distinct

let ArraytoList (hand: ArrayHand): ListHand =
  List.fold (fun acc elem -> if hand[elem] <> 0 then (Tile elem, hand[elem])::acc else acc) [] [1..9] |> List.rev |> ListHand

let TryParseHeadlessHand (hand: ArrayHand) (toitsu: Toitsu) (kantsu: Kantsu list): ParsedNormalHand list =
  let result = TryParseHeadlessHandAsMuch (ArraytoList hand) (ParsedHand (kantsu, [], [], toitsu))
  List.filter (fun (ParsedHand (kan: Kantsu list, shun: Shuntsu list, ko: Kotsu list, toi: Toitsu)) -> kan.Length + shun.Length + ko.Length = 4) result

let TryParseNormalHand ((hand, kantsu): Hand): ParsedNormalHand list =
  List.fold (fun acc elem ->
             if hand[elem] >= 2 then
               (TryParseHeadlessHand (Array.updateAt elem (hand[elem] - 2) hand) (Toitsu (Tile elem)) kantsu) :: acc
             else
               acc) [] [1..9]
    |> List.filter ((<>) []) |> List.concat

let TryParseChitoitsu (hand: ArrayHand): ParsedChitoitsu option =
  if Array.forall (fun x -> x = 0 || x = 2) hand then
    Array.mapi (fun i x -> if x = 0 then Tile 0 else Tile i) hand |> Array.filter ((<>) (Tile 0)) |> Array.toList |> ParsedChitoitsu |> Some
  else
    None

let Parse ((hand, kantsu): Hand): ParsedHand list =
  let normalParses = TryParseNormalHand (hand, kantsu) |> List.map NormalHand
  match TryParseChitoitsu hand with
    | None -> normalParses
    | Some(x) ->
        // Chitoitsu specifically requires 7 pairs (14 tiles). An empty hand or a hand with 4 pairs is not valid.
        let (ParsedChitoitsu tiles) = x
        if tiles.Length = 7 then
            Chitoitsu x :: normalParses
        else
            normalParses
