using Godot;
using System.Collections.Generic;

public partial class Character : CharacterBody2D
{
	private PCMovement movementComponent;

	public override void _Ready()
{
	GD.Print("🔄 Initializing Character...");

	// Debug: Print all children of Character
	foreach (Node child in GetChildren())
	{
		GD.Print($"🟢 Found child: {child.Name}");
	}

	movementComponent = GetNodeOrNull<PCMovement>("PCMovement");

	if (movementComponent == null)
	{
		GD.PrintErr("❌ PCMovement not found! Check if it's inside Character.tscn.");
	}
}


	private void ApplyCharacterData()
	{
		GD.Print("✅ Character stats loaded!");
	}
}
