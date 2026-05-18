module Fu

open Types
open Utils

let fu (hand: ParsedHand) (machiType: Machi) (tsumoTile: Tile): int =
  match hand with
    | Chitoitsu _ -> 25
    | NormalHand normalHand ->
      // Base fu
      let (ParsedHand (kan, shun, ko, toi)) = normalHand
      let ankoFu = 4 * (List.length ko)
      let ankoYaoFu = 4 * (List.filter (fun (Kotsu t) -> t = Tile 1 || t = Tile 9) ko |> List.length)
      let ankanFu = 16 * (List.length kan)
      let ankanYaoFu = 16 * (List.filter (fun (Kantsu t) -> t = Tile 1 || t = Tile 9) kan |> List.length)
      let machiFu =
        match machiType with
          | Ryoumenmachi(_) | Shanponmachi(_) -> 0
          | Kanchanmachi(_) | Penchanmachi(_) | Tanki(_) -> 2
      let baseFu = 20 + if ankoFu + ankanFu + machiFu = 0 then 0 else 2 // Always menzen tsumo
      
      roundUpTo (baseFu + ankoFu + ankanFu + machiFu + ankoYaoFu + ankanYaoFu) 10
