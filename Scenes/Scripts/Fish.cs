using Godot;
using System;

public class Fish : Catchable
{
	public override float Move()
	{
		return (float)ProjectSettings.GetSetting("game/game/fish_speed");
	}

	public override void OnCaught(ComboManager comboManager)
	{
		GD.Print("I've been caught!");
	}
}
