module Items

open Yaku
open Types
open System

let allItems : Item list = [
  { id = Guid.NewGuid(); name = "Tanyao";
    description = "Grants +1 Yaku (score multiplier) if your hand has no 1 or 9 tiles.";
    rarity = Common;
    cost = 100;
    effect = fun state _ event -> match event with | OnYakuCalc _ when tanyaopRaw state.hand -> [ItemEffect.ExtraScore (1, 0)] | OnScoreCalc _ when tanyaopRaw state.hand -> [ItemEffect.ExtraScore (1, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Pinfu";
    description = "Grants +1 Yaku (score multiplier) and +10 fu if your hand consists of four sequences (e.g. 2-3-4) and a pair, and you were waiting to complete a sequence.";
    rarity = Common;
    cost = 100;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when pinfup p m t -> [ItemEffect.ExtraScore (1, 10)] | OnScoreCalc (p, m, t, _) when pinfup p m t -> [ItemEffect.ExtraScore (1, 10); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Iipeikou";
    description = "Grants +1 Yaku (score multiplier) if your hand contains two identical sequences (e.g. two 2-3-4 sets).";
    rarity = Common;
    cost = 100;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when iipeikoup p m t -> [ItemEffect.ExtraScore (1, 0)] | OnScoreCalc (p, m, t, _) when iipeikoup p m t -> [ItemEffect.ExtraScore (1, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Ryanpeikou";
    description = "Grants +3 Yaku (score multipliers) if your hand contains two sets of identical sequences.";
    rarity = Uncommon;
    cost = 125;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when ryanpeikoup p m t -> [ItemEffect.ExtraScore (3, 0)] | OnScoreCalc (p, m, t, _) when ryanpeikoup p m t -> [ItemEffect.ExtraScore (3, 0); PrintName] | _ -> [];
    state = Nothing};

  { id = Guid.NewGuid(); name = "Ittsu";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three sequences forming a straight from 1 to 9 (1-2-3, 4-5-6, and 7-8-9).";
    rarity = Uncommon;
    cost = 125;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when ittsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t, _) when ittsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

//     { id = Guid.NewGuid(); name = "Toitoihou"; description = "Grants +2 Yaku (score multipliers) if your hand consists entirely of four triplets (or quads) and a pair, with no sequences."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when toitoihoup p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "Sanankou";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three concealed triplets (drawn by yourself, not stolen).";
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sanankoup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t, _) when sanankoup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Sankantsu";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three quads (four of a kind).";
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sankantsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t, _) when sankantsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Chitoitsu";
    description = "Grants +2 Yaku (score multipliers) if your hand consists of exactly seven distinct pairs.";
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when chitoitsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t, _) when chitoitsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "Chanta"; description = "Grants +2 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chantap p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Junchan"; description = "Grants +3 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junchanp p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Chinitsu"; description = "Grants +6 Yaku (score multipliers) unconditionally since all tiles in this game belong to the same suit."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ -> [ItemEffect.ExtraScore (6, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "Suuankou";
    description = "Grants +13 Yaku (massive score multiplier) if your hand contains four concealed triplets (drawn by yourself).";
    rarity = Rare;
    cost = 300;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when suuankoup p m t -> [ItemEffect.ExtraScore (13, 0)] | OnScoreCalc (p, m, t, _) when suuankoup p m t -> [ItemEffect.ExtraScore (13, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Sukantsu";
    description = "Grants +13 Yaku (massive score multiplier) if your hand contains four quads (four of a kind).";
    rarity = Rare;
    cost = 300;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sukantsup p m t -> [ItemEffect.ExtraScore (13, 0)] | OnScoreCalc (p, m, t, _) when sukantsup p m t -> [ItemEffect.ExtraScore (13, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "Chinroutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 1s and 9s."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when chinroutoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Ryuuiisou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 2, 3, 4, 6, and 8 tiles."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when ryuuiisoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "ChuurenPoutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand consists of three 1s, three 9s, and one of every other number (1112345678999) plus one extra tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chuurenPoutoup p m t -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "SuuankouTanki";
    description = "Grants +26 Yaku (colossal score multiplier) if you complete four concealed triplets by drawing the final tile to form the pair.";
    rarity = Legendary;
    cost = 600;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when suuankouTankip p m t -> [ItemEffect.ExtraScore (26, 0)] | OnScoreCalc (p, m, t, _) when suuankouTankip p m t -> [ItemEffect.ExtraScore (26, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "JunseiChuurenPoutou"; description = "Grants +26 Yaku (colossal score multiplier) if you complete the exact 1112345678999 hand by drawing a matching tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junseiChuurenPoutoup p m t -> [ItemEffect.ExtraScore (26, 0)] | _ -> [] }

  { id = Guid.NewGuid(); name = "Trash to Treasure";
    description = "Gain +10 mults whenever you discard a terminal tile (1 or 9).";
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnDiscard t when t.IsTerminal() -> [ItemEffect.ModifyGameState { state with baseScore = (fst state.baseScore, snd state.baseScore + 10) }; PrintStr "+10 mult from discarding 1/9"] | _ -> [];
    state = Nothing };
  
  { id = Guid.NewGuid(); name = "죽은 자의 소생";
    description = "황패유국 시 버림패와 사용하지 않은 영상패, 도라패를 다시 섞어 패산으로 합니다. 이 아이템은 한 번 사용 후 다시 상점으로 돌아갑니다."
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | WhenPileEmpty -> [ModifyGameState { state with pile = Array.randomShuffleWith state.rng state.discardPile; discardPile = [||] }; SelfDestruct ] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "ㅁㄴㅇㄹ"
    description = "마지막 쯔모 기회에 +2판"
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.tsumoLeft = 1 -> [ExtraScore (2, 0)] | OnScoreCalc _ when state.tsumoLeft = 1 -> [ExtraScore (2, 0); PrintName ] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid(); name = "도라 +1"
    description = "도라 1장 추가, 단 본장 3회 후 삭제"
    rarity = Common;
    cost = 100;
    effect =
      fun state item event ->
        let (Integer n) = item.state
        match event with
          | Honba -> if n <> 1 then [DiscloseNMoreDora 1; UpdateItemState <| Integer (n - 1)] else printfn "asdf"; [DiscloseNMoreDora 1; SelfDestruct]
          | _ -> []
    state = Integer 3}

  { id = Guid.NewGuid();
    name = "짝수";
    description = "짝수 패 하나당 +10부"
    rarity = Uncommon;
    cost = 200;
    effect =
      fun state item event ->
        match event with
          | OnScoreCalc (_, _, _, Hand (h, t, k)) ->
            let evenCount =
              if t.Value() % 2 = 0 then 1 else 0
              + (Array.sum <| Array.mapi (fun i x -> if i % 2 = 0 then x else 0) h)
              + 4 * (List.length <| List.filter (fun (Kantsu (Tile x)) -> x % 2 = 0) k)
            [ ExtraScore (0, evenCount * 10) ]
          | _ -> []
    state = Nothing; };

  { id = Guid.NewGuid();
    name = "홀수";
    description = "홀수 패 하나당 +10부"
    rarity = Uncommon;
    cost = 200;
    effect =
      fun state item event ->
        match event with
          | OnScoreCalc (_, _, _, Hand (h, t, k)) ->
            let evenCount =
              if t.Value() % 2 = 1 then 1 else 0
              + (Array.sum <| Array.mapi (fun i x -> if i % 2 = 1 then x else 0) h)
              + 4 * (List.length <| List.filter (fun (Kantsu (Tile x)) -> x % 2 = 1) k)
            [ ExtraScore (0, evenCount * 10) ]
          | _ -> []
    state = Nothing; };

  { id = Guid.NewGuid();
    name = "건강박수";
    description = "1 혹은 9 패 하나당 +10부"
    rarity = Uncommon;
    cost = 200;
    effect =
      fun state item event ->
        match event with
          | OnScoreCalc (_, _, _, Hand (h, t, k)) ->
            let evenCount =
              if t.IsTerminal() then 1 else 0
              + (Array.sum <| Array.mapi (fun i x -> if i % 2 = 1 then x else 0) h)
              + 4 * (List.length <| List.filter (fun (Kantsu (Tile x)) -> x % 2 = 1) k)
            [ ExtraScore (0, evenCount * 10) ]
          | _ -> []
    state = Nothing; };

  { id = Guid.NewGuid();
    name = "Rinshankaihou"
    description = "+1 han if won immediately after declaring kan"
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.isRinshanKaihouApplicable -> [ExtraScore (1, 0)] | OnScoreCalc _ when state.isRinshanKaihouApplicable -> [ExtraScore (1, 0); PrintName ] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Tenhou"
    description = "+5 han if won by the first draw"
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.isTenhouApplicable -> [ExtraScore (1, 0)] | OnScoreCalc _ when state.isTenhouApplicable -> [ExtraScore (1, 0); PrintName ] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "모듈러"
    description = "8-9-1이나 9-1-2 슌쯔 허용. 변짱대기 없음."
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | Parsing -> [AllowWrapAroundShuntsu]
    state = Nothing }

  // { id = Guid.NewGuid();
  //   name = "Riichi"
  //   description = "You can keep going after declaring tsumo" }
  ]
