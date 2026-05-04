module Yaku

open Types

let Sukantsup (ParsedHand (kan, _, _, _): ParsedNormalHand) (_: Tile) = List.length kan = 4
