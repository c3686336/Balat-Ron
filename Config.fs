module Config

let tsumoPerRound = 3

let initialGoalScore = 2000I

let nextGoalScore (currentGoalScore: bigint) =
    2I * currentGoalScore

let calculateGoldsEarned (tsumoLeft: int) =
    tsumoLeft * 100 + 50

let numberOfShopItems = 3
