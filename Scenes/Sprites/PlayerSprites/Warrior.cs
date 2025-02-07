using Godot;
using System;

public partial class Warrior : CharacterBody2D
{
	private float speed = 100f; // Default speed in case CharacterData fails

	public override void _Ready()
	{
		// ✅ Ensure CharacterData is accessible
		if (CharacterData.Instance == null)
		{
			GD.PrintErr("❌ CharacterData.Instance is NULL! Using default values.");
		}
		else
		{
			// ✅ Safely retrieve movement speed from CharacterData
			if (CharacterData.Instance.current_stats.ContainsKey("movement_speed"))
			{
				speed = CharacterData.Instance.current_stats["movement_speed"];
			}
			else
			{
				GD.PrintErr("❌ 'movement_speed' not found in CharacterData. Using default.");
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Vector2.Zero;

		// Get movement input
		if (Input.IsActionPressed("ui_right"))
			direction.X += 1;
		if (Input.IsActionPressed("ui_left"))
			direction.X -= 1;
		if (Input.IsActionPressed("ui_down"))
			direction.Y += 1;
		if (Input.IsActionPressed("ui_up"))
			direction.Y -= 1;

		// Normalize to prevent diagonal speed boost
		direction = direction.Normalized();

		// Move character
		Velocity = direction * speed * (float)delta;
		MoveAndSlide();

		// Play animation based on direction
		AnimatedSprite2D anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (direction != Vector2.Zero)
		{
			anim.Play(GetAnimationFromDirection(direction));
		}
		else
		{
			anim.Pause();
		}
	}

	private string GetAnimationFromDirection(Vector2 direction)
	{
		if (direction.X > 0 && direction.Y == 0)
			return "walk_E";
		if (direction.X < 0 && direction.Y == 0)
			return "walk_W";
		if (direction.Y > 0 && direction.X == 0)
			return "walk_S";
		if (direction.Y < 0 && direction.X == 0)
			return "walk_N";
		if (direction.X > 0 && direction.Y > 0)
			return "walk_SE";
		if (direction.X > 0 && direction.Y < 0)
			return "walk_NE";
		if (direction.X < 0 && direction.Y > 0)
			return "walk_SW";
		if (direction.X < 0 && direction.Y < 0)
			return "walk_NW";
		return "idle";
	}
}
