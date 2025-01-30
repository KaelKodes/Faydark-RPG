using Godot;
using System.Collections.Generic;

public partial class Character : CharacterBody2D
{
	private float speed = 100f; // Default movement speed
	private AnimatedSprite2D animatedSprite;

	public override void _Ready()
	{
		GD.Print("🔄 Initializing Character...");

		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if (CharacterData.Instance != null && CharacterData.Instance.current_stats.Count > 0)
		{
			ApplyCharacterData();
		}
		else
		{
			GD.PrintErr("❌ CharacterData not found or stats are missing!");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Vector2.Zero;

		// Get movement speed from stats
		speed = CharacterData.Instance.current_stats.ContainsKey("movement_speed")
			? CharacterData.Instance.current_stats["movement_speed"]
			: 100f;

		// Get movement input
		if (Input.IsActionPressed("ui_right"))
			direction.X += 1;
		if (Input.IsActionPressed("ui_left"))
			direction.X -= 1;
		if (Input.IsActionPressed("ui_down"))
			direction.Y += 1;
		if (Input.IsActionPressed("ui_up"))
			direction.Y -= 1;

		// Normalize direction to avoid diagonal speed boost
		direction = direction.Normalized();

		// Move character
		Velocity = direction * speed;  // ✅ Fixes slow movement
		MoveAndSlide();


		// Play animation based on direction
		if (direction != Vector2.Zero)
		{
			string animName = GetAnimationFromDirection(direction);
			animatedSprite.Play(animName);
		}
		else
		{
			animatedSprite.Pause();
		}
	}

	private void ApplyCharacterData()
	{
		speed = CharacterData.Instance.current_stats.ContainsKey("movement_speed")
			? CharacterData.Instance.current_stats["movement_speed"]
			: 100f;

		GD.Print($"✅ Character stats loaded! Movement Speed: {speed}");
	}

	private string GetAnimationFromDirection(Vector2 direction)
	{
		if (direction.X > 0 && direction.Y == 0) return "walk_E";
		if (direction.X < 0 && direction.Y == 0) return "walk_W";
		if (direction.Y > 0 && direction.X == 0) return "walk_S";
		if (direction.Y < 0 && direction.X == 0) return "walk_N";
		if (direction.X > 0 && direction.Y > 0) return "walk_SE";
		if (direction.X > 0 && direction.Y < 0) return "walk_NE";
		if (direction.X < 0 && direction.Y > 0) return "walk_SW";
		if (direction.X < 0 && direction.Y < 0) return "walk_NW";
		return "idle";
	}
}
