extends Node

# ✅ Add the signal declaration
signal time_updated(day, hour, minute)

# Time Tracking
var is_active = false
var game_day: int = 1
var game_hour: int = 5
var game_minute: int = 0
var is_time_paused: bool = false
var time_speed: float = 1.0  # 1.0 = normal speed, later expand for 2x/4x
var time_timer: Timer = null

func set_active(state: bool):
	is_active = state
	print("⏳ TimeManager Active:", is_active)

func _ready():
	print("⏳ TimeManager Initialized!")

	# ✅ Ensure time_updated signal exists
	add_user_signal("time_updated")

	# Timer setup
	time_timer = Timer.new()
	add_child(time_timer)
	time_timer.wait_time = 1.0  # 1 real second = 1 in-game minute
	time_timer.autostart = true
	time_timer.one_shot = false
	time_timer.connect("timeout", Callable(self, "_advance_time"))

	update_time_display()

func _advance_time():
	if is_time_paused:
		return  # Stop updating if paused

	game_minute += 1
	if game_minute >= 60:
		game_minute = 0
		game_hour += 1
		if game_hour >= 24:
			game_hour = 0
			game_day += 1  # New Day!

	print("🕒 Time Updated: Day", game_day, "Time:", game_hour, ":", str(game_minute).pad_zeros(2))

	update_time_display()

	# ✅ Emit the signal so other scripts can update UI
	emit_signal("time_updated", game_day, game_hour, game_minute)

func update_time_display():
	var time_string = "Day %d, %02d:%02d" % [game_day, game_hour, game_minute]

	# ✅ Ensure LocationUI updates time correctly
	var location_ui = get_node_or_null("/root/GameScene/UI/LocationUI")
	if location_ui:
		location_ui._update_time_display(game_day, game_hour, game_minute)
