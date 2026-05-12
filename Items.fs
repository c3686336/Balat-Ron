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
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when pinfup p m t -> [ItemEffect.ExtraScore (1, 10)] | OnScoreCalc (p, m, t) when pinfup p m t -> [ItemEffect.ExtraScore (1, 10); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Iipeikou";
    description = "Grants +1 Yaku (score multiplier) if your hand contains two identical sequences (e.g. two 2-3-4 sets).";
    rarity = Common;
    cost = 100;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when iipeikoup p m t -> [ItemEffect.ExtraScore (1, 0)] | OnScoreCalc (p, m, t) when iipeikoup p m t -> [ItemEffect.ExtraScore (1, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Ryanpeikou";
    description = "Grants +3 Yaku (score multipliers) if your hand contains two sets of identical sequences.";
    rarity = Uncommon;
    cost = 125;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when ryanpeikoup p m t -> [ItemEffect.ExtraScore (3, 0)] | OnScoreCalc (p, m, t) when ryanpeikoup p m t -> [ItemEffect.ExtraScore (3, 0); PrintName] | _ -> [];
    state = Nothing};

  { id = Guid.NewGuid(); name = "Ittsu";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three sequences forming a straight from 1 to 9 (1-2-3, 4-5-6, and 7-8-9).";
    rarity = Uncommon;
    cost = 125;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when ittsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t) when ittsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

//     { id = Guid.NewGuid(); name = "Toitoihou"; description = "Grants +2 Yaku (score multipliers) if your hand consists entirely of four triplets (or quads) and a pair, with no sequences."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when toitoihoup p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "Sanankou";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three concealed triplets (drawn by yourself, not stolen).";
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when sanankoup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t) when sanankoup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Sankantsu";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three quads (four of a kind).";
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when sankantsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t) when sankantsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Chitoitsu";
    description = "Grants +2 Yaku (score multipliers) if your hand consists of exactly seven distinct pairs.";
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when chitoitsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t) when chitoitsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "Chanta"; description = "Grants +2 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chantap p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Junchan"; description = "Grants +3 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junchanp p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Chinitsu"; description = "Grants +6 Yaku (score multipliers) unconditionally since all tiles in this game belong to the same suit."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ -> [ItemEffect.ExtraScore (6, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "Suuankou";
    description = "Grants +13 Yaku (massive score multiplier) if your hand contains four concealed triplets (drawn by yourself).";
    rarity = Rare;
    cost = 300;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when suuankoup p m t -> [ItemEffect.ExtraScore (13, 0)] | OnScoreCalc (p, m, t) when suuankoup p m t -> [ItemEffect.ExtraScore (13, 0); PrintName] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Sukantsu";
    description = "Grants +13 Yaku (massive score multiplier) if your hand contains four quads (four of a kind).";
    rarity = Rare;
    cost = 300;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when sukantsup p m t -> [ItemEffect.ExtraScore (13, 0)] | OnScoreCalc (p, m, t) when sukantsup p m t -> [ItemEffect.ExtraScore (13, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "Chinroutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 1s and 9s."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when chinroutoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Ryuuiisou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 2, 3, 4, 6, and 8 tiles."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when ryuuiisoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "ChuurenPoutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand consists of three 1s, three 9s, and one of every other number (1112345678999) plus one extra tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chuurenPoutoup p m t -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "SuuankouTanki";
    description = "Grants +26 Yaku (colossal score multiplier) if you complete four concealed triplets by drawing the final tile to form the pair.";
    rarity = Legendary;
    cost = 600;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when suuankouTankip p m t -> [ItemEffect.ExtraScore (26, 0)] | OnScoreCalc (p, m, t) when suuankouTankip p m t -> [ItemEffect.ExtraScore (26, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "JunseiChuurenPoutou"; description = "Grants +26 Yaku (colossal score multiplier) if you complete the exact 1112345678999 hand by drawing a matching tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junseiChuurenPoutoup p m t -> [ItemEffect.ExtraScore (26, 0)] | _ -> [] }

  { id = Guid.NewGuid(); name = "Trash to Treasure";
    description = "Gain +10 mults whenever you discard a terminal tile (1 or 9).";
    rarity = Uncommon;
    cost = 150;
    effect = fun state _ event -> match event with | OnDiscard t when t.IsTerminal() -> [ItemEffect.ModifyGameState { state with baseScore = (fst state.baseScore, snd state.baseScore + 10) }; PrintStr "+10 mult from discarding 1/9"] | _ -> [];
    state = Nothing };
  
  { id = Guid.NewGuid(); name = "죽은 자의 소생";
    description = "황패유국 시 버림패를 다시 섞어 패산으로 합니다. 이 아이템은 한 번 사용 후 다시 상점으로 돌아갑니다."
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



  // { id = Guid.NewGuid(); name = "도라 +1"
  //   description = "도라 1장 추가"
  //   rarity = Uncommon;
  //   cost = 200;
  //   effect = fun state _ event ->}
  ]
