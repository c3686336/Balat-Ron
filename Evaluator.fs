module Evaluator

open FSharp.Collections

type Tile =
  | Tile of int

  override this.ToString() =
    let (Tile v) = this
    [|"Wat";"🀐";"🀑";"🀒";"🀓";"🀔";"🀕";"🀖";"🀗";"🀘"|][v]

  member this.Value() =
    let (Tile v) = this
    v

type Kantsu =
  | Kantsu of Tile

  override this.ToString (): string =
    let (Kantsu t) = this
    $"{t}🀫🀫{t}"

type Shuntsu =
  | Shuntsu of Tile * Tile * Tile

  override this.ToString (): string =
    let (Shuntsu (a, b, c)) = this
    $"{a}{b}{c}"

type Kotsu =
  | Kotsu of Tile 

  override this.ToString (): string =
    let (Kotsu t) = this
    $"{t}{t}{t}"

type Toitsu =
  | Toitsu of Tile

  override this.ToString (): string =
    let (Toitsu t) = this
    $"{t}{t}"

type ListHand =
  | ListHand of (Tile * int) list // Tile's name and count

type ArrayHand = int array

type Hand =
  ArrayHand * Kantsu list

type ParsedHand =
  | ParsedHand of Kantsu list * Shuntsu list * Kotsu list * Toitsu

  override this.ToString (): string =
    let (ParsedHand (kan, shun, ko, toi)) = this
    $"Kantsu: {kan}, Shuntsu: {shun}, Kotsu: {ko}, Toitsu: {toi}"

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

let rec TryParseHeadlessHandAsMuch (hand: ListHand) (parsedHand: ParsedHand): ParsedHand list =
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

let TryParseHeadlessHand (hand: ArrayHand) (toitsu: Toitsu) (kantsu: Kantsu list): ParsedHand list =
  let result = TryParseHeadlessHandAsMuch (ArraytoList hand) (ParsedHand (kantsu, [], [], toitsu))
  List.filter (fun (ParsedHand (kan: Kantsu list, shun: Shuntsu list, ko: Kotsu list, toi: Toitsu)) -> kan.Length + shun.Length + ko.Length = 4) result

let TryParse ((hand, kantsu): Hand): ParsedHand list =
  List.fold (fun acc elem ->
             if hand[elem] >= 2 then
               (TryParseHeadlessHand (Array.updateAt elem (hand[elem] - 2) hand) (Toitsu (Tile elem)) kantsu) :: acc
             else
               acc) [] [1..9]
    |> List.filter ((<>) []) |> List.concat
