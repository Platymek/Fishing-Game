using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Game : Node2D
{
	[Export] private System.Collections.Generic.Dictionary<PackedScene, int> _catchables;
	[Export] private NodePath _leftSpawn;
	[Export] private NodePath _rightSpawn;
	[Export] private NodePath _characters;
	[Export] private NodePath _comboManager;
	
	private Queue<PackedScene> _spawnQueue;
	private Queue<bool> _spawnSideQueue;
	

	public override void _Ready()
	{
		base._Ready();
		
		
		var spawnArray = new Array<PackedScene>();

		foreach (PackedScene scene in _catchables.Keys)
		{
			for (int i = 0; i < _catchables[scene]; i++)
				spawnArray.Add(scene);
		}

		
		var spawnSide = new Array<bool>();
		
		for (int i = 0; i < (int)ProjectSettings.GetSetting("game/game/spawn_side_count"); i++)
		{
			spawnSide.Add(false);
			spawnSide.Add(true);
		}
		
		
		GD.Randomize();
		spawnArray.Shuffle();
		spawnSide.Shuffle();
		
		
		_spawnQueue		= new Queue<PackedScene>();
		foreach (var p in spawnArray) _spawnQueue.Enqueue(p);
		
		_spawnSideQueue = new Queue<bool>();
		foreach (var b in spawnSide) _spawnSideQueue.Enqueue(b);


		SpawnCatchable();

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
		var scene = _spawnQueue.Dequeue();

		
		var catchable = scene.Instance<Catchable>();
		GetNode(_characters).AddChild(catchable);
		
		catchable.Flipped = side;
		catchable.GlobalPosition = side
			? GetNode<Node2D>(_rightSpawn) .GlobalPosition
			: GetNode<Node2D>(_leftSpawn ).GlobalPosition;

		catchable.Connect("Caught", this, nameof(OnCatchableCaught));
		
		
		_spawnSideQueue.Enqueue(side);
		_spawnQueue.Enqueue(scene);
	}

	private void OnCatchableCaught(Catchable catchable)
	{
		catchable.OnCaught(GetNode<ComboManager>(_comboManager));
	}
}
