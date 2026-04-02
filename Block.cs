using Godot;

namespace QuickPersistentGame;

public partial class Block : Node2D
{
	[Export] public int Health = 1;
	[Export] public Label HealthDisplay;
	[Export] public Sprite2D Sprite;
	
	public Level Level;
	public Vector2I GridPosition = Vector2I.Zero;
	public bool PowerUp;
	
	public bool Damage(int points)
	{
		Health -= points;
		if (Health > 0)
		{
			Update();
			return false;
		}
		// Destroy if health depleted.
		QueueFree();
		Level.Blocks[GridPosition.X, GridPosition.Y] = null;
		if (PowerUp)
			Level.Strength++;
		return true;
	}
	
	private void Update()
	{
		Sprite.Modulate = Color.FromHsv(Health / 64f + 0.5f, 0.8f, 1f);
		HealthDisplay.Text = Health.ToString();
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Update();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (PowerUp)
			Sprite.Modulate = Color.FromHsv(Health / 64f + 0.5f, 0.8f,
				Mathf.Sin(Time.GetTicksMsec() / 64f) / float.Tau + 0.5f);
	}
}