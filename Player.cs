using System;
using Godot;

namespace QuickPersistentGame;

public partial class Player : Node2D
{
	public Level Level;
	public Vector2I GridPosition = Vector2I.Zero;
	
	private int scale = 16;
	
	private void Move(Vector2I change)
	{
		Vector2I newPosition = GridPosition + change;
		try
		{
			if (!Level.Blocks[newPosition.X, newPosition.Y]?.Damage(Level.Strength) == true) return;
		}
		catch (Exception)
		{
			return;
		}
		Position += change * scale;
		GridPosition = newPosition;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("moveLeft")) Move(new(-1, 0));
		if (@event.IsActionPressed("moveRight")) Move(new(1, 0));
		if (@event.IsActionPressed("moveUp")) Move(new(0, -1));
		if (@event.IsActionPressed("moveDown")) Move(new(0, 1));
	}
}