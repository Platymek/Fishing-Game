using Godot;
using System;

public class Results : Control
{
    [Export] private NodePath _button;
    [Export] private NodePath _turnCountLabel;
    
    [Signal] public delegate void RestartRequested();

    public override void _Ready()
    {
        base._Ready();

        GetNode<Button>(_button).Connect("pressed", this, nameof(RequestRestart));
    }

    public void Initialise(int turns)
    {
        GetNode<Label>(_turnCountLabel).Text = $"{turns}";
    }

    void RequestRestart()
    {
        EmitSignal(nameof(RestartRequested));
    }
}
