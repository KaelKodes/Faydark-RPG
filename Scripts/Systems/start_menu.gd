extends Control


@onready var new_button: Button = $MarginContainer/HBoxContainer/VBoxContainer/New_Button
@onready var load_button: Button = $MarginContainer/HBoxContainer/VBoxContainer/Load_Button
@onready var settings_button: Button = $MarginContainer/HBoxContainer/VBoxContainer/Settings_Button
@onready var quit_button: Button = $MarginContainer/HBoxContainer/VBoxContainer/Quit_Button


func _ready() -> void:
	# ✅ Disable GameScene & SceneManager at startup
	if has_node("/root/GameScene"):
		get_node("/root/GameScene").set_process(false)
	if has_node("/root/SceneManager"):
		get_node("/root/SceneManager").set_process(false)

func _on_new_button_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/UI/CharacterCreation.tscn")


func _on_quit_button_pressed() -> void:
	get_tree().quit()
