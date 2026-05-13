module Config

let tsumoPerRound = 5

let initialGoalScore = 4000I // - 3000I

let nextGoalScore (currentGoalScore: bigint) =
    currentGoalScore + currentGoalScore

let calculateGoldsEarned (tsumoLeft: int) =
    tsumoLeft * 50 + 25 // + 99999

let numberOfShopItems = 10

let maxItems = 5

let discount x = x * 9 / 10
