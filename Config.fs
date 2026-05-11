module Config

let tsumoPerRound = 5

let initialGoalScore = 4000I

let nextGoalScore (currentGoalScore: bigint) =
    currentGoalScore + currentGoalScore

let calculateGoldsEarned (tsumoLeft: int) =
    tsumoLeft * 100 + 50

let numberOfShopItems = 3

let maxItems = 5
