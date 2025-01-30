using Godot;
using MySqlConnector;
using System;
using System.Collections.Generic;

public partial class CharacterCreation : Node2D
{
	private const string CONNECTION_STRING = "Server=localhost;Database=faydark_db;User=root;Password=;";

	private VBoxContainer vitalsBox;
	private VBoxContainer offenseBox;
	private VBoxContainer defenseBox;
	private VBoxContainer statsBox;
	private VBoxContainer mentalBox;  // Added Mental Stats Box

	private OptionButton classDropdown;
	private OptionButton personalityDropdown;

	private Dictionary<string, Label> statLabels = new Dictionary<string, Label>();

	private Dictionary<string, Dictionary<string, float>> classModifiers = new Dictionary<string, Dictionary<string, float>>();
	private Dictionary<string, Dictionary<string, float>> personalityModifiers = new Dictionary<string, Dictionary<string, float>>();
	private string selectedClass = "Base";
	private string selectedPersonality = "None";


	private Dictionary<string, float> baseStats = new Dictionary<string, float>();
	private string baseWeaponType = "None";
	private string baseArmorType = "None";

	public override void _Ready()
{
	// Find UI elements
	vitalsBox = GetNode<VBoxContainer>("PreviewContainer/VitalsBox");
	offenseBox = GetNode<VBoxContainer>("PreviewContainer/OffenseBox");
	defenseBox = GetNode<VBoxContainer>("PreviewContainer/DefenseBox");
	statsBox = GetNode<VBoxContainer>("PreviewContainer/StatsBox");
	mentalBox = GetNode<VBoxContainer>("Mental_VBox");  // Assign the Mental Stats box

	classDropdown = GetNode<OptionButton>("PlayerInputContainer/OptionButton");
	personalityDropdown = GetNode<OptionButton>("PlayerInputContainer/OptionButton2");

	// Load classes and stats
	selectedClass = "Base";
	LoadClasses();
	LoadPersonalities();
	LoadBaseStats();

	// ✅ Ensure `selectedPersonality` is set to a valid personality
	if (personalityDropdown.GetItemCount() > 0)
	{
		selectedPersonality = personalityDropdown.GetItemText(0);
		GD.Print($"✅ Default Personality Selected: {selectedPersonality}");
	}
}


	private void LoadClasses()
{
	using (var connection = new MySqlConnection(CONNECTION_STRING))
	{
		try
		{
			connection.Open();
			GD.Print("✅ Connected to MySQL - Loading Unlocked Classes...");

			string query = "SELECT * FROM classes WHERE unlocked = 1;";
			using (var command = new MySqlCommand(query, connection))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string className = reader["class_name"].ToString();
					GD.Print($"🔹 Adding Class: {className}");
					classDropdown.AddItem(className);

					// Store class modifiers
					Dictionary<string, float> modifiers = new Dictionary<string, float>();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						string column = reader.GetName(i);
						if (column.StartsWith("mod_"))
						{
							string statName = column.Substring(4); // Remove 'mod_' prefix
							modifiers[statName] = Convert.ToSingle(reader[column]);
						}
					}
					classModifiers[className] = modifiers;
				}
			}

			// 🛠 DEBUGGING: Print all stored class modifiers
			foreach (var key in classModifiers.Keys)
			{
				GD.Print($"✅ Class Stored: {key}");
			}

			classDropdown.Connect("item_selected", new Callable(this, nameof(OnClassSelected)));
		}
		catch (Exception ex)
		{
			GD.PrintErr("❌ Failed to load classes: ", ex.Message);
		}
	}
}




	private void LoadBaseStats()
{
	using (var connection = new MySqlConnection(CONNECTION_STRING))
	{
		try
		{
			connection.Open();
			GD.Print("✅ Connected to MySQL - Loading Base Stats...");

			string query = "SELECT * FROM classes WHERE class_name = 'Base';";
			using (var command = new MySqlCommand(query, connection))
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					baseStats.Clear();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						string column = reader.GetName(i);
						if (column.StartsWith("mod_"))
						{
							baseStats[column.Substring(4)] = Convert.ToSingle(reader[column]);
						}
					}
					GD.Print("🔹 Loaded Base Stats: ", string.Join(", ", baseStats.Keys));

					// ✅ Ensure all personality stats exist in baseStats (even if not in Base class)
					string[] missingStats = {
						"weapon_preference", "attack_style", "loot_vs_monsters",
						"item_priority", "upgrade_focus", "healing_priority", "healing_use",
						"healing_item_vs_spell", "resource_efficiency", "hazard_avoidance",
						"ambush_reaction", "treasure_priority", "exploration_focus"
					};
					foreach (string stat in missingStats)
					{
						if (!baseStats.ContainsKey(stat))
						{
							baseStats[stat] = 0.5f; // Default neutral value
							GD.Print($"🔹 Added missing base stat: {stat}");
						}
					}

					CreateStatLabels();
					UpdateUI();
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("❌ Failed to load Base stats: ", ex.Message);
		}
	}
}



	private void CreateStatLabels()
{
	// Clear all containers before adding stats
	ClearContainer(vitalsBox);
	ClearContainer(offenseBox);
	ClearContainer(defenseBox);
	ClearContainer(statsBox);
	ClearContainer(mentalBox);

	// 🔹 **Add Section Titles First**
	AddSectionTitle("Vitals", vitalsBox);
	AddSectionTitle("Offense", offenseBox);
	AddSectionTitle("Defense", defenseBox);
	AddSectionTitle("Stats", statsBox);
	// "Personality Traits" label is already in UI, so skipping mentalBox.

	foreach (var key in baseStats.Keys)
	{
		VBoxContainer targetBox = statsBox; // Default to General Stats

		// 🔹 **Vitals**
		if (key.Equals("health") || key.Equals("mana") || key.Equals("stamina") ||
			key.Equals("movement_speed") || key.Equals("hp_regen") || key.Equals("mp_regen") ||
			key.Equals("st_regen") || key.Equals("weight"))
		{
			targetBox = vitalsBox;
		}
		// 🔹 **Offense**
		else if (key.Equals("attack_damage") || key.Equals("ranged_damage") || key.Equals("spell_damage") ||
				 key.Equals("attack_range") || key.Equals("attack_speed") || key.Equals("cast_speed") ||
				 key.Equals("weapon_type") || key.Equals("hit_chance_bonus"))
		{
			targetBox = offenseBox;
		}
		// 🔹 **Defense**
		else if (key.Equals("armor_class") || key.Equals("armor_type") || key.Equals("defense") ||
				 key.Equals("dodge") || key.Equals("block") || key.Equals("parry"))
		{
			targetBox = defenseBox;
		}
		// 🔹 **Stats (General Character Stats)**
		else if (key.Equals("agility") || key.Equals("constitution") || key.Equals("dexterity") ||
				 key.Equals("intelligence") || key.Equals("strength") || key.Equals("wisdom") ||
				 key.Equals("charisma"))
		{
			targetBox = statsBox;
		}
		// 🔹 **Personality Stats (Mental)**
		else if (key.Equals("aggression") || key.Equals("weapon_preference") || key.Equals("attack_style") ||
				 key.Equals("distance") || key.Equals("exploration_focus") || key.Equals("revealing") ||
				 key.Equals("loot_vs_monsters") || key.Equals("item_priority") || key.Equals("upgrade_focus") ||
				 key.Equals("healing_priority") || key.Equals("healing_use") || key.Equals("healing_item_vs_spell") ||
				 key.Equals("resource_efficiency") || key.Equals("hazard_avoidance") || key.Equals("ambush_reaction") ||
				 key.Equals("treasure_priority") || key.Equals("courage") || key.Equals("subservience"))
		{
			targetBox = mentalBox;
		}

		// Add Label to the Correct Box
		AddStatLabel(key, key.Replace("_", " ").ToUpper(), targetBox);
	}
}

private void AddSectionTitle(string title, VBoxContainer container)
{
	Label titleLabel = new Label();
	titleLabel.Text = title.ToUpper(); // Convert to ALL CAPS
	titleLabel.AddThemeFontSizeOverride("font_size", 20);  // Larger font
	titleLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));  // White text
	titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty()); // Remove background

	container.AddChild(titleLabel);
}





	private void ClearContainer(VBoxContainer container)
	{
		foreach (Node child in container.GetChildren())
		{
			container.RemoveChild(child);
			child.QueueFree();
		}
	}

	private void AddStatLabel(string statKey, string statName, VBoxContainer container)
{
	Label statLabel = new Label();
	statLabel.Name = statKey;
	statLabel.Text = $"{FormatStatName(statName)}: {baseStats[statKey]}";
	container.AddChild(statLabel);
	statLabels[statKey] = statLabel;
}
	private string FormatStatName(string rawName)
{
	// Replace underscores with spaces, split words, capitalize each word
	string[] words = rawName.Split('_');
	for (int i = 0; i < words.Length; i++)
	{
		words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1); // Capitalize first letter
	}
	return string.Join(" ", words); // Rejoin words into a proper name
}


	private void UpdateUI(Dictionary<string, float> updatedStats = null)
{
	foreach (var key in statLabels.Keys)
	{
		statLabels[key].Text = updatedStats != null && updatedStats.ContainsKey(key)
			? $"{FormatStatName(key)}: {updatedStats[key]}"
			: $"{FormatStatName(key)}: {baseStats[key]}";
	}
}
private void LoadPersonalities()
{
	using (var connection = new MySqlConnection(CONNECTION_STRING))
	{
		try
		{
			connection.Open();
			GD.Print("✅ Connected to MySQL - Loading Unlocked Personalities...");

			string query = "SELECT * FROM PersonalityLibrary WHERE locked = 0;";
			using (var command = new MySqlCommand(query, connection))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string personalityName = reader["Personality_name"].ToString();
					GD.Print($"🔹 Adding Personality: {personalityName}");
					personalityDropdown.AddItem(personalityName);

					Dictionary<string, float> modifiers = new Dictionary<string, float>();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						string column = reader.GetName(i);
						if (!column.Equals("Personality_name") && !column.Equals("locked"))
						{
							modifiers[column] = Convert.ToSingle(reader[column]);
						}
					}
					personalityModifiers[personalityName] = modifiers;
				}
			}

			// 🛠 Debugging: Print all stored personality modifiers
			foreach (var personality in personalityModifiers.Keys)
			{
				GD.Print($"✅ Personality Stored: {personality}");
				foreach (var stat in personalityModifiers[personality])
				{
					GD.Print($"   🔹 {stat.Key}: {stat.Value}");
				}
			}

			personalityDropdown.Connect("item_selected", new Callable(this, nameof(OnPersonalitySelected)));
		}
		catch (Exception ex)
		{
			GD.PrintErr("❌ Failed to load personalities: ", ex.Message);
		}
	}
}





private void OnPersonalitySelected(long index)
{
	selectedPersonality = personalityDropdown.GetItemText((int)index);
	GD.Print($"🔄 Resetting to Base, then applying personality modifiers for: {selectedPersonality}");
	UpdateStats(); // This ensures stats update after selection
}

private void OnClassSelected(long index)
{
	selectedClass = classDropdown.GetItemText((int)index);
	GD.Print($"🔄 Resetting to Base, then applying class modifiers for: {selectedClass}");
	UpdateStats(); // This ensures stats update after selection
}


private void UpdateStats()
{
	Dictionary<string, float> updatedStats = new Dictionary<string, float>(baseStats);
	GD.Print("🔄 Resetting to Base Stats...");

	// Ensure all personality stats exist
	string[] missingStats = {
		"weapon_preference", "attack_style", "loot_vs_monsters",
		"item_priority", "upgrade_focus", "healing_priority", "healing_use",
		"healing_item_vs_spell", "resource_efficiency", "hazard_avoidance",
		"ambush_reaction", "treasure_priority", "exploration_focus"
	};
	foreach (string stat in missingStats)
	{
		if (!updatedStats.ContainsKey(stat))
		{
			updatedStats[stat] = 0.5f; // Default neutral value
			GD.Print($"🔹 Added missing updated stat: {stat}");
		}
	}

	// Apply class modifiers
	if (classModifiers.ContainsKey(selectedClass))
	{
		GD.Print($"✅ Applying Class Modifiers for: {selectedClass}");
		foreach (var key in classModifiers[selectedClass].Keys)
		{
			if (updatedStats.ContainsKey(key))
			{
				updatedStats[key] += classModifiers[selectedClass][key];
				GD.Print($"🔹 {key}: {updatedStats[key]} (after class mod)");
			}
			else
			{
				GD.PrintErr($"❌ Class Modifier Key Not Found: {key}");
			}
		}
	}
	else
	{
		GD.PrintErr($"❌ No class modifiers found for {selectedClass}");
	}

	// Apply personality modifiers
	if (personalityModifiers.ContainsKey(selectedPersonality))
	{
		GD.Print($"✅ Applying Personality Modifiers for: {selectedPersonality}");
		foreach (var key in personalityModifiers[selectedPersonality].Keys)
		{
			// Normalize stat names
			string formattedKey = key.ToLower()
				.Replace("weaponpreference", "weapon_preference")
				.Replace("attackstyle", "attack_style")
				.Replace("lootvsmonsters", "loot_vs_monsters")
				.Replace("itempriority", "item_priority")
				.Replace("upgradefocus", "upgrade_focus")
				.Replace("healingpriority", "healing_priority")
				.Replace("healinguse", "healing_use")
				.Replace("healingitemvsspell", "healing_item_vs_spell")
				.Replace("resourceefficiency", "resource_efficiency")
				.Replace("hazardavoidance", "hazard_avoidance")
				.Replace("ambushreaction", "ambush_reaction")
				.Replace("treasurepriority", "treasure_priority")
				.Replace("explorationfocus", "exploration_focus");

			if (updatedStats.ContainsKey(formattedKey))
			{
				updatedStats[formattedKey] += personalityModifiers[selectedPersonality][key];
				updatedStats[formattedKey] = (float)Math.Round(updatedStats[formattedKey], 2);
				GD.Print($"🔹 {formattedKey}: {updatedStats[formattedKey]} (after personality mod)");
			}
			else
			{
				GD.PrintErr($"❌ Personality Modifier Key Not Found: {formattedKey}");
			}
		}
	}
	else
	{
		GD.PrintErr($"❌ No personality modifiers found for {selectedPersonality}");
	}

	GD.Print("✅ Final Updated Stats: ");
	foreach (var key in updatedStats.Keys)
	{
		GD.Print($"🔹 {key}: {updatedStats[key]}");
	}

	UpdateUI(updatedStats); // Update UI with final stats
}

}
