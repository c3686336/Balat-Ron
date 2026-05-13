module Evaluator

open FSharp.Collections
open Types
open Yaku
open Fu
open Utils
open System

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

let tryRemoveOne v (lst: (Tile * int) list) =
  lst |> List.map (fun (Tile t, cnt) -> if t = v then (Tile t, cnt - 1) else (Tile t, cnt)) |> filterZero

let tryParseWrapShuntsuFromList (hand: (Tile * int) list): (Shuntsu * (Tile * int) list) option =
  let has v = hand |> List.exists (fun (Tile t, cnt) -> t = v && cnt > 0)
  printfn "asdf"
  if has 1 && has 8 && has 9 then
    let removed = hand |> tryRemoveOne 1 |> tryRemoveOne 8 |> tryRemoveOne 9
    printfn "asdf2"
    Some (Shuntsu (Tile 8, Tile 9, Tile 1), removed)
  elif has 1 && has 2 && has 9 then
    let removed = hand |> tryRemoveOne 1 |> tryRemoveOne 2 |> tryRemoveOne 9
    Some (Shuntsu (Tile 9, Tile 1, Tile 2), removed)
  else
    None

let tryParseShuntsu wrapAround (hand: ListHand): (Shuntsu * ListHand) option =
  match hand with
    | ListHand ((Tile a, a1) :: (Tile b, a2) :: (Tile c, a3) :: xs) when a1 > 0 && a2 > 0 && a3 > 0 && b = a + 1 && c = a + 2 ->
      (Tile a, a1 - 1) :: (Tile b, a2 - 1) :: (Tile c, a3 - 1) :: xs
        |> filterZero
        |> fun x -> Shuntsu (Tile a, Tile b, Tile c), ListHand x
        |> Some
    | ListHand ((Tile 1, cnt1) :: _) when wrapAround && cnt1 > 0 ->
      let (ListHand inner) = hand
      tryParseWrapShuntsuFromList inner |> Option.map (fun (s, lst) -> s, ListHand lst)
    | _ -> None

let rec tryParseHeadlessHandAsMuch wrapAround (hand: ListHand) (parsedHand: ParsedNormalHand): ParsedNormalHand list =
  match hand with
    | ListHand [] -> [parsedHand]
    | hand ->
      let (ParsedHand (parsedKantsu, parsedShuntsu, parsedKotsu, parsedToitsu)) = parsedHand
      printfn "%A" hand
      let shuntsu = tryParseShuntsu wrapAround hand
      printfn "%A" shuntsu
      let kotsu = tryParseKotsu hand
      
      let shuntsuParsingResult =
        shuntsu
        |> Option.map (fun (newShuntsu, hand) -> tryParseHeadlessHandAsMuch wrapAround hand (ParsedHand (parsedKantsu, newShuntsu::parsedShuntsu, parsedKotsu, parsedToitsu)))
        |> Option.defaultValue []
      let kotsuParsingResult =
        kotsu
        |> Option.map (fun (newKotsu: Kotsu, hand) -> tryParseHeadlessHandAsMuch wrapAround hand (ParsedHand (parsedKantsu, parsedShuntsu, newKotsu::parsedKotsu, parsedToitsu)))
        |> Option.defaultValue []

      shuntsuParsingResult @ kotsuParsingResult |> List.distinct

let arraytoList (hand: ArrayHand): ListHand =
  List.fold (fun acc elem -> if hand[elem] <> 0 then (Tile elem, hand[elem])::acc else acc) [] [1..9] |> List.rev |> ListHand

let tryParseHeadlessHand wrapAround (hand: ArrayHand) (toitsu: Toitsu) (kantsu: Kantsu list): ParsedNormalHand list =
  let result = tryParseHeadlessHandAsMuch wrapAround (arraytoList hand) (ParsedHand (kantsu, [], [], toitsu))
  List.filter (fun (ParsedHand (kan: Kantsu list, shun: Shuntsu list, ko: Kotsu list, toi: Toitsu)) -> kan.Length + shun.Length + ko.Length = 4) result

let tryParseNormalHand wrapAround ((Hand (hand, tsumo, kantsu)): Hand): ParsedNormalHand list =
  let handWithTsumo = Array.updateAt (tsumo.Value ()) (hand[tsumo.Value ()] + 1) hand
  List.fold (fun acc elem ->
             if handWithTsumo[elem] >= 2 then
               (tryParseHeadlessHand wrapAround (Array.updateAt elem (handWithTsumo[elem] - 2) handWithTsumo) (Toitsu (Tile elem)) kantsu) :: acc
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

let parseHand wrapAround ((Hand (hand, tsumo, kantsu)): Hand): ParsedHand list =
  let normalParses = tryParseNormalHand wrapAround (Hand (hand, tsumo, kantsu)) |> List.map NormalHand
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

let everyParsing wrapAround hand =
  let (Hand (_, tsumo, _)) = hand

  parseHand wrapAround hand
    |> List.map (fun x ->
                 parseMachi x tsumo
                   |> List.map (fun y -> (x, y, tsumo)))
    |> List.concat

let score han (fuVal: int) =
  roundUpTo (6I * bigint fuVal * (pown 2I (han + 2))) 100I

// let calculateCanonicalParsing (state: GameState) =
//   let nDora = calculateDora state.hand (Array.toList state.dora)
//   let (Hand (_, tsumo, _)) = state.hand
  
//   let result =
//     parseHand state.hand
//     |> List.map (fun x ->
//                  parseMachi x tsumo
//                    |> List.map (fun y -> (x, y)))
//     |> List.concat
//     |> List.map (fun (parsedHand, machi) ->
//                  let fuVal = fu parsedHand machi tsumo + snd state.baseScore
//                  let totalYakuHan =
//                      state.items
//                      |> List.sumBy (fun item ->
//                          item.effect state item (OnYakuCalc (parsedHand, machi, tsumo))
//                          |> List.sumBy (function | ItemEffect.Yaku h -> int h | _ -> 0)
//                      )
//                  let finalHan = nDora + totalYakuHan + fst state.baseScore

//                  (score finalHan fuVal, parsedHand, machi))

//   match result with
//     | [] -> None
//     | x -> Some(List.maxBy (fun (score, _, _) -> score) x)

// let calculateScoreFromCanonical (state: GameState) =
//   match calculateCanonicalParsing state with
//   | None -> None
//   | Some (_, parsedHand, machi) ->
//     let nDora = calculateDora state.hand (Array.toList state.dora)
//     let (Hand (_, tsumo, _)) = state.hand
//     let fuVal = fu parsedHand machi tsumo + snd state.baseScore
//     let activeYakus =
//         state.items
//         |> List.choose (fun item ->
//             let effs = item.effect state item (OnScoreCalc (parsedHand, machi, tsumo))
//             let han = effs |> List.sumBy (function | ItemEffect.Yaku h -> int h | _ -> 0)
//             let printed = effs |> List.exists (function | PrintName -> true | _ -> false)
//             if printed || han > 0 then Some (int han, item.name) else None
//         )
//     let totalHan = activeYakus |> List.sumBy fst
//     let names = activeYakus |> List.map snd
//     let finalHan = nDora + totalHan + fst state.baseScore

//     Some (finalHan, fuVal, score finalHan fuVal, $"Dora {nDora}" :: names)
