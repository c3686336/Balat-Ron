module Program

open Evaluator;

[<EntryPoint>]
let main argv =
  let result = TryParse ([|0;3;1;2;2;1;1;1;1;0|], [9])
  // let result = TryParseHeadlessHandAsMuch 
  
  printfn "%A" result |> ignore

  0
