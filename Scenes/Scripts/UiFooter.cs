using Godot;
using System;
using System.Media;

public class UiFooter : Control
{
	[Export] private NodePath _levelPath;
	[Export] private NodePath _turnPath;
	
	public void Initialise(Progressor progressor, Game game)
	{
		progressor.Connect(nameof(Progressor.LeveledUp), this, nameof(OnLeveledUp));
		game.Connect(nameof(Game.TurnsIncreased), this, nameof(OnTurnsIncreased));
	}

	public void OnLeveledUp(int level, int xp, int xpBonus)
	{
		GetNode<Label>(_levelPath).Text = (level + 1).ToString();
	}

	public void OnTurnsIncreased(int turns)
	{
		GetNode<Label>(_turnPath).Text = (turns + 1).ToString();
	}
}
