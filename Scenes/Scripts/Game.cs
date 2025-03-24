using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Game : Node2D
{
	[Export] private System.Collections.Generic.Dictionary<PackedScene, int> _positiveCatchables;
	[Export] private System.Collections.Generic.Dictionary<PackedScene, int> _negativeCatchables;
	[Export] private NodePath _leftSpawn;
	[Export] private NodePath _rightSpawn;
	[Export] private NodePath _characters;
	private ComboManager _comboManager;
	
	private Queue<PackedScene> _positiveSpawnQueue;
	private Queue<PackedScene> _negativeSpawnQueue;
	private Queue<bool> _spawnSideQueue;
	private Queue<bool> _negativeChanceQueue;
	

	public override void _Ready()
	{
		base._Ready();

		GD.Randomize();
		InitialiseSideSpawning();
		InitialiseNegativeChanceQueue();
		InitialiseSpawningScenes(_positiveCatchables, out _positiveSpawnQueue);
		InitialiseSpawningScenes(_negativeCatchables, out _negativeSpawnQueue);
		SpawnCatchable();
		CreateSpawnTimer();
		CreateComboManager();
	}
	
	private void InitialiseSpawningScenes(
		System.Collections.Generic.Dictionary<PackedScene, int> catchables,
		out Queue<PackedScene> spawnQueue)
	{
		var spawnArray = new Array<PackedScene>();

		foreach (PackedScene scene in catchables.Keys)
		{
			for (int i = 0; i < catchables[scene]; i++)
				spawnArray.Add(scene);
		}

		spawnArray.Shuffle();
		spawnQueue = new Queue<PackedScene>();
		foreach (var p in spawnArray) spawnQueue.Enqueue(p);
	}

	private void InitialiseSideSpawning()
	{
		var spawnSide = new Array<bool>();
		
		for (int i = 0; i < (int)ProjectSettings.GetSetting("game/game/spawn_side_count"); i++)
		{
			spawnSide.Add(false);
			spawnSide.Add(true);
		}
		
		spawnSide.Shuffle();
		
		_spawnSideQueue = new Queue<bool>();
		foreach (var b in spawnSide) _spawnSideQueue.Enqueue(b);
	}

	private void InitialiseNegativeChanceQueue()
	{
		var negativeChanceArray = new Array<bool>();
		
		negativeChanceArray.Add(true);

		for (int i = 0; i < (int)ProjectSettings.GetSetting("game/game/positive_to_negative_ratio") - 1; i++)
		{
			negativeChanceArray.Add(false);
		}
		
		_negativeChanceQueue = new Queue<bool>();
		foreach (var b in negativeChanceArray) _negativeChanceQueue.Enqueue(b);
	}

	private void CreateSpawnTimer()
	{
		var spawnTimer = new Timer();
		spawnTimer.Name = "SpawnTimer";
		AddChild(spawnTimer, true);
		spawnTimer.WaitTime = (float)ProjectSettings.GetSetting("game/game/spawn_rate");
		spawnTimer.Connect("timeout", this, "SpawnCatchable");
		spawnTimer.Start();
	}

	private void SpawnCatchable()
	{
		var side  = _spawnSideQueue.Dequeue();
		Spawn(side, _positiveSpawnQueue);
		_spawnSideQueue.Enqueue(side);
		
		var negative = _negativeChanceQueue.Dequeue();
		if (negative) Spawn(!side, _negativeSpawnQueue);
		_negativeChanceQueue.Enqueue(negative);
	}

	private void Spawn(bool side, Queue<PackedScene> spawnQueue)
	{
		var scene = spawnQueue.Dequeue();
		var catchable = scene.Instance<Catchable>();
		GetNode(_characters).AddChild(catchable);
		
		catchable.Flipped = side;
		catchable.GlobalPosition = side
			? GetNode<Node2D>(_rightSpawn) .GlobalPosition
			: GetNode<Node2D>(_leftSpawn ).GlobalPosition;

		catchable.Connect("Caught", this, nameof(OnCatchableCaught));
		spawnQueue.Enqueue(scene);
	}

	private void CreateComboManager()
	{
		_comboManager = new ComboManager();
		AddChild(_comboManager);
	}

	private void OnCatchableCaught(Catchable catchable)
	{
		catchable.OnCaught(_comboManager);
	}
}
