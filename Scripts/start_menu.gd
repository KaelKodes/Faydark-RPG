extends Control

@onready var new: Button = $MarginContainer/HBoxContainer/VBoxContainer/New
@onready var load: Button = $MarginContainer/HBoxContainer/VBoxContainer/Load
@onready var settings: Button = $MarginContainer/HBoxContainer/VBoxContainer/Settings
@onready var quit: Button = $MarginContainer/HBoxContainer/VBoxContainer/Quit
@export var start_level = preload("res://scenes/main.tscn")


func _on_new_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/CharacterCreation.tscn")


func _on_quit_pressed() -> void:
	get_tree().quit()
