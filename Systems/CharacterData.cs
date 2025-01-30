using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterData : Node  // ✅ Fix: Added "partial"
{
	public int CharacterID { get; set; } = -1;
	public int PersonalityID { get; set; } = -1;

	public string CharacterName { get; set; } = "New Hero";
	public string SelectedClass { get; set; } = "None";
	public string SelectedPersonality { get; set; } = "None";

	public Dictionary<string, float> Stats { get; private set; } = new Dictionary<string, float>();
	public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
	public string CurrentZone { get; set; } = "StartingArea";
	public List<int> Inventory { get; private set; } = new List<int>();

	public void SetCharacterData(int id, string name, string charClass, string personality, Dictionary<string, float> stats, Vector3 position, string zone, List<int> inventory)
	{
		CharacterID = id;
		CharacterName = name;
		SelectedClass = charClass;
		SelectedPersonality = personality;
		Stats = stats;
		Position = position;
		CurrentZone = zone;
		Inventory = inventory;
	}

	public void PrintCharacterData()
	{
		GD.Print($"🔥 Character Data Stored in Memory:");
		GD.Print($"   - ID: {CharacterID}");
		GD.Print($"   - Name: {CharacterName}");
		GD.Print($"   - Class: {SelectedClass}");
		GD.Print($"   - Personality: {SelectedPersonality}");
		GD.Print($"   - Location: {CurrentZone} @ {Position}");
		GD.Print($"   - Inventory Items: {string.Join(", ", Inventory)}");

		GD.Print($"   - Stats:");
		foreach (var stat in Stats)
		{
			GD.Print($"     {stat.Key}: {stat.Value}");
		}
	}
}
