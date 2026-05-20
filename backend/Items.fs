module Items

open Yaku
open Types
open System

let private countHandTiles predicate (Hand (hand, tsumo, kantsu)) =
  let handCount =
    hand
    |> Array.mapi (fun i count -> if i >= 1 && predicate (Tile i) then count else 0)
    |> Array.sum
  let tsumoCount = if predicate tsumo then 1 else 0
  let kantsuCount = kantsu |> List.sumBy (fun (Kantsu t) -> if predicate t then 4 else 0)
  handCount + tsumoCount + kantsuCount

let private distinctPairCount (Hand (hand, tsumo, _)) =
  let counts = Array.copy hand
  counts[tsumo.Value()] <- counts[tsumo.Value()] + 1
  [1..9] |> List.filter (fun i -> counts[i] >= 2) |> List.length

let private sequenceCount parsedHand =
  match parsedHand with
  | NormalHand (ParsedHand (_, shuntsu, _, _)) -> shuntsu.Length
  | _ -> 0

let private allSequences parsedHand =
  match parsedHand with
  | NormalHand (ParsedHand ([], shuntsu, [], _)) -> shuntsu.Length = 4
  | _ -> false

let private hasWrapAroundSequence parsedHand =
  match parsedHand with
  | NormalHand (ParsedHand (_, shuntsu, _, _)) ->
    shuntsu |> List.exists (function | Shuntsu (Tile 8, Tile 9, Tile 1) | Shuntsu (Tile 9, Tile 1, Tile 2) -> true | _ -> false)
  | _ -> false

let private edgeWait machi =
  match machi with
  | Penchanmachi _ -> true
  | _ -> false

let allItems : Item list = [
  { id = Guid.NewGuid(); name = "Tanyao";
    description = "Grants +1 Yaku (score multiplier) if your hand has no 1 or 9 tiles.";
    rarity = Common;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc _ when tanyaopRaw state.hand -> [ItemEffect.ExtraScore (1, 0)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Pinfu";
    description = "Grants +1 Yaku (score multiplier) and +20 fu if your hand consists of four sequences (e.g. 2-3-4) and a pair, and you were waiting to complete a sequence.";
    rarity = Common;
    cost = 160;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when pinfup p m t -> [ItemEffect.ExtraScore (1, 20)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Iipeikou";
    description = "Grants +1 Yaku (score multiplier) and +10 fu if your hand contains two identical sequences (e.g. two 2-3-4 sets).";
    rarity = Common;
    cost = 135;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when iipeikoup p m t -> [ItemEffect.ExtraScore (1, 10)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Ryanpeikou";
    description = "Grants +4 Yaku (score multipliers) if your hand contains two sets of identical sequences.";
    rarity = Uncommon;
    cost = 260;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when ryanpeikoup p m t -> [ItemEffect.ExtraScore (4, 0)] | _ -> [];
    state = Nothing};

  { id = Guid.NewGuid(); name = "Ittsu";
    description = "Grants +3 Yaku (score multipliers) if your hand contains three sequences forming a straight from 1 to 9 (1-2-3, 4-5-6, and 7-8-9).";
    rarity = Uncommon;
    cost = 275;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when ittsup p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [];
    state = Nothing };

//     { id = Guid.NewGuid(); name = "Toitoihou"; description = "Grants +2 Yaku (score multipliers) if your hand consists entirely of four triplets (or quads) and a pair, with no sequences."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when toitoihoup p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "Sanankou";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three concealed triplets (drawn by yourself, not stolen).";
    rarity = Uncommon;
    cost = 220;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sanankoup p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Sankantsu";
    description = "Grants +4 Yaku (score multipliers) if your hand contains three quads (four of a kind).";
    rarity = Rare;
    cost = 350;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sankantsup p m t -> [ItemEffect.ExtraScore (4, 0)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Chitoitsu";
    description = "Grants +3 Yaku (score multipliers) if your hand consists of exactly seven distinct pairs.";
    rarity = Uncommon;
    cost = 230;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when chitoitsup p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "Chanta"; description = "Grants +2 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chantap p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Junchan"; description = "Grants +3 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junchanp p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Chinitsu"; description = "Grants +6 Yaku (score multipliers) unconditionally since all tiles in this game belong to the same suit."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ -> [ItemEffect.ExtraScore (6, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "Suuankou";
    description = "Grants +6 Yaku if your hand contains four concealed triplets (drawn by yourself).";
    rarity = Legendary;
    cost = 500;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when suuankoup p m t -> [ItemEffect.ExtraScore (6, 0)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Sukantsu";
    description = "Grants +8 Yaku (massive score multiplier) if your hand contains four quads (four of a kind).";
    rarity = Mythical;
    cost = 650;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sukantsup p m t -> [ItemEffect.ExtraScore (8, 0)] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "Chinroutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 1s and 9s."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when chinroutoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Ryuuiisou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 2, 3, 4, 6, and 8 tiles."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when ryuuiisoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "ChuurenPoutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand consists of three 1s, three 9s, and one of every other number (1112345678999) plus one extra tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chuurenPoutoup p m t -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "SuuankouTanki";
    description = "Grants +8 Yaku if you complete four concealed triplets by drawing the final tile to form the pair.";
    rarity = Mythical;
    cost = 700;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when suuankouTankip p m t -> [ItemEffect.ExtraScore (8, 0)] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "JunseiChuurenPoutou"; description = "Grants +26 Yaku (colossal score multiplier) if you complete the exact 1112345678999 hand by drawing a matching tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junseiChuurenPoutoup p m t -> [ItemEffect.ExtraScore (26, 0)] | _ -> [] }

  { id = Guid.NewGuid(); name = "Trash to Treasure";
    description = "Gain +10 fu whenever you discard a terminal tile (1 or 9).";
    rarity = Uncommon;
    cost = 125;
    effect = fun state _ event -> match event with | OnDiscard t when t.IsTerminal() -> [ItemEffect.ExtraScore (0, 10)] | _ -> [];
    state = Nothing };
  
  { id = Guid.NewGuid(); name = "죽은 자의 소생";
    description = "황패유국 시 버림패를 다시 섞어 패산으로 합니다. 이 아이템은 한 번 사용 후 파괴됩니다."
    rarity = Rare;
    cost = 250;
    effect = fun state _ event -> match event with | WhenPileEmpty -> [ShufflePile DiscardToDrawPile; SelfDestruct ] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "ㅁㄴㅇㄹ"
    description = "마지막 쯔모 기회에 +2판"
    rarity = Uncommon;
    cost = 200;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.honbaLeft = 1 -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid(); name = "도라 +1"
    description = "도라 1장 추가, 단 본장 3회 후 삭제"
    rarity = Rare;
    cost = 300;
    effect =
      fun state item event ->
        match event with
          | Honba ->
            match item.state with
              | Integer n when n <> 1 -> [DiscloseNMoreDora 1; UpdateItemState <| Integer (n - 1)]
              | Integer _ -> [DiscloseNMoreDora 1; SelfDestruct]
              | Nothing -> []
          | _ -> []
    state = Integer 3}

  { id = Guid.NewGuid();
    name = "짝수";
    description = "짝수 패 하나당 +10부"
    rarity = Uncommon;
    cost = 225;
    effect =
      fun state item event ->
        match event with
          | OnYakuCalc (_, _, _, Hand (h, t, k)) ->
            let evenCount =
              (if t.Value() % 2 = 0 then 1 else 0)
              + (Array.sum <| Array.mapi (fun i x -> if i % 2 = 0 then x else 0) h)
              + 4 * (List.length <| List.filter (fun (Kantsu (Tile x)) -> x % 2 = 0) k)
            [ ExtraScore (0, evenCount * 10) ]
          | _ -> []
    state = Nothing; };

  { id = Guid.NewGuid();
    name = "홀수";
    description = "홀수 패 하나당 +10부"
    rarity = Uncommon;
    cost = 225;
    effect =
      fun state item event ->
        match event with
          | OnYakuCalc (_, _, _, Hand (h, t, k)) ->
            let evenCount =
              (if t.Value() % 2 = 1 then 1 else 0)
              + (Array.sum <| Array.mapi (fun i x -> if i % 2 = 1 then x else 0) h)
              + 4 * (List.length <| List.filter (fun (Kantsu (Tile x)) -> x % 2 = 1) k)
            [ ExtraScore (0, evenCount * 10) ]
          | _ -> []
    state = Nothing; };

  { id = Guid.NewGuid();
    name = "건강박수";
    description = "1 혹은 9 패 하나당 +20부"
    rarity = Uncommon;
    cost = 225;
    effect =
      fun state item event ->
        match event with
          | OnYakuCalc (_, _, _, Hand (h, t, k)) ->
            let terminalCount =
              (if t.IsTerminal() then 1 else 0)
              + (Array.sum <| Array.mapi (fun i x -> if i = 1 || i = 9 then x else 0) h)
              + 4 * (List.length <| List.filter (fun (Kantsu t) -> t.IsTerminal()) k)
            [ ExtraScore (0, terminalCount * 20) ]
          | _ -> []
    state = Nothing; };

  { id = Guid.NewGuid();
    name = "Rinshankaihou"
    description = "+3 han if won immediately after declaring kan"
    rarity = Uncommon;
    cost = 175;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.isRinshanKaihouApplicable -> [ExtraScore (3, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Tenhou"
    description = "+3 han if won by the first draw"
    rarity = Uncommon;
    cost = 100;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.isTenhouApplicable -> [ExtraScore (3, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "모듈러"
    description = "8-9-1이나 9-1-2 슌쯔 허용. 변짱대기 없음."
    rarity = Rare;
    cost = 250;
    effect = fun state _ event -> match event with | Parsing -> [AllowWrapAroundShuntsu] | _ -> []
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Dora Collector"
    description = "Gain +5 fu for each revealed dora indicator."
    rarity = Uncommon;
    cost = 175;
    effect = fun state _ event -> match event with | OnYakuCalc _ -> [ExtraScore (0, Array.length state.dora * 5)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Red Indicator"
    description = "Gain +1 han if at least 3 dora indicators are revealed."
    rarity = Rare;
    cost = 275;
    effect = fun state _ event -> match event with | OnYakuCalc _ when Array.length state.dora >= 3 -> [ExtraScore (1, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Dead Wall Miner"
    description = "The next 2 kans each reveal 1 additional dora indicator, then this item breaks."
    rarity = Rare;
    cost = 325;
    effect =
      fun state item event ->
        match event, item.state with
        | OnKan _, Integer n when n > 1 -> [DiscloseNMoreDora 1; UpdateItemState (Integer (n - 1))]
        | OnKan _, Integer _ -> [DiscloseNMoreDora 1; SelfDestruct]
        | _ -> [];
    state = Integer 2 }

  { id = Guid.NewGuid();
    name = "Kan Tax"
    description = "Gain +20 fu whenever you declare kan."
    rarity = Uncommon;
    cost = 175;
    effect = fun state _ event -> match event with | OnKan _ -> [ExtraScore (0, 20)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Quad Market"
    description = "Gain 50 gold whenever you declare kan."
    rarity = Uncommon;
    cost = 175;
    effect = fun state _ event -> match event with | OnKan _ -> [AddGold 50] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Backpack"
    description = "Increase your max item slots by 1 while held. You cannot own duplicate items."
    rarity = Rare;
    cost = 325;
    effect = fun state _ event -> match event with | WhenObtained -> [AddMaxItems 1] | WhenSold -> [AddMaxItems -1] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Fourfold Path"
    description = "Gain +2 han if your winning hand has at least 2 declared kans."
    rarity = Rare;
    cost = 350;
    effect = fun state _ event -> match event with | OnYakuCalc (_, _, _, Hand (_, _, kantsu)) when List.length kantsu >= 2 -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Patience"
    description = "Gain +1 han if you discarded at least 3 tiles this hand."
    rarity = Uncommon;
    cost = 200;
    effect = fun state _ event -> match event with | OnYakuCalc _ when Array.length state.discardPile >= 3 -> [ExtraScore (1, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Deep Wall"
    description = "Gain +2 han if fewer than 10 tiles remain in the draw pile."
    rarity = Rare;
    cost = 325;
    effect = fun state _ event -> match event with | OnYakuCalc _ when Array.length state.pile < 10 -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Chanta"
    description = "Gain +4 han if every set and the pair contains a terminal tile."
    rarity = Uncommon;
    cost = 300;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when chantap p m t -> [ExtraScore (4, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Edge Lord"
    description = "Gain +1 han when winning on an edge wait."
    rarity = Common;
    cost = 125;
    effect = fun state _ event -> match event with | OnYakuCalc (_, m, _, _) when edgeWait m -> [ExtraScore (1, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Sacred Ends"
    description = "Gain +1 han and +10 fu when the winning tile is 1 or 9."
    rarity = Uncommon;
    cost = 200;
    effect = fun state _ event -> match event with | OnYakuCalc (_, _, t, _) when t.IsTerminal() -> [ExtraScore (1, 10)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Middle Road"
    description = "Gain +5 fu for each 4, 5, or 6 tile in your winning hand."
    rarity = Uncommon;
    cost = 175;
    effect =
      fun state _ event ->
        match event with
        | OnYakuCalc (_, _, _, hand) ->
          let bonusFu = countHandTiles (fun t -> let v = t.Value() in v = 4 || v = 5 || v = 6) hand * 5
          if bonusFu > 0 then [ExtraScore (0, bonusFu)] else []
        | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Perfect Flow"
    description = "Gain +1 han and +20 fu if all 4 sets in your winning hand are sequences."
    rarity = Uncommon;
    cost = 250;
    effect = fun state _ event -> match event with | OnYakuCalc (p, _, _, _) when allSequences p -> [ExtraScore (1, 20)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Open Road"
    description = "Gain +10 fu for each sequence in your winning hand."
    rarity = Common;
    cost = 175;
    effect =
      fun state _ event ->
        match event with
        | OnYakuCalc (p, _, _, _) ->
          let bonusFu = sequenceCount p * 10
          if bonusFu > 0 then [ExtraScore (0, bonusFu)] else []
        | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Modular Payoff"
    description = "Gain +2 han if your winning hand uses an 8-9-1 or 9-1-2 sequence."
    rarity = Rare;
    cost = 325;
    effect = fun state _ event -> match event with | OnYakuCalc (p, _, _, _) when hasWrapAroundSequence p -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Pair Collector"
    description = "Gain +5 fu for each distinct pair in your winning hand before parsing."
    rarity = Common;
    cost = 125;
    effect = fun state _ event -> match event with | OnYakuCalc (_, _, _, hand) -> [ExtraScore (0, distinctPairCount hand * 5)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Seven Mirrors"
    description = "Gain +2 han if your winning hand is Chitoitsu."
    rarity = Uncommon;
    cost = 225;
    effect = fun state _ event -> match event with | OnYakuCalc (Chitoitsu _, _, _, _) -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Heavy Hand"
    description = "Gain +1 han if item effects have already added at least +30 fu this hand."
    rarity = Rare;
    cost = 300;
    effect = fun state _ event -> match event with | OnYakuCalc _ when snd state.baseScore >= 30 -> [ExtraScore (1, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Riichi"
    description = "Declaring tsumo does not progress the honba or decrease tsumo left. You must discard before declaring tsumo again."
    rarity = Rare;
    cost = 450;
    effect = fun state _ event -> match event with | OnTsumo -> [SuppressHonba] | _ -> [];
    state = Nothing}
  ]
