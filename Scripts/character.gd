extends Node

# Player stats
var base_stats = {
	"str": 10, "dex": 10, "con": 10, "int": 10, "wis": 10, "cha": 10,
	"health": 100, "mana": 50, "stamina": 75, "movement_speed": 100,
	"movement_range": 5, "hp_regen": 5, "mp_regen": 3, "st_regen": 4, "weight": 0,
	"attack_damage": 15, "ranged_damage": 10, "spell_damage": 12,
	"attack_range": 1, "attack_speed": 1.0, "cast_speed": 1.2, "weapon_type": "melee",
	"hit_chance_bonus": 5, "defense": 10, "armor_class": 12, "armor_type": "light",
	"dodge": 5, "block": 10, "parry": 8
}

var gear_stats = {
	"str": 0, "dex": 0, "con": 0, "int": 0, "wis": 0, "cha": 0,
	"health": 0, "mana": 0, "stamina": 0, "movement_speed": 0,
	"movement_range": 0, "hp_regen": 0, "mp_regen": 0, "st_regen": 0, "weight": 0,
	"attack_damage": 0, "ranged_damage": 0, "spell_damage": 0,
	"attack_range": 0, "attack_speed": 0, "cast_speed": 0, "weapon_type": "",
	"hit_chance_bonus": 0, "defense": 0, "armor_class": 0, "armor_type": "",
	"dodge": 0, "block": 0, "parry": 0
}

var mental_stats = {
	"Aggression": 0.5,
	"WeaponPreference": 0.5,
	"AttackStyle": 0.5,
	"Distance": 0.5,
	"ExplorationFocus": 0.5,
	"Revealing": 0.5,
	"LootVsMonsters": 0.5,
	"ItemPriority": 0.5,
	"UpgradeFocus": 0.5,
	"HealingPriority": 0.5,
	"HealingUse": 0.5,
	"HealingItemVsSpell": 0.5,
	"ResourceEfficiency": 0.5,
	"HazardAvoidance": 0.5,
	"AmbushReaction": 0.5,
	"TreasurePriority": 0.5,
	"Health": 0.5,
	"Mana": 0.5,
	"Stamina": 0.5,
	"Courage": 0.5
}

var current_stats = {}
var level = 1
var experience = 0

# Initialize stats
func _ready():
	# Calculate current stats as base + gear
	update_current_stats()
	print("Character initialized with stats:", current_stats)

# Update current stats based on base and gear stats
func update_current_stats():
	for stat in base_stats.keys():
		if stat == "weapon_type" or stat == "armor_type":
			current_stats[stat] = gear_stats[stat] if gear_stats[stat] != "" else base_stats[stat]
		else:
			current_stats[stat] = base_stats[stat] + gear_stats[stat]

	# Calculate derived stats if needed
	current_stats["max_health"] = current_stats["health"]
	current_stats["max_mana"] = current_stats["mana"]
	current_stats["max_stamina"] = current_stats["stamina"]

# Modify a specific stat
func modify_stat(stat_name: String, value: int):
	if not current_stats.has(stat_name):
		print("Error: Stat does not exist:", stat_name)
		return

	current_stats[stat_name] += value
	print(stat_name, "updated to", current_stats[stat_name])

# Save character data to a file
func save_character():
	var save_data = {
		"base_stats": base_stats,
		"gear_stats": gear_stats,
		"current_stats": current_stats,
		"level": level,
		"experience": experience
	}
	var file = FileAccess.open("user://character_save.json", FileAccess.WRITE)
	file.store_string(JSON.stringify(save_data))
	file.close()
	print("Character saved.")

# Load character data from a file
func load_character():
	var file = FileAccess.open("user://character_save.json", FileAccess.READ)
	if not file:
		print("No save file found.")
		return

	var json_instance = JSON.new()
	var parse_result = json_instance.parse(file.get_as_text())
	if parse_result.error != OK:
		print("Failed to parse JSON:", JSON.new().error_string(parse_result.error))
		return
	var save_data = parse_result.result
	file.close()

	base_stats = save_data["base_stats"]
	gear_stats = save_data["gear_stats"]
	current_stats = save_data["current_stats"]
	level = save_data["level"]
	experience = save_data["experience"]
	print("Character loaded with stats:", current_stats)

# Reset a stat to its base value
func reset_stat(stat_name: String):
	if base_stats.has(stat_name):
		current_stats[stat_name] = base_stats[stat_name]
		print(stat_name, "reset to base value:", base_stats[stat_name])

# Level up the character
func level_up():
	level += 1
	experience = 0  # Reset experience after leveling up
	# Example: Add to base stats on level up
	base_stats["health"] += 10
	base_stats["mana"] += 5
	base_stats["stamina"] += 7
	base_stats["attack_damage"] += 2
	base_stats["carry_capacity"] += 5
	update_current_stats()
	print("Level up! New level:", level, "Stats:", current_stats)

# Gain experience
func gain_experience(amount: int):
	experience += amount
	print("Gained experience:", amount, "Total experience:", experience)
	if experience >= 100:  # Example threshold for leveling up
		level_up()

func apply_personality_preview(personality_data: Dictionary):
	for key in mental_stats.keys():
		mental_stats[key] = 0.5

	print("Reset mental stats to 0.5")

	for pref_key in personality_data.get("Preferences", {}).keys():
		mental_stats[pref_key] = personality_data["Preferences"][pref_key]

	for threshold_key in personality_data.get("RiskThresholds", {}).keys():
		mental_stats[threshold_key] = personality_data["RiskThresholds"][threshold_key]

	if personality_data.has("Courage"):
		mental_stats["Courage"] = personality_data["Courage"]

	print("Mental stats updated:", mental_stats)


func finalize_personality(personality_data: Dictionary):
	for key in personality_data.keys():
		if mental_stats.has(key):
			mental_stats[key] = personality_data[key]
