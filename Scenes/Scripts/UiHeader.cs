using Godot;
using System;

public class UiHeader : Control
{
    [Export] private States _initialState;
    
    [Export] private NodePath _comboContainerPath;
    [Export] private NodePath _comboBarPath;
    [Export] private NodePath _comboValuePath;
    [Export] private NodePath _comboMaxLabelPath;
    
    [Export] private NodePath _xpMessagePath;
    [Export] private NodePath _xpMessageLabelPath;
    [Export] private NodePath _levelUpMessagePath;
    [Export] private NodePath _maxLevelMessagePath;
    
    private ProgressBar ComboBar        { get => GetNode<ProgressBar>(_comboBarPath); }
    private Label       ComboValueLabel { get => GetNode<Label>      (_comboValuePath); }
    private Control     LevelUpMessage  { get => GetNode<Control>    (_levelUpMessagePath); }
    private Control     XpMessage       { get => GetNode<Control>    (_xpMessagePath); }
    private Label       XpMessageLabel  { get => GetNode<Label>      (_xpMessageLabelPath); }
    
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
                
                case States.XpMessage:
                    _messageTimer.Disconnect("timeout", this, nameof(OnXpMessageTimeout));
                    XpMessage.Visible = false;
                    _messageTimer.Stop();
                    break;
                
                case States.LevelUpMessage:
                    _messageTimer.Disconnect("timeout", this, nameof(OnLevelUpMessageTimeout));
                    LevelUpMessage.Visible = false;
                    _messageTimer.Stop();
                    break;
            }
            
            GD.Print($"UI Header State changed from {_state} to {value}");
            _state = value;
            
            // enter
            switch (_state)
            {
                case States.Combo: GetNode<Control>(_comboContainerPath).Visible = true; break;
                case States.Xp:    GetNode<Control>(_xpBarContainerPath).Visible = true; break;
                
                case States.XpMessage: 
                    _messageTimer.Connect("timeout", this, nameof(OnXpMessageTimeout));
                    _messageTimer.Start(); 
                    break;
                
                case States.LevelUpMessage: 
                    _messageTimer.Connect("timeout", this, nameof(OnLevelUpMessageTimeout));
                    _messageTimer.Start(); 
                    break;
                
                case States.MaxLevelMessage: 
                    _messageTimer.Connect("timeout", this, nameof(OnMaxLevelMessageTimeout));
                    _messageTimer.Start(); 
                    break;
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
                    _xpFillTimer.Disconnect("timeout", this, nameof(OnXpTimerTimeout));
                    _xpFillTimer.Stop();
                    _queuedLevelUp = false;
                    break;
                
                case XpStates.XpFullFill:
                    _xpFillTimer.Disconnect("timeout", this, nameof(OnXpFullFillTimeout));
                    _xpFillTimer.Stop();
                    _queuedLevelUp = false;
                    _oldXpProgress = 0;
                    XpBar.Value = 0;
                    break;
            }
            
            GD.Print($"UI Header XpState changed from {_xpState} to {value}");
            _xpState = value;

            // enter
            switch (_xpState)
            {
                case XpStates.XpFill:
                    _xpFillTimer.Connect("timeout", this, nameof(OnXpTimerTimeout));
                    _xpFillTimer.Start();
                    break;
                
                case XpStates.XpFullFill:
                    _xpFillTimer.Connect("timeout", this, nameof(OnXpFullFillTimeout));
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
    private Timer _messageTimer;
    
    private float _oldXpProgress;
    private float _newXpProgress;
    private int _messageFlashCount = 3;
    private bool _queuedLevelUp = false;
    private bool _queuedMaxLevel = false;


    [Signal] public delegate void MaxLevelMessageFinished();

    
    
    public override void _Ready()
    {
        base._Ready();

        ComboBar.Value  = 0;
        XpBar.Value     = 0;
        
        State = _initialState;

        _xpFillTimer = new Timer();
        _xpFillTimer.OneShot = true;
        _xpFillTimer.WaitTime = 0.5f;
        AddChild(_xpFillTimer);
        
        _messageTimer = new Timer();
        _messageTimer.OneShot = true;
        _messageTimer.WaitTime = 1f;
        AddChild(_messageTimer);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        

        switch (State)
        {
            case States.XpMessage:
                FlashMessage(_xpMessagePath);
                break;
            
            case States.LevelUpMessage:
                FlashMessage(_levelUpMessagePath);
                break;
            
            case States.MaxLevelMessage:
                FlashMessage(_maxLevelMessagePath);
                break;
            
            case States.Xp:
                
                switch (XpState)
                {
                    case XpStates.XpFill:
                        
                        XpBar.Value = _oldXpProgress 
                                    + (_newXpProgress - _oldXpProgress)
                                    * GetTimePercentage(_xpFillTimer);
                        break;
                    
                    case XpStates.XpFullFill:
                        
                        XpBar.Value = _oldXpProgress 
                                    + (1 - _oldXpProgress)
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
        
        progressor.Connect(nameof(Progressor.MaxLevelSurpassed), this, 
            nameof(OnMaxLevelSurpassed));
        
        
        comboManager.Connect(nameof(ComboManager.ComboValueIncreased), this, 
            nameof(OnComboValueIncreased));

        comboManager.Connect(nameof(ComboManager.TimerDecreased), this, 
            nameof(OnComboTimerDecreased));
    }

    public void OnXpIncreased(int xp, int xpBonus, Progressor progressor)
    {
        if (_queuedMaxLevel) return;
        
        XpMessageLabel.Text = xpBonus.ToString();
        XpRemaining.Text = $"{progressor.GetCurrentLevelXpRequirement() - xp}";
        _newXpProgress = (float)xp / progressor.GetCurrentLevelXpRequirement();
        State = States.XpMessage;
    }

    public void OnLeveledUp(int level, int xp, int xpBonus, Progressor progressor)
    {
        if (_queuedMaxLevel) return;
        
        XpMessageLabel.Text = xpBonus.ToString();
        XpRemaining.Text = $"{progressor.GetCurrentLevelXpRequirement() - xp}";
        _newXpProgress = (float)xp / progressor.GetCurrentLevelXpRequirement();
        State = States.XpMessage;
        _queuedLevelUp = true;
        GetNode<Label>(_comboMaxLabelPath).Text =
            (progressor.CurrentLevel + (int)ProjectSettings.GetSetting("game/combo/starting_min")).ToString();
    }

    public void OnMaxLevelSurpassed()
    {
        if (_queuedMaxLevel) return;
        
        _queuedLevelUp = true;
        _queuedMaxLevel = true;
        State = States.XpMessage;
        GD.Print("hello!");
    }

    public void OnXpTimerTimeout()
    {
        if (XpState == XpStates.XpFullFill)
        {
            XpState = XpStates.XpFill;
            _oldXpProgress = 0;
            XpBar.Value = 0;
            return;
        }
        
        XpState     = XpStates.Idle;
        XpBar.Value = _oldXpProgress = _newXpProgress;
    }

    public void OnComboValueIncreased(int combo)
    {
        if (_queuedMaxLevel) return;
        
        State = States.Combo;
        ComboValueLabel.Text = combo.ToString();
    }

    private void OnXpFullFillTimeout()
    {
        XpBar.Value = 0;
        
        State = !_queuedMaxLevel
              ? States.LevelUpMessage
              : States.MaxLevelMessage;
    }

    private float GetTimePercentage(Timer timer)
    {
        return 1 - Mathf.Pow(timer.TimeLeft / timer.WaitTime, 2);
    }

    private void OnXpMessageTimeout()
    {
        State = States.Xp;
        XpState = _queuedLevelUp
                ? XpStates.XpFullFill
                : XpStates.XpFill;
    }

    private void OnLevelUpMessageTimeout()
    {
        State = States.Xp;
        XpState = XpStates.XpFill;
    }

    private void FlashMessage(NodePath messagePath)
    {
        GetNode<Control>(messagePath).Visible = true;
    }

    private void OnComboTimerDecreased(float progress)
    {
        ComboBar.Value = progress;
    }

    private void OnMaxLevelMessageTimeout()
    {
        EmitSignal(nameof(MaxLevelMessageFinished));
    }
}
