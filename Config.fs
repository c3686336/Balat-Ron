module Config

let tsumoPerRound = 5

let initialGoalScore = 4000I

let nextGoalScore (currentGoalScore: bigint) =
    currentGoalScore + currentGoalScore

let calculateGoldsEarned (tsumoLeft: int) =
    tsumoLeft * 50 + 25 

let numberOfShopItems = 4

let maxItems = 5
