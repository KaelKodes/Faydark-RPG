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
	public void LoadData(CharacterData data)
{
	if (data == null)
	{
		GD.PrintErr("❌ No character data to load!");
		return;
	}

	this.Name = data.CharacterName;
	this.Position = new Vector2(data.Position.X, data.Position.Y);
	GD.Print($"✅ Character {this.Name} loaded at {this.Position}");
}

}
