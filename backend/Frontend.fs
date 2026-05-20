module Frontend

open Types
open GameState
open Godot
open System

let mutable private state = Unchecked.defaultof<GameState>
let mutable private root: Control = null

let mutable private kanMode = false
let mutable private inputLocked = false

let private createStateFromSeedText () =
    let seedText = root.GetNode<LineEdit>("MainMenu/VBoxContainer/SeedLineEdit").Text.Trim()
    let rng =
        match Int32.TryParse seedText with
        | true, seed -> Random(seed)
        | false, _ -> Random()
    createGameState rng

let renderLog log onFinished =
    View.animateGameEvents log onFinished

let private doStep input =
    let (s, log) = GameState.update state input
    state <- s
    s, log

let private afterLog log continuation =
    inputLocked <- true
    renderLog log (fun () ->
        inputLocked <- false
        continuation())

let rec wireHandClicks () =
    let hc = root.GetNode<HBoxContainer>("InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer")
    View.setupHandHover hc
    for child in hc.GetChildren() do
        match child with
        | :? TextureButton as b ->
            b.add_Pressed(fun () ->
                let v = b.GetMeta("tile_value").AsInt32()
                if v < 1 || v > 9 then ()
                elif inputLocked then ()
                elif kanMode && GameState.canKan (Tile v) state then
                    kanMode <- false
                    View.setKanModeHint false
                    View.setNextKanAnimationSources (Tile v) b
                    View.captureSortingMovesForDrawAnimation
                    let (_, log) = doStep (DeclareKan(Tile v))
                    afterLog log (fun () -> View.refreshInGame state; wireHandClicks())
                elif kanMode then
                    View.animateFloat "Cannot kan" (Color(1f, 0.3f, 0.3f))
                    View.refreshInGame state; wireHandClicks()
                elif GameState.canDiscard (Tile v) state then
                    View.setNextHandAnimationSource b
                    View.captureCurrentTsumoForDrawAnimation state.hand (Some(Tile v))
                    let (_, log) = doStep (Discard(Tile v))
                    afterLog log (fun () -> View.refreshInGame state; wireHandClicks()))
        | _ -> ()

let wireActionButtons () =
    let tsumoBtn = root.GetNode<Button>("InGame/ButtonContainer/TsumoButton")
    tsumoBtn.add_Pressed(fun () ->
        if not inputLocked && GameState.canTsumo state then
            let (s, log) = doStep DeclareTsumo
            afterLog log (fun () ->
                match s.phase with
                | ScorePresentation -> View.showPhase "ScorePresentation"; View.showScoreBreakdown log s false
                | _ -> View.refreshInGame state; wireHandClicks()))

    let kanBtn = root.GetNode<Button>("InGame/ButtonContainer/KanButton")
    kanBtn.add_Pressed(fun () ->
        if not inputLocked then
            if kanMode then
                kanMode <- false
                View.setKanModeHint false
            elif GameState.canAnyKan state then
                kanMode <- true
                View.setKanModeHint true)

    let confirmPileEmptyBtn = root.GetNode<Button>("InGame/ButtonContainer/ConfirmPileEmptyButton")
    confirmPileEmptyBtn.add_Pressed(fun () ->
        if not inputLocked && GameState.isPileEmpty state then
            let (s, log) = doStep ConfirmPileEmpty
            afterLog log (fun () ->
                match s.phase with
                | ScorePresentation -> View.showPhase "ScorePresentation"; View.showScoreBreakdown log s true
                | _ -> View.refreshInGame state; wireHandClicks()))

let rec wireShopButtons () =
    let bl = root.GetNode<VBoxContainer>("Shop/ShopPanel/ShopItemList")
    for child in bl.GetChildren() do
        match child with
        | :? Button as b ->
            let idx = b.GetMeta("item_idx").AsInt32()
            if idx < state.shopItems.Length then
                let item = { state.shopItems[idx] with id = Guid.NewGuid() }
                b.add_Pressed(fun () ->
                    if not inputLocked then
                        let (_, log) = doStep (Buy item)
                        afterLog log (fun () ->
                            View.refreshShop state
                            View.refreshInGame state
                            wireShopButtons()))
        | _ -> ()
    let sl = root.GetNode<VBoxContainer>("Shop/ShopPanel/PlayerItemList")
    for child in sl.GetChildren() do
        match child with
        | :? Button as b ->
            let idx = b.GetMeta("sell_idx").AsInt32()
            if idx < state.items.Length then
                let it = state.items[idx]
                b.add_Pressed(fun () ->
                    if not inputLocked then
                        let (_, log) = doStep (Sell it)
                        afterLog log (fun () ->
                            View.refreshShop state
                            View.refreshInGame state
                            wireShopButtons()))
        | _ -> ()

let init (r: Control) =
    root <- r
    View.init r
    View.cachePhases()
    state <- createStateFromSeedText()
    wireActionButtons()

    r.GetNode<Button>("MainMenu/VBoxContainer/StartButton").add_Pressed(fun () ->
        if not inputLocked then
            state <- createStateFromSeedText()
            let (s, log) = doStep Start
            afterLog log (fun () -> View.showPhase "InGame"; View.refreshInGame s; wireHandClicks()))

    r.GetNode<Button>("ScorePresentation/ScorePanel/ConfirmButton").add_Pressed(fun () ->
        if not inputLocked then
            let (s, log) = doStep ConfirmScore
            afterLog log (fun () ->
                match s.phase with
                | Shop -> View.showPhase "Shop"; View.refreshShop s; wireShopButtons()
                | InGame -> View.showPhase "InGame"; View.refreshInGame s; wireHandClicks()
                | GameOver -> View.refreshGameOver s; View.showPhase "GameOver"
                | _ -> ()))

    r.GetNode<Button>("Shop/ExitShopButton").add_Pressed(fun () ->
        if not inputLocked then
            let (s, log) = doStep ExitShop
            afterLog log (fun () -> View.showPhase "InGame"; View.refreshInGame s; wireHandClicks()))

    r.GetNode<Button>("GameOver/PanelContainer/MarginContainer/GameOverPanel/RestartButton").add_Pressed(fun () ->
        if not inputLocked then
            state <- createStateFromSeedText()
            kanMode <- false
            View.showPhase "MainMenu")

    View.showPhase "MainMenu"
