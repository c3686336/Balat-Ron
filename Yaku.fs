module Yaku

open Types
open Fu

let isTerminal (Tile t) = t = 1 || t = 9
let isGreen (Tile t) = t = 2 || t = 3 || t = 4 || t = 6 || t = 8

let sukantsup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
    | NormalHand (ParsedHand (k, _, _, _)) -> List.length k = 4
    | _ -> false

let tanyaop (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, shuntsu, kotsu, Toitsu tToi)) ->
    let hasTerminalInShuntsu (Shuntsu (Tile a, _, Tile c)) = a = 1 || c = 9
    let noTerminal = 
      not (List.exists (fun (Kantsu t) -> isTerminal t) kantsu) &&
      not (List.exists (fun (Kotsu t) -> isTerminal t) kotsu) &&
      not (List.exists hasTerminalInShuntsu shuntsu) &&
      not (isTerminal tToi)
    noTerminal
  | Chitoitsu (ParsedChitoitsu tiles) ->
    not (List.exists isTerminal tiles)

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

let chinroutoup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, [], kotsu, Toitsu tToi)) ->
    let allTerminal =
      (kantsu |> List.forall (fun (Kantsu t) -> isTerminal t)) &&
      (kotsu |> List.forall (fun (Kotsu t) -> isTerminal t)) &&
      (isTerminal tToi)
    allTerminal
  | Chitoitsu (ParsedChitoitsu tiles) ->
    tiles |> List.forall isTerminal
  | _ -> false

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

let ryuuiisoup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, shuntsu, kotsu, Toitsu tToi)) ->
    (kantsu |> List.forall (fun (Kantsu t) -> isGreen t)) &&
    (kotsu |> List.forall (fun (Kotsu t) -> isGreen t)) &&
    (shuntsu |> List.forall (fun (Shuntsu (a, b, c)) -> isGreen a && isGreen b && isGreen c)) &&
    (isGreen tToi)
  | Chitoitsu (ParsedChitoitsu tiles) ->
    tiles |> List.forall isGreen

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

let chinitsup (parsedHand: ParsedHand) (_: Machi) (_: Tile) =
  true

let chantap (parsedHand: ParsedHand) (machi: Machi) (tsumo: Tile) =
  junchanp parsedHand machi tsumo

let yakuList = [
  (tanyaop, 1, "Tanyao")
  (pinfup, 1, "Pinfu")
  (iipeikoup, 1, "Iipeikou")
  (ryanpeikoup, 3, "Ryanpeikou")
  (ittsup, 2, "Ittsu")
  (toitoihoup, 2, "Toitoihou")
  (sanankoup, 2, "Sanankou")
  (sankantsup, 2, "Sankantsu")
  (chitoitsup, 2, "Chitoitsu")
  (chantap, 2, "Chanta")
  (junchanp, 3, "Junchan")
  (chinitsup, 6, "Chinitsu")
  (suuankoup, 13, "Suuankou")
  (sukantsup, 13, "Sukantsu")
  (chinroutoup, 13, "Chinroutou")
  (ryuuiisoup, 13, "Ryuuiisou")
  (chuurenPoutoup, 13, "ChuurenPoutou")
  (suuankouTankip, 26, "SuuankouTanki")
  (junseiChuurenPoutoup, 26, "JunseiChuurenPoutou")
]
