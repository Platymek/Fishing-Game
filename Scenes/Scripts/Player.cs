using Godot;
using System;

public class Player : Node2D
{
    // exported properties //

    [Export] private NodePath _animationPlayer;
    private AnimationPlayer AnimationPlayer { get => GetNode<AnimationPlayer>(_animationPlayer); }
    
    
    // states and actions //
    
    enum States
    {
        Idle,
        Moving,
        Catching,
    }

    private States _state;

    private States State
    {
        get => _state;

        set
        {
            switch (_state)
            {
                case States.Catching:
                    _catchTimer.Stop();
                    break;
            }
            
            _state = value;
            
            switch (_state)
            {
                case States.Idle:
                    AnimationPlayer.Play("RESET", customSpeed: 1f / _catchDuration);
                    break;
                
                case States.Catching:
                    _catchTimer.Start();
                    AnimationPlayer.Play("catch", customSpeed: 1f / _catchDuration);
                    break;
            }
        }
    }

    void ProcessState(float delta)
    {
        switch (State)
        {
            case States.Idle:
                if (AllowMove()) goto case States.Moving;
                break;
            
            case States.Moving:
                
                // returns true if catch position reached
                if (MoveToCatchPosition(delta))
                {
                    Catch();
                    goto case States.Catching;
                }
                break;
            
            case States.Catching:
                break;
        }
    }
    
    
    // properties //
    
    private float _movementSpeed;
    private float _catchDuration;
    private float _catchWidth;
    private float _catchBufferDuration;

    private Timer _catchBuffer;
    private Timer _catchTimer;
    private float _catchPosition;
    private float _requestedCatchPosition;
    
    
    // node methods //

    public override void _Ready()
    {
        base._Ready();
        
        State = States.Idle;
        
        _movementSpeed  = (float)ProjectSettings.GetSetting("game/player/movement_speed");
        _catchDuration  = (float)ProjectSettings.GetSetting("game/player/catch_duration");
        _catchWidth     = (float)ProjectSettings.GetSetting("game/player/catch_width");
        _catchBufferDuration    = (float)ProjectSettings.GetSetting("game/player/catch_buffer");

        _catchBuffer = new Timer();
        _catchBuffer.Name = "catchBuffer";
        AddChild(_catchBuffer, true);
        _catchBuffer.OneShot = true;
        _catchBuffer.WaitTime = _catchBufferDuration;
        
        _catchTimer = new Timer();
        _catchTimer.Name = "catchTimer";
        AddChild(_catchTimer, true);
        _catchTimer.OneShot = true;
        _catchTimer.WaitTime = _catchDuration;
        _catchTimer.Connect("timeout", this, "OnCatchTimerTimeout");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        ProcessInput();
        ProcessState(delta);
    }
    
    
    // other methods //

    bool MoveToCatchPosition(float delta)
    {
        Position = new Vector2(
            Mathf.MoveToward(Position.x, _catchPosition, _movementSpeed * delta),
            Position.y);
        
        return Position.x == _catchPosition;
    }

    void Catch()
    {
        State = States.Catching;
    }

    bool AllowMove()
    {
        if (_catchBuffer.IsStopped()) return false;

        Move(_requestedCatchPosition);
        return true;
    }

    void Move(float x)
    {
        State = States.Moving;

        float lowerBound = Position.x - (_catchWidth / 2);
        float upperBound = Position.x + (_catchWidth / 2);
        
        if (x < lowerBound
        ||  x > upperBound)
            _catchPosition = x;
    }

    void ProcessInput()
    {
        if (Input.IsActionJustPressed("catch"))
        {
            _requestedCatchPosition = GetViewport()
                .GetParent<ViewportContainer>()
                .GetLocalMousePosition().x;
                
            _catchBuffer.Start();
        }
    }

    void OnCatchTimerTimeout()
    {
        State = States.Idle;
    }
}
