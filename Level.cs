using System;
using Godot;

namespace QuickPersistentGame;

public partial class Level : Node2D
{
	[Export] public PackedScene BlockScene;
	[Export] public PackedScene PlayerScene;
	[Export] public Vector2I Size = new(12, 8);
	[Export] public Label StrengthDisplay;
	[Export] public Label Timer;
	[Export] public Container EndScreen;
	
	public Block[,] Blocks;
	public int Strength = 1;
	
	private RandomNumberGenerator random = new();
	private int difficulty = 1;
	private ulong startTime;
	private Player player;
	
	private ConfigFile stats = new();
	
	public override void _Ready()
	{
		// Try to load previous stats.
		Error err = stats.Load("user://stats.cfg");
		if (err == Error.Ok)
		{
			Strength = (int)stats.GetValue("LastRun", "strength");
			difficulty = (int)stats.GetValue("LastRun", "difficulty");
		}
		// Create player.
		player = PlayerScene.Instantiate<Player>();
		player.Level = this;
		AddChild(player);
		// Generate level.
		Regenerate();
	}
	
	private void Regenerate()
	{
		// Move player to start.
		player.Position = new(0f, Size.Y / 2 * 16f);
		player.GridPosition = new(0, Size.Y / 2);
		// Clear blocks array.
		if (Blocks != null)
			foreach (Block block in Blocks)	
				block?.QueueFree();
		Blocks = new Block[Size.X + 4, Size.Y];
		// Generate new blocks.
		for (int y = 0; y < Size.Y; y++)
		for (int x = 2; x < Size.X + 2; x++)
		{
			Block block = BlockScene.Instantiate<Block>();
			block.Position = new Vector2(x, y) * 16;
			block.Health = random.RandiRange(1, difficulty * 2);
			block.Level = this;
			block.PowerUp = random.Randf() > (Size.X * Size.Y - 0.5f) / (Size.X * Size.Y);
			block.GridPosition = new(x, y);
			Blocks[x, y] = block;
			AddChild(block);
		}
		// Add power up.
		
		// Reset timer.
		startTime = Time.GetTicksMsec();
	}
	
	public override void _Process(double delta)
	{
		if (EndScreen.Visible) return;
		// Reset if player reaches level end.
		if (player.GridPosition.X == Size.X - 1 + 4)
		{
			difficulty++;
			// Save stats.
			stats.SetValue("LastRun", "strength", Strength);
			stats.SetValue("LastRun", "difficulty", difficulty);
			stats.Save("user://stats.cfg");
			
			EndScreen.Visible = true;
		}
		// Update strength.
		StrengthDisplay.Text = "Strength: " + Strength;
		TimeSpan timeRemaining = TimeSpan.FromSeconds(5f) - TimeSpan.FromMilliseconds(Time.GetTicksMsec() - startTime);
		if (timeRemaining.Milliseconds >= 0f)
			// Update timer.
			Timer.Text = $"Time: {timeRemaining.Seconds}:{timeRemaining.Milliseconds.ToString().PadZeros(2)[..2]}";
		// Fail if time depleted.
		else
		{
			Timer.Text = "Time: 0:00";
			difficulty = 1;
			Strength = 1;
			// Save stats.
			stats.SetValue("LastRun", "strength", Strength);
			stats.SetValue("LastRun", "difficulty", difficulty);
			stats.Save("user://stats.cfg");
			
			EndScreen.Visible = true;
		}
	}
	
	public override void _Input(InputEvent @event)
	{
		if (!EndScreen.Visible) return;
		if (Input.IsKeyPressed(Key.Escape))
			GetTree().Quit();
		if (Input.IsKeyPressed(Key.Enter))
		{
			EndScreen.Visible = false;
			Regenerate();
		}
	}
}