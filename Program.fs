open Types
open Evaluator

let testHand (name: string) (arr: int array) =
    let result = ParseHand (arr, [])
    printfn $"=== %s{name} ==="
    printfn $"Found %d{result.Length} parsings."
    for r in result do
        printfn $"%O{r}"
    printfn ""

testHand "Ryanpeikou wait" [|0; 2; 2; 2; 2; 2; 2; 2; 0; 0|]
testHand "Iipeikou with kotsu" [|0; 3; 2; 2; 2; 2; 3; 0; 0; 0|]
testHand "Overlapping shuntsu 1" [|0; 1; 2; 3; 2; 1; 0; 0; 0; 5|] // 14 tiles. wait 1+2+3+2+1+5 = 14
testHand "Overlapping shuntsu 2" [|0; 3; 4; 4; 3; 0; 0; 0; 0; 0|]
