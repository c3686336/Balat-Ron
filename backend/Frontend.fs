module Frontend

open Types
open GameState
open Godot
open System

let mutable private state = Unchecked.defaultof<GameState>
let mutable private root: Control = null

let mutable private kanMode = false

let renderLog log =
    for event in log do
        match event with
        | TileDiscarded t -> View.animateFloat $"Discarded {t}" (Color(1f, 0.5f, 0.4f))
        | RinshangDrawn _ -> View.animateFloat "Rinshan draw!" (Color(0.4f, 0.5f, 1f))
        | Kan _ -> View.animateFloat "KAN!" (Color(1f, 0.8f, 0.2f))
        | PileEmpty -> View.animateFloat "Pile empty!" (Color(1f, 0.3f, 0.3f))
        | Scored (_, _, sc) -> View.animateFloat $"{sc}pts" (Color(0.3f, 1f, 0.3f))
        | EarnedGold n -> View.animateFloat $"+{n} gold" (Color(1f, 0.9f, 0.3f))
        | Bought item -> View.animateFloat $"Bought {item.name}" (Color(0.4f, 1f, 0.6f))
        | Sold item -> View.animateFloat $"Sold {item.name}" (Color(1f, 0.6f, 0.4f))
        | NotEnoughGold -> View.animateFloat "Not enough gold" (Color(1f, 0.3f, 0.3f))
        | InventoryFull -> View.animateFloat "Inventory full" (Color(1f, 0.3f, 0.3f))
        | ShopItemNotFound -> View.animateFloat "Not available" (Color(1f, 0.3f, 0.3f))
        | ShuffledPile _ -> () // Removed pile shrink animation to prevent shifting
        | NextRound r -> View.animateFloat $"Round {r}!" (Color(0.6f, 0.6f, 1f))
        | ItemTriggered item -> View.animateFloat item.name (Color(0.7f, 0.5f, 1f))
        | _ -> ()

let private doStep input =
    let (s, log) = GameState.update state input
    state <- s; renderLog log
    s, log

let rec wireHandClicks () =
    let hc = root.GetNode<HBoxContainer>("InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer")
    View.setupHandHover hc
    for child in hc.GetChildren() do
        match child with
        | :? TextureButton as b ->
            b.add_Pressed(fun () ->
                let v = b.GetMeta("tile_value").AsInt32()
                if v < 1 || v > 9 then ()
                elif kanMode && GameState.canKan (Tile v) state then
                    kanMode <- false
                    doStep (DeclareKan(Tile v)) |> ignore
                    View.refreshInGame state; wireHandClicks()
                elif kanMode then
                    kanMode <- false
                    View.animateFloat "Cannot kan!" (Color(1f, 0.3f, 0.3f))
                elif GameState.isPileEmpty state then
                    let (s, _) = doStep ConfirmPileEmpty
                    match s.phase with
                    | ScorePresentation -> View.showPhase "ScorePresentation"; View.showScoreBreakdown [] s true
                    | _ -> View.refreshInGame state; wireHandClicks()
                elif GameState.canDiscard (Tile v) state then
                    doStep (Discard(Tile v)) |> ignore
                    View.refreshInGame state; wireHandClicks())
        | _ -> ()

    let tsumoBtn = root.GetNode<Button>("InGame/ButtonContainer/TsumoButton")
    tsumoBtn.add_Pressed(fun () ->
        if GameState.canTsumo state then
            let (s, log) = doStep DeclareTsumo
            match s.phase with
            | ScorePresentation -> View.showPhase "ScorePresentation"; View.showScoreBreakdown log s false
            | _ -> ())

    let kanBtn = root.GetNode<Button>("InGame/ButtonContainer/KanButton")
    kanBtn.add_Pressed(fun () ->
        let (Types.Hand (_, tsumo, _)) = state.hand
        if GameState.canKan tsumo state then
            kanMode <- true
            View.animateFloat "Select tile to KAN" (Color(1f, 0.8f, 0.2f)))

let rec wireShopButtons () =
    let bl = root.GetNode<VBoxContainer>("Shop/ShopPanel/ShopItemList")
    for child in bl.GetChildren() do
        match child with
        | :? Button as b ->
            let idx = b.GetMeta("item_idx").AsInt32()
            if idx < state.shopItems.Length then
                let item = { state.shopItems[idx] with id = Guid.NewGuid() }
                b.add_Pressed(fun () ->
                    doStep (Buy item) |> ignore
                    View.refreshShop state; wireShopButtons())
        | _ -> ()
    let sl = root.GetNode<VBoxContainer>("Shop/ShopPanel/PlayerItemList")
    for child in sl.GetChildren() do
        match child with
        | :? Button as b ->
            let idx = b.GetMeta("sell_idx").AsInt32()
            if idx < state.items.Length then
                let it = state.items[idx]
                b.add_Pressed(fun () ->
                    doStep (Sell it) |> ignore
                    View.refreshShop state; wireShopButtons())
        | _ -> ()

let init (r: Control) =
    root <- r
    View.init r
    View.cachePhases()
    state <- createGameState (Random())

    r.GetNode<Button>("MainMenu/VBoxContainer/StartButton").add_Pressed(fun () ->
        let (s, _) = doStep Start
        View.showPhase "InGame"; View.refreshInGame s; wireHandClicks())

    r.GetNode<Button>("ScorePresentation/ScorePanel/ConfirmButton").add_Pressed(fun () ->
        let (s, _) = doStep ConfirmScore
        match s.phase with
        | Shop -> View.showPhase "Shop"; View.refreshShop s; wireShopButtons()
        | InGame -> View.showPhase "InGame"; View.refreshInGame s; wireHandClicks()
        | GameOver -> View.showPhase "GameOver"
        | _ -> ())

    r.GetNode<Button>("Shop/ShopPanel/ExitShopButton").add_Pressed(fun () ->
        let (s, _) = doStep ExitShop
        View.showPhase "InGame"; View.refreshInGame s; wireHandClicks())

    r.GetNode<Button>("GameOver/PanelContainer/MarginContainer/GameOverPanel/RestartButton").add_Pressed(fun () ->
        state <- createGameState (Random())
        View.showPhase "InGame"; View.refreshInGame state; wireHandClicks())

    View.showPhase "MainMenu"
