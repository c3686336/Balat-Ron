module Yaku

open Types
open Fu

let isTerminal (Tile t) = t = 1 || t = 9
let isGreen (Tile t) = t = 2 || t = 3 || t = 4 || t = 6 || t = 8

let sukantsup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
    | NormalHand (ParsedHand (k, _, _, _)) -> List.length k = 4
    | _ -> false

let tanyaopRaw (hand: Hand) =
  let (Hand(arr, tsumo, kantsu)) = hand
  arr.[1] = 0 && arr.[9] = 0 &&
  tsumo.Value() <> 1 && tsumo.Value() <> 9 &&
  kantsu |> List.forall (fun (Kantsu t) -> t.Value() <> 1 && t.Value() <> 9)

let pinfup (parsedHand: ParsedHand) (machi: Machi) (tsumo: Tile) =
   20 = fu parsedHand machi tsumo

let iipeikoup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (_, shuntsu, _, _)) ->
    let groups = shuntsu |> List.countBy id
    let pairs = groups |> List.sumBy (fun (_, count) -> count / 2)
    pairs = 1
  | _ -> false

let ryanpeikoup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (_, shuntsu, _, _)) ->
    let groups = shuntsu |> List.countBy id
    let pairs = groups |> List.sumBy (fun (_, count) -> count / 2)
    pairs >= 2
  | _ -> false

let ittsup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (_, shuntsu, _, _)) ->
    let has123 = shuntsu |> List.contains (Shuntsu (Tile 1, Tile 2, Tile 3))
    let has456 = shuntsu |> List.contains (Shuntsu (Tile 4, Tile 5, Tile 6))
    let has789 = shuntsu |> List.contains (Shuntsu (Tile 7, Tile 8, Tile 9))
    has123 && has456 && has789
  | _ -> false

let toitoihoup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, kotsu, _)) ->
    kantsu.Length + kotsu.Length = 4 && kotsu.Length <> 4 // Shouldn't overlap with Sukantsu
  | _ -> false

let sanankoup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, kotsu, _)) ->
    kantsu.Length + kotsu.Length = 3
  | _ -> false

let suuankoup (parsedHand: ParsedHand) (_: Machi) (tsumo: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, kotsu, Toitsu tToi)) ->
    kantsu.Length + kotsu.Length = 4 && tToi <> tsumo
  | _ -> false

let suuankouTankip (parsedHand: ParsedHand) (_: Machi) (tsumo: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, kotsu, Toitsu tToi)) ->
    kantsu.Length + kotsu.Length = 4 && tToi = tsumo
  | _ -> false

let sankantsup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, _, _)) -> kantsu.Length = 3
  | _ -> false

let chitoitsup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | Chitoitsu _ -> true
  | _ -> false

let chinroutoupRaw (hand: Hand) =
  let (Hand(arr, tsumo, kantsu)) = hand
  (seq {2..8} |> Seq.forall (fun i -> arr.[i] = 0)) &&
  (tsumo.Value() = 1 || tsumo.Value() = 9) &&
  kantsu |> List.forall (fun (Kantsu t) -> t.Value() = 1 || t.Value() = 9)

let junchanp (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, shuntsu, kotsu, Toitsu tToi)) ->
    let hasTerminalInShuntsu (Shuntsu (Tile a, _, Tile c)) = a = 1 || c = 9
    shuntsu.Length > 0 &&
    (kantsu |> List.forall (fun (Kantsu t) -> isTerminal t)) &&
    (kotsu |> List.forall (fun (Kotsu t) -> isTerminal t)) &&
    (shuntsu |> List.forall hasTerminalInShuntsu) &&
    (isTerminal tToi)
  | _ -> false

let ryuuiisoupRaw (hand: Hand) =
  let (Hand(arr, tsumo, kantsu)) = hand
  let isGreen v = v = 2 || v = 3 || v = 4 || v = 6 || v = 8
  (seq {1..9} |> Seq.forall (fun i -> isGreen i || arr.[i] = 0)) &&
  isGreen (tsumo.Value()) &&
  kantsu |> List.forall (fun (Kantsu t) -> isGreen (t.Value()))

let junseiChuurenPoutoup (parsedHand: ParsedHand) (_: Machi) (tsumo: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand ([], shuntsu, kotsu, Toitsu (Tile tToi))) ->
    let counts = Array.zeroCreate 10
    kotsu |> List.iter (fun (Kotsu (Tile t)) -> counts[t] <- counts[t] + 3)
    shuntsu |> List.iter (fun (Shuntsu (Tile a, Tile b, Tile c)) -> counts[a] <- counts[a] + 1; counts[b] <- counts[b] + 1; counts[c] <- counts[c] + 1)
    counts[tToi] <- counts[tToi] + 2
    
    let (Tile t) = tsumo
    counts[t] <- counts[t] - 1
    
    counts[1] = 3 && counts[9] = 3 &&
    (seq { 2 .. 8 } |> Seq.forall (fun i -> counts[i] = 1))
  | _ -> false

let chuurenPoutoup (parsedHand: ParsedHand) (machi: Machi) (tsumo: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand ([], shuntsu, kotsu, Toitsu (Tile tToi))) ->
    let counts = Array.zeroCreate 10
    kotsu |> List.iter (fun (Kotsu (Tile t)) -> counts[t] <- counts[t] + 3)
    shuntsu |> List.iter (fun (Shuntsu (Tile a, Tile b, Tile c)) -> counts[a] <- counts[a] + 1; counts[b] <- counts[b] + 1; counts[c] <- counts[c] + 1)
    counts[tToi] <- counts[tToi] + 2
    
    let isBaseChuuren = 
        counts[1] >= 3 && counts[9] >= 3 &&
        (seq { 2 .. 8 } |> Seq.forall (fun i -> counts[i] >= 1))
        
    isBaseChuuren && not (junseiChuurenPoutoup parsedHand machi tsumo)
  | _ -> false

let chantap (parsedHand: ParsedHand) (machi: Machi) (tsumo: Tile) =
  junchanp parsedHand machi tsumo

let yakuItems : Item list = [
  ("Tanyao", "All Simples", [YakuTrigger], ByRawHand tanyaopRaw, fun () -> [ItemEffect.Yaku 1u])
  ("Pinfu", "Pinfu", [YakuTrigger], ByParsedHand pinfup, fun () -> [ItemEffect.Yaku 1u])
  ("Iipeikou", "Pure Double Sequence", [YakuTrigger], ByParsedHand iipeikoup, fun () -> [ItemEffect.Yaku 1u])
  ("Ryanpeikou", "Twice Pure Double Sequence", [YakuTrigger], ByParsedHand ryanpeikoup, fun () -> [ItemEffect.Yaku 3u])
  ("Ittsu", "Pure Straight", [YakuTrigger], ByParsedHand ittsup, fun () -> [ItemEffect.Yaku 2u])
  ("Toitoihou", "All Pongs", [YakuTrigger], ByParsedHand toitoihoup, fun () -> [ItemEffect.Yaku 2u])
  ("Sanankou", "Three Concealed Pongs", [YakuTrigger], ByParsedHand sanankoup, fun () -> [ItemEffect.Yaku 2u])
  ("Sankantsu", "Three Quads", [YakuTrigger], ByParsedHand sankantsup, fun () -> [ItemEffect.Yaku 2u])
  ("Chitoitsu", "Seven Pairs", [YakuTrigger], ByParsedHand chitoitsup, fun () -> [ItemEffect.Yaku 2u])
  ("Chanta", "Half Outside Hand", [YakuTrigger], ByParsedHand chantap, fun () -> [ItemEffect.Yaku 2u])
  ("Junchan", "Fully Outside Hand", [YakuTrigger], ByParsedHand junchanp, fun () -> [ItemEffect.Yaku 3u])
  ("Chinitsu", "Full Flush", [YakuTrigger], Always, fun () -> [ItemEffect.Yaku 6u])
  ("Suuankou", "Four Concealed Pongs", [YakuTrigger], ByParsedHand suuankoup, fun () -> [ItemEffect.Yaku 13u])
  ("Sukantsu", "Four Quads", [YakuTrigger], ByParsedHand sukantsup, fun () -> [ItemEffect.Yaku 13u])
  ("Chinroutou", "All Terminals", [YakuTrigger], ByRawHand chinroutoupRaw, fun () -> [ItemEffect.Yaku 13u])
  ("Ryuuiisou", "All Green", [YakuTrigger], ByRawHand ryuuiisoupRaw, fun () -> [ItemEffect.Yaku 13u])
  ("ChuurenPoutou", "Nine Gates", [YakuTrigger], ByParsedHand chuurenPoutoup, fun () -> [ItemEffect.Yaku 13u])
  ("SuuankouTanki", "Four Concealed Pongs Single Wait", [YakuTrigger], ByParsedHand suuankouTankip, fun () -> [ItemEffect.Yaku 26u])
  ("JunseiChuurenPoutou", "Pure Nine Gates", [YakuTrigger], ByParsedHand junseiChuurenPoutoup, fun () -> [ItemEffect.Yaku 26u])
]
