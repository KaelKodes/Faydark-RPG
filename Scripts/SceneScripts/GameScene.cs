using Godot;
using System;


public partial class GameScene : Node2D  // ✅ Ensure GameScene inherits from Node2D
{
	private Node mapDisplay;

	public override void _Ready()
	{
		GD.Print("🔄 Initializing Game Scene...");
		LoadStartingMap();
	}

	private void LoadStartingMap()
	{
		// ✅ Get MapDisplay using full path from GameScene
		mapDisplay = GetNodeOrNull("UI/MapDisplay");

		if (mapDisplay == null)
		{
			GD.PrintErr("❌ MapDisplay node not found! Check GameScene.tscn.");
			return; // Stop execution if MapDisplay is missing
		}

		GD.Print("✅ MapDisplay found. Loading StartingGlade.tscn...");

		PackedScene mapScene = (PackedScene)ResourceLoader.Load("res://Scenes/Maps/StartingGlade.tscn");
		if (mapScene != null)
		{
			Node mapInstance = mapScene.Instantiate();
			mapDisplay.AddChild(mapInstance); // ✅ Attach the scene to the correct node
			GD.Print("✅ Starting map loaded: StartingGlade.tscn");
		}
		else
		{
			GD.PrintErr("❌ Failed to load StartingGlade.tscn");
		}
	}
}
