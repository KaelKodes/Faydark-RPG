extends Node2D

# References to key nodes
@onready var line_edit: LineEdit = $VBoxContainer/LineEdit
@onready var class_dropdown: OptionButton = $VBoxContainer/OptionButton2
@onready var personality_dropdown: OptionButton = $VBoxContainer/OptionButton
@onready var preview_container: HBoxContainer = $PreviewContainer
@onready var vitals_box: VBoxContainer = $PreviewContainer/VitalsBox
@onready var offense_box: VBoxContainer = $PreviewContainer/OffenseBox
@onready var defense_box: VBoxContainer = $PreviewContainer/DefenseBox
@onready var stats_box: VBoxContainer = $PreviewContainer/StatsBox
@onready var start_game_button: Button = $"Start Game"

# Character properties
var selected_class = ""
var selected_personality = ""# Current selected personality
var current_personality = null

var character_name = ""
var character_node = null  # Reference to the character.gd instance

# Mapping of stats to their respective columns
var stat_columns = {}
var stat_nodes = {}  # Store direct references to stat labels

func _ready():
	# Preload character node
	character_node = preload("res://Scripts/character.gd").new()

	# Ensure class dropdown signal is connected
	if not class_dropdown.is_connected("item_selected", Callable(self, "_on_class_dropdown_item_selected")):
		class_dropdown.connect("item_selected", Callable(self, "_on_class_dropdown_item_selected"))

	# Debugging for personality dropdown
	print("Initializing personality dropdown...")
	var personalities = personality_library.get_all_personalities()
	if personalities.size() == 0:
		print("ERROR: No personalities found in the personality library!")
	else:
		print("Personality data loaded: ", personalities)

	# Ensure personality dropdown signal is connected
	var personality_dropdown = $VBoxContainer/OptionButton
	if not personality_dropdown.is_connected("item_selected", Callable(self, "_on_personality_selected")):
		personality_dropdown.connect("item_selected", Callable(self, "_on_personality_selected"))

	# Populate the personality dropdown
	for personality in personalities:
		if "Pname" in personality:
			print("Adding personality to dropdown: ", personality["Pname"])
			personality_dropdown.add_item(personality["Pname"])
		else:
			print("WARNING: Missing 'Pname' key in personality data: ", personality)

	# Initialize with the first personality's stats but allow updates from dropdown
	if personalities.size() > 0:
		var first_personality = personalities[0]
		personality_dropdown.select(0)  # Automatically select the first item
		Character.apply_personality_preview(first_personality)  # Apply its stats
		print("Initialized mental stats with first personality:", first_personality)
		update_mental_stats_ui()  # Update the UI

	# Populate other dropdowns and initialize stats
	populate_class_dropdown()
	initialize_stat_columns()
	populate_stat_labels()
	create_mental_stats_ui()




# Initialize stat-to-column mapping
func initialize_stat_columns():
	stat_columns = {
		# Vitals
		"health": vitals_box,
		"mana": vitals_box,
		"stamina": vitals_box,
		"movement_speed": vitals_box,
		"movement_range": vitals_box,
		"hp_regen": vitals_box,
		"mp_regen": vitals_box,
		"st_regen": vitals_box,
		"weight": vitals_box,

		# Offense
		"attack_damage": offense_box,
		"ranged_damage": offense_box,
		"spell_damage": offense_box,
		"attack_range": offense_box,
		"attack_speed": offense_box,
		"cast_speed": offense_box,
		"weapon_type": offense_box,
		"hit_chance_bonus": offense_box,

		# Defense
		"defense": defense_box,
		"dodge": defense_box,
		"block": defense_box,
		"armor_class": defense_box,
		"armor_type": defense_box,
		"parry": defense_box,

		# Remaining Stats
		"str": stats_box,
		"int": stats_box,
		"wis": stats_box,
		"dex": stats_box,
		"con": stats_box,
		"cha": stats_box
	}

# Populate class dropdown dynamically
func populate_class_dropdown():
	var classes = class_library.get_unlockable_classes()
	for class_key in classes.keys():
		var class_data = classes[class_key]
		class_dropdown.add_item(class_data["name"])

# Populate personality dropdown dynamically
func populate_personality_dropdown():
	if personality_dropdown == null:
		print("ERROR: PersonalityDropdown is not initialized!")
		return

	personality_dropdown.clear()  # Clear existing items
	var unlocked_personalities = personality_library.get_unlocked_personalities()
	if unlocked_personalities.size() > 0:
		for personality in unlocked_personalities:
			personality_dropdown.add_item(personality["Pname"])

		# Initialize mental stats with the first personality
		var first_personality = unlocked_personalities[0]
		personality_dropdown.select(0)  # Highlight first item
		Character.apply_personality_preview(first_personality)  # Apply personality
		update_mental_stats_ui()  # Refresh the UI
	else:
		print("No personalities unlocked.")




# Populate stat labels dynamically
func populate_stat_labels():
	# Clear existing labels and nodes
	stat_nodes.clear()
	for column in [vitals_box, offense_box, defense_box, stats_box]:
		for child in column.get_children():
			if child is HBoxContainer:
				child.queue_free()

	# Add new stat rows using HBoxContainer
	for stat_key in character_node.base_stats.keys():
		var column = stat_columns.get(stat_key, stats_box)  # Default to stats_box

		# Create a container for the stat (name and value)
		var stat_container = HBoxContainer.new()
		column.add_child(stat_container)

		# Create the name label
		var name_label = Label.new()
		name_label.text = stat_key.capitalize().replace("_", " ") + ":"
		name_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		stat_container.add_child(name_label)

		# Create the value label
		var value_label = Label.new()
		value_label.name = stat_key
		value_label.text = str(character_node.base_stats.get(stat_key, 0))  # Default to 0
		value_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		value_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
		stat_container.add_child(value_label)

		# Store reference
		stat_nodes[stat_key] = value_label
		print("Created label for stat:", stat_key)

func update_stat_labels():
	for stat_key in character_node.base_stats.keys():
		if stat_nodes.has(stat_key):
			var value_label = stat_nodes[stat_key]
			if value_label:
				value_label.text = str(character_node.base_stats[stat_key])
				print("Updated UI for stat:", stat_key, "to:", character_node.base_stats[stat_key])
			else:
				print("Warning: Missing label for stat:", stat_key)


# Update stats dynamically based on selections
func _update_stats():
	character_node.update_current_stats()
	for stat_key in character_node.base_stats.keys():
		if stat_nodes.has(stat_key):
			var value_label = stat_nodes[stat_key]
			value_label.text = str(character_node.current_stats[stat_key])

# Handle class selection
func _on_class_dropdown_item_selected(index):
	var selected_class_name = class_dropdown.get_item_text(index)
	var class_data = class_library.get_class_data(selected_class_name)
	if class_data:
		character_node.base_stats = {  # Reset stats
			"str": 0, "dex": 0, "con": 0, "int": 0, "wis": 0, "cha": 0,
			"health": 0, "mana": 0, "stamina": 0, "attack_damage": 0,
			"defense": 0, "dodge": 0, "block": 0
		}
		for stat_key in class_data["stats"].keys():
			character_node.base_stats[stat_key] = class_data["stats"][stat_key]
		update_stat_labels()

# Handle personality selection
func _on_personality_selected(index: int):
	var personality_dropdown = $VBoxContainer/OptionButton
	if personality_dropdown == null:
		print("ERROR: Personality dropdown is null.")
		return

	var personality_name = personality_dropdown.get_item_text(index)
	print("Selected personality:", personality_name)

	var personality_data = personality_library.get_personality_data(personality_name)
	if personality_data:
		Character.apply_personality_preview(personality_data)  # Update mental stats
		update_mental_stats_ui()  # Refresh UI
		print("Mental stats updated with selected personality:", Character.mental_stats)
	else:
		print("Error: Personality data not found for:", personality_name)




func refresh_stat_labels():
	populate_stat_labels()
	update_stat_labels()

func create_mental_stats_ui():
	# Clear existing children from Mental_VBox
	clear_children($Mental_VBox)

	# Dynamically create labels for each mental stat
	for stat_name in Character.mental_stats.keys():
		var label = Label.new()
		label.name = stat_name  # Use the stat name as the label's node name
		label.text = "%s: %.2f" % [stat_name, Character.mental_stats[stat_name]]
		$Mental_VBox.add_child(label)

	# Dynamically create labels for each mental stat
func update_mental_stats_ui():
	for child in $Mental_VBox.get_children():
		var stat_name = child.name
		if Character.mental_stats.has(stat_name):
			child.text = "%s: %.2f" % [stat_name, Character.mental_stats[stat_name]]

func clear_children(node: Node):
	for child in node.get_children():
		child.queue_free()



# Start the game
func _on_start_game_pressed():
	character_name = line_edit.text.strip_edges()

	# Validate character creation inputs
	if selected_class == "" or selected_personality == "" or character_name == "":
		print("Please complete character creation before starting!")
		return

	# Apply the selected personality to the character
	var personality_data = personality_library.get_personality_data(selected_personality)
	if personality_data:
		character_node.finalize_personality(personality_data)
	else:
		print("Error: Selected personality data not found!")
		return

	# Save character data globally or locally
	character_node.character_name = character_name
	character_node.character_class = selected_class
	character_node.save_character()

	# Transition to the next scene
	get_tree().change_scene("res://Scenes/GameScene.tscn")
