using Godot;
using System;

public class Hazard : Catchable
{
	public override void _Ready()
	{
		base._Ready();

		HasPriority = true;
	}

	public override float Move()
	{
		return (float)ProjectSettings.GetSetting("game/game/hazard_speed");
	}

	public override void OnCaught(ComboManager comboManager)
	{
		comboManager.Fail();
		QueueFree();
	}
}
