using Godot;

public partial class Main : Control
{
    public override void _Ready()
    {
        Frontend.init(this);
    }
}
