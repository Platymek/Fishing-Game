using Godot;
using System;
using Array = Godot.Collections.Array;

public class Main : Control
{
    [Export] private States _initialState;
    
    [Export] private PackedScene _gameScene;
    [Export] private PackedScene _resultsScene;
    [Export] private PackedScene _uiHeaderScene;
    [Export] private PackedScene _uiFooterScene;
    
    [Export] private NodePath _gameContainer;
    [Export] private NodePath _menuContainer;
    [Export] private NodePath _uiHeaderContainer;
    [Export] private NodePath _uiFooterContainer;
    
    
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
                case States.Game:    DestroyGame();    break;
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

        var uiHeader = CreateUiHeader();
        var uiFooter = CreateUiFooter();
        var game = CreateGame(uiHeader, uiFooter);
    }

    Game CreateGame(UiHeader uiHeader, UiFooter uiFooter)
    {
        var game = _gameScene.Instance<Game>();
        
        game.Name = "Game";
        GetNode(_gameContainer).AddChild(game);

        
        game.Connect(nameof(Game.GameEnded), 
            this, 
            nameof(SetStateToResults),
            flags: (uint)ConnectFlags.Deferred);
        
        
        game.Initialise(uiHeader, uiFooter);
        return game;
    }

    UiHeader CreateUiHeader()
    {
        var uiHeader = _uiHeaderScene.Instance<UiHeader>();
        
        uiHeader.Name = "UiHeader";
        GetNode(_uiHeaderContainer).AddChild(uiHeader);
        
        return uiHeader;
    }

    UiFooter CreateUiFooter()
    {
        var uiFooter = _uiFooterScene.Instance<UiFooter>();
        
        uiFooter.Name = "UiFooter";
        GetNode(_uiFooterContainer).AddChild(uiFooter);
        
        return uiFooter;
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
        
        foreach (Node child
                 in GetNode(_uiHeaderContainer).GetChildren())
        {
            child.QueueFree();
        }
        
        foreach (Node child
                 in GetNode(_uiFooterContainer).GetChildren())
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
