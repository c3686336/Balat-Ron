# Balat-Ron

Balat-Ron is a Godot 4 / F# game that combines a simplified bamboo-only mahjong hand game with Balatro-style shop items. The player draws, discards, declares `KAN`, declares `TSUMO`, earns score, and buys items that create different scoring builds.

## How To Run

I will attach a pre-built exported game alongside this repository for Windows, MacOS and Linux. If you only want to play the game, use that pre-built version instead of building from source. They are attached in the "Releases" section. For the macOS version, you'll have to disable the gatekeeper.

Download and unzip the `.zip` file for your operating system:

* **Windows:** Double-click `Balat-Ron-Windows.exe` in the folder named `Balat-Ron-Windows`.
* **Linux:** Double-click `Balat-Ron-Linux` or run `./Balat-Ron-Linux` in your terminal in the folder named `Balat-Ron-Linux`
* **macOS:** Open `Balat-Ron.app` following the instructions below.

### How to Bypass Gatekeeper
Because this game is an indie project and not digitally signed by Apple, your Mac's Gatekeeper security will block it the first time you try to open it. This is completely normal. 

1. **Double-click** the game to try opening it. You will get a warning that the app is from an untrusted developer. Click **Done** or **OK**.
2. Open your Mac's **System Settings** and click on **Privacy & Security** in the left sidebar.
3. Scroll all the way down to the **Security** section. You will see a message stating the game was blocked.
4. Click the **Open Anyway** button next to it, then enter your Mac password (or use Touch ID) to confirm.

The game will now open! You only have to do this once. In the future, you can just double-click the game to play it normally. 

### Troubleshooting: "Application cannot be opened" or "Damaged"
If macOS refuses to launch the game even after clicking "Open Anyway," or if it immediately says the application cannot be opened, your extraction tool likely stripped away the file's internal execution permissions. 

You can fix this in a few seconds using the Mac Terminal:

1. Open the **Terminal** app (Press `Cmd + Space`, type "Terminal", and press Enter).
2. Type `chmod -R +x ` (make sure to leave a **space** after the `+x`).
3. Drag and drop the `Balat-Ron.app` folder from your Finder window directly into the Terminal window. This will automatically paste the correct path.
4. Press **Enter**. 

The permissions are now restored, and you can launch the game normally.

### Fallback Option
If the macOS build repeatedly fails to launch despite trying the troubleshooting steps above, please consider running the **Windows** or **Linux** version of the game instead. Those builds are less restrictive and may work better depending on your system configuration.

To run from source, install:

- Godot 4 with .NET support, not the standard non-.NET Godot build
- .NET SDK targeting `net10.0`

The source project is a Godot project. The file Godot needs to open is:

```text
project.godot
```

Steps to run from the Godot editor:

1. Open Godot.
2. Click `Import` or `Scan`, depending on the Godot project manager screen.
3. Select this repository folder, or select `project.godot` inside this repository.
4. After the project opens, Godot should use `game.tscn` as the main scene.
5. Press the play button in the top-right of the editor.

If Godot asks which scene to run, choose:

```text
game.tscn
```

The C# entry script is `Main.cs`, but you do not open or run that file directly. Godot loads `game.tscn`, which uses `Main.cs` to start the F# frontend.

If you want to compile from the command line first, run this from the repository root:

```sh
dotnet build Balat-Ron.csproj --no-restore
```

After that, open `project.godot` in Godot and run `game.tscn` as described above.

## Requirement Changes

I changed several parts of the original proposal while keeping the main idea of "Mahjongg + Balatro".

1. Removed wrong-action penalties.

The proposal said invalid `TSUMO` or `KAN` attempts would deduct score. In the final game, invalid actions are disabled or ignored instead. This made the UI easier to understand and avoided punishing players for button mistakes.

The original design also mentioned a penalty if the player declared `TSUMO` without a valid hand. I removed this because the game already has a limited number of scoring attempts per round, and accidental penalties made the game feel worse rather than more strategic. This is the biggest deviation from the requirements.

2. Changed hand upgrades/yaku into shop items.

The proposal had a separate `HandUpgrade` concept. In the final version, most yaku-like scoring bonuses are represented as items. This better matches the Balatro-style design because the player builds a scoring engine through shop choices.

## Requirement Expansions

1. Expanded the `Item` model beyond a simple score function.

The original requirement described items as something like `GameState -> (int, int) -> (int, int)`. The final item system is event-based. Items can react to actions such as discard, kan, scoring, round start, being bought, being sold, or pile exhaustion. This was necessary for items such as `Backpack`, `Dora Lantern`, `Riichi`, and `Graveyard Revival`.

2. Changed deck exhaustion behavior.

The proposal said deck exhaustion would cause a penalty and reshuffle. In the final game, an `END ROUND` button appears when the pile is empty. When pressed, the player gets the penalty of reshuffling the deck without getting any additional points from items triggered at `TSUMO`. In addition to that, some items can now interact with that moment, such as `Graveyard Revival`. This made the rule clearer and gave room for late-round items.

3. Added duplicate-item prevention and dynamic item slots.

The original proposal had a fixed item list idea. The final game prevents owning duplicate items and supports items that change maximum inventory size, such as `Backpack`. This makes build choices clearer and avoids degenerate stacking.

## LLM Attribution

1. What you used the LLM for,

I used an LLM mainly for several kinds of implementation help. First, I used it for trivial refactoring that affected a large part of the codebase, which went well. Second, I used it for frontend/view work in Godot and F#, including UI wiring, score presentation, shop display, item descriptions, and layout iteration. Third, I used it to create various tools and tests, including the balance simulator, which also went well. The animation code was also written with LLM help and then adjusted by me.

2. What you had to manually change or reprompt because the LLM did not understand your first prompt,

The main place I had to manually change or reprompt was the frontend layout. The LLM-generated layouts worked functionally, but the in-game screen did not have a coherent layout at first. I had to remake the in-game layout and adjust other screens so UI elements did not spill off the screen. I also had to manually adjust the animation code because animations were very finicky: small timing, source-position, or refresh-order mistakes caused visual glitches.

3. The main point that the LLM was not able to do correctly.

The main thing the LLM was not able to do correctly on its own was coherent visual layout. It was good at code refactoring, UI logic, connecting screens, tools, tests, and draft animation code, but it did not reliably produce a balanced Godot layout that looked good and fit within the game screen without manual adjustment. It also could not reliably get animation details right without manual tuning.

## Attributions
- UI Theme : https://softwave.itch.io/godot-retro-theme-space-worm
- Mahjong Tiles : https://natonato.itch.io/simple-tiny-mahjong-tiles

## How To Play
The game uses 36 tiles: four copies each of numbers 1 to 9. The 1 tile is shown as a bird.

There are two piles in the center. The left pile is the draw pile, and the right pile is the reserve pile.

One tile from the reserve pile is revealed. This is the Dora. When scoring, each tile in your hand that is one number higher than the Dora gives +1 han. If the Dora is 9, each 1 tile gives +1 han instead.

Your hand is at the bottom of the screen. Tap a tile to discard it and draw a new tile.

If you have four identical tiles, you can press the Kan button to set them aside. You cannot discard from them, and they are forced to be evaluated as three identical tiles. After a Kan, you draw a special tile from the reserve pile.

Your goal is to make a complete hand: four groups and one pair. A group can be three identical tiles or three tiles in a row.

When your hand is complete, the Tsumo button lights up. You can press it to score, or ignore it and keep playing.

Scoring uses han and fu. The score is 6 × fu × 2^(2 + han).

After scoring, all tiles are shuffled and play continues.

Each round has a limited number of Tsumo declarations. Reach the target score before you run out, or you lose.

If you reach the target score, you go to the shop. You can buy or sell items with gold. You earn more gold when you finish with more Tsumo declarations remaining. Selling items gives 90% of their value.

## How To Play (한국어)
이 게임은 1부터 9까지의 숫자가 각각 4장씩, 총 36장의 패를 사용합니다. 1 패는 새 모양으로 표시됩니다.

화면 중앙에는 두 개의 패 더미가 있습니다. 왼쪽은 뽑기 더미, 오른쪽은 예비 더미입니다.

예비 더미에서 패 한 장이 공개되며, 이것이 '도라(Dora)'가 됩니다. 점수를 계산할 때, 손패에 도라보다 숫자가 1 높은 패가 있을 때마다 +1판(han)을 얻습니다. 만약 도라가 9라면, 대신 1 패가 있을 때마다 +1판을 얻습니다.

플레이어의 손패는 화면 하단에 표시됩니다. 패를 탭하면 해당 패를 버리고 새로운 패를 뽑습니다.

똑같은 패를 4장 가지고 있다면, '깡(Kan)' 버튼을 눌러 따로 빼놓을 수 있습니다. 이 패들은 버릴 수 없으며, 완성형을 계산할 때 똑같은 패 3장(몸통)으로 취급됩니다. 깡을 선언한 후에는 예비 더미에서 특별한 패를 한 장 뽑습니다.

최종 목표는 몸통 4개와 머리(쌍) 1개를 모아 완성된 손패를 만드는 것입니다. 몸통은 똑같은 패 3장 또는 연속된 숫자 패 3장으로 구성할 수 있습니다.

손패가 완성되면 '쯔모(Tsumo)' 버튼에 불이 들어옵니다. 버튼을 눌러 점수를 획득하거나, 이를 무시하고 플레이를 계속할 수도 있습니다.

점수 계산에는 '판(han)'과 '부(fu)'를 사용합니다. 점수는 6 × 부 × 2^(2 + 판)으로 계산됩니다.

점수 계산이 끝나면 모든 패를 다시 섞고 플레이가 계속됩니다.

각 라운드마다 선언할 수 있는 쯔모 횟수가 제한되어 있습니다. 제한 횟수가 모두 소진되기 전에 목표 점수에 도달해야 하며, 그러지 못하면 패배합니다.

목표 점수에 도달하면 상점으로 이동합니다. 상점에서는 골드를 사용하여 아이템을 구매하거나 판매할 수 있습니다. 라운드가 끝났을 때 남은 쯔모 선언 횟수가 많을수록 더 많은 골드를 획득합니다. 아이템을 판매할 때는 원래 가치의 90%를 받습니다.

## Items

`han` means score multiplier. `fu` means base score. A run is three consecutive numbers, such as `2-3-4`. A triplet is three matching tiles. A quad is four matching tiles declared with `KAN`. A pair is two matching tiles.

| Item | English Description | Korean Translation |
|---|---|---|
| Tanyao (탕야오) | Gain +1 han (score multiplier) if your winning hand contains no 1 or 9 tiles. | 완성한 손패에 1이나 9가 하나도 없으면 +1판을 얻습니다. 판은 점수 배율입니다. |
| Pinfu (핑후) | Gain +1 han (score multiplier) and +20 fu (base score) if your winning hand has four three-number runs, one pair, and wins by completing a run from either side. | 완성한 손패가 숫자 3개짜리 연속 묶음 4개와 같은 패 2개짜리 한 쌍으로 이루어져 있고, 양쪽에서 이어질 수 있는 연속 묶음을 완성하며 이겼다면 +1판과 +20부를 얻습니다. 부는 기본 점수입니다. |
| Iipeikou (이페코) | Gain +1 han (score multiplier) and +10 fu (base score) if your winning hand has two identical three-number runs, such as two 2-3-4 runs. | 완성한 손패에 같은 숫자 연속 묶음이 두 개 있으면 +1판과 +10부를 얻습니다. 예: 2-3-4가 두 묶음. |
| Ryanpeikou (량페코) | Gain +4 han (score multiplier) if your winning hand has two different pairs of identical three-number runs. | 완성한 손패에 서로 다른 같은 연속 묶음 쌍이 두 종류 있으면 +4판을 얻습니다. |
| Ittsu (일기통관) | Gain +3 han (score multiplier) if your winning hand has the three runs 1-2-3, 4-5-6, and 7-8-9. | 완성한 손패에 1-2-3, 4-5-6, 7-8-9 연속 묶음이 모두 있으면 +3판을 얻습니다. |
| Sanankou (산안커) | Gain +2 han (score multiplier) if your winning hand has at least three triplets or quads. A triplet is three matching tiles; a quad is four matching tiles declared with KAN. | 완성한 손패에 같은 패 3개짜리 묶음 또는 KAN으로 선언한 같은 패 4개짜리 묶음이 합쳐서 3개 이상 있으면 +2판을 얻습니다. |
| Sankantsu (산깡쯔) | Gain +4 han (score multiplier) if your winning hand has at least three declared quads. A quad is four matching tiles declared with KAN. | 완성한 손패에 KAN으로 선언한 같은 패 4개짜리 묶음이 3개 이상 있으면 +4판을 얻습니다. |
| Chitoitsu (치또이츠) | Gain +3 han (score multiplier) if your winning hand is exactly seven different pairs. A pair is two matching tiles. | 완성한 손패가 서로 다른 같은 패 2개짜리 쌍 7개로만 이루어져 있으면 +3판을 얻습니다. |
| Suuankou (스안커) | Gain +6 han (score multiplier) if your winning hand has four triplets or quads. A triplet is three matching tiles; a quad is four matching tiles declared with KAN. | 완성한 손패에 같은 패 3개짜리 묶음 또는 KAN으로 선언한 같은 패 4개짜리 묶음이 합쳐서 4개 있으면 +6판을 얻습니다. |
| Sukantsu (스깡쯔) | Gain +8 han (large score multiplier) if your winning hand has four declared quads. A quad is four matching tiles declared with KAN. | 완성한 손패에 KAN으로 선언한 같은 패 4개짜리 묶음이 4개 있으면 +8판을 얻습니다. |
| SuuankouTanki (스안커 단기) | Gain +8 han (large score multiplier) if your winning hand has four triplets or quads and the winning tile completes the final pair of two matching tiles. | 완성한 손패에 같은 패 3개짜리 묶음 또는 4개짜리 묶음이 4개 있고, 이긴 패가 마지막 같은 패 2개짜리 쌍을 완성했다면 +8판을 얻습니다. |
| Trash to Treasure | Gain +10 fu (base score) whenever you discard a 1 or 9 tile. | 1 또는 9 패를 버릴 때마다 +10부를 얻습니다. |
| Graveyard Revival | When the draw pile runs out, shuffle all discarded tiles back into the draw pile. Breaks after use. | 뽑을 패 더미가 비었을 때, 버린 패를 모두 다시 섞어서 뽑을 패 더미로 만듭니다. 한 번 사용하면 파괴됩니다. |
| Last Draw Gambit | Gain +2 han (score multiplier) if you win with the TSUMO button on your final allowed win attempt of the round. | 라운드에서 마지막으로 허용된 TSUMO 승리 시도에서 이기면 +2판을 얻습니다. |
| Dora Lantern | At the start of each hand, reveal 1 extra dora indicator, which can make more tiles count as score bonuses. Breaks after 3 hands. | 각 손패가 시작될 때 도라 표시패를 1개 더 공개합니다. 도라 표시패는 추가 점수를 주는 패를 정합니다. 3번의 손패 후 파괴됩니다. |
| Even Chorus | Gain +10 fu (base score) for each even-numbered tile in your winning hand. | 완성한 손패의 짝수 패 하나당 +10부를 얻습니다. |
| Odd Chorus | Gain +10 fu (base score) for each odd-numbered tile in your winning hand. | 완성한 손패의 홀수 패 하나당 +10부를 얻습니다. |
| Terminal Applause | Gain +20 fu (base score) for each 1 or 9 tile in your winning hand. | 완성한 손패의 1 또는 9 패 하나당 +20부를 얻습니다. |
| Rinshankaihou (영상개화) | Gain +3 han (score multiplier) if you win immediately after pressing KAN and drawing the replacement tile. | KAN을 누르고 보충 패를 뽑은 직후 바로 이기면 +3판을 얻습니다. |
| Tenhou (천화) | Gain +3 han (score multiplier) if you win before discarding any tile this hand. | 이번 손패에서 어떤 패도 버리기 전에 이기면 +3판을 얻습니다. |
| Modular Sequence | Allows three-number runs to wrap around from 9 back to 1, so 8-9-1 and 9-1-2 can count as runs. These wrapped runs do not trigger bonuses for completing 1-2-3 or 7-8-9 from the outside. | 숫자 연속 묶음이 9에서 1로 이어질 수 있게 합니다. 그래서 8-9-1과 9-1-2도 연속 묶음으로 인정됩니다. 단, 이런 순환 묶음은 1-2-3이나 7-8-9를 바깥쪽에서 완성하는 보너스를 발동시키지 않습니다. |
| Dora Collector | Gain +5 fu (base score) for each revealed dora indicator. Dora indicators mark which tiles give bonus score. | 공개된 도라 표시패 하나당 +5부를 얻습니다. 도라 표시패는 어떤 패가 추가 점수를 주는지 정합니다. |
| Red Indicator | Gain +1 han (score multiplier) if at least 3 dora indicators are revealed. Dora indicators mark which tiles give bonus score. | 공개된 도라 표시패가 3개 이상이면 +1판을 얻습니다. 도라 표시패는 어떤 패가 추가 점수를 주는지 정합니다. |
| Dead Wall Miner | The next 2 times you press KAN, reveal 1 extra dora indicator. Dora indicators mark bonus-score tiles. Breaks after the second use. | 다음 2번 KAN을 누를 때마다 도라 표시패를 1개 더 공개합니다. 도라 표시패는 추가 점수를 주는 패를 정합니다. 두 번째 사용 후 파괴됩니다. |
| Kan Tax | Gain +20 fu (base score) whenever you press KAN to declare four matching tiles. | KAN을 눌러 같은 패 4개를 선언할 때마다 +20부를 얻습니다. |
| Quad Market | Gain 50 gold whenever you press KAN to declare four matching tiles. | KAN을 눌러 같은 패 4개를 선언할 때마다 50골드를 얻습니다. |
| Backpack | Increase your maximum item slots by 2 while held. Since Backpack uses one slot, this gives 1 extra usable item slot. | 보유 중 최대 아이템 칸을 2칸 늘립니다. Backpack 자체가 1칸을 차지하므로 실제로는 사용할 수 있는 칸이 1칸 늘어납니다. |
| Fourfold Path | Gain +2 han (score multiplier) if your winning hand has at least 2 declared quads. A quad is four matching tiles declared with KAN. | 완성한 손패에 KAN으로 선언한 같은 패 4개짜리 묶음이 2개 이상 있으면 +2판을 얻습니다. |
| Patience | Gain +1 han (score multiplier) if you discarded at least 3 tiles before winning this hand. | 이번 손패에서 이기기 전에 패를 3개 이상 버렸다면 +1판을 얻습니다. |
| Deep Wall | Gain +2 han (score multiplier) if fewer than 10 tiles remain in the draw pile when you win. | 이길 때 뽑을 패 더미에 남은 패가 10개 미만이면 +2판을 얻습니다. |
| Chanta (찬타) | Gain +4 han (score multiplier) if every group in your winning hand includes a 1 or 9 tile. Groups are runs, triplets, quads, and the pair. | 완성한 손패의 모든 묶음에 1 또는 9 패가 포함되어 있으면 +4판을 얻습니다. 묶음은 연속 묶음, 같은 패 3개, 같은 패 4개, 그리고 같은 패 2개짜리 쌍을 뜻합니다. |
| Edge Lord | Gain +1 han (score multiplier) if your winning tile completes 1-2-3 with the 3, or 7-8-9 with the 7. | 이긴 패가 3이어서 1-2-3을 완성하거나, 7이어서 7-8-9를 완성했다면 +1판을 얻습니다. |
| Sacred Ends | Gain +1 han (score multiplier) and +10 fu (base score) if the tile you win with is 1 or 9. | 이긴 패가 1 또는 9라면 +1판과 +10부를 얻습니다. |
| Middle Road | Gain +5 fu (base score) for each 4, 5, or 6 tile in your winning hand. | 완성한 손패의 4, 5, 6 패 하나당 +5부를 얻습니다. |
| Perfect Flow | Gain +1 han (score multiplier) and +20 fu (base score) if all 4 sets in your winning hand are three-number runs. | 완성한 손패의 4개 묶음이 모두 숫자 3개짜리 연속 묶음이면 +1판과 +20부를 얻습니다. |
| Open Road | Gain +10 fu (base score) for each three-number run in your winning hand. | 완성한 손패의 숫자 3개짜리 연속 묶음 하나당 +10부를 얻습니다. |
| Modular Payoff | Gain +2 han (score multiplier) if your winning hand uses an 8-9-1 or 9-1-2 wrap-around run. | 완성한 손패에 8-9-1 또는 9-1-2 순환 연속 묶음이 있으면 +2판을 얻습니다. |
| Pair Collector | Gain +5 fu (base score) for each different pair of matching tiles in your winning hand. | 완성한 손패의 서로 다른 같은 패 2개짜리 쌍 하나당 +5부를 얻습니다. |
| Seven Mirrors | Gain +2 han (score multiplier) if your winning hand is exactly seven different pairs. | 완성한 손패가 서로 다른 같은 패 2개짜리 쌍 7개로만 이루어져 있으면 +2판을 얻습니다. |
| Heavy Hand | Gain +1 han (score multiplier) for every 100 fu (base score) already added by item effects this hand. | 이번 손패에서 아이템 효과로 이미 얻은 부가 100부일 때마다 +1판을 얻습니다. |
| Riichi (리치) | When you win with the TSUMO button, the round does not spend one of your allowed TSUMO attempts. You must discard before using TSUMO again. | TSUMO 버튼으로 이겨도 라운드의 허용된 TSUMO 시도 횟수가 줄어들지 않습니다. 다시 TSUMO를 사용하려면 먼저 패를 버려야 합니다. |
