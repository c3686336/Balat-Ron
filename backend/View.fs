module View

open Godot
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
    r

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
    let kc = root.GetNode<HBoxContainer>("InGame/VBoxContainer/Hand/HBoxContainer/Kans/MarginContainer/KanContainer")
    clear kc
    let u = ura()
    for (Types.Kantsu (Types.Tile v)) in kantsu do
        kc.AddChild(mkTexRect (tex v))
        for _ in 1..2 do kc.AddChild(mkTexRect u)
        kc.AddChild(mkTexRect (tex v))
        let gap = new Control()
        gap.CustomMinimumSize <- Vector2(8f, 0f)
        kc.AddChild(gap)

// ── Pile render ──
// Draw order: rightmost TopRow → rightmost BottomRow → next-left TopRow → next-left BottomRow → ...
// Both rows use RTL, so GetChildren[0] is the rightmost tile.
let refreshPile (remaining: int) =
    let showCol = Color(1f, 1f, 1f, 1f)
    let hideCol = Color(1f, 1f, 1f, 0f)

    let getPreSepChildren (row: HBoxContainer) =
        let all = row.GetChildren()
        let sepIdx = all |> Seq.tryFindIndex (fun c -> c.Name.ToString().Contains("DoraSeparater")) |> Option.defaultValue 5
        all |> Seq.take sepIdx |> Seq.choose (fun c -> match c with :? TextureButton as b -> Some b | _ -> None) |> Seq.toArray

    let top = root.GetNode<HBoxContainer>("InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/TopRow")
    let bot = root.GetNode<HBoxContainer>("InGame/VBoxContainer/ItemsContainer/Pile/MarginContainer/VBoxContainer/BottomRow")
    let topPre = getPreSepChildren top
    let botPre = getPreSepChildren bot

    // Post-separator (dead wall) always visible
    let showPost (row: HBoxContainer) =
        let all = row.GetChildren()
        let sepIdx = all |> Seq.tryFindIndex (fun c -> c.Name.ToString().Contains("DoraSeparater")) |> Option.defaultValue 5
        for b in all |> Seq.skip (sepIdx + 1) |> Seq.choose (fun c -> match c with :? TextureButton as b -> Some b | _ -> None) do
            b.Modulate <- showCol

    let len = min topPre.Length botPre.Length

    // Interleave: rightmost Bot [0], rightmost Top [0], next Bot [1], next Top [1], ...
    let order = ResizeArray<TextureButton>(len * 2)
    for i in 0 .. len - 1 do
        order.Add(botPre[i])
        order.Add(topPre[i])

    for i in 0 .. order.Count - 1 do
        order[i].Modulate <- if i < remaining then showCol else hideCol

    showPost top
    showPost bot

// ── Discard ──
let refreshDiscard (tiles: Types.Tile array) =
    let g = root.GetNode<GridContainer>("InGame/Discard/DiscardContainer")
    clear g
    for t in tiles do
        g.AddChild(mkTexRect (tex (t.Value())))

// ── Items ──
let refreshItems (items: Types.Item list) =
    let box = root.GetNode<VBoxContainer>("InGame/VBoxContainer/ItemsContainer/MarginContainer/PanelContainer/MarginContainer/VBoxContainer/ItemScroll/VBoxContainer")
    clear box
    for item in items do
        let p = new PanelContainer()
        let v = new VBoxContainer()
        let n = new Label()
        n.Text <- item.name
        n.AddThemeFontSizeOverride("font_size", 12)
        v.AddChild(n)
        v.AddChild(new HSeparator())
        let d = new Label()
        d.Text <- item.description
        d.AutowrapMode <- TextServer.AutowrapMode.WordSmart
        d.AddThemeFontSizeOverride("font_size", 10)
        v.AddChild(d)
        p.AddChild(v)
        box.AddChild(p)

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
    let (Types.Hand (_, tsumo, _)) = state.hand
    kanBtn.Disabled <- not (GameState.canKan tsumo state)
    tsumoBtn.Disabled <- not (GameState.canTsumo state)

// ── Full InGame refresh ──
let refreshInGame (state: Types.GameState) =
    refreshHand state.hand
    refreshPile state.pile.Length
    refreshDiscard state.discardPile
    refreshItems state.items
    refreshInfo state.round state.gold state.honbaLeft state.baseScore
    refreshScore state.currentScore state.goalScore
    refreshButtonStates state

// ── Shop ──
let refreshShop (state: Types.GameState) =
    root.GetNode<Label>("Shop/ShopPanel/GoldLabel").Text <- $"Gold: {state.gold}  ·  Items: {state.items.Length}/{Config.maxItems}"
    let bl = root.GetNode<VBoxContainer>("Shop/ShopPanel/ShopItemList")
    clear bl
    for i in 0 .. state.shopItems.Length - 1 do
        let item = state.shopItems[i]
        let b = new Button()
        b.SetMeta("item_idx", i)
        b.Text <- $"[{i}] {item.name} ({item.cost}G)"
        bl.AddChild(b)
    let sl = root.GetNode<VBoxContainer>("Shop/ShopPanel/PlayerItemList")
    clear sl
    for i in 0 .. state.items.Length - 1 do
        let item = state.items[i]
        let b = new Button()
        b.SetMeta("sell_idx", i)
        b.Text <- $"[{i}] Sell {item.name} (+{Config.discount item.cost}G)"
        sl.AddChild(b)

// ── Score breakdown ──
let showScoreBreakdown (log: Types.GameEvents list) (state: Types.GameState) (isEmptyPileEnd: bool) =
    let el = root.GetNode<VBoxContainer>("ScorePresentation/ScorePanel/ScoreBreakdown/ExtraScoreList")
    let dl = root.GetNode<Label>("ScorePresentation/ScorePanel/ScoreBreakdown/DoraLabel")
    let title = root.GetNode<Label>("ScorePresentation/ScorePanel/ScoringTitle")
    title.Text <- if isEmptyPileEnd then "Round End" else "TSUMO!"
    clear el
    dl.Text <- "Dora: +0"
    let (han, fu) = state.baseScore
    root.GetNode<Label>("ScorePresentation/ScorePanel/ScoreBreakdown/HanFuLabel").Text <- $"{han} han  ·  {fu} fu"
    root.GetNode<Label>("ScorePresentation/ScorePanel/FinalScoreLabel").Text <- $"{state.currentScore} points"
    root.GetNode<Label>("ScorePresentation/ScorePanel/GoalProgress").Text <- $"{state.currentScore} / {state.goalScore}"
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
let animateFloat (text: string) (col: Color) =
    let l = new Label()
    l.Text <- text
    l.HorizontalAlignment <- Godot.HorizontalAlignment.Center
    l.AddThemeColorOverride("font_color", col)
    l.AddThemeFontSizeOverride("font_size", 18)
    l.Position <- Vector2(300f, 200f)
    root.AddChild(l)
    let t = root.CreateTween()
    t.TweenProperty(l, "position:y", 160.0, 0.8) |> ignore
    t.Parallel().TweenProperty(l, "modulate:a", 0.0, 0.6) |> ignore
    t.add_Finished(fun () -> l.QueueFree())

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
