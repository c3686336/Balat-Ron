module Items

open Yaku
open Types

let allItems : Item list = [
  { name = "Tanyao";
    description = "Grants +1 Yaku (score multiplier) if your hand has no 1 or 9 tiles.";
    rarity = Common;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc _ when tanyaopRaw state.hand -> [ItemEffect.ExtraScore (1, 0)] | OnScoreCalc _ when tanyaopRaw state.hand -> [ItemEffect.ExtraScore (1, 0); PrintName] | _ -> [];
    state = Nothing };

  { name = "Pinfu";
    description = "Grants +1 Yaku (score multiplier) if your hand consists of four sequences (e.g. 2-3-4) and a pair, and you were waiting to complete a sequence.";
    rarity = Common;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when pinfup p m t -> [ItemEffect.ExtraScore (1, 0)] | OnScoreCalc (p, m, t) when pinfup p m t -> [ItemEffect.ExtraScore (1, 0); PrintName] | _ -> [];
    state = Nothing };

  { name = "Iipeikou";
    description = "Grants +1 Yaku (score multiplier) if your hand contains two identical sequences (e.g. two 2-3-4 sets).";
    rarity = Common;
    cost = 100;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when iipeikoup p m t -> [ItemEffect.ExtraScore (1, 0)] | OnScoreCalc (p, m, t) when iipeikoup p m t -> [ItemEffect.ExtraScore (1, 0); PrintName] | _ -> [];
    state = Nothing };

  { name = "Ryanpeikou";
    description = "Grants +3 Yaku (score multipliers) if your hand contains two sets of identical sequences.";
    rarity = Uncommon;
    cost = 75;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when ryanpeikoup p m t -> [ItemEffect.ExtraScore (3, 0)] | OnScoreCalc (p, m, t) when ryanpeikoup p m t -> [ItemEffect.ExtraScore (3, 0); PrintName] | _ -> [];
    state = Nothing};

  { name = "Ittsu";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three sequences forming a straight from 1 to 9 (1-2-3, 4-5-6, and 7-8-9).";
    rarity = Uncommon;
    cost = 75;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when ittsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t) when ittsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

//     { name = "Toitoihou"; description = "Grants +2 Yaku (score multipliers) if your hand consists entirely of four triplets (or quads) and a pair, with no sequences."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when toitoihoup p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
  { name = "Sanankou";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three concealed triplets (drawn by yourself, not stolen).";
    rarity = Uncommon;
    cost = 50;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when sanankoup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t) when sanankoup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

  { name = "Sankantsu";
    description = "Grants +2 Yaku (score multipliers) if your hand contains three quads (four of a kind).";
    rarity = Uncommon;
    cost = 50;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when sankantsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t) when sankantsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };

  { name = "Chitoitsu";
    description = "Grants +2 Yaku (score multipliers) if your hand consists of exactly seven distinct pairs.";
    rarity = Uncommon;
    cost = 50;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when chitoitsup p m t -> [ItemEffect.ExtraScore (2, 0)] | OnScoreCalc (p, m, t) when chitoitsup p m t -> [ItemEffect.ExtraScore (2, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { name = "Chanta"; description = "Grants +2 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chantap p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
//     { name = "Junchan"; description = "Grants +3 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junchanp p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [] }
//     { name = "Chinitsu"; description = "Grants +6 Yaku (score multipliers) unconditionally since all tiles in this game belong to the same suit."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ -> [ItemEffect.ExtraScore (6, 0)] | _ -> [] }
  { name = "Suuankou";
    description = "Grants +13 Yaku (massive score multiplier) if your hand contains four concealed triplets (drawn by yourself).";
    rarity = Rare;
    cost = 50;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when suuankoup p m t -> [ItemEffect.ExtraScore (13, 0)] | OnScoreCalc (p, m, t) when suuankoup p m t -> [ItemEffect.ExtraScore (13, 0); PrintName] | _ -> [];
    state = Nothing };

  { name = "Sukantsu";
    description = "Grants +13 Yaku (massive score multiplier) if your hand contains four quads (four of a kind).";
    rarity = Rare;
    cost = 50;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when sukantsup p m t -> [ItemEffect.ExtraScore (13, 0)] | OnScoreCalc (p, m, t) when sukantsup p m t -> [ItemEffect.ExtraScore (13, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { name = "Chinroutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 1s and 9s."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when chinroutoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { name = "Ryuuiisou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 2, 3, 4, 6, and 8 tiles."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when ryuuiisoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { name = "ChuurenPoutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand consists of three 1s, three 9s, and one of every other number (1112345678999) plus one extra tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chuurenPoutoup p m t -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
  { name = "SuuankouTanki";
    description = "Grants +26 Yaku (colossal score multiplier) if you complete four concealed triplets by drawing the final tile to form the pair.";
    rarity = Legendary;
    cost = 50;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t) when suuankouTankip p m t -> [ItemEffect.ExtraScore (26, 0)] | OnScoreCalc (p, m, t) when suuankouTankip p m t -> [ItemEffect.ExtraScore (26, 0); PrintName] | _ -> [];
    state = Nothing };
  
//     { name = "JunseiChuurenPoutou"; description = "Grants +26 Yaku (colossal score multiplier) if you complete the exact 1112345678999 hand by drawing a matching tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junseiChuurenPoutoup p m t -> [ItemEffect.ExtraScore (26, 0)] | _ -> [] }

  { name = "Trash to Treasure";
    description = "Gain +500 points whenever you discard a terminal tile (1 or 9).";
    rarity = Uncommon;
    cost = 100;
    effect = fun state _ event -> match event with | OnDiscard t when t.IsTerminal() -> [ItemEffect.ModifyGameState { state with currentScore = state.currentScore + 500I }; PrintStr "+500 from discarding 1/9"] | _ -> [];
    state = Nothing };
  ]
