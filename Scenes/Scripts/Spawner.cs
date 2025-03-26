using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public class Spawner : Node
{
    private Node2D _leftSpawn;
    private Node2D _rightSpawn;
    private Node _catchablesNode;
    private Queue<string> _positiveSpawnQueue;
    private Queue<string> _negativeSpawnQueue;
    private Queue<bool> _spawnSideQueue;
    private Queue<bool> _negativeChanceQueue;
    
    private SceneDictionary _sd
    {
        get => GetNode<SceneDictionary>("/root/SceneDictionary");
    }
    

    public override void _Ready()
    {
        base._Ready();
        
        InitialiseSpawningScenes(_sd.PositiveCatchableChances, out _positiveSpawnQueue);
        InitialiseSpawningScenes(_sd.NegativeCatchableChances, out _negativeSpawnQueue);
        
        InitialiseSideSpawning();
        InitialiseNegativeChanceQueue();
    }

    public void Initialise(
        Node2D leftSpawn, 
        Node2D rightSpawn, 
        Node catchablesNode)
    {
        _leftSpawn      = leftSpawn;
        _rightSpawn     = rightSpawn;
        _catchablesNode = catchablesNode;
    }
    
    private void InitialiseSpawningScenes(
        Godot.Collections.Dictionary<string, int> catchables,
        out Queue<string> spawnQueue)
    {
        var spawnArray = new Array<string>();

        foreach (string scene in catchables.Keys)
        {
            for (int i = 0; i < catchables[scene]; i++)
                spawnArray.Add(scene);
        }

        spawnArray.Shuffle();
        spawnQueue = new Queue<string>();
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

    public void SpawnCatchable(Game game)
    {
        var side  = _spawnSideQueue.Dequeue();
        Spawn(game, side, _positiveSpawnQueue);
        _spawnSideQueue.Enqueue(side);
		
        var negative = _negativeChanceQueue.Dequeue();
        if (negative) Spawn(game, !side, _negativeSpawnQueue);
        _negativeChanceQueue.Enqueue(negative);
    }

    private void Spawn(Game game, bool side, Queue<string> spawnQueue)
    {
        var scene = spawnQueue.Dequeue();
        var catchable = _sd.CatchableDictionary[scene].Instance<Catchable>();
        _catchablesNode.AddChild(catchable);
		
        catchable.Flipped = side;
        catchable.GlobalPosition = side
            ? _rightSpawn.GlobalPosition
            : _leftSpawn .GlobalPosition;

        catchable.Connect("Caught", game, nameof(game.OnCatchableCaught));
        spawnQueue.Enqueue(scene);
    }
}
