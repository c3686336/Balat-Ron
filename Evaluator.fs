module Evaluator

open FSharp.Collections
open Types
open Yaku
open Fu
open Utils

let filterZero = List.filter (snd >> (<>) 0) 

let tryParseNTile (n: int) (hand: ListHand): (Tile * ListHand) option =
  match hand with
    | ListHand ((t, c) :: xs) when c >= n ->
      (t, c - n) :: xs
        |> filterZero
        |> fun x -> t, ListHand x
        |> Some
    | _ -> None

let tryParseToitsu hand: (Toitsu * ListHand) option = tryParseNTile 2 hand |> Option.map (fun (a, b) -> (Toitsu a, b))

let tryParseKotsu hand : (Kotsu * ListHand) option = tryParseNTile 3 hand |> Option.map (fun (a, b) -> (Kotsu a, b))

let tryParseShuntsu (hand: ListHand): (Shuntsu * ListHand) option =
  match hand with
    | ListHand ((Tile a, a1) :: (Tile b, a2) :: (Tile c, a3) :: xs) when a1 > 0 && a2 > 0 && a3 > 0 && b = a + 1 && c = a + 2 ->
      (Tile a, a1 - 1) :: (Tile b, a2 - 1) :: (Tile c, a3 - 1) :: xs
        |> filterZero
        |> fun x -> Shuntsu (Tile a, Tile b, Tile c), ListHand x
        |> Some
    | _ -> None

let rec tryParseHeadlessHandAsMuch (hand: ListHand) (parsedHand: ParsedNormalHand): ParsedNormalHand list =
  match hand with
    | ListHand [] -> [parsedHand]
    | hand ->
      // Order of Shuntsu and Kotsu shoudn't matter
      let (ParsedHand (parsedKantsu, parsedShuntsu, parsedKotsu, parsedToitsu)) = parsedHand
      let shuntsu = tryParseShuntsu hand
      let kotsu = tryParseKotsu hand
      
      let shuntsuParsingResult =
        shuntsu
        |> Option.map (fun (newShuntsu, hand) -> tryParseHeadlessHandAsMuch hand (ParsedHand (parsedKantsu, newShuntsu::parsedShuntsu, parsedKotsu, parsedToitsu)))
        |> Option.defaultValue []
      let kotsuParsingResult =
        kotsu
        |> Option.map (fun (newKotsu: Kotsu, hand) -> tryParseHeadlessHandAsMuch hand (ParsedHand (parsedKantsu, parsedShuntsu, newKotsu::parsedKotsu, parsedToitsu)))
        |> Option.defaultValue []

      shuntsuParsingResult @ kotsuParsingResult |> List.distinct

let arraytoList (hand: ArrayHand): ListHand =
  List.fold (fun acc elem -> if hand[elem] <> 0 then (Tile elem, hand[elem])::acc else acc) [] [1..9] |> List.rev |> ListHand

let tryParseHeadlessHand (hand: ArrayHand) (toitsu: Toitsu) (kantsu: Kantsu list): ParsedNormalHand list =
  let result = tryParseHeadlessHandAsMuch (arraytoList hand) (ParsedHand (kantsu, [], [], toitsu))
  List.filter (fun (ParsedHand (kan: Kantsu list, shun: Shuntsu list, ko: Kotsu list, toi: Toitsu)) -> kan.Length + shun.Length + ko.Length = 4) result

let tryParseNormalHand ((Hand (hand, tsumo, kantsu)): Hand): ParsedNormalHand list =
  let handWithTsumo = Array.updateAt (tsumo.Value ()) (hand[tsumo.Value ()] + 1) hand
  List.fold (fun acc elem ->
             if handWithTsumo[elem] >= 2 then
               (tryParseHeadlessHand (Array.updateAt elem (handWithTsumo[elem] - 2) handWithTsumo) (Toitsu (Tile elem)) kantsu) :: acc
             else
               acc) [] [1..9]
    |> List.filter ((<>) []) |> List.concat

let tryParseChitoitsu (hand: ArrayHand): ParsedChitoitsu option =
  if Array.forall (fun x -> x = 0 || x = 2) hand then
    Array.mapi (fun i x -> if x = 0 then Tile 0 else Tile i) hand |> Array.filter ((<>) (Tile 0)) |> Array.toList |> ParsedChitoitsu |> Some
  else
    None

let parseMachiNormalHand (ParsedHand (kan, shun, ko, toi)) (tsumo: Tile) =
  let ryoumens =
    shun
    |> List.choose (fun (Shuntsu (a, b, c)) ->
                 match a, b, c with
                   | x, y, z when x = tsumo && z <> Tile 9 ->
                     Some(Ryoumenmachi (x, y))
                   | x, y, z when z = tsumo && x <> Tile 1 ->
                     Some(Ryoumenmachi (y, z))
                   | _ -> None)
  let kanchans = 
    shun
    |> List.choose (fun (Shuntsu (a, b, c)) ->
                 match a, b, c with
                   | x, y, z when y = tsumo ->
                     Some(Kanchanmachi (x, z))
                   | _ -> None)
  let penchans = 
    shun
    |> List.choose (fun (Shuntsu (a, b, c)) ->
                 match a, b, c with
                   | x, y, z when x = tsumo && z = Tile 9 ->
                     Some(Penchanmachi (x, y))
                   | x, y, z when z = tsumo && x = Tile 1 ->
                     Some(Penchanmachi (y, z))
                   | _ -> None)
  let shunpons =
    ko
    |> List.choose (fun (Kotsu (a)) ->
                    match a with
                      | x when x = tsumo ->
                        let (Toitsu toiTile) = toi
                        Some(Shanponmachi (toiTile, a))
                      | _ -> None)
  let tankis =
    match toi with
      | (Toitsu a) when a = tsumo -> [Tanki a]
      | _ -> []

  ryoumens @ kanchans @ penchans @ shunpons @ tankis
  
let parseMachi hand tsumo =
  match hand with
    | Chitoitsu x -> [Tanki tsumo]
    | NormalHand hand -> parseMachiNormalHand hand tsumo

let parseHand ((Hand (hand, tsumo, kantsu)): Hand): ParsedHand list =
  let normalParses = tryParseNormalHand (Hand (hand, tsumo, kantsu)) |> List.map NormalHand
  let handWithTsumo = Array.updateAt (tsumo.Value ()) (hand[tsumo.Value ()] + 1) hand
  match tryParseChitoitsu handWithTsumo with
    | None ->
      normalParses
    | Some(x) ->
        // Chitoitsu specifically requires 7 pairs (14 tiles). An empty hand or a hand with 4 pairs is not valid.
        let (ParsedChitoitsu tiles) = x
        if tiles.Length = 7 then
            Chitoitsu x :: normalParses
        else
            normalParses

let calculateDora ((Hand (arrayHand, tsumo, kantsu)): Hand) doraIndicators =
  let doraInPlayableHand = List.map (fun (doraIndicator: Tile) -> arrayHand[doraIndicator.DoraTile().Value()]) doraIndicators |> List.sum
  let doraInKantsu =
    List.map (fun (doraIndicator: Tile) -> List.filter (fun (Kantsu (Tile x)) -> (=) x (doraIndicator.DoraTile().Value())) kantsu |> List.length) doraIndicators
      |> List.sumBy (fun x -> x * 4)
  let doraInTsumo = List.filter (fun (x: Tile) -> (x.DoraTile ()) = tsumo) doraIndicators |> List.length

  doraInPlayableHand + doraInKantsu + doraInTsumo

let score han (fuVal: int) =
  roundUpTo (6I * bigint fuVal * (pown 2I (han + 2))) 100I
        
let calculateScore hand doraIndicators additionalYaku additionalYakuNames =
  let nDora = calculateDora hand doraIndicators
  let (Hand (_, tsumo, _)) = hand
  
  let result =
    parseHand hand
    |> List.map (fun x ->
                 parseMachi x tsumo
                   |> List.map (fun y -> (x, y)))
    |> List.concat
    |> List.map (fun (hand, machi) ->
                 let fuVal = fu hand machi tsumo
                 let (han, names) =
                   (List.map (fun (yakup, han, name) ->
                              if yakup hand machi tsumo then (han, name) else (0, name)) yakuList
                      |> List.fold (fun state (han, name) -> (fst state + han, if han = 0 then snd state else name :: snd state)) (nDora + additionalYaku, $"Dora {nDora}" :: additionalYakuNames)) 
                 (han, fuVal, score han fuVal, names))

  match result with
    | [] -> None
    | x -> Some(List.maxBy (fun (_, _, score, _) -> score) x)
