module Config

let tsumoPerRound = 3

let initialGoalScore = 1500I

let nextGoalScore (currentGoalScore: bigint) =
    currentGoalScore + currentGoalScore / 2I

let calculateGoldsEarned (tsumoLeft: int) =
    tsumoLeft * 100 + 50

let numberOfShopItems = 3

let maxItems = 5
