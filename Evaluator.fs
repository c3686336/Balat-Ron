module Evaluator

open FSharp.Collections

type Tile =
  | Tile of int

  override this.ToString() =
    let (Tile v) = this
    [|"Wat";"🀐";"🀑";"🀒";"🀓";"🀔";"🀕";"🀖";"🀗";"🀘"|][v]

type Kantsu = Tile
type Shuntsu = Tile * Tile * Tile
type Kotsu = Tile
type Toitsu = Tile
type ListHand = (Tile * int) list // Tile's name and count
type ArrayHand = int array

type Hand =
  ArrayHand * Kantsu list

type ParsedHand = Kantsu list * Shuntsu list * Kotsu list * Toitsu

let FilterZero = List.filter (snd >> (<>) 0) 

let TryParseNTile (n: int) (hand: ListHand): (Tile * ListHand) option =
  match hand with
    | (t, c) :: xs when c >= n ->
      (t, c - n) :: xs
        |> FilterZero
        |> fun x -> t, x
        |> Some
    | _ -> None

let TryParseToitsu = TryParseNTile 2
let TryParseKotsu = TryParseNTile 3

let TryParseShuntsu (hand: ListHand): (Shuntsu * ListHand) option =
  match hand with
    | (Tile a, a1) :: (Tile b, a2) :: (Tile c, a3) :: xs when a1 > 0 && a2 > 0 && a3 > 0 && b = a + 1 && c = a + 2 ->
      (Tile a, a1 - 1) :: (Tile b, a2 - 1) :: (Tile c, a3 - 1) :: xs
        |> FilterZero
        |> fun x -> (Tile a, Tile b, Tile c), x
        |> Some
    | _ -> None

let rec TryParseHeadlessHandAsMuch (hand: ListHand) (parsedHand: ParsedHand): ParsedHand list =
  match hand with
    | [] -> [parsedHand]
    | hand ->
      // Order of Shuntsu and Kotsu shoudn't matter
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

      shuntsuParsingResult @ kotsuParsingResult |> List.distinct

let ArraytoList (hand: ArrayHand): ListHand =
  List.fold (fun acc elem -> if hand[elem] <> 0 then (Tile elem, hand[elem])::acc else acc) [] [1..9] |> List.rev

let TryParseHeadlessHand (hand: ArrayHand) (toitsu: Toitsu) (kantsu: Kantsu list): ParsedHand list =
  let result = TryParseHeadlessHandAsMuch (ArraytoList hand) (kantsu, [], [], toitsu)
  List.filter (fun (kan: Kantsu list, shun: Shuntsu list, ko: Kotsu list, toi: Toitsu) -> kan.Length + shun.Length + ko.Length = 4) result

let TryParse ((hand, kantsu): Hand): ParsedHand list =
  List.fold (fun acc elem ->
             if hand[elem] >= 2 then
               (TryParseHeadlessHand (Array.updateAt elem (hand[elem] - 2) hand) (Tile elem) kantsu) :: acc
             else
               acc) [] [1..9]
    |> List.filter ((<>) []) |> List.concat
