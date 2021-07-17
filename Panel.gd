extends Panel


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var root;
var current_scene = null;

# Called when the node enters the scene tree for the first time.
#func _ready():

func _on_Button_pressed():
	Global.goto_scene("res://Main.tscn")
	# Called every frame. 'delta' is the elapsed time since the previous frame.
	#func _process(delta):
	#	pass
	# Add the next level
