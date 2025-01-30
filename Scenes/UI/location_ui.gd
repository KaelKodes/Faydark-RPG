extends Control  # ✅ Ensure this matches the node type in your scene

func _ready():
	var time_manager = get_node_or_null("/root/TimeManager")
	if time_manager:
		print("✅ Connected to TimeManager!")
		time_manager.connect("time_updated", Callable(self, "_update_time_display"))
	else:
		print("❌ ERROR: TimeManager not found!")

func _update_time_display(day, hour, minute):
	var time_string = "Day %d, %02d:%02d" % [day, hour, minute]

	var time_label = get_node_or_null("TimeLabel")
	if time_label:
		time_label.text = time_string
		print("🕒 Time Updated in UI:", time_string)
	else:
		print("❌ ERROR: TimeLabel not found in LocationUI!")
