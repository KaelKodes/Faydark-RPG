extends CharacterBody2D

@export var speed: float = 50.0  # Slimes move slower

#func _physics_process(delta):
#	var direction = Vector2.ZERO

	# Example AI movement (replace with AI logic later)
	#if Input.is_action_pressed("ui_right"):  # Temporary manual control for testing
	#	direction.x += 1
	#if Input.is_action_pressed("ui_left"):
	#	direction.x -= 1
	#if Input.is_action_pressed("ui_down"):
	#	direction.y += 1
	#if Input.is_action_pressed("ui_up"):
	#	direction.y -= 1

	# Normalize direction to prevent diagonal speed boost
	#direction = direction.normalized()

	# Determine animation and flipping
	#if direction != Vector2.ZERO:
	#	var anim_name = get_animation_from_direction(direction, "walk")
	#	$AnimatedSprite2D.play(anim_name)
	#else:
	#	var anim_name = get_animation_from_direction(Vector2(0, 0), "idle")
	#	$AnimatedSprite2D.play(anim_name)

	# Flip sprite if moving West
	#if direction.x < 0:
	#	$AnimatedSprite2D.flip_h = true
	#elif direction.x > 0:
	#	$AnimatedSprite2D.flip_h = false

	# Move the Slime
	#velocity = direction * speed
	#move_and_slide()

# Function to get correct animation
func get_animation_from_direction(direction: Vector2, action: String) -> String:
	if direction.x != 0:  # Moving left or right
		return action + "_E"  # Always use East-facing animation
	elif direction.y > 0:
		return action + "_S"
	elif direction.y < 0:
		return action + "_N"
	return action + "_S"  # Default to South idle
