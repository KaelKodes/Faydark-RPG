extends Node

# Personality System Notes:
# Each preference or attribute uses a scale from 0.0 to 1.0:
# - Aggression: 0.0 (avoids combat) to 1.0 (seeks combat).
# - WeaponPreference: 0.0 (melee focus) to 1.0 (ranged focus).
# - AttackStyle: 0.0 (physical attacks) to 1.0 (magic attacks).
# - Distance: 0.0 (close combat) to 1.0 (keeping distance).
# - ExplorationFocus: 0.0 (wandering) to 1.0 (linear exploration).
# - Revealing: 0.0 (minimal exploration) to 1.0 (full map reveal).
# - LootVsMonsters: 0.0 (prioritizes fighting monsters) to 1.0 (prioritizes looting).
# - ItemPriority: 0.0 (practical items) to 1.0 (high-value items).
# - UpgradeFocus: 0.0 (general loot) to 1.0 (weapons/armor upgrades).
# - HealingPriority: 0.0 (continue exploring) to 1.0 (prioritize healing).
# - HealingUse: 0.0 (out-damage threats) to 1.0 (heal frequently).
# - HealingItemVsSpell: 0.0 (prefers healing items) to 1.0 (prefers healing spells).
# - ResourceEfficiency: 0.0 (liberal use of resources) to 1.0 (hoards resources).
# - HazardAvoidance: 0.0 (ignores hazards) to 1.0 (actively avoids hazards).
# - AmbushReaction: 0.0 (retreats during ambushes) to 1.0 (fights back).
# - TreasurePriority: 0.0 (low priority for treasure) to 1.0 (high priority).
# - Courage: 0.0 (easily scared) to 1.0 (fearless).

# Return all personalities available in the game
func get_all_personalities() -> Array:
	var personalities = [
		get_aggressive_personality(),
		get_cautious_personality(),
		get_timid_personality()
	]

	# Debugging: Check if personalities are populated
	if personalities.size() == 0:
		print("ERROR: No personalities defined in PersonalityLibrary.")
	else:
		print("DEBUG: Personalities loaded: ", personalities)

	return personalities

func get_personality_data(personality_name: String) -> Dictionary:
	for personality in get_all_personalities():
		print("Checking personality:", personality["Pname"])  # Debugging
		if personality["Pname"] == personality_name:
			print("Personality matched:", personality_name)
			return personality
	print("No matching personality found for:", personality_name)
	return {}



func get_unlocked_personalities() -> Array:
	var unlocked_personalities = []
	for personality in get_all_personalities():
		if personality.has("Unlocked") and personality["Unlocked"] == 1:
			unlocked_personalities.append(personality)
	return unlocked_personalities




# Returns a dictionary representing the Aggressive personality
func get_aggressive_personality() -> Dictionary:
	return {
		"PID": 1,  # Unique Personality ID
		"Pname": "Aggressive",
		"Description": "A bold and fearless combatant who prioritizes offense over defense.",
		"Unlocked": 1,  # 0 = False, 1 = True; appears in character creation if true
		"Preferences": {
			"Aggression": 0.9,  # Strong preference for fighting over avoiding
			"WeaponPreference": 0.3,  # Leans toward melee
			"AttackStyle": 0.2,  # Focuses on physical attacks
			"Distance": 0.1,  # Prefers close combat
			"ExplorationFocus": 0.8,  # Prefers linear exploration
			"Revealing": 0.4,  # Reveals some areas, but not exhaustive
			"LootVsMonsters": 0.3,  # Prefers defeating monsters over looting
			"ItemPriority": 0.6,  # Balanced between high-value and practical items
			"UpgradeFocus": 0.8,  # Strong focus on upgrading weapons/armor
			"HealingPriority": 0.2,  # Rarely heals while exploring
			"HealingUse": 0.4,  # Balances healing and out-damaging threats
			"HealingItemVsSpell": 0.3,  # Prefers using healing items
			"ResourceEfficiency": 0.5,  # Balances resource use and conservation
			"HazardAvoidance": 0.3,  # Mild tendency to avoid hazards
			"AmbushReaction": 0.6,  # Moderate chance to fight back during ambushes
			"TreasurePriority": 0.4  # Balanced approach to treasure
		},
		"RiskThresholds": {"Health": 0.1, "Mana": 0.2, "Stamina": 0.2},
		"Courage": 0.7  # Will engage multiple enemies unless outnumbered significantly
	}

# Returns a dictionary representing the Cautious personality
func get_cautious_personality() -> Dictionary:
	return {
		"PID": 2,  # Unique Personality ID
		"Pname": "Cautious",
		"Description": "A strategic thinker who values balance between offense and defense.",
		"Unlocked": 1,  # 0 = False, 1 = True; appears in character creation if true
		"Preferences": {
			"Aggression": 0.5,  # Balanced between fighting and avoiding
			"WeaponPreference": 0.5,  # Neutral preference between melee and ranged
			"AttackStyle": 0.5,  # Balanced between physical and magic
			"Distance": 0.5,  # Can engage in close and ranged combat equally
			"ExplorationFocus": 0.6,  # Slight preference for linear exploration
			"Revealing": 0.7,  # Prefers to reveal most areas
			"LootVsMonsters": 0.5,  # Balances loot and defeating enemies
			"ItemPriority": 0.7,  # Prefers practical items slightly
			"UpgradeFocus": 0.5,  # Neutral on upgrade focus
			"HealingPriority": 0.6,  # Tends to heal more frequently
			"HealingUse": 0.6,  # Leans toward healing over out-damaging threats
			"HealingItemVsSpell": 0.5,  # Neutral preference
			"ResourceEfficiency": 0.6,  # Tends to conserve resources
			"HazardAvoidance": 0.7,  # Actively avoids hazards
			"AmbushReaction": 0.4,  # Cautious about fighting back in ambushes
			"TreasurePriority": 0.6  # Slight preference for treasure
		},
		"RiskThresholds": {"Health": 0.5, "Mana": 0.3, "Stamina": 0.3},
		"Courage": 0.5  # Balanced bravery
	}

# Returns a dictionary representing the Timid personality
func get_timid_personality() -> Dictionary:
	return {
		"PID": 3,  # Unique Personality ID
		"Pname": "Timid",
		"Description": "A cautious individual who avoids confrontation and prioritizes survival.",
		"Unlocked": 1,  # 0 = False, 1 = True; appears in character creation if true
		"Preferences": {
			"Aggression": 0.1,  # Avoids fighting when possible
			"WeaponPreference": 0.8,  # Prefers ranged attacks
			"AttackStyle": 0.9,  # Strong preference for magic attacks
			"Distance": 0.9,  # Prefers keeping distance
			"ExplorationFocus": 0.3,  # Wanders more than following a linear path
			"Revealing": 0.9,  # Prefers to reveal everything
			"LootVsMonsters": 0.8,  # Strong preference for looting over combat
			"ItemPriority": 0.9,  # Focuses on high-value items
			"UpgradeFocus": 0.4,  # Less emphasis on upgrades
			"HealingPriority": 0.8,  # Heals frequently while exploring
			"HealingUse": 0.9,  # Strong focus on healing over out-damaging threats
			"HealingItemVsSpell": 0.8,  # Prefers using healing spells
			"ResourceEfficiency": 0.8,  # Hoards resources
			"HazardAvoidance": 0.9,  # Actively avoids hazards
			"AmbushReaction": 0.2,  # Likely to retreat during ambushes
			"TreasurePriority": 0.8  # Strong preference for treasure
		},
		"RiskThresholds": {"Health": 0.75, "Mana": 0.4, "Stamina": 0.4},
		"Courage": 0.2  # Easily scared and retreats frequently
	}

# Calculate dynamic courage based on context
func calculate_effective_courage(base_courage: float, enemy_count: int, ally_modifier: float, health: float, max_health: float) -> float:
	var enemy_threshold = base_courage * 2 * (1 + ally_modifier)  # Adjust threshold by allies
	var health_factor = health / max_health  # Scale courage based on health proportion

	# Calculate effective courage
	var effective_courage = base_courage - ((enemy_count - enemy_threshold) / 10) - (1 - health_factor)
	return max(0.0, effective_courage)  # Ensure courage does not drop below 0

# Debugging utility for library initialization
func _ready():
	print("Testing PersonalityLibrary initialization...")
	var test_personalities = get_all_personalities()
	if test_personalities.size() == 0:
		print("ERROR: PersonalityLibrary returned no personalities.")
	else:
		print("DEBUG: PersonalityLibrary successfully loaded the following personalities:")
		for personality in test_personalities:
			print("- PID: ", personality["PID"], " | Name: ", personality["Pname"], " | Description: ", personality["Description"], " | Unlocked: ", personality["Unlocked"], " | Preferences: ", personality["Preferences"], " | Risk Thresholds: ", personality["RiskThresholds"], " | Courage: ", personality["Courage"])
