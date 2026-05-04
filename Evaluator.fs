module Evaluator

open FSharp.Collections

type Tile = int
type Kantsu = Tile
type Shuntsu = Tile * Tile * Tile
type Kotsu = Tile
type Toitsu = Tile
type Hand = int array
type ParsedHand = Kantsu list * Shuntsu list * Kotsu list * Toitsu

let (>=>) f g = fun x -> f x |> Option.bind g

let internal IsValidTile (i: Tile): bool =
  0 < i && i < 10

let internal TryTakeNTile (i: Tile) (hand: Hand) (n: int): Hand option =
  if IsValidTile i then
    match hand[i] with
      | n when n >= n -> Array.updateAt i (hand[i] - n) hand |> Some
      | _ -> None
  else
    None

let internal TryTakeTile (i: Tile) (hand: Hand): Hand option =
  TryTakeNTile i hand 1

let internal TryParseShuntsuAt (i: int) (hand: Hand): (Shuntsu * Hand) option =
  // TryTakeTile i hand
  //   |> Option.bind (fun hand -> TryTakeTile (i + 1) hand)
  //   |> Option.bind (fun hand -> TryTakeTile (i + 2) hand)
  //   |> Option.bind (fun hand -> Some(Shuntsu (i, i + 1, i + 2), hand))
  let TryTakeShuntsu =
    TryTakeTile i
    >=> TryTakeTile (i + 1)
    >=> TryTakeTile (i + 2)

  hand
    |> TryTakeShuntsu
    |> Option.map (fun hand -> (Shuntsu(i, i+1, i+2), hand))

let internal TryParseKotsuAt (i: int) (hand: Hand): (Kotsu * Hand) option =
  TryTakeNTile i hand 3
    |> Option.map (fun hand -> (i, hand))

let internal TryParseToitsuAt (i: int) (hand: Hand): (Toitsu * Hand) option =
  TryTakeNTile i hand 2
    |> Option.map (fun hand -> (i, hand))

// let rec ParsePartialHandFrom (partialParsed: ParsedHand) (hand: Hand) (i: int): ParsedHand list =
//   match i with
//   | i when 1 <= i && i <= 9 && hand[i] > 0 ->
//     let tryShuntsu = TryParseShuntsuAt i hand
//     let tryKotsu = TryParseKotsuAt i hand
//     let shuntsuResult =
//       match tryShuntsu with
//         | Some(shuntsu, hand) ->
//           let (parsedKantsu, parsedShuntsu, parsedKotsu, parsedToitsu) = partialParsed
//           ParsePartialHandFrom (parsedKantsu, shuntsu::parsedShuntsu, parsedKotsu, parsedToitsu) hand i
//         | None -> []
//     let kotsuResult =
//           match tryKotsu with
//             | Some(kotsu, hand) ->
//               let (parsedKantsu, parsedShuntsu, parsedKotsu, parsedToitsu) = partialParsed
//               ParsePartialHandFrom (parsedKantsu, parsedShuntsu, kotsu::parsedKotsu, parsedToitsu) hand i
//             | None -> []
//     match shuntsuResult @ kotsuResult with
//       | [] -> None
//       | x -> x |> Some
//   | i when 1 <= i && 1 <= 9 && hand[i] = 0 ->
//     ParsePartialHandFrom partialParsed hand (i + 1)
//   | _ -> Some([])

let rec ParsePartialHandFromIndex (partialParsed: ParsedHand) (hand: Hand) (i: int): ParsedHand list option =
  // Parses remaining tiles from the hand and concatenates it to the partialParsed. Assume that any tiles less than i is all zero.
  // If parsing fails, return None
  match hand[i] with
    | n when n > 0 ->
      let (parsedKantsu, parsedShuntsu, parsedKotsu, parsedToitsu) = partialParsed;
      
      let shuntsu = TryParseShuntsuAt i hand
      let kotsu = TryParseKotsuAt i hand

      match shuntsu, kotsu with
        | None, None ->
          // Parsing failed
          None
        | shuntsu, kotsu ->
          match shuntsu with
            | Some(shuntsu, newHand) ->
              let newPartialParsed =
                (parsedKantsu, shuntsu::parsedShuntsu, parsedKotsu, parsedToitsu)
              let rest = ParsePartialHandFromIndex newPartialParsed newHand i
    | _ ->
      ParsePartialHandFromIndex partialParsed hand (i + 1)
