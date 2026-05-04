module Program

open Evaluator;

[<EntryPoint>]
let main argv =
  let result = TryParse ([|0;3;1;1;2;1;1;1;1;3|], [])
  printfn $"{result}" |> ignore
  let result = TryParse ([|0;4;1;1;1;1;1;1;1;3|], [])
  printfn $"{result}" |> ignore
  let result = TryParse ([|0;3;1;4;2;1;1;1;1;3|], [])
  printfn $"{result}" |> ignore
  let result = TryParse ([|0;3;1;1;1;1;1;1;1;4|], [])
  printfn $"{result.ToString()}" |> ignore
  // let result = TryParseHeadlessHandAsMuch [(Tile 1, 4); (Tile 2, 1); (Tile 3, 1); (Tile 4, 1); (Tile 5, 1); (Tile 6, 1); (Tile 7, 1); (Tile 8, 1); (Tile 9, 1)] ([], [], [], Tile 9)
  let result = TryParseHeadlessHandAsMuch (ListHand [(Tile 1, 4); (Tile 2, 1); (Tile 3, 1)]) (ParsedHand ([], [], [], Toitsu (Tile 9)))
  printfn $"{result}" |> ignore

  0
