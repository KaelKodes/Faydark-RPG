using Godot;

public partial class PCMovement : Node
{
	private CharacterBody2D character;
	private float speed = 100f;
	private bool draftMode = false; // If true, player takes manual control
	private AnimatedSprite2D animatedSprite;
	private bool aiControlLogged = false; // Prevent AI spam

	public override void _Ready()
	{
		character = GetParent<CharacterBody2D>();
		if (character == null)
		{
			GD.PrintErr("PCMovement must be a child of CharacterBody2D");
			QueueFree();
		}

		animatedSprite = character.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// Retrieve movement speed from CharacterData
		if (CharacterData.Instance != null && CharacterData.Instance.current_stats.ContainsKey("movement_speed"))
		{
			speed = CharacterData.Instance.current_stats["movement_speed"];
		}
		else
		{
			GD.PrintErr("⚠️ Player Stats not found, using default speed");
			speed = 100f;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Toggle Draft Mode with Ctrl + 8
		if (Input.IsActionJustPressed("toggle_draft_mode"))
		{
			SetDraftMode(!draftMode);
		}

		if (draftMode)
		{
			HandlePlayerInput();
		}
		else
		{
			HandleAIMovement();
		}

		// Update player tile position
		UpdatePlayerTile();
	}

	private void HandlePlayerInput()
	{
		Vector2 direction = Vector2.Zero;
		if (Input.IsActionPressed("ui_right")) direction.X += 1;
		if (Input.IsActionPressed("ui_left")) direction.X -= 1;
		if (Input.IsActionPressed("ui_down")) direction.Y += 1;
		if (Input.IsActionPressed("ui_up")) direction.Y -= 1;

		direction = direction.Normalized();
		character.Velocity = direction * speed;
		character.MoveAndSlide();

		UpdateAnimation(direction);
	}

	private void HandleAIMovement()
	{
		// Prevent spam by only printing once
		if (!aiControlLogged)
		{
			GD.Print("AI is controlling movement (not implemented yet)");
			aiControlLogged = true;
		}
	}

	private void UpdateAnimation(Vector2 direction)
	{
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

	public void SetDraftMode(bool isActive)
	{
		draftMode = isActive;
		GD.Print($"Draft mode: {(draftMode ? "ON" : "OFF")}");

		// Reset AI log when switching modes
		if (!draftMode)
		{
			aiControlLogged = false;
		}
	}

	private void UpdatePlayerTile()
{
	if (character == null)
	{
		GD.PrintErr("❌ character is NULL in UpdatePlayerTile()!");
		return;
	}

	if (World.Instance == null)
	{
		GD.PrintErr("❌ World.Instance is NULL in UpdatePlayerTile()!");
		return;
	}

	if (SceneManager.Instance == null)
	{
		GD.PrintErr("❌ SceneManager.Instance is NULL in UpdatePlayerTile()!");
		return;
	}

	// ✅ Now safely compute newTilePos
	Vector2I newTilePos = new Vector2I((int)character.Position.X / 100, (int)character.Position.Y / 100);

	if (newTilePos != World.playerTile)
	{
		World.SetPlayerPosition(newTilePos);
		SceneManager.Instance.LoadCurrentBiome(); // 🚀 Trigger scene transition if needed
	}
}


}
