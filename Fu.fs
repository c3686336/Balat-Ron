module Fu

open Types
open Yaku

let Fu (hand: ParsedHand) (machiType: Machi) (tsumoTile: Tile): int =
  match hand with
    | Chitoitsu _ -> 25
    | NormalHand normalHand ->
      // Base fu
      let (ParsedHand (kan, shun, ko, toi)) = normalHand
      let baseFu = 20 + if Pinfup hand machiType tsumoTile then 0 else 2 // Always menzen tsumo
      let ankoFu = 4 * (List.length ko)
      let ankanFu = 16 * (List.length kan)
      let machiFu =
        match machiType with
          | Ryoumenmachi(_) | Shanponmachi(_) -> 0
          | Kanchanmachi(_) | Penchanmachi(_) | Tanki(_) -> 2
      
      baseFu + ankoFu + ankanFu
