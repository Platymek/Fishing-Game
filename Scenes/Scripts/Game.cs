using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Game : Node2D
{
	[Export] private NodePath _leftSpawn;
	[Export] private NodePath _rightSpawn;
	[Export] private NodePath _catchablesNode;
	
	private ComboManager _comboManager;

	public override void _Ready()
	{
		base._Ready();

		GD.Randomize();
		CreateComboManager();
		CreateSpawner();
	}

	private void CreateSpawnTimer(Spawner spawner)
	{
		var spawnTimer = new Timer();
		spawnTimer.Name = "SpawnTimer";
		AddChild(spawnTimer, true);
		spawnTimer.WaitTime = (float)ProjectSettings.GetSetting("game/game/spawn_rate");
		spawnTimer.Connect("timeout", spawner, nameof(spawner.SpawnCatchable), new Array{this});
		spawnTimer.Start();
	}

	private void CreateComboManager()
	{
		_comboManager = new ComboManager();
		AddChild(_comboManager);
	}

	private void CreateSpawner()
	{
		var spawner = new Spawner();
		spawner.Name = "Spawner";
		AddChild(spawner);
		spawner.Initialise(
			GetNode<Node2D>(_leftSpawn), 
			GetNode<Node2D>(_rightSpawn), 
			GetNode(_catchablesNode));
		
		CreateSpawnTimer(spawner);
		spawner.SpawnCatchable(this);
	}

	public void OnCatchableCaught(Catchable catchable)
	{
		catchable.OnCaught(_comboManager);
	}
}
