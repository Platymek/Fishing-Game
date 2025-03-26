using Godot;
using System;

public class SceneDictionary : Node
{
	[Export] public Godot.Collections.Dictionary<string, PackedScene> CatchableDictionary;
	[Export] public Godot.Collections.Dictionary<string, int> PositiveCatchableChances;
	[Export] public Godot.Collections.Dictionary<string, int> NegativeCatchableChances;
}
