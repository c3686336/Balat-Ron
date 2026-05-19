module UiSetup

open Godot

let setupHover (root: Control) =
    let handContainer =
        root.GetNode<HBoxContainer>("InGame/VBoxContainer/Hand/HBoxContainer/MarginContainer/HandContainer")

    for child in handContainer.GetChildren() do
        match child with
        | :? TextureButton as btn ->
            btn.add_MouseEntered(fun () ->
                let tween = root.CreateTween()
                tween.SetTrans(Tween.TransitionType.Quad) |> ignore
                tween.SetEase(Tween.EaseType.Out) |> ignore
                tween.TweenProperty(btn, "position:y", -8.0, 0.12) |> ignore)
            btn.add_MouseExited(fun () ->
                let tween = root.CreateTween()
                tween.SetTrans(Tween.TransitionType.Quad) |> ignore
                tween.SetEase(Tween.EaseType.Out) |> ignore
                tween.TweenProperty(btn, "position:y", 0.0, 0.12) |> ignore)
        | _ -> ()
