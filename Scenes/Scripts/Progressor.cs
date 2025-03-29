using Godot;
using System;

public class Progressor : Node
{
    public int MaxLevel
    {
        get => 
            (int)ProjectSettings.GetSetting("game/combo/max")
        -   (int)ProjectSettings.GetSetting("game/combo/starting_min");
    }

    public int CurrentLevel;
    public int Xp;
    
    [Signal] public delegate void LeveledUp(int level, int xp);
    [Signal] public delegate void XpIncreased(int xp);
    [Signal] public delegate void MaxLevelSurpassed();

    public override void _Ready()
    {
        base._Ready();

        Xp = 0;
        CurrentLevel = 0;
    }

    public void GainXp(int xp)
    {
        Xp += xp;

        if (Xp < GetLevelXpRequirement(CurrentLevel))
        {
            EmitSignal(nameof(XpIncreased), Xp);
            return;
        }
        
        Xp -= GetLevelXpRequirement(CurrentLevel);
        CurrentLevel++;

        if (CurrentLevel >= MaxLevel)
        {
            EmitSignal(nameof(MaxLevelSurpassed));
            return;
        }
        
        EmitSignal(nameof(LeveledUp), CurrentLevel, Xp);
    }

    public static int GetLevelXpRequirement(int level)
    {
        return (int)(Mathf.Pow(2, level + (int)ProjectSettings.GetSetting("game/combo/starting_min") - 1)
            * (float)ProjectSettings.GetSetting("game/combo/xp_requirement_scale"));
    }

    public int GetCurrentLevelXpRequirement()
    {
        return GetLevelXpRequirement(CurrentLevel);
    }
}
