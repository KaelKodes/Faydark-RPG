extends Node2D

var is_shaking = false  # Flag to prevent repeated shaking

func _ready():
	# Ensure AnimatedSprite2D starts at frame 0
	if $AnimatedSprite2D:
		$AnimatedSprite2D.frame = 0
		$AnimatedSprite2D.stop()

		# Connect the animation_finished signal
		$AnimatedSprite2D.animation_finished.connect(_on_animation_finished)

	# Connect Area2D's body_entered signal
	if $Area2D and not $Area2D.body_entered.is_connected(_on_body_entered):
		$Area2D.body_entered.connect(_on_body_entered)

func _on_body_entered(body):
	# Only shake if not already shaking
	if body is CharacterBody2D and not is_shaking:
		is_shaking = true
		print("Tree was hit by a character!")
		$AnimatedSprite2D.play("shake")  # Play shake animation

func _on_animation_finished():
	if $AnimatedSprite2D.animation == "shake":
		$AnimatedSprite2D.stop()  # Stop the animation
		is_shaking = false  # Reset the flag
