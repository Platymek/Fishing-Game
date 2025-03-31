using Godot;
using System;

public class UiHeader : Control
{
    [Export] private States _initialState;
    
    [Export] private NodePath _comboContainerPath;
    [Export] private NodePath _comboBarPath;
    [Export] private NodePath _comboValuePath;
    
    private ProgressBar ComboBar        { get => GetNode<ProgressBar>(_comboBarPath); }
    private Label       ComboValueLabel { get => GetNode<Label>      (_comboValuePath); }
    
    enum States
    {
        Xp,
        Combo,
        XpMessage,
        LevelUpMessage,
        MaxLevelMessage,
    }

    private States _state = States.Combo;

    States State
    {
        get => _state;

        set
        {
            // exit
            switch (_state)
            {
                case States.Combo: GetNode<Control>(_comboContainerPath).Visible = false; break;
                
                case States.Xp:     
                    GetNode<Control>(_xpBarContainerPath).Visible = false;
                    _xpFillTimer.Stop();
                    break;
            }
            
            GD.Print($"State changed from {_state} to {value}");
            _state = value;
            
            // enter
            switch (_state)
            {
                case States.Combo: GetNode<Control>(_comboContainerPath).Visible = true; break;
                case States.Xp:    GetNode<Control>(_xpBarContainerPath).Visible = true; break;
            }
        }
    }
    
    
    enum XpStates
    {
        Idle,
        XpFill,
        XpFullFill,
    }

    private XpStates _xpState;

    XpStates XpState
    {
        get => _xpState;
        
        set 
        {
            // exit
            switch (_xpState)
            {
                case XpStates.XpFill:
                case XpStates.XpFullFill:
                    DisconnectXpFillTimer();
                    _xpFillTimer.Stop();
                    break;
            }
            
            _xpState = value;

            // enter
            switch (_xpState)
            {
                case XpStates.XpFill:
                case XpStates.XpFullFill:
                    ConnectXpFillTimer();
                    _xpFillTimer.Start();
                    break;
            }
        }
    }
    
    
    [Export] private NodePath _xpBarContainerPath;
    [Export] private NodePath _xpBarRemainingPath;
    [Export] private NodePath _xpBarPath;
    
    private ProgressBar XpBar { get => GetNode<ProgressBar>(_xpBarPath); }
    private Label XpRemaining { get => GetNode<Label>      (_xpBarRemainingPath); }

    private Timer _xpFillTimer;
    
    private float oldXpProgress;
    private float newXpProgress;
    
    
    public override void _Ready()
    {
        base._Ready();

        ComboBar.Value  = 0;
        XpBar.Value     = 0;
        
        State = _initialState;

        _xpFillTimer = new Timer();
        _xpFillTimer.OneShot = true;
        AddChild(_xpFillTimer);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        switch (State)
        {
            case States.Xp:
                
                switch (XpState)
                {
                    case XpStates.XpFill:
                        
                        XpBar.Value = oldXpProgress 
                                    + (newXpProgress - oldXpProgress)
                                    * GetTimePercentage(_xpFillTimer);
                        break;
                    
                    case XpStates.XpFullFill:
                        
                        XpBar.Value = oldXpProgress 
                                    + (1 - oldXpProgress)
                                    * GetTimePercentage(_xpFillTimer);
                        break;
                }
                break;
        }
    }

    public void Initialise(
        ComboManager comboManager, 
        Progressor progressor)
    {
        XpRemaining.Text = $"{progressor.GetCurrentLevelXpRequirement()}";
        
        
        progressor.Connect(nameof(Progressor.XpIncreased), this, 
            nameof(OnXpIncreased),
            new Godot.Collections.Array{progressor});
        
        progressor.Connect(nameof(Progressor.LeveledUp), this, 
            nameof(OnLeveledUp),
            new Godot.Collections.Array{progressor});
        
        
        comboManager.Connect(nameof(ComboManager.ComboValueIncreased), this, 
            nameof(OnComboValueIncreased));
    }

    public void OnXpIncreased(int xp, Progressor progressor)
    {
        if (State == States.Xp) return;
        
        State = States.Xp;
        XpState = XpStates.XpFill;
        newXpProgress = (float)xp / progressor.GetCurrentLevelXpRequirement();
    }

    public void OnLeveledUp(int level, int xp, Progressor progressor)
    {
        State = States.Xp;
        XpState = XpStates.XpFullFill;
        newXpProgress = (float)xp / progressor.GetCurrentLevelXpRequirement();
    }

    public void OnXpTimerTimeout()
    {
        if (XpState == XpStates.XpFullFill)
        {
            XpState = XpStates.XpFill;
            oldXpProgress = 0;
            return;
        }
        
        XpState     = XpStates.Idle;
        XpBar.Value = oldXpProgress = newXpProgress;
    }

    public void OnComboValueIncreased(int combo)
    {
        State = States.Combo;
        ComboValueLabel.Text = combo.ToString();
    }

    private void ConnectXpFillTimer()
    {
        _xpFillTimer.Connect("timeout", this, nameof(OnXpTimerTimeout));
    }

    private void DisconnectXpFillTimer()
    {
        _xpFillTimer.Disconnect("timeout", this, nameof(OnXpTimerTimeout));
    }

    private float GetTimePercentage(Timer timer)
    {
        return 1 - Mathf.Pow(timer.TimeLeft / timer.WaitTime, 2);
    }
}
