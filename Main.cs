using Godot;
using System;
using Array = Godot.Collections.Array;

public class Main : Control
{
    [Export] private States _initialState;
    [Export] private PackedScene _gameScene;
    [Export] private PackedScene _resultsScene;
    [Export] private NodePath _gameContainer;
    [Export] private NodePath _menuContainer;
    
    enum States
    {
        Game,
        Results
    }

    private States _state;
    States State
    {
        get => _state;

        set
        {
            switch (_state)
            {
                case States.Game: DestroyGame(); break;
                case States.Results: DestroyResults(); break;
            }
            
            _state = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();

        switch (_initialState)
        {
            case States.Game:    SetStateToGame();          break;
            case States.Results: SetStateToResults(0); break;
        }
    }

    void SetState(States state)
    {
        State = state;
    }

    void SetStateToGame()
    {
        State = States.Game;
        
        
        var game = _gameScene.Instance<Game>();
        
        game.Name = "Game";
        GetNode(_gameContainer).AddChild(game);

        
        game.Connect(nameof(Game.GameEnded), 
            this, 
            nameof(SetStateToResults),
            flags: (uint)ConnectFlags.Deferred);
    }

    void SetStateToResults(int turns)
    {
        State = States.Results;
        
        
        var results = _resultsScene.Instance<Results>();
        
        results.Name = "Results";
        results.Initialise(turns);
        GetNode(_menuContainer).AddChild(results);

        
        results.Connect(nameof(Results.RestartRequested), 
            this, 
            nameof(SetStateToGame),
            flags: (uint)ConnectFlags.Deferred);
    }

    void DestroyGame()
    {
        foreach (Node child 
        in GetNode(_gameContainer).GetChildren())
        {
            child.QueueFree();
        }
    }

    void DestroyResults()
    {
        foreach (Node child 
        in GetNode(_menuContainer).GetChildren())
        {
            child.QueueFree();
        }
    }
}
