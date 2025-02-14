using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterData : Node
{
	public static CharacterData Instance { get; private set; } // ✅ Singleton

	public int CharacterID { get; set; } = -1;
	public int PersonalityID { get; set; } = -1;
	public string CharacterName { get; set; } = "New Hero";
	public string SelectedClass { get; set; } = "None";
	public string SelectedPersonality { get; set; } = "None";
	public Dictionary<string, float> Stats { get; private set; } = new Dictionary<string, float>();
	public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
	public string CurrentZone { get; set; } = "StartingArea";
	public List<int> Inventory { get; private set; } = new List<int>();
	public float CurrentHP { get; set; } = 100f;
	public float MaxHP { get; set; } = 100f;
	public float CurrentMP { get; set; } = 50f;
	public float MaxMP { get; set; } = 50f;
	public float CurrentST { get; set; } = 75f;
	public float MaxST { get; set; } = 75f;

	// ✅ Correctly define `current_stats` without `[Export]`
	public Dictionary<string, float> current_stats { get; private set; } = new Dictionary<string, float>();

	 public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
			SetProcess(false); // ✅ Disable processing if unused
		}
		else
		{
			QueueFree();
		}
	}

	// ✅ C# method for warrior.cs to safely get stats
	public float GetStat(string stat_name)
	{
		if (current_stats.ContainsKey(stat_name))
			return current_stats[stat_name];

		GD.PrintErr($"❌ Stat '{stat_name}' not found in CharacterData!");
		return 0.0f;  // Default if stat is missing
	}
	 // ✅ Public method to modify stats in CharacterCreation.cs
	public void SetStat(string statName, float value)
	{
		if (current_stats.ContainsKey(statName))
		{
			current_stats[statName] = value;
		}
		else
		{
			current_stats.Add(statName, value);
		}

		//GD.Print($"✅ Stat updated: {statName} = {value}");
	}

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
