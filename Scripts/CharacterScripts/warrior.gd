extends CharacterBody2D

func _physics_process(delta):
	var direction = Vector2.ZERO

	# ✅ Get CharacterData instance safely
	var character_data = get_node_or_null("/root/CharacterData")

	if character_data == null:
		print("❌ CharacterData not found!")
		return

	# ✅ Get movement speed using GetStat()
	var speed = character_data.call("GetStat", "movement_speed")

	# Get movement input
	if Input.is_action_pressed("ui_right"):
		direction.x += 1
	if Input.is_action_pressed("ui_left"):
		direction.x -= 1
	if Input.is_action_pressed("ui_down"):
		direction.y += 1
	if Input.is_action_pressed("ui_up"):
		direction.y -= 1

	# Normalize direction to avoid diagonal speed boost
	direction = direction.normalized()

	# Move character
	velocity = direction * speed * delta  # ✅ Ensures frame-rate independent movement
	move_and_slide()


	# Play animation based on direction
	if direction != Vector2.ZERO:
		var anim_name = get_animation_from_direction(direction)
		$AnimatedSprite2D.play(anim_name)
	else:
		$AnimatedSprite2D.pause()

# Determine animation based on movement direction
func get_animation_from_direction(direction: Vector2) -> String:
	if direction.x > 0 and direction.y == 0:
		return "walk_E"
	elif direction.x < 0 and direction.y == 0:
		return "walk_W"
	elif direction.y > 0 and direction.x == 0:
		return "walk_S"
	elif direction.y < 0 and direction.x == 0:
		return "walk_N"
	elif direction.x > 0 and direction.y > 0:
		return "walk_SE"
	elif direction.x > 0 and direction.y < 0:
		return "walk_NE"
	elif direction.x < 0 and direction.y > 0:
		return "walk_SW"
	elif direction.x < 0 and direction.y < 0:
		return "walk_NW"
	return "idle"
