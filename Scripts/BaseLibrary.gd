extends Node
class_name BaseLibrary

# Core data storage
var data: Dictionary = {}

# Add a new entry
func add_entry(key: String, value: Dictionary):
	data[key] = value

# Remove an entry
func remove_entry(key: String):
	data.erase(key)

# Get all entries
func get_all() -> Dictionary:
	return data

# Filter entries (e.g., "base" entries only)
func get_filtered(filter_func: Callable) -> Dictionary:
	var result: Dictionary = {}
	for key in data.keys():
		if filter_func.call(data[key]):
			result[key] = data[key]
	return result

# Default filter for "base" entries
func is_base(entry: Dictionary) -> bool:
	return entry.get("unlockable", 1) == 0  # Default to "locked" if unlockable is missing
