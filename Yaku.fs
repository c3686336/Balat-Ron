module Yaku

open Types

let isTerminal (Tile t) = t = 1 || t = 9
let isGreen (Tile t) = t = 2 || t = 3 || t = 4 || t = 6 || t = 8

let Sukantsup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
    | NormalHand (ParsedHand (k, _, _, _)) -> List.length k = 4
    | _ -> false

let Tanyaop (parsedHand: ParsedHand) (_: Tile) =
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

let Pinfup (parsedHand: ParsedHand) (tsumo: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand ([], shuntsu, [], _)) when shuntsu.Length = 4 ->
    let (Tile t) = tsumo
    shuntsu |> List.exists (fun (Shuntsu (Tile a, _, Tile c)) ->
      (t = a && a <= 6) || (t = c && c >= 4)
    )
  | _ -> false

let Iipeikoup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (_, shuntsu, _, _)) ->
    let groups = shuntsu |> List.countBy id
    let pairs = groups |> List.sumBy (fun (_, count) -> count / 2)
    pairs = 1
  | _ -> false

let Ryanpeikoup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (_, shuntsu, _, _)) ->
    let groups = shuntsu |> List.countBy id
    let pairs = groups |> List.sumBy (fun (_, count) -> count / 2)
    pairs >= 2
  | _ -> false

let Ittsup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (_, shuntsu, _, _)) ->
    let has123 = shuntsu |> List.contains (Shuntsu (Tile 1, Tile 2, Tile 3))
    let has456 = shuntsu |> List.contains (Shuntsu (Tile 4, Tile 5, Tile 6))
    let has789 = shuntsu |> List.contains (Shuntsu (Tile 7, Tile 8, Tile 9))
    has123 && has456 && has789
  | _ -> false

let Toitoihoup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, kotsu, _)) ->
    kantsu.Length + kotsu.Length = 4
  | _ -> false

let Sanankoup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, kotsu, _)) ->
    kantsu.Length + kotsu.Length = 3
  | _ -> false

let Suuankoup (parsedHand: ParsedHand) (tsumo: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, kotsu, Toitsu tToi)) ->
    kantsu.Length + kotsu.Length = 4 && tToi <> tsumo
  | _ -> false

let SuuankouTankip (parsedHand: ParsedHand) (tsumo: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, kotsu, Toitsu tToi)) ->
    kantsu.Length + kotsu.Length = 4 && tToi = tsumo
  | _ -> false

let Sankantsup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, _, _, _)) -> kantsu.Length = 3
  | _ -> false

let Chitoitsup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | Chitoitsu _ -> true
  | _ -> false

let Chinroutoup (parsedHand: ParsedHand) (_: Tile) =
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

let Junchanp (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, shuntsu, kotsu, Toitsu tToi)) ->
    let hasTerminalInShuntsu (Shuntsu (Tile a, _, Tile c)) = a = 1 || c = 9
    shuntsu.Length > 0 &&
    (kantsu |> List.forall (fun (Kantsu t) -> isTerminal t)) &&
    (kotsu |> List.forall (fun (Kotsu t) -> isTerminal t)) &&
    (shuntsu |> List.forall hasTerminalInShuntsu) &&
    (isTerminal tToi)
  | _ -> false

let Ryuuiisoup (parsedHand: ParsedHand) (_: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand (kantsu, shuntsu, kotsu, Toitsu tToi)) ->
    (kantsu |> List.forall (fun (Kantsu t) -> isGreen t)) &&
    (kotsu |> List.forall (fun (Kotsu t) -> isGreen t)) &&
    (shuntsu |> List.forall (fun (Shuntsu (a, b, c)) -> isGreen a && isGreen b && isGreen c)) &&
    (isGreen tToi)
  | Chitoitsu (ParsedChitoitsu tiles) ->
    tiles |> List.forall isGreen

let JunseiChuurenPoutoup (parsedHand: ParsedHand) (tsumo: Tile) =
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

let ChuurenPoutoup (parsedHand: ParsedHand) (tsumo: Tile) =
  match parsedHand with
  | NormalHand (ParsedHand ([], shuntsu, kotsu, Toitsu (Tile tToi))) ->
    let counts = Array.zeroCreate 10
    kotsu |> List.iter (fun (Kotsu (Tile t)) -> counts[t] <- counts[t] + 3)
    shuntsu |> List.iter (fun (Shuntsu (Tile a, Tile b, Tile c)) -> counts[a] <- counts[a] + 1; counts[b] <- counts[b] + 1; counts[c] <- counts[c] + 1)
    counts[tToi] <- counts[tToi] + 2
    
    let isBaseChuuren = 
        counts[1] >= 3 && counts[9] >= 3 &&
        (seq { 2 .. 8 } |> Seq.forall (fun i -> counts[i] >= 1))
        
    isBaseChuuren && not (JunseiChuurenPoutoup parsedHand tsumo)
  | _ -> false

let Chinitsup (parsedHand: ParsedHand) (_: Tile) =
  true

let Chantap (parsedHand: ParsedHand) (tsumo: Tile) =
  Junchanp parsedHand tsumo

let YakuList = [
  (Tanyaop, 1)
  (Pinfup, 1)
  (Iipeikoup, 1)
  (Ryanpeikoup, 3)
  (Ittsup, 2)
  (Toitoihoup, 2)
  (Sanankoup, 2)
  (Sankantsup, 2)
  (Chitoitsup, 2)
  (Chantap, 2)
  (Junchanp, 3)
  (Chinitsup, 6)
  (Suuankoup, 13)
  (Sukantsup, 13)
  (Chinroutoup, 13)
  (Ryuuiisoup, 13)
  (ChuurenPoutoup, 13)
  (SuuankouTankip, 26)
  (JunseiChuurenPoutoup, 26)
]
