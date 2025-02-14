using Godot;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

	private LineEdit characterNameInput;
	public string CharacterName { get; private set; } = "New Hero"; // Default name

	private Dictionary<string, Dictionary<string, float>> classModifiers = new Dictionary<string, Dictionary<string, float>>();
	private Dictionary<string, Dictionary<string, float>> personalityModifiers = new Dictionary<string, Dictionary<string, float>>();
	// Stores PersonalityID for each personality name
	private Dictionary<string, int> personalityIDs = new Dictionary<string, int>();
	private Dictionary<string, bool> personalityLocked = new Dictionary<string, bool>();


	private string selectedClass = "Base";
	private string selectedPersonality = "None";


	private Dictionary<string, float> baseStats = new Dictionary<string, float>();
	private string baseWeaponType = "None";
	private string baseArmorType = "None";

	public override void _Ready()
	{
		// Find UI elements
		characterNameInput = GetNode<LineEdit>("PlayerInputContainer/LineEdit");
		vitalsBox = GetNode<VBoxContainer>("PreviewContainer/VitalsBox");
		offenseBox = GetNode<VBoxContainer>("PreviewContainer/OffenseBox");
		defenseBox = GetNode<VBoxContainer>("PreviewContainer/DefenseBox");
		statsBox = GetNode<VBoxContainer>("PreviewContainer/StatsBox");
		mentalBox = GetNode<VBoxContainer>("Mental_VBox");

		classDropdown = GetNode<OptionButton>("PlayerInputContainer/OptionButton");
		personalityDropdown = GetNode<OptionButton>("PlayerInputContainer/OptionButton2");

		// ✅ Connect personality dropdown to selection function
		if (!personalityDropdown.IsConnected("item_selected", new Callable(this, nameof(OnPersonalitySelected))))
		{
			GD.Print("✅ Connecting personalityDropdown to OnPersonalitySelected()");
			personalityDropdown.Connect("item_selected", new Callable(this, nameof(OnPersonalitySelected)));
		}
		else
		{
			GD.Print("⚠️ personalityDropdown is already connected.");
		}


		// Load data
		selectedClass = "Base"; // ✅ This is correct
		LoadClasses();
		LoadPersonalities();
		LoadBaseStats();

		// ✅ Ensure `selectedPersonality` is set to a valid personality AFTER loading
		if (personalityDropdown.GetItemCount() > 0)
		{
			selectedPersonality = personalityDropdown.GetItemText(0);
			GD.Print($"✅ Default Personality Selected: {selectedPersonality}");

			// ✅ Manually trigger personality selection to ensure modifiers apply
			OnPersonalitySelected(0);
		}
		else
		{
			GD.PrintErr("❌ No personalities were loaded. Check LoadPersonalities().");
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

			// ✅ Manually add "Base" class to classModifiers
			if (!classModifiers.ContainsKey("Base"))
			{
				GD.Print("⚠️ 'Base' class not found in database. Adding manually...");
				classModifiers["Base"] = new Dictionary<string, float>(); // Initialize empty stats
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
		GD.Print("🔄 Updating UI with new stats...");

		foreach (var key in statLabels.Keys)
		{
			if (updatedStats != null && updatedStats.ContainsKey(key))
			{
				float roundedValue = (float)Math.Round(updatedStats[key], 2); // ✅ Round to 2 decimals
				statLabels[key].Text = $"{FormatStatName(key)}: {roundedValue.ToString("0.00")}"; // ✅ Always 2 decimals
				//GD.Print($"🔹 UI Updated: {key} = {roundedValue.ToString("0.00")}");
			}
			else if (baseStats.ContainsKey(key))
			{
				float roundedValue = (float)Math.Round(baseStats[key], 2);
				statLabels[key].Text = $"{FormatStatName(key)}: {roundedValue.ToString("0.00")}";
			}
			else
			{
				GD.PrintErr($"❌ Warning: Key '{key}' not found in baseStats or updatedStats.");
			}
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

				string query = "SELECT * FROM PersonalityLibrary WHERE Locked = 0;";
				using (var command = new MySqlCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					personalityDropdown.Clear(); // ✅ Clear the dropdown before adding new items

					while (reader.Read())
					{
						string personalityName = reader["personality_name"].ToString();
						int personalityID = Convert.ToInt32(reader["personalityid"]);  // Ensure correct ID column

						GD.Print($"🔹 Found Personality: {personalityName} (ID: {personalityID})");

						// ✅ Add personality to dropdown
						personalityDropdown.AddItem(personalityName);

						Dictionary<string, float> modifiers = new Dictionary<string, float>();

						// Extract only numeric stat columns
						for (int i = 0; i < reader.FieldCount; i++)
						{
							string column = reader.GetName(i).ToLower(); // Normalize to lowercase

							// Ignore non-numeric columns
							if (column.Equals("personality_name") || column.Equals("personalityid") || column.Equals("locked"))
							{
								continue; // Skip non-stat columns
							}

							// ✅ Ensure column value is a valid float before converting
							if (float.TryParse(reader[column].ToString(), out float value))
							{
								modifiers[column] = value;
							}
							else
							{
								GD.PrintErr($"❌ Failed to convert personality stat '{column}' to float.");
							}
						}

						// ✅ Debug: Print all valid personality stat keys
						GD.Print($"✅ Personality '{personalityName}' modifiers: {string.Join(", ", modifiers.Keys)}");

						// Store the correctly formatted personality modifiers
						personalityModifiers[personalityName] = modifiers;


					}
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("❌ Failed to load personalities: ", ex.Message);
			}

		}
	}

	private void OnPersonalitySelected(long index)
	{
		GD.Print($"✅ OnPersonalitySelected() CALLED with index: {index}");

		selectedPersonality = personalityDropdown.GetItemText((int)index);
		GD.Print($"✅ Personality Selected: {selectedPersonality}");

		if (!personalityModifiers.ContainsKey(selectedPersonality))
		{
			GD.PrintErr($"❌ Personality '{selectedPersonality}' not found in personalityModifiers!");
			return;
		}

		// Apply modifiers
		Dictionary<string, float> updatedStats = UpdateStats();
		//GD.Print($"🔹 Updated Stats After Personality Change: {string.Join(", ", updatedStats)}");

		// Update the UI
		UpdateUI(updatedStats);
	}

	private void OnClassSelected(long index)
	{
		selectedClass = classDropdown.GetItemText((int)index);
		GD.Print($"🔄 Resetting to Base, then applying class modifiers for: {selectedClass}");

		// Call UpdateStats() and store result
		Dictionary<string, float> updatedStats = UpdateStats();

		// Pass the updated stats to UpdateUI
		UpdateUI(updatedStats);
	}

	private Dictionary<string, float> UpdateStats()
	{
		Dictionary<string, float> updatedStats = new Dictionary<string, float>(baseStats);
		GD.Print("🔄 Resetting to Base Stats...");

		// ✅ Step 1: Reset all personality-related stats before applying any new modifiers
		if (personalityModifiers.ContainsKey(selectedPersonality))
		{
			GD.Print($"🔄 Resetting previous personality modifiers before applying {selectedPersonality}...");

			foreach (var key in personalityModifiers[selectedPersonality].Keys)
			{
				if (updatedStats.ContainsKey(key) && baseStats.ContainsKey(key))
				{
					updatedStats[key] = baseStats[key]; // Reset to Base class default
					//GD.Print($"🔄 Reset {key} to base value: {updatedStats[key]}");
				}
			}
		}

		// ✅ Step 2: Apply Personality Modifiers FIRST
		if (personalityModifiers.ContainsKey(selectedPersonality))
		{
			GD.Print($"✅ Applying Personality Modifiers for: {selectedPersonality}");
			GD.Print($"🔍 Available personality keys: {string.Join(", ", personalityModifiers[selectedPersonality].Keys)}");

			foreach (var key in personalityModifiers[selectedPersonality].Keys)
			{
				if (!updatedStats.ContainsKey(key))
				{
					updatedStats[key] = 0.5f; // Default neutral value if missing
					GD.Print($"🔹 Added missing personality stat: {key}");
				}

				float before = updatedStats[key];
				updatedStats[key] += personalityModifiers[selectedPersonality][key];

				// ✅ Clamp value to stay between 0 and 1
				updatedStats[key] = Mathf.Clamp(updatedStats[key], 0.00f, 1.00f);

				float after = updatedStats[key];
				//GD.Print($"🔹 {key}: {before} -> {after} (after personality mod, clamped)");

			}
		}
		else
		{
			GD.PrintErr($"❌ No personality modifiers found for {selectedPersonality}");
		}

		// ✅ Step 3: Apply Class Modifiers SECOND
		if (classModifiers.ContainsKey(selectedClass))
		{
			GD.Print($"✅ Applying Class Modifiers for: {selectedClass}");

			foreach (var key in classModifiers[selectedClass].Keys)
			{
				if (updatedStats.ContainsKey(key))
				{
					float before = updatedStats[key];
					updatedStats[key] += classModifiers[selectedClass][key];

					// ✅ Clamp PTs to keep value between 0 and 1
					// Only clamp personality-related stats
if (personalityModifiers.ContainsKey(selectedPersonality) && personalityModifiers[selectedPersonality].ContainsKey(key))
{
	updatedStats[key] = Mathf.Clamp(updatedStats[key], 0.00f, 1.00f);
	//GD.Print($"🔹 Personality Stat Clamped: {key} -> {updatedStats[key]}");
}
else
{
	//GD.Print($"🔹 Non-Personality Stat (No Clamp): {key} -> {updatedStats[key]}");
}


					float after = updatedStats[key];
					//GD.Print($"🔹 {key}: {before} -> {after} (after class mod, clamped)");
				}
			}
		}
		else
		{
			GD.PrintErr($"❌ No class modifiers found for {selectedClass}");
		}

		// ✅ Debug output before returning stats
		//GD.Print($"✅ Final Stats After Modifiers Applied: {string.Join(", ", updatedStats)}");

		return updatedStats;
	}

private async void OnConfirmPressed()
{
	CharacterName = characterNameInput.Text.Trim();
	if (CharacterName == "")
	{
		CharacterName = "New Hero";
	}

	GD.Print($"✅ Character Name Set: {CharacterName}");

	CharacterData characterData = CharacterData.Instance;

	int selectedPersonalityID = personalityIDs.ContainsKey(selectedPersonality) ? personalityIDs[selectedPersonality] : -1;
	bool isPersonalityLocked = personalityLocked.ContainsKey(selectedPersonality) ? personalityLocked[selectedPersonality] : false;

	Dictionary<string, float> fullStats = new Dictionary<string, float>(baseStats);

	// ✅ Apply class modifiers safely
	foreach (var key in classModifiers[selectedClass].Keys)
	{
		if (fullStats.ContainsKey(key))
		{
			fullStats[key] += classModifiers[selectedClass][key];
		}
	}

	// ✅ Apply personality modifiers safely
	if (personalityModifiers.ContainsKey(selectedPersonality))
	{
		foreach (var key in personalityModifiers[selectedPersonality].Keys)
		{
			if (fullStats.ContainsKey(key))
			{
				fullStats[key] += personalityModifiers[selectedPersonality][key];
			}
			else
			{
				GD.PrintErr($"⚠️ Warning: Personality stat '{key}' not found in baseStats.");
			}
		}
	}
	else
	{
		GD.PrintErr($"❌ No personality modifiers found for {selectedPersonality}");
	}

	Vector3 defaultPosition = new Vector3(0, 0, 0);
	string defaultZone = "StartingArea";
	List<int> emptyInventory = new List<int>();

	characterData.SetCharacterData(-1, CharacterName, selectedClass, selectedPersonality, fullStats, defaultPosition, defaultZone, emptyInventory);
	characterData.PersonalityID = selectedPersonalityID;

	// ✅ Correct way to update `current_stats`
	foreach (var stat in fullStats)
	{
		characterData.SetStat(stat.Key, stat.Value);
	}

	characterData.PrintCharacterData();

	// ✅ Load World first
	GD.Print("🌍 Loading World Scene...");
	GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
}


}
