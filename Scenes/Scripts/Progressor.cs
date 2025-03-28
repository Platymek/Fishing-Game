using Godot;
using System;

public class Progressor : Node
{
    private int MaxLevel
    {
        get => 
            (int)ProjectSettings.GetSetting("game/combo/max")
        -   (int)ProjectSettings.GetSetting("game/combo/starting_min");
    }

    private int _currentLevel;
    private float _xp;
    
    [Signal] public delegate void LeveledUp(int level, int xp);
    [Signal] public delegate void XpIncreased(int xp);
    [Signal] public delegate void MaxLevelSurpassed();

    public override void _Ready()
    {
        base._Ready();

        _xp = 0;
        _currentLevel = 0;
    }

    public void GainXP(int xp)
    {
        _xp += xp;

        if (_xp < GetLevelXPRequirement(_currentLevel))
        {
            EmitSignal(nameof(XpIncreased), _xp);
            return;
        }
        
        _currentLevel++;

        if (_currentLevel >= MaxLevel)
        {
            EmitSignal(nameof(MaxLevelSurpassed));
            return;
        }
        
        EmitSignal(nameof(LeveledUp), _currentLevel, _xp);
        _xp -= GetLevelXPRequirement(_currentLevel);
    }

    private int GetLevelXPRequirement(int level)
    {
        return (int)Mathf.Pow(2, level + (int)ProjectSettings.GetSetting("game/combo/starting_min")) * 2;
    }
}
