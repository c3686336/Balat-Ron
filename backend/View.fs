module View

open Godot
open System
open System.Globalization
open System.Collections.Generic

let mutable private root: Control = null
let tileTextures = Dictionary<int, Texture2D>()

let init (r: Control) =
    root <- r
    for v in 1..9 do
        tileTextures[v] <- GD.Load<Texture2D>($"res://mahjong_tiles/Bamboo{v}.png")

let getRoot() = root
let private tex v = tileTextures[v]
let private ura () = GD.Load<Texture2D>("res://Ura.png")

// ── Phase ──
let phaseControls = Dictionary<string, Control>()
let cachePhases () =
    for n in ["MainMenu"; "InGame"; "ScorePresentation"; "Shop"; "GameOver"] do
        phaseControls[n] <- root.GetNode<Control>(n)

let showPhase name =
    for kv in phaseControls do kv.Value.Hide()
    if phaseControls.ContainsKey name then phaseControls[name].Show()

// ── Helpers ──
let clear (parent: Node) =
    for c in parent.GetChildren() do parent.RemoveChild(c); c.QueueFree()

let tileBtnSize = Vector2(20f, 28f)

let private scoreScientificThreshold = 1000000000I

let private formatScore (score: bigint) =
    if score >= scoreScientificThreshold then
        (float score).ToString("0.###e+0", CultureInfo.InvariantCulture)
    else
        score.ToString()

let mkTexBtn tileValue =
    let b = new TextureButton()
    b.TextureNormal <- tex tileValue
    b.SetMeta("tile_value", tileValue)
    b.StretchMode <- TextureButton.StretchModeEnum.KeepAspectCentered
    b.Set("expand_icon", true)
    b.CustomMinimumSize <- tileBtnSize
    b

let mkTexRect t =
    let r = new TextureRect()
    r.Texture <- t
    r.CustomMinimumSize <- tileBtnSize
    r.Size <- tileBtnSize
    r

let mkWallBtn () =
    let b = new TextureButton()
    b.TextureNormal <- ura()
    b.StretchMode <- TextureButton.StretchModeEnum.KeepAspectCentered
    b.Set("expand_icon", true)
    b.CustomMinimumSize <- tileBtnSize
    b

// ── Hand render ──
let refreshHand (hand: Types.Hand) =
    let (Types.Hand (arr, tsumo, kantsu)) = hand
    let hc = root.GetNode<HBoxContainer>("InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer")
    clear hc

    // Standard spacing, no fan overlap
    hc.AddThemeConstantOverride("separation", 2)

    for v in 1..9 do
        for _ in 1 .. arr[v] do
            hc.AddChild(mkTexBtn v)

    // Tsumo tile: visual distinction with 20px gap
    // 2px from separation + 16px spacer + 2px from separation = 20px total horizontal gap
    let sep = new Control()
    sep.CustomMinimumSize <- Vector2(16f, 0f)
    hc.AddChild(sep)

    let tb = mkTexBtn (tsumo.Value())
    hc.AddChild(tb)

    // Kans
    let kc = root.GetNode<HBoxContainer>("InGame/Kans/MarginContainer/KanContainer")
    clear kc
    let u = ura()
    for (Types.Kantsu (Types.Tile v)) in List.rev kantsu do
        kc.AddChild(mkTexRect (tex v))
        for _ in 1..2 do kc.AddChild(mkTexRect u)
        kc.AddChild(mkTexRect (tex v))
        let gap = new Control()
        gap.CustomMinimumSize <- Vector2(8f, 0f)
        kc.AddChild(gap)

// ── Pile render ──
// Draw order: rightmost TopRow -> rightmost BottomRow -> next-left TopRow -> next-left BottomRow -> ...
// Both rows use RTL, so GetChildren[0] is the rightmost tile.
let refreshPile (remaining: int) (rinshangRemaining: int) (dora: Types.Pile) =
    let showCol = Color(1f, 1f, 1f, 1f)
    let hideCol = Color(1f, 1f, 1f, 0f)
    let deadWallTilesPerRow = 7
    let rinshangTileCount = 4
    let firstDoraIndex = 2

    let sepIndex (row: HBoxContainer) =
        row.GetChildren()
        |> Seq.tryFindIndex (fun c -> c.Name.ToString().Contains("DoraSeparater"))
        |> Option.defaultValue 5

    let postSepButtons (row: HBoxContainer) =
        let all = row.GetChildren()
        let sepIdx = sepIndex row
        all
        |> Seq.skip (sepIdx + 1)
        |> Seq.choose (fun c -> match c with :? TextureButton as b -> Some b | _ -> None)
        |> Seq.toArray

    let ensureDeadWallSlots (row: HBoxContainer) =
        let mutable post = postSepButtons row
        while post.Length < deadWallTilesPerRow do
            row.AddChild(mkWallBtn())
            post <- postSepButtons row

    let getPreSepChildren (row: HBoxContainer) =
        let all = row.GetChildren()
        let sepIdx = sepIndex row
        all |> Seq.take sepIdx |> Seq.choose (fun c -> match c with :? TextureButton as b -> Some b | _ -> None) |> Seq.toArray

    let top = root.GetNode<HBoxContainer>("InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/TopRow")
    let bot = root.GetNode<HBoxContainer>("InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/BottomRow")
    ensureDeadWallSlots top
    ensureDeadWallSlots bot

    let topPre = getPreSepChildren top
    let botPre = getPreSepChildren bot

    // Post-separator (dead wall) always visible
    let refreshDeadWall () =
        let topPost = postSepButtons top
        let botPost = postSepButtons bot
        let rinshangDrawOrder =
            [| if topPost.Length > 0 then yield topPost[0]
               if botPost.Length > 0 then yield botPost[0]
               if topPost.Length > 1 then yield topPost[1]
               if botPost.Length > 1 then yield botPost[1] |]
        let consumedRinshang = rinshangTileCount - rinshangRemaining |> max 0 |> min rinshangTileCount

        for i in 0 .. topPost.Length - 1 do
            topPost[i].Modulate <- showCol
            let doraIndex = i - firstDoraIndex
            topPost[i].TextureNormal <-
                if doraIndex >= 0 && doraIndex < dora.Length then tex (dora[doraIndex].Value()) else ura()

        for b in botPost do
            b.Modulate <- showCol
            b.TextureNormal <- ura()

        for i in 0 .. rinshangDrawOrder.Length - 1 do
            rinshangDrawOrder[i].TextureNormal <- ura()
            rinshangDrawOrder[i].Modulate <- if i < consumedRinshang then hideCol else showCol

    let len = min topPre.Length botPre.Length

    // Interleave: rightmost Bot [0], rightmost Top [0], next Bot [1], next Top [1], ...
    let order = ResizeArray<TextureButton>(len * 2)
    for i in 0 .. len - 1 do
        order.Add(botPre[i])
        order.Add(topPre[i])

    for i in 0 .. order.Count - 1 do
        order[i].Modulate <- if i < remaining then showCol else hideCol

    refreshDeadWall()

// ── Discard ──
let refreshDiscard (tiles: Types.Tile array) =
    let g = root.GetNode<GridContainer>("InGame/Discard/DiscardContainer")
    clear g
    for t in tiles do
        g.AddChild(mkTexRect (tex (t.Value())))

// ── Items ──
let private itemDetailTitle (item: Types.Item) =
    $"{item.name}  [{item.rarity}]"

let private itemShopDetailTitle (action: string) (item: Types.Item) =
    $"{action} {item.name}  [{item.rarity}]"

let private showInGameItemDetail (item: Types.Item) =
    let panel = root.GetNode<Control>("InGame/ItemDetailPanel")
    panel.Visible <- true
    root.GetNode<Label>("InGame/ItemDetailPanel/MarginContainer/VBoxContainer/ItemDetailTitle").Text <- itemDetailTitle item
    root.GetNode<Label>("InGame/ItemDetailPanel/MarginContainer/VBoxContainer/ItemDetailDescription").Text <- item.description

let private hideInGameItemDetail () =
    root.GetNode<Control>("InGame/ItemDetailPanel").Visible <- false

let private setShopItemDetail (title: string) (description: string) =
    root.GetNode<Label>("Shop/ShopPanel/ShopDetailPanel/MarginContainer/VBoxContainer/ItemDetailTitle").Text <- title
    root.GetNode<Label>("Shop/ShopPanel/ShopDetailPanel/MarginContainer/VBoxContainer/ItemDetailDescription").Text <- description

let refreshItems (items: Types.Item list) =
    let box = root.GetNode<VBoxContainer>("InGame/VBoxContainer/ItemsContainer/MarginContainer/PanelContainer/MarginContainer/VBoxContainer/ItemScroll/VBoxContainer")
    clear box
    hideInGameItemDetail()
    for item in items do
        let b = new Button()
        b.Text <- item.name
        b.CustomMinimumSize <- Vector2(104f, 0f)
        b.add_MouseEntered(fun () -> showInGameItemDetail item)
        b.add_MouseExited(fun () -> hideInGameItemDetail())
        box.AddChild(b)

// ── Info ──
let refreshInfo (round: int) (gold: int) (honbaLeft: int) (baseScore: int * int) =
    root.GetNode<Label>("InGame/InfoContainer/MarginContainer/VBoxContainer/RoundLabel").Text <- $"-- ROUND {round} --"
    root.GetNode<Label>("InGame/InfoContainer/MarginContainer/VBoxContainer/GoldLabel").Text <- $"Gold: {gold}"
    root.GetNode<Label>("InGame/InfoContainer/MarginContainer/VBoxContainer/TsumoLabel").Text <- $"{honbaLeft} tsumo left."
    let (h, f) = baseScore
    root.GetNode<Label>("InGame/InfoContainer/MarginContainer/VBoxContainer/ExtraScoreLabel").Text <- $"+ {h} han\n+ {f} fu"

// ── Score label ──
let refreshScore (score: bigint) (goal: bigint) =
    root.GetNode<Label>("InGame/VBoxContainer/ScorePanel/MarginContainer/HBoxContainer/ScoreLabel").Text <- $"Score: {score} / {goal}"

// ── Button states ──
let refreshButtonStates (state: Types.GameState) =
    let kanBtn = root.GetNode<Button>("InGame/ButtonContainer/KanButton")
    let tsumoBtn = root.GetNode<Button>("InGame/ButtonContainer/TsumoButton")
    let confirmPileEmptyBtn = root.GetNode<Button>("InGame/ButtonContainer/ConfirmPileEmptyButton")
    let pileEmpty = GameState.isPileEmpty state
    kanBtn.Disabled <- not (GameState.canAnyKan state)
    tsumoBtn.Disabled <- not (GameState.canTsumo state)
    confirmPileEmptyBtn.Visible <- pileEmpty
    confirmPileEmptyBtn.Disabled <- not pileEmpty

// ── Full InGame refresh ──
let refreshInGame (state: Types.GameState) =
    refreshHand state.hand
    refreshPile state.pile.Length state.rinshang.Length state.dora
    refreshDiscard state.discardPile
    refreshItems state.items
    refreshInfo state.round state.gold state.honbaLeft state.baseScore
    refreshScore state.currentScore state.goalScore
    refreshButtonStates state

// ── Shop ──
let refreshShop (state: Types.GameState) =
    root.GetNode<Label>("Shop/ShopPanel/GoldLabel").Text <- $"Gold: {state.gold}  ·  Items: {state.items.Length}/{state.maxItems}"
    setShopItemDetail "Hover an item" "Item descriptions appear here."
    let bl = root.GetNode<VBoxContainer>("Shop/ShopPanel/ShopItemList")
    clear bl
    for i in 0 .. state.shopItems.Length - 1 do
        let item = state.shopItems[i]
        let owned = state.items |> List.exists (fun x -> x.name = item.name)
        let b = new Button()
        b.SetMeta("item_idx", i)
        b.Text <- if owned then $"OWNED {item.name}" else $"BUY {item.name} ({item.cost}G)"
        b.add_MouseEntered(fun () -> setShopItemDetail (itemShopDetailTitle (if owned then "OWNED" else $"BUY {item.cost}G") item) item.description)
        bl.AddChild(b)
    let sl = root.GetNode<VBoxContainer>("Shop/ShopPanel/PlayerItemList")
    clear sl
    for i in 0 .. state.items.Length - 1 do
        let item = state.items[i]
        let b = new Button()
        b.SetMeta("sell_idx", i)
        b.Text <- $"SELL {item.name} (+{Config.discount item.cost}G)"
        b.add_MouseEntered(fun () -> setShopItemDetail (itemShopDetailTitle $"SELL +{Config.discount item.cost}G" item) item.description)
        sl.AddChild(b)

let refreshGameOver (state: Types.GameState) (seed: string) =
    root.GetNode<Label>("GameOver/PanelContainer/MarginContainer/GameOverPanel/FinalRoundLabel").Text <- $"Reached round {state.round}"
    root.GetNode<Label>("GameOver/PanelContainer/MarginContainer/GameOverPanel/SeedLabel").Text <- $"Seed: {seed}"

// ── Score breakdown ──
let showScoreBreakdown (log: Types.GameEvents list) (state: Types.GameState) (isEmptyPileEnd: bool) =
    let el = root.GetNode<VBoxContainer>("ScorePresentation/ScorePanel/ScoreBreakdown/ExtraScoreList")
    let dl = root.GetNode<Label>("ScorePresentation/ScorePanel/ScoreBreakdown/DoraLabel")
    let title = root.GetNode<Label>("ScorePresentation/ScorePanel/ScoringTitle")
    title.Text <- if isEmptyPileEnd then "Round End" else "TSUMO!"
    clear el
    dl.Text <- "Dora: +0"
    let (han, fu) = state.baseScore
    let addedScore =
        log
        |> List.tryPick (function | Types.Scored (_, _, score) -> Some score | _ -> None)
        |> Option.defaultValue 0I
    root.GetNode<Label>("ScorePresentation/ScorePanel/ScoreBreakdown/HanFuLabel").Text <- $"{han} han  ·  {fu} fu"
    root.GetNode<Label>("ScorePresentation/ScorePanel/FinalScoreLabel").Text <- $"+{formatScore addedScore} points"
    root.GetNode<Label>("ScorePresentation/ScorePanel/GoalProgress").Text <- $"{formatScore state.currentScore} / {formatScore state.goalScore}"
    for e in log do
        match e with
        | Types.EarnedExtraScore (h, f, Types.Dora) -> dl.Text <- $"Dora: +{h}"
        | Types.EarnedExtraScore (h, f, Types.ScoreReason.ItemEffect item) ->
            let l = new Label()
            l.Text <- $"+{h} han, +{f} fu ({item.name})"
            l.HorizontalAlignment <- Godot.HorizontalAlignment.Center
            el.AddChild(l)
        | _ -> ()

// ── Animations ──
let private tryControl (path: string) =
    let nodePath = new NodePath(path)
    if root <> null && root.HasNode(nodePath) then
        Some(root.GetNode<Control>(nodePath))
    else
        None

let private centerOf (c: Control) =
    c.GlobalPosition + c.Size / 2.0f

let private tileTopLeftFromCenter (center: Vector2) =
    center - tileBtnSize / 2.0f

let private controlTileTopLeft (c: Control) =
    tileTopLeftFromCenter (centerOf c)

let private controlsAt path =
    match tryControl path with
    | Some parent ->
        parent.GetChildren()
        |> Seq.choose (fun c -> match c with :? Control as ctrl -> Some ctrl | _ -> None)
        |> Seq.toArray
    | None -> [||]

let private textureButtonsAt path =
    controlsAt path
    |> Array.choose (fun c -> match c with :? TextureButton as b -> Some b | _ -> None)

let private handTileTopLeft (tile: Types.Tile) =
    textureButtonsAt "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer"
    |> Array.tryFind (fun b -> b.HasMeta("tile_value") && b.GetMeta("tile_value").AsInt32() = tile.Value())
    |> Option.map controlTileTopLeft

let mutable private nextHandSource: (Vector2 * Control option) option = None
let mutable private nextKanSources: (Vector2 * Control option) list = []
let mutable private previousSortSource: (Types.Tile * Vector2 * Vector2 * Control) list option = None
let private flyingTiles = ResizeArray<Control>()
let private handInsertedTileWidth = tileBtnSize.X - 2.0f

let private easeTween (t: Tween) =
    t.SetTrans(Tween.TransitionType.Quad) |> ignore
    t.SetEase(Tween.EaseType.InOut) |> ignore

let private animateMinWidth (c: Control) (width: float) (duration: float) =
    let t = root.CreateTween()
    easeTween t
    t.TweenProperty(c, "custom_minimum_size:x", width, duration) |> ignore

let private collapseHandControl (c: Control) =
    let t = root.CreateTween()
    easeTween t
    t.TweenProperty(c, "custom_minimum_size:x", 0.0, 0.34) |> ignore
    t.TweenCallback(Callable.From(Action(fun () -> c.Hide()))) |> ignore

let private hideAndReturn (source: Control) =
    let pos = controlTileTopLeft source
    source.CustomMinimumSize <- tileBtnSize
    source.Modulate <- Color(1f, 1f, 1f, 0f)
    pos, Some source

let setNextHandAnimationSource (source: Control) =
    nextHandSource <- Some(hideAndReturn source)

let private takePreviousTsumo () =
    let value = previousSortSource
    previousSortSource <- None
    value

let setNextKanAnimationSources (tile: Types.Tile) (source: Control) =
    let matching =
        textureButtonsAt "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer"
        |> Array.filter (fun b -> b.HasMeta("tile_value") && b.GetMeta("tile_value").AsInt32() = tile.Value())
        |> Array.sortBy (fun b -> b.GlobalPosition.X)
        |> Array.toList

    let selected =
        match matching with
        | [] -> [source]
        | xs ->
            let sourcePos = controlTileTopLeft source
            let withoutSource = xs |> List.filter (fun b -> controlTileTopLeft b <> sourcePos)
            source :: (withoutSource |> List.map (fun b -> b :> Control)) |> List.truncate 4

    let sourcePos = controlTileTopLeft source
    let positions =
        selected
            |> List.map
                (fun s ->
                 let empty = new Control()
                 let topLeft = controlTileTopLeft s
                 empty.Position <- s.Position
                 empty.CustomMinimumSize <- s.Size
                 s.ReplaceBy(empty)
                 topLeft, Some(empty))

    let padded =
        if positions.Length >= 4 then positions
        else positions @ List.replicate (4 - positions.Length) (sourcePos, None)

    nextKanSources <- padded |> List.truncate 4

let private takeHandSource fallback =
    match nextHandSource with
    | Some (pos, source) ->
        nextHandSource <- None
        pos
    | None -> fallback

let private takeKanSources fallback =
    match nextKanSources with
    | [] -> List.replicate 4 fallback
    | sources ->
        nextKanSources <- []
        for (_, source) in sources do
            source |> Option.iter collapseHandControl
        let positions = sources |> List.map fst
        if positions.Length >= 4 then positions
        else positions @ List.replicate (4 - positions.Length) fallback

let private lastHandTileControl () =
    textureButtonsAt "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer"
    |> Array.tryLast // Returns the Tsumo piece's controller

let private lastHandTileTopLeft () =
    lastHandTileControl()
    |> Option.map controlTileTopLeft

let private currentDiscardSourceControl () =
    match nextHandSource with
    | Some (_, source) -> source
    | None -> None

let captureSortingMovesForDrawAnimation =
    let buttons =
        textureButtonsAt "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer"
        // |> fun a -> Option.fold (fun s v -> Array.insertAt 0 v s) a (lastHandTileControl())
        |> Array.sortBy (fun b -> b.GlobalPosition.X)
    let firstPos =
        buttons
            |> Array.tryHead
            |> Option.map controlTileTopLeft
            |> Option.defaultValue (Vector2(143.0f, 356.0f))
    // let firstPos = Vector2(143.0f, 356.0f)

    previousSortSource <-
      buttons
          |> Array.sortBy (fun b -> b.GetMeta("tile_value").AsInt32())
          |> Array.mapi (fun i x ->
                         let tile = Types.Tile (x.GetMeta("tile_value").AsInt32())
                         let fromPos = controlTileTopLeft x |> fun x -> Vector2(x.X, 356.f)
                         let toPos = firstPos + Vector2((float32 i) * (tileBtnSize.X + 2.0f), 0f)
                         (tile, fromPos, toPos, x :> Control))
          |> Array.toList
          |> Some

let captureCurrentTsumoForDrawAnimation (hand: Types.Hand) (discarded: Types.Tile option) =
    let (Types.Hand (arr, tsumo, _)) = hand
    let isTsumoGiri = currentDiscardSourceControl() |> Option.exists(fun s -> lastHandTileControl() |> Option.exists(fun b -> Object.ReferenceEquals(s, b)))
    match discarded, lastHandTileControl() with
    | Some discardedTile, _ when discardedTile = tsumo && isTsumoGiri ->
        previousSortSource <- None
    | _, Some tsumoControl ->
        let pos = controlTileTopLeft tsumoControl
        let sourceControl = currentDiscardSourceControl()
        let adjusted = Array.copy arr
        match discarded with
        | Some discardedTile ->
            let value = discardedTile.Value()
            adjusted[value] <- max 0 (adjusted[value] - 1)
        | None -> ()
        let buttons =
            textureButtonsAt "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer"
            |> Array.sortBy (fun b -> b.GlobalPosition.X)
        let firstPos =
            buttons
            |> Array.tryHead
            |> Option.map controlTileTopLeft
            |> Option.defaultValue pos
        let finalCounts = Array.copy adjusted
        finalCounts[tsumo.Value()] <- finalCounts[tsumo.Value()] + 1
        let controlsForValue value =
            let standard =
                buttons
                |> Array.choose (fun b ->
                    let isSource = sourceControl |> Option.exists (fun s -> Object.ReferenceEquals(s, b))
                    let isTsumo = Object.ReferenceEquals(tsumoControl, b)
                    if not isSource && not isTsumo && b.GetMeta("tile_value").AsInt32() = value then
                        Some(b :> Control)
                    else
                        None)
                |> Array.toList
            if value = tsumo.Value() then standard @ [tsumoControl :> Control] else standard
        let controlQueues =
            [| for value in 0 .. 9 -> ResizeArray<Control>(controlsForValue value) |]
        let moves =
            [ for value in 1 .. 9 do
                for _ in 1 .. finalCounts[value] do
                    let slotIndex =
                        [1 .. value - 1] |> List.sumBy (fun i -> finalCounts[i])
                        |> fun before -> before + (finalCounts[value] - controlQueues[value].Count)
                    if controlQueues[value].Count > 0 then
                        let c = controlQueues[value][0]
                        controlQueues[value].RemoveAt(0)
                        let fromPos = controlTileTopLeft c |> fun x -> Vector2(x.X, max 356.f x.Y) // TODO: REMOVE THIS DIRTY HACK! It's done to reject starting positions if the tile is nudged upwards due to hover.
                        let toPos = firstPos + Vector2(float32 slotIndex * (tileBtnSize.X + 2.0f), 0f) |> fun x -> Vector2(x.X, max 356.f x.Y)
                        if abs (fromPos.X - toPos.X) > 0.1f || abs (fromPos.Y - toPos.Y) > 0.1f then
                            yield Types.Tile value, fromPos, toPos, c ]
        let tsumoToPos =
            moves
            |> List.tryPick (fun (_, _, toPos, c) -> if Object.ReferenceEquals(c, tsumoControl) then Some toPos else None)
            |> Option.defaultValue pos
        previousSortSource <- Some(moves)
    | _, None -> previousSortSource <- None

let private sortedHandSlotTopLeft (tile: Types.Tile) (oldArr: int array) =
    let before =
        [1 .. tile.Value() - 1]
        |> List.sumBy (fun i -> oldArr[i])
    let sameTileCount = oldArr[tile.Value()]
    let index = before + sameTileCount
    match textureButtonsAt "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer" with
    | buttons when buttons.Length > 0 ->
        let first = buttons |> Array.minBy (fun b -> b.GlobalPosition.X)
        Some(controlTileTopLeft first + Vector2(float32 index * (tileBtnSize.X + 2.0f), 0f))
    | _ -> None

let private liveWallSource () =
    let top = textureButtonsAt "InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/TopRow" |> Array.truncate 5 |> Array.filter (fun x -> x.Modulate.A > 0.1f)
    let bottom = textureButtonsAt "InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/BottomRow" |> Array.truncate 5 |> Array.filter (fun x -> x.Modulate.A > 0.1f)

    if top.Length = 0 && bottom.Length = 0 then
        None
    else if top.Length >= bottom.Length then
        Some(top[top.Length - 1])
    else
        Some(bottom[bottom.Length - 1])

let private rinshangSource () =
    let bottom = textureButtonsAt "InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/BottomRow" |> Array.skip 5
    let top = textureButtonsAt "InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/TopRow" |> Array.skip 5
    [| if top.Length > 0 then yield top[0]
       if bottom.Length > 0 then yield bottom[0]
       if top.Length > 1 then yield top[1]
       if bottom.Length > 1 then yield bottom[1] |]
    |> Array.tryFind (fun b -> b.Modulate.A > 0.1f)

let private hideSourceAndGetTopLeft (source: TextureButton option) fallback =
    match source with
    | Some b ->
        let pos = controlTileTopLeft b
        b.Modulate <- Color(1f, 1f, 1f, 0f)
        pos
    | None -> fallback

let private hideSourceDelayedAndGetTopLeft (source: TextureButton option) fallback delay =
    match source with
    | Some b ->
        let pos = controlTileTopLeft b
        if delay > 0.0 then
            let t = root.CreateTween()
            t.TweenInterval(delay) |> ignore
            t.TweenCallback(Callable.From(Action(fun () -> b.Modulate <- Color(1f, 1f, 1f, 0f)))) |> ignore
        else
            b.Modulate <- Color(1f, 1f, 1f, 0f)
        pos
    | None -> fallback

let private kanDestinationTopLefts fallback =
    let start =
        match tryControl "InGame/Kans/MarginContainer/KanContainer" with
        | Some c -> c.GlobalPosition
        | None -> fallback
    [ start
      start + Vector2(22f, 0f)
      start + Vector2(44f, 0f)
      start + Vector2(66f, 0f) ]

let private nextDiscardSlotTopLeft () =
    match tryControl "InGame/Discard/DiscardContainer" with
    | Some (:? GridContainer as g) ->
        let existing =
            g.GetChildren()
            |> Seq.filter (fun c -> match c with :? TextureRect -> true | _ -> false)
            |> Seq.length
        let columns = max 1 g.Columns
        let col = existing % columns
        let row = existing / columns
        let x = g.GlobalPosition.X + float32 col * (tileBtnSize.X + 2.0f)
        let y = g.GlobalPosition.Y + float32 row * (tileBtnSize.Y - 1.0f)
        Some(Vector2(x, y))
    | _ -> None

let private pulseControl (c: Control) (col: Color) =
    let baseScale = c.Scale
    let baseModulate = c.Modulate
    c.PivotOffset <- c.Size / 2.0f
    let t = root.CreateTween()
    t.TweenProperty(c, "scale:x", float (baseScale.X * 1.03f), 0.16) |> ignore
    t.Parallel().TweenProperty(c, "scale:y", float (baseScale.Y * 1.03f), 0.16) |> ignore
    t.Parallel().TweenProperty(c, "modulate", col, 0.16) |> ignore
    t.TweenInterval(0.12) |> ignore
    t.TweenProperty(c, "scale:x", float baseScale.X, 0.24) |> ignore
    t.Parallel().TweenProperty(c, "scale:y", float baseScale.Y, 0.24) |> ignore
    t.Parallel().TweenProperty(c, "modulate", baseModulate, 0.24) |> ignore

let private pulsePath path col =
    tryControl path |> Option.iter (fun c -> pulseControl c col)

let private shakeControl (c: Control) (col: Color) =
    let basePos = c.Position
    let baseModulate = c.Modulate
    let t = root.CreateTween()
    t.TweenProperty(c, "position:x", float (basePos.X + 4.0f), 0.08) |> ignore
    t.Parallel().TweenProperty(c, "modulate", col, 0.08) |> ignore
    t.TweenProperty(c, "position:x", float (basePos.X - 4.0f), 0.12) |> ignore
    t.TweenProperty(c, "position:x", float basePos.X, 0.08) |> ignore
    t.Parallel().TweenProperty(c, "modulate", baseModulate, 0.22) |> ignore

let private shakePath path col =
    tryControl path |> Option.iter (fun c -> shakeControl c col)

let animateFloat (text: string) (col: Color) =
    let l = new Label()
    l.Text <- text
    l.HorizontalAlignment <- Godot.HorizontalAlignment.Center
    l.AddThemeColorOverride("font_color", col)
    l.AddThemeFontSizeOverride("font_size", 16)
    l.Position <- Vector2(300f, 200f)
    l.Scale <- Vector2(1.0f, 1.0f)
    root.AddChild(l)
    let t = root.CreateTween()
    t.TweenProperty(l, "position:y", 184.0, 0.25) |> ignore
    t.TweenInterval(0.75) |> ignore
    t.TweenProperty(l, "position:y", 168.0, 0.45) |> ignore
    t.Parallel().TweenProperty(l, "modulate:a", 0.0, 0.45) |> ignore
    t.add_Finished(fun () -> l.QueueFree())

let mutable private kanModeLabel: Label option = None

let setKanModeHint active =
    match active, kanModeLabel with
    | true, Some l when GodotObject.IsInstanceValid(l) ->
        match tryControl "InGame/VBoxContainer/Hand" with
        | Some hand -> l.Position <- hand.GlobalPosition + Vector2(0f, -70f)
        | None -> ()
        l.Show()
    | true, _ ->
        let l = new Label()
        l.Text <- "SELECT KAN TILE"
        l.HorizontalAlignment <- Godot.HorizontalAlignment.Center
        l.AddThemeFontSizeOverride("font_size", 16)
        l.AddThemeColorOverride("font_color", Color(1f, 1f, 1f))
        l.AddThemeColorOverride("font_shadow_color", Color(0f, 0f, 0f, 1f))
        l.AddThemeConstantOverride("shadow_offset_x", 2)
        l.AddThemeConstantOverride("shadow_offset_y", 2)
        l.Size <- Vector2(600f, 24f)
        match tryControl "InGame/VBoxContainer/Hand" with
        | Some hand -> l.Position <- hand.GlobalPosition + Vector2(0f, -70f)
        | None -> l.Position <- Vector2(0f, 234f)
        root.AddChild(l)
        kanModeLabel <- Some l
    | false, Some l when GodotObject.IsInstanceValid(l) ->
        l.Hide()
    | false, _ -> ()

let private animateTileFlyWithTextureDelayed (tile: Types.Tile) (finalTexture: Texture2D option) (fromPos: Vector2) (toPos: Vector2) (delay: float) =
    let r = new TextureRect()
    r.Texture <- tex (tile.Value())
    r.CustomMinimumSize <- tileBtnSize
    r.Size <- tileBtnSize
    r.Position <- fromPos
    r.PivotOffset <- tileBtnSize / 2.0f
    r.Scale <- Vector2(1.0f, 1.0f)
    r.Modulate <- if delay > 0.0 then Color(1f, 1f, 1f, 0f) else Color(1f, 1f, 1f, 0.95f)
    root.AddChild(r)
    flyingTiles.Add(r)
    let t = root.CreateTween()
    if delay > 0.0 then
        t.TweenInterval(delay) |> ignore
        t.TweenCallback(Callable.From(Action(fun () -> r.Modulate <- Color(1f, 1f, 1f, 0.95f)))) |> ignore
    easeTween t
    t.TweenProperty(r, "position", toPos, 0.34) |> ignore
    match finalTexture with
    | Some final when final <> r.Texture ->
        let flip = root.CreateTween()
        if delay > 0.0 then
            flip.TweenInterval(delay) |> ignore
        flip.TweenInterval(0.13) |> ignore
        easeTween flip
        flip.TweenProperty(r, "scale:x", 0.0, 0.08) |> ignore
        flip.TweenCallback(Callable.From(Action(fun () -> r.Texture <- final))) |> ignore
        flip.TweenProperty(r, "scale:x", 1.0, 0.08) |> ignore
    | _ -> ()

let private animateTileFlyDelayed (tile: Types.Tile) (fromPos: Vector2) (toPos: Vector2) (delay: float) =
    animateTileFlyWithTextureDelayed tile None fromPos toPos delay

let private animateTileFly (tile: Types.Tile) (fromPos: Vector2) (toPos: Vector2) =
    animateTileFlyDelayed tile fromPos toPos 0.0

let private sortedHandInsertChildIndex (slotIndex: int) =
    match tryControl "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer" with
    | Some (:? HBoxContainer as hc) ->
        let children = hc.GetChildren()
        let mutable visibleTileCount = 0
        let mutable result: int option = None
        let mutable i = 0
        while i < children.Count && result.IsNone do
            match children[i] with
            | :? Control as c when c.HasMeta("hand_animation_spacer") ->
                ()
            | :? TextureButton as b when b.HasMeta("tile_value") ->
                if b.Visible && b.Modulate.A > 0.1f then
                    if visibleTileCount >= slotIndex then
                        result <- Some i
                    else
                        visibleTileCount <- visibleTileCount + 1
                elif visibleTileCount >= slotIndex then
                    result <- Some i
            | _ ->
                result <- Some i
            i <- i + 1
        result |> Option.defaultValue children.Count |> Some
    | _ -> None

let private openSortedHandGap (slotIndex: int) =
    match tryControl "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer", sortedHandInsertChildIndex slotIndex with
    | Some (:? HBoxContainer as hc), Some childIndex ->
        let gap = new Control()
        gap.CustomMinimumSize <- Vector2(0f, 0f)
        gap.SetMeta("hand_animation_spacer", true)
        hc.AddChild(gap)
        hc.MoveChild(gap, childIndex)
        animateMinWidth gap (float handInsertedTileWidth) 0.34
        None
    | _ -> None

let private closeHandAfterKan () =
    let buttons =
        textureButtonsAt "InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer"
        // |> fun a -> Option.fold (fun s v -> Array.insertAt 0 v s) a (lastHandTileControl())
        |> Array.sortBy (fun b -> b.GlobalPosition.X)
    let firstPos = Vector2(143.0f, 356.0f) // TODO: Remove this magic value
    // let firstPos = Vector2(143.0f, 356.0f)

    buttons
        |> Array.sortBy (fun b -> b.GetMeta("tile_value").AsInt32())
        |> Array.mapi (fun i x ->
                       let tile = Types.Tile (x.GetMeta("tile_value").AsInt32())
                       let fromPos = controlTileTopLeft x |> fun x -> Vector2(x.X, 356.f)
                       let toPos = firstPos + Vector2((float32 i) * (tileBtnSize.X + 2.0f), 0f)
                       x.Modulate <- Color(1f, 1f, 1f, 0f)
                       animateTileFly tile fromPos toPos)
        |> ignore

// let private openKanAsideSlots () =
//     match tryControl "InGame/Kans/MarginContainer/KanContainer" with
//     | Some (:? HBoxContainer as kc) ->
//         let newGroupWidth = tileBtnSize.X * 4.0f + 2.0f * 3.0f
//         let group = new Control()
//         group.CustomMinimumSize <- Vector2(0f, 0f)
//         kc.AddChild(group)
//         kc.MoveChild(group, 0)
//         animateMinWidth group (float newGroupWidth) 0.34
//         kanDestinationTopLefts kc.GlobalPosition
//     | _ -> []

let private animateTsumoIntoSortedSlot fallback =
    match takePreviousTsumo() with
    | Some (moves) ->
        for (tile, fromPos, toPos, liveControl) in moves do
            liveControl.Modulate <- Color(1f, 1f, 1f, 0f)
            animateTileFly tile fromPos toPos
    | None -> ()

let private fadeFlyingTiles () =
    for c in flyingTiles do
        if GodotObject.IsInstanceValid(c) then
            let t = root.CreateTween()
            t.TweenProperty(c, "modulate:a", 0.0, 0.25) |> ignore
            t.add_Finished(fun () -> c.QueueFree())
    flyingTiles.Clear()

let private targetCenter path fallback =
    tryControl path |> Option.map centerOf |> Option.defaultValue fallback

let private tilesText tiles =
    tiles |> List.map string |> String.concat ""

let private kantsuCount () =
    match tryControl "InGame/Kans/MarginContainer/KanContainer" with
    | Some c ->
        c.GetChildren()
        |> Seq.filter (fun child -> match child with :? TextureRect -> true | _ -> false)
        |> Seq.length
        |> fun n -> n / 4
    | None -> 0

let animateGameEvent (event: Types.GameEvents): float =
    let handPath = "InGame/VBoxContainer/Hand"
    let pilePath = "InGame/VBoxContainer/ItemsContainer/Pile"
    let discardPath = "InGame/Discard"
    let scorePath = "InGame/VBoxContainer/ScorePanel"
    let infoPath = "InGame/InfoContainer"
    let buttonsPath = "InGame/ButtonContainer"
    let itemPath = "InGame/VBoxContainer/ItemsContainer/MarginContainer/PanelContainer"
    let shopPath = "Shop/ShopPanel"
    let scorePresentationPath = "ScorePresentation/ScorePanel"
    let gameOverPath = "GameOver/PanelContainer"
    let mainMenuPath = "MainMenu/VBoxContainer"
    let phasePath phase =
        match phase with
        | Types.GameStarting -> mainMenuPath
        | Types.InGame -> handPath
        | Types.ScorePresentation -> scorePresentationPath
        | Types.Shop -> shopPath
        | Types.GameOver -> gameOverPath
    let pileFallback = tileTopLeftFromCenter (targetCenter pilePath (Vector2(300f, 120f)))
    let pileTopLeft = liveWallSource() |> Option.map controlTileTopLeft |> Option.defaultValue pileFallback
    let rinshangTopLeft = rinshangSource() |> Option.map controlTileTopLeft |> Option.defaultValue pileTopLeft
    let handTopLeft = lastHandTileTopLeft() |> Option.defaultValue (tileTopLeftFromCenter (targetCenter handPath (Vector2(300f, 320f))))
    let discardTopLeft = nextDiscardSlotTopLeft() |> Option.defaultValue (tileTopLeftFromCenter (targetCenter discardPath (Vector2(130f, 160f))))

    match event with
    | Types.GameStarted ->
        0.05
    | Types.TileDiscarded t ->
        let fallback = handTileTopLeft t |> Option.defaultValue handTopLeft
        let fromPos = takeHandSource fallback
        animateTileFly t fromPos discardTopLeft
        1.15
    | Types.TileDrawn t ->
        let movedOldTsumo = animateTsumoIntoSortedSlot handTopLeft
        let delay = 0.32
        let newTsumoTopLeft = handTopLeft

        let fromPos = hideSourceDelayedAndGetTopLeft (liveWallSource()) pileTopLeft delay
        animateTileFlyDelayed t fromPos newTsumoTopLeft delay
        0.85
    | Types.RinshangDrawn t ->
        let delay = 0.0
        let newTsumoTopLeft = handTopLeft + Vector2(tileBtnSize.X * 2f + 0f, 0f)

        let fromPos = hideSourceDelayedAndGetTopLeft (rinshangSource()) rinshangTopLeft delay
        animateTileFlyDelayed t fromPos newTsumoTopLeft delay
        0.85
    | Types.PileEmpty ->
        shakePath pilePath (Color(1f, 0.25f, 0.25f))
        animateFloat "PILE EMPTY" (Color(1f, 0.25f, 0.25f))
        1.5
    | Types.GameEvents.Kan t ->
        let fallback = handTileTopLeft t |> Option.defaultValue handTopLeft
        let sources = takeKanSources fallback

        closeHandAfterKan()

        let kc = root.GetNode<HBoxContainer>("InGame/Kans/MarginContainer/KanContainer")
        let kcTopLeft = kc.GlobalPosition + Vector2(kc.Size.X - 20f, 0f)
        let nk = float32 <| kantsuCount()
        let kcTopRight = kcTopLeft - Vector2(nk * 98.0f, 0f)
        let offset = Vector2(2f + 20f, 0f)

        List.zip sources [kcTopRight - 3f * offset; kcTopRight - 2f * offset; kcTopRight - offset; kcTopRight]
        |> List.iteri (fun i (fromPos, toPos) ->
            let finalTexture = if i = 1 || i = 2 then Some(ura()) else None
            animateTileFlyWithTextureDelayed t finalTexture fromPos toPos 0.0)
        1.15
    | Types.Scored (_, _, score) ->
        0.05
    | Types.EarnedExtraScore (han, fu, reason) ->
        0.05
    | Types.EarnedExtraHonba n ->
        pulsePath infoPath (Color(0.7f, 0.7f, 1f))
        animateFloat $"+{n} HONBA" (Color(0.7f, 0.7f, 1f))
        1.5
    | Types.ScoringStarted ->
        0.05
    | Types.EffectTriggered (_, item, effects) ->
        0.05
    | Types.ShopEntered ->
        0.05
    | Types.PresentedItem items ->
        0.05
    | Types.Bought item ->
        0.05
    | Types.Sold item ->
        0.05
    | Types.EarnedGold n ->
        0.05
    | Types.ShuffledPile scope ->
        0.05
    | Types.DrawnHand _ ->
        0.05
    | Types.RevealedDora tiles ->
        0.05
    | Types.RevealedUraDora tiles ->
        0.05
    | Types.TransitionedPhaseTo phase ->
        0.05
    | Types.NotEnoughGold ->
        0.05
    | Types.InventoryFull ->
        0.05
    | Types.ShopItemNotFound ->
        0.05
    | Types.ItemAlreadyOwned ->
        animateFloat "ALREADY OWNED" (Color(1f, 0.85f, 0.3f))
        1.5
    | Types.ItemTriggered item ->
        0.05
    | Types.UpdatedItemState (item, _) ->
        0.05
    | Types.ItemDestroyed item ->
        shakePath itemPath (Color(1f, 0.45f, 0.45f))
        animateFloat $"BREAK {item.name}" (Color(1f, 0.45f, 0.45f))
        1.5
    | Types.NextRound r ->
        pulsePath infoPath (Color(0.6f, 0.6f, 1f))
        animateFloat $"ROUND {r}" (Color(0.6f, 0.6f, 1f))
        1.5
    | Types.PeekDrawPile (_, t) ->
        animateTileFly t pileTopLeft (pileTopLeft + Vector2(0f, -48f))
        1.15
    | Types.PeekRinshang (_, t) ->
        animateTileFly t rinshangTopLeft (rinshangTopLeft + Vector2(0f, -48f))
        1.15
    | Types.GameOverEvent ->
        shakePath gameOverPath (Color(1f, 0.25f, 0.25f))
        animateFloat "GAME OVER" (Color(1f, 0.25f, 0.25f))
        1.5

let private gameEventDuration event =
    match event with
    | Types.TileDrawn _
    | Types.RinshangDrawn _ -> 0.85
    | Types.TileDiscarded _
    | Types.GameEvents.Kan _
    | Types.PeekDrawPile _
    | Types.PeekRinshang _ -> 0.48
    | Types.PileEmpty
    | Types.EarnedExtraHonba _
    | Types.ItemDestroyed _
    | Types.NextRound _
    | Types.ItemAlreadyOwned
    | Types.GameOverEvent -> 1.5
    | _ -> 0.05

let animateGameEvents (events: Types.GameEvents list) (onFinished: unit -> unit) =
    let t = root.CreateTween()
    let mutable queued = false
    for event in events do
        let duration = gameEventDuration event
        if duration > 0.1 then
            queued <- true
            t.TweenCallback(Callable.From(Action(fun () -> animateGameEvent event |> ignore))) |> ignore
            t.TweenInterval(duration) |> ignore
    if queued then
        t.TweenCallback(Callable.From(Action(fun () ->
            onFinished()
            fadeFlyingTiles()))) |> ignore
    else
        onFinished()
        fadeFlyingTiles()

let animatePileShrink () =
    let r = root.GetNode<Control>("InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/TopRow")
    let t = root.CreateTween()
    t.TweenProperty(r, "scale", Vector2(1.1f, 1.1f), 0.1) |> ignore
    t.TweenProperty(r, "scale", Vector2(1f, 1f), 0.15) |> ignore

// ── Hover ──
let setupHandHover (handContainer: HBoxContainer) =
    for child in handContainer.GetChildren() do
        match child with
        | :? TextureButton as b ->
            b.add_MouseEntered(fun () ->
                let tw = root.CreateTween()
                tw.TweenProperty(b, "position:y", -8.0, 0.12) |> ignore)
            b.add_MouseExited(fun () ->
                let tw = root.CreateTween()
                tw.TweenProperty(b, "position:y", 0.0, 0.12) |> ignore)
        | _ -> ()
