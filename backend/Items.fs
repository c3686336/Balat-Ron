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
    description = "Gain +1 han (score multiplier) if your winning hand contains no 1 or 9 tiles.";
    rarity = Common;
    cost = 150;
    effect = fun state _ event -> match event with | OnYakuCalc _ when tanyaopRaw state.hand -> [ItemEffect.ExtraScore (1, 0)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Pinfu";
    description = "Gain +1 han (score multiplier) and +20 fu (base score) if your winning hand has four three-number runs, one pair, and wins by completing a run from either side.";
    rarity = Common;
    cost = 160;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when pinfup p m t -> [ItemEffect.ExtraScore (1, 20)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Iipeikou";
    description = "Gain +1 han (score multiplier) and +10 fu (base score) if your winning hand has two identical three-number runs, such as two 2-3-4 runs.";
    rarity = Common;
    cost = 135;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when iipeikoup p m t -> [ItemEffect.ExtraScore (1, 10)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Ryanpeikou";
    description = "Gain +4 han (score multiplier) if your winning hand has two different pairs of identical three-number runs.";
    rarity = Uncommon;
    cost = 260;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when ryanpeikoup p m t -> [ItemEffect.ExtraScore (4, 0)] | _ -> [];
    state = Nothing};

  { id = Guid.NewGuid(); name = "Ittsu";
    description = "Gain +3 han (score multiplier) if your winning hand has the three runs 1-2-3, 4-5-6, and 7-8-9.";
    rarity = Uncommon;
    cost = 275;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when ittsup p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [];
    state = Nothing };

//     { id = Guid.NewGuid(); name = "Toitoihou"; description = "Grants +2 Yaku (score multipliers) if your hand consists entirely of four triplets (or quads) and a pair, with no sequences."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when toitoihoup p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "Sanankou";
    description = "Gain +2 han (score multiplier) if your winning hand has at least three triplets or quads. A triplet is three matching tiles; a quad is four matching tiles declared with KAN.";
    rarity = Uncommon;
    cost = 220;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sanankoup p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Sankantsu";
    description = "Gain +4 han (score multiplier) if your winning hand has at least three declared quads. A quad is four matching tiles declared with KAN.";
    rarity = Rare;
    cost = 350;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sankantsup p m t -> [ItemEffect.ExtraScore (4, 0)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Chitoitsu";
    description = "Gain +3 han (score multiplier) if your winning hand is exactly seven different pairs. A pair is two matching tiles.";
    rarity = Uncommon;
    cost = 230;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when chitoitsup p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "Chanta"; description = "Grants +2 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chantap p m t -> [ItemEffect.ExtraScore (2, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Junchan"; description = "Grants +3 Yaku (score multipliers) if every set and the pair in your hand contains at least one terminal (1 or 9) tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junchanp p m t -> [ItemEffect.ExtraScore (3, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Chinitsu"; description = "Grants +6 Yaku (score multipliers) unconditionally since all tiles in this game belong to the same suit."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ -> [ItemEffect.ExtraScore (6, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "Suuankou";
    description = "Gain +6 han (score multiplier) if your winning hand has four triplets or quads. A triplet is three matching tiles; a quad is four matching tiles declared with KAN.";
    rarity = Legendary;
    cost = 500;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when suuankoup p m t -> [ItemEffect.ExtraScore (6, 0)] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Sukantsu";
    description = "Gain +8 han (large score multiplier) if your winning hand has four declared quads. A quad is four matching tiles declared with KAN.";
    rarity = Mythical;
    cost = 650;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when sukantsup p m t -> [ItemEffect.ExtraScore (8, 0)] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "Chinroutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 1s and 9s."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when chinroutoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "Ryuuiisou"; description = "Grants +13 Yaku (massive score multiplier) if your hand is composed entirely of 2, 3, 4, 6, and 8 tiles."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc _ when ryuuiisoupRaw state.hand -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
//     { id = Guid.NewGuid(); name = "ChuurenPoutou"; description = "Grants +13 Yaku (massive score multiplier) if your hand consists of three 1s, three 9s, and one of every other number (1112345678999) plus one extra tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when chuurenPoutoup p m t -> [ItemEffect.ExtraScore (13, 0)] | _ -> [] }
  { id = Guid.NewGuid(); name = "SuuankouTanki";
    description = "Gain +8 han (large score multiplier) if your winning hand has four triplets or quads and the winning tile completes the final pair of two matching tiles.";
    rarity = Mythical;
    cost = 700;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when suuankouTankip p m t -> [ItemEffect.ExtraScore (8, 0)] | _ -> [];
    state = Nothing };
  
//     { id = Guid.NewGuid(); name = "JunseiChuurenPoutou"; description = "Grants +26 Yaku (colossal score multiplier) if you complete the exact 1112345678999 hand by drawing a matching tile."; rarity = Common; cost = 50; effect = fun state event -> match event with | OnYakuCalc (p, m, t) when junseiChuurenPoutoup p m t -> [ItemEffect.ExtraScore (26, 0)] | _ -> [] }

  { id = Guid.NewGuid(); name = "Trash to Treasure";
    description = "Gain +10 fu (base score) whenever you discard a 1 or 9 tile.";
    rarity = Uncommon;
    cost = 125;
    effect = fun state _ event -> match event with | OnDiscard t when t.IsTerminal() -> [ItemEffect.ExtraScore (0, 10)] | _ -> [];
    state = Nothing };
  
  { id = Guid.NewGuid(); name = "Graveyard Revival";
    description = "When the draw pile runs out, shuffle all discarded tiles back into the draw pile. Breaks after use."
    rarity = Rare;
    cost = 250;
    effect = fun state _ event -> match event with | WhenPileEmpty -> [ShufflePile DiscardToDrawPile; SelfDestruct ] | _ -> [];
    state = Nothing };

  { id = Guid.NewGuid(); name = "Last Draw Gambit"
    description = "Gain +2 han (score multiplier) if you win with the TSUMO button on your final allowed win attempt of the round."
    rarity = Uncommon;
    cost = 200;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.honbaLeft = 1 -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid(); name = "Dora Lantern"
    description = "At the start of each hand, reveal 1 extra dora indicator, which can make more tiles count as score bonuses. Breaks after 3 hands."
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
    name = "Even Chorus";
    description = "Gain +10 fu (base score) for each even-numbered tile in your winning hand."
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
    name = "Odd Chorus";
    description = "Gain +10 fu (base score) for each odd-numbered tile in your winning hand."
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
    name = "Terminal Applause";
    description = "Gain +20 fu (base score) for each 1 or 9 tile in your winning hand."
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
    description = "Gain +3 han (score multiplier) if you win immediately after pressing KAN and drawing the replacement tile."
    rarity = Uncommon;
    cost = 175;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.isRinshanKaihouApplicable -> [ExtraScore (3, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Tenhou"
    description = "Gain +3 han (score multiplier) if you win before discarding any tile this hand."
    rarity = Uncommon;
    cost = 100;
    effect = fun state _ event -> match event with | OnYakuCalc _ when state.isTenhouApplicable -> [ExtraScore (3, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Modular Sequence"
    description = "Allows three-number runs to wrap around from 9 back to 1, so 8-9-1 and 9-1-2 can count as runs. These wrapped runs do not trigger bonuses for completing 1-2-3 or 7-8-9 from the outside."
    rarity = Rare;
    cost = 250;
    effect = fun state _ event -> match event with | Parsing -> [AllowWrapAroundShuntsu] | _ -> []
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Dora Collector"
    description = "Gain +5 fu (base score) for each revealed dora indicator. Dora indicators mark which tiles give bonus score."
    rarity = Uncommon;
    cost = 175;
    effect = fun state _ event -> match event with | OnYakuCalc _ -> [ExtraScore (0, Array.length state.dora * 5)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Red Indicator"
    description = "Gain +1 han (score multiplier) if at least 3 dora indicators are revealed. Dora indicators mark which tiles give bonus score."
    rarity = Rare;
    cost = 275;
    effect = fun state _ event -> match event with | OnYakuCalc _ when Array.length state.dora >= 3 -> [ExtraScore (1, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Dead Wall Miner"
    description = "The next 2 times you press KAN, reveal 1 extra dora indicator. Dora indicators mark bonus-score tiles. Breaks after the second use."
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
    description = "Gain +20 fu (base score) whenever you press KAN to declare four matching tiles."
    rarity = Uncommon;
    cost = 175;
    effect = fun state _ event -> match event with | OnKan _ -> [ExtraScore (0, 20)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Quad Market"
    description = "Gain 50 gold whenever you press KAN to declare four matching tiles."
    rarity = Uncommon;
    cost = 175;
    effect = fun state _ event -> match event with | OnKan _ -> [AddGold 50] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Backpack"
    description = "Increase your maximum item slots by 2 while held. Since Backpack uses one slot, this gives 1 extra usable item slot."
    rarity = Rare;
    cost = 325;
    effect = fun state _ event -> match event with | WhenObtained -> [AddMaxItems 2] | WhenSold -> [AddMaxItems -2] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Fourfold Path"
    description = "Gain +2 han (score multiplier) if your winning hand has at least 2 declared quads. A quad is four matching tiles declared with KAN."
    rarity = Rare;
    cost = 350;
    effect = fun state _ event -> match event with | OnYakuCalc (_, _, _, Hand (_, _, kantsu)) when List.length kantsu >= 2 -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Patience"
    description = "Gain +1 han (score multiplier) if you discarded at least 3 tiles before winning this hand."
    rarity = Uncommon;
    cost = 200;
    effect = fun state _ event -> match event with | OnYakuCalc _ when Array.length state.discardPile >= 3 -> [ExtraScore (1, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Deep Wall"
    description = "Gain +2 han (score multiplier) if fewer than 10 tiles remain in the draw pile when you win."
    rarity = Rare;
    cost = 325;
    effect = fun state _ event -> match event with | OnYakuCalc _ when Array.length state.pile < 10 -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Chanta"
    description = "Gain +4 han (score multiplier) if every group in your winning hand includes a 1 or 9 tile. Groups are runs, triplets, quads, and the pair."
    rarity = Uncommon;
    cost = 300;
    effect = fun state _ event -> match event with | OnYakuCalc (p, m, t, _) when chantap p m t -> [ExtraScore (4, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Edge Lord"
    description = "Gain +1 han (score multiplier) if your winning tile completes 1-2-3 with the 3, or 7-8-9 with the 7."
    rarity = Common;
    cost = 125;
    effect = fun state _ event -> match event with | OnYakuCalc (_, m, _, _) when edgeWait m -> [ExtraScore (1, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Sacred Ends"
    description = "Gain +1 han (score multiplier) and +10 fu (base score) if the tile you win with is 1 or 9."
    rarity = Uncommon;
    cost = 200;
    effect = fun state _ event -> match event with | OnYakuCalc (_, _, t, _) when t.IsTerminal() -> [ExtraScore (1, 10)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Middle Road"
    description = "Gain +5 fu (base score) for each 4, 5, or 6 tile in your winning hand."
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
    description = "Gain +1 han (score multiplier) and +20 fu (base score) if all 4 sets in your winning hand are three-number runs."
    rarity = Uncommon;
    cost = 250;
    effect = fun state _ event -> match event with | OnYakuCalc (p, _, _, _) when allSequences p -> [ExtraScore (1, 20)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Open Road"
    description = "Gain +10 fu (base score) for each three-number run in your winning hand."
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
    description = "Gain +2 han (score multiplier) if your winning hand uses an 8-9-1 or 9-1-2 wrap-around run."
    rarity = Rare;
    cost = 325;
    effect = fun state _ event -> match event with | OnYakuCalc (p, _, _, _) when hasWrapAroundSequence p -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Pair Collector"
    description = "Gain +5 fu (base score) for each different pair of matching tiles in your winning hand."
    rarity = Common;
    cost = 125;
    effect = fun state _ event -> match event with | OnYakuCalc (_, _, _, hand) -> [ExtraScore (0, distinctPairCount hand * 5)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Seven Mirrors"
    description = "Gain +2 han (score multiplier) if your winning hand is exactly seven different pairs."
    rarity = Uncommon;
    cost = 225;
    effect = fun state _ event -> match event with | OnYakuCalc (Chitoitsu _, _, _, _) -> [ExtraScore (2, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Heavy Hand"
    description = "Gain +1 han (score multiplier) for every 100 fu (base score) already added by item effects this hand."
    rarity = Rare;
    cost = 300;
    effect = fun state _ event -> match event with | OnYakuCalc _ -> [ExtraScore ((snd state.baseScore) / 100, 0)] | _ -> [];
    state = Nothing }

  { id = Guid.NewGuid();
    name = "Riichi"
    description = "When you win with the TSUMO button, the round does not spend one of your allowed TSUMO attempts. You must discard before using TSUMO again."
    rarity = Rare;
    cost = 450;
    effect = fun state _ event -> match event with | OnTsumo -> [SuppressHonba] | _ -> [];
    state = Nothing}
  ]
