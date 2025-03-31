using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Game : Node2D
{
	[Signal] public delegate void GameEnded(int turns);
	[Export] private NodePath _leftSpawn;
	[Export] private NodePath _rightSpawn;
	[Export] private NodePath _catchablesNode;

	private ComboManager _comboManager;
	private int _turns;

	public void Initialise(UiHeader uiHeader)
	{
		GD.Randomize();
		
		
		var comboManager = CreateComboManager();
		var spawner		 = CreateSpawner();
		var progressor	 = CreateProgressor(comboManager);
		CreateSpawnTimer(spawner);

		_turns = 0;
		
		
		uiHeader.Initialise(comboManager, progressor);
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

	private ComboManager CreateComboManager()
	{
		_comboManager = new ComboManager();
		AddChild(_comboManager);
		return _comboManager;
	}

	private Spawner CreateSpawner()
	{
		var spawner = new Spawner();
		spawner.Name = "Spawner";
		AddChild(spawner);
		spawner.Initialise(
			GetNode<Node2D>(_leftSpawn), 
			GetNode<Node2D>(_rightSpawn), 
			GetNode(_catchablesNode));
		
		spawner.SpawnCatchable(this);
		return spawner;
	}

	private Progressor CreateProgressor(ComboManager comboManager)
	{
		var progressor = new Progressor();
		progressor.Name = "Progressor";
		AddChild(progressor);

		progressor.Connect(nameof(Progressor.LeveledUp), comboManager, nameof(comboManager.SetLimit));
		progressor.Connect(nameof(Progressor.LeveledUp), this, nameof(OnLeveledUp));
		progressor.Connect(nameof(Progressor.MaxLevelSurpassed), this, nameof(EndGame));
		
		comboManager.Connect(nameof(ComboManager.ComboEnded), this, nameof(OnXpGained), 
			new Array { progressor });
		
		return progressor;
	}

	public void OnCatchableCaught(Catchable catchable)
	{
		catchable.OnCaught(_comboManager);
	}

	public void OnXpGained(int combo, int xp, Progressor progressor)
	{
		progressor.GainXp(xp);
		_turns++;
		GD.Print($"Lv{progressor.CurrentLevel + 1}/{progressor.MaxLevel + 1}, " +
		         $"Xp Gained: {xp} -> {progressor.Xp} / {progressor.GetCurrentLevelXpRequirement()}, " +
		         $"turn {_turns}");
	}

	public void OnLeveledUp(int level, int xp)
	{
		GD.Print($"Leveled Up: {level}");
	}

	public void EndGame()
	{
		EmitSignal(nameof(GameEnded), _turns);
	}
}
