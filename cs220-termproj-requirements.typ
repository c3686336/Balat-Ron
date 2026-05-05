#set document(author: "20250803 홍유찬", title: "CS20200 Term Project Proposal")
#set text(font: "Noto Serif CJK KR")

#align(center)[
    #text(2em, weight: "bold")[#title()] \
    #v(1em)
    #text(1.2em)[20250803 홍유찬]
]

#v(2em)

= Mahjongg + Balatro

== Game Concept
Use the 1 to 9 bamboo tiles of the standard Riichi Mahjongg deck. Player draws 14 tiles at the beginning. Player has two choices. They could either declare Tsumo, or discard a tile and draw another one from the pile. If the player declares Tsumo, then corresponding score is given and the game continues with another 14 tiles. At each round, there is a set number of Tsumo you can declare and required score to clear the round. After each round, the player can use their score to buy items that boost the earned score for specific hands or tiles.

To declare Tsumo, the hand has to have a pair and 4 triplets. Triplets are either three same tiles or three consecutively numbered tiles. If the player declares Tsumo without meeting the requirements, they'll get some penalty points.

After Tsumo, the score is calculated based on exponent score (Determined based on the overall hand) and multiplier score (Bonus score determined based on the types of pair and triplets in the hand). The score is
$
    6 X 2^(Y + 2)
$
Where $X$ is the multiplier and $Y$ is the exponent,
Rounded up at ten's place.

Kan is another action that the player can take on their 14 tile hand. If there's 4 of the same tiles present, then the player can declare Kan to set aside these 4 tiles as a triplet (It's treated like a triplet even though it has 4 tiles), draw one more time (Because 4 tiles are being treated like 3, the player now only has 13 tiles in their hand), and reveal another Dora Indicator. The play continues normally afterwards.

Dora Indicator is a set of tiles revealed to the player separate to their hand. The player does not draw from 도라표시패. If a tile in player's hand when they declare Tsumo happens to equal one of the 도라표시패's value plus one modulo 10, then an extra 1 exponent is given to the player.

Cf.
#link("https://namu.wiki/w/%EB%A6%AC%EC%B9%98%EB%A7%88%EC%9E%91/%EC%A0%90%EC%88%98#s-3.3")[Score calculation in Riichi Mahjongg]
#link("https://namu.wiki/w/%EB%A6%AC%EC%B9%98%EB%A7%88%EC%9E%91/%EC%97%AD/%EC%9A%94%EC%95%BD")[Hands scores of Riichi Mahjongg]
However, the specifics of each scores won't be that important.

== Requirements

=== Implementation Requirements

+ A 'Tile' type that specifies one of the bamboo tiles
+ A 'Hand' type that specifies player's hand, such that the base score can be computed from it. It cannot be a simple list of tiles because of score being dependent on the tile that was drawn last and Kan being treated as a triplet.
+ A 'HandUpgrade' type that specifies upgrades done to the possible hands (역).
+ A function that calculates the base exponent and multiplier score.
  ```fs
  baseScore: GameState -> 'HandUpgrade list -> 'Item list -> (int, int)
  ```
+ A 'Pile' type that is a list of tiles not yet drawn.
+ A 'DiscardPile' type that is a list of tiles discarded.
+ A 'DoraIndicator' type that is a list of Dora Indicator (Dora indicator)
+ A 'GameState' type that is the product type of theHand, Pile and DiscardPile.
+ An 'Item' type that specifies a specific item that modifies the score based on various game conditions, like "+1 multiplier for every 5s in the hand". Composing order for the item type shouldn't matter therefore it should have a consistent ordering.
  ```fs
  type Item = GameState -> (int, int) -> (int, int)
  ```
+ A function to calculate the actual score from the player's hand and items they have.
  ```fs
  score: Hand -> 'Item list -> bigint
  ```
+ The core imperative logic that counts scores and rounds, manages the two game states, handles game over and displays everything to the screen.
+ The randomness of the game should be only dependent on a single seed number that is set right when the game begins.

=== Game Requirements

+ There should be two phases, playing and buying.
+ In the playing phase, the player should see the items that they have, their 14 hand tiles, their score, Tsumo left, Dora Indicator and the score they have to beat to not lose.
+ In the playing phase, the player should be able to select one of the tiles or the Tsumo button to play as written above.
+ If the deck is exhausted, then the player gets some penalty and tiles are reshuffled to play the subround again.
+ If the hand contains 4 of the same tiles, then the player can declare Kan to set aside these the 4 tiles, draw another tile, reveal another Dora Indicator from the bottom of the pile and choose between Tsumo, discarding or maybe another Kan.
+ If the player presses Tsumo or Kan when they cannot, their score should be deducted.
+ After pressing Tsumo, the player should see the result screen that shows information such as added score, the base exponent and multiplier, which items affected the score, etc.
+ The game will be played in the terminal. However, if there's enough time, I might be able to make GUI of the game.
+ After set number of Tsumo, if the player did not fulfill the required score, then the game is over and the game either shuts down or goes back to the title screen.
+ If the player could fulfill the required score, then the game transitions to the buying phase where you can choose from randomly appearing list of items or hand upgrades to buy with their secondary currency. The secondary currency is earned for each Tsumo left in a round.
+ Also optionally, have a in-game score reference.
