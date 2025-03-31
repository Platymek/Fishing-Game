using Godot;
using System;

public class ComboManager : Node
{
    [Signal] public delegate void ComboValueIncreased(int combo);
    [Signal] public delegate void ComboValueDecreased(int combo);
    [Signal] public delegate void ComboValueChanged(int combo);
    [Signal] public delegate void ComboEnded(int combo, int xp);
    [Signal] public delegate void ComboSucceeded(int combo, int xp);
    [Signal] public delegate void ComboFailed(int combo, int xp);
    [Signal] public delegate void TimerDecreased(float progress);
    
    private float DrainDuration { get => (float)ProjectSettings.GetSetting("game/combo/drain_duration"); }
    
    private int _combo = 0;
    private int _limit = 3;
    private Timer _drainer;
    

    public override void _Ready()
    {
        base._Ready();
        
        _drainer = new Timer();
        _drainer.Name = "ComboDrainer";
        AddChild(_drainer, true);
        _drainer.OneShot = true;
        _drainer.WaitTime = DrainDuration;
        _drainer.Connect("timeout", this, nameof(Finish));
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (!_drainer.IsStopped())
        {
            EmitSignal(nameof(TimerDecreased), _drainer.TimeLeft);
            GD.Print($"TimeLeft: {_drainer.TimeLeft}");
        }
    }

    public void Add(int value)
    {
        _combo = Math.Min(_combo + value, _limit);
        
        if (_combo < _limit)
        {
            _drainer.Start();
            EmitSignal(nameof(ComboValueIncreased), _combo);
            GD.Print($"Combo Added: {_combo}");
        }
        else
        {
            _drainer.Stop();
            GD.Print("Limit Reached");
            Finish();
        }
    }

    public void Finish()
    {
        var xp = Mathf.Pow(2, _combo - 1);
        EmitSignal(nameof(ComboEnded), _combo, xp);
        EmitSignal(nameof(ComboSucceeded), _combo, xp);
        
        GD.Print($"Combo finished. Combo of: {_combo} with {xp} xp");
        _drainer.Stop();
        _combo = 0;
    }

    public void Fail()
    {
        var xp = _combo;
        EmitSignal(nameof(ComboEnded), _combo, xp);
        EmitSignal(nameof(ComboFailed), _combo, xp);
        
        GD.Print($"Combo Failed. Combo of: {_combo} with {xp} xp");
        _drainer.Stop();
        _combo = 0;
    }

    public void SetLimit(int limit, int xp)
    {
        _limit = limit + (int)ProjectSettings.GetSetting("game/combo/starting_min");
    }
}
