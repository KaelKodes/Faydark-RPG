extends BaseLibrary
class_name ClassLibrary

# Initialize the class library
func _ready():
	data = {
		"Warrior": {
			"name": "Warrior",
			"description": "A strong melee fighter specializing in close combat.",
			"unlockable": 0,
			"stats": {
				"str": 15, "con": 15, "dex": 10, "int": 10, "wis": 10, "cha": 10,
				"health": 120, "mana": 30, "stamina": 100, "attack_damage": 15,
				"defense": 10, "dodge": 5
			},
			"armor": {
				"starting_armor": "Mail",
				"highest_armor": "Plate"
			}
		},
		"Ranger": {
			"name": "Ranger",
			"description": "A nimble fighter skilled in ranged combat and evasion.",
			"unlockable": 0,
			"stats": {
				"str": 10, "con": 10, "dex": 15, "int": 10, "wis": 10, "cha": 10,
				"health": 100, "mana": 40, "stamina": 120, "attack_damage": 12,
				"defense": 8, "dodge": 10
			},
			"armor": {
				"starting_armor": "Leather",
				"highest_armor": "Mail"
			}
		},
		"Mage": {
			"name": "Mage",
			"description": "A master of elemental spells, capable of dealing devastating magical damage.",
			"unlockable": 0,
			"stats": {
				"str": 8, "con": 10, "dex": 10, "int": 15, "wis": 15, "cha": 12,
				"health": 80, "mana": 120, "stamina": 60, "attack_damage": 8,
				"defense": 5, "dodge": 3
			},
			"armor": {
				"starting_armor": "Cloth",
				"highest_armor": "Cloth"
			}
		}
	}

# Retrieve a specific class by name
func get_class_data(name: String) -> Dictionary:
	return data.get(name, null)


# Apply class stats to base stats
func apply_class(class_data: Dictionary, base_stats: Dictionary):
	if class_data == null or base_stats == null:
		print("Error: Missing class data or base stats.")
		return

	for stat_key in class_data["stats"].keys():
		base_stats[stat_key] += class_data["stats"][stat_key]

# Filter classes based on unlockable status
func get_unlockable_classes() -> Dictionary:
	return get_filtered(func(entry): return entry["unlockable"] == 0)
