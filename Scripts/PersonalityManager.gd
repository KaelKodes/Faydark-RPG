extends BaseLibrary

var current_personality = null

func _ready():
	var data = personality_library.get_all_personalities()
	print("Initialized personality data from library:", data)


# Retrieve a specific personality by name
func get_personality(Pname: String) -> Dictionary:
	if personality_library.personality_data.has(Pname):
		return personality_library.personality_data[Pname]
	print("Error: Personality", Pname, "not found.")
	return {}


# Example: Filter personalities based on a filter function
func get_filtered(filter_func: Callable) -> Dictionary:
	var result: Dictionary = {}
	for key in personality_library.personality_data.keys():
		if filter_func.call(personality_library.personality_data[key]):
			result[key] = personality_library.personality_data[key]
	return result


# Default filter for "base" personalities
func is_base(entry: Dictionary) -> bool:
	return entry.get("unlockable", 1) == 0

# Adjust a personality stat
func adjust_personality_stat(stat_name: String, value: float):
	if not personality_stats.has(stat_name):
		print("Error: Invalid personality stat:", stat_name)
		return
	personality_stats[stat_name] = clamp(personality_stats[stat_name] + value, 0, 100)
	print(stat_name, "adjusted to", personality_stats[stat_name])

# Track healing or exploration
func track_healing_or_exploration(action: String):
	healing_decisions.append(action)
	if healing_decisions.size() > 5:
		healing_decisions.pop_front()

	var heal_count = healing_decisions.count("heal")
	var explore_count = healing_decisions.count("explore")

	if heal_count >= 3:
		adjust_personality_stat("healing_priority", 0.1)
	elif explore_count >= 3:
		adjust_personality_stat("healing_priority", -0.1)

# Track combat behavior
func track_combat_action(action: String):
	combat_decisions.append(action)
	if combat_decisions.size() > 10:
		combat_decisions.pop_front()

	var melee_count = combat_decisions.count("melee")
	var ranged_count = combat_decisions.count("ranged")

	if melee_count / combat_decisions.size() >= 0.8:
		adjust_personality_stat("weapon_preference", -0.1)
	elif ranged_count / combat_decisions.size() >= 0.8:
		adjust_personality_stat("weapon_preference", 0.1)

# Track healing method
func track_healing_method(method: String):
	healing_methods.append(method)
	if healing_methods.size() > 5:
		healing_methods.pop_front()

	var item_count = healing_methods.count("item")
	var spell_count = healing_methods.count("spell")

	if item_count >= 3:
		adjust_personality_stat("healing_item_vs_spell", -0.1)
	elif spell_count >= 3:
		adjust_personality_stat("healing_item_vs_spell", 0.1)

# Apply personality modifiers to character stats
signal personality_applied(personality_data)

func apply_personality_to_character(character_node: Node, personality_data: Dictionary):
	if personality_data == null:
		print("Error: No personality selected.")
		return

	# Revert previous personality effects
	if current_personality != null:
		for stat_key in current_personality.get("traits", {}).keys():
			if character_node.base_stats.has(stat_key):
				var old_value = character_node.base_stats[stat_key]
				character_node.base_stats[stat_key] -= current_personality["traits"][stat_key]
				print("Reverted stat:", stat_key, "from", old_value, "to", character_node.base_stats[stat_key])

	# Apply new personality traits
	for stat_key in personality_data.get("traits", {}).keys():
		if character_node.base_stats.has(stat_key):
			var old_value = character_node.base_stats[stat_key]
			character_node.base_stats[stat_key] += personality_data["traits"][stat_key]
			print("Applied stat:", stat_key, "from", old_value, "to", character_node.base_stats[stat_key])

	# Update current personality
	current_personality = personality_data
	emit_signal("personality_applied", personality_data)
	print("Personality applied:", personality_data)


# Personality stats
var personality_stats = {
	"aggression": 50,
	"weapon_preference": 50,
	"attack_style": 50,
	"distance": 50,
	"focus": 50,
	"revealing": 50,
	"loot_vs_monsters": 50,
	"healing_priority": 50,
	"healing_use": 50,
	"healing_item_vs_spell": 50
}

# Action tracking
var healing_decisions = []
var combat_decisions = []
var healing_methods = []
