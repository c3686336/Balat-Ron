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
