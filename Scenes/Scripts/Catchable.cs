using Godot;
using System;

public abstract class Catchable : Node2D
{
    [Signal] public delegate void Caught(Catchable catchable);
    [Export] private NodePath _flippables;

    public bool Flipped
    {
        get => GetNode<Node2D>(_flippables).Scale.x < 0;

        set => GetNode<Node2D>(_flippables).Scale = new Vector2(
            value ? -1 : 1, 
            GetNode<Node2D>(_flippables).Scale.y);
    }
    
    
    public abstract float Move();
    public abstract void OnCaught(ComboManager comboManager);
    
    public void Catch() { EmitSignal(nameof(Caught), this); }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Position = new Vector2(
            Position.x + Move() * (Flipped ? -1 : 1) * delta,
            Position.y);
    }
}
