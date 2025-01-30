using Godot;
using System;

public partial class SceneLoader : Node // ✅ Added "partial"
{
	private string currentScenePath = "res://Scenes/Maps/StartingGlade.tscn";

	public void ChangeScene(string newScenePath)
	{
		GD.Print($"🔄 Changing scene to: {newScenePath}");

		// Load the new scene
		PackedScene newScene = (PackedScene)ResourceLoader.Load(newScenePath);
		if (newScene != null)
		{
			// Remove the current scene from the display
			foreach (Node child in GetNode("MapDisplay").GetChildren())
			{
				child.QueueFree();
			}

			// Load and attach the new scene
			Node newSceneInstance = newScene.Instantiate();
			GetNode("MapDisplay").AddChild(newSceneInstance);

			// Update current scene reference
			currentScenePath = newScenePath;
		}
		else
		{
			GD.PrintErr($"❌ Failed to load scene: {newScenePath}");
		}
	}
}
