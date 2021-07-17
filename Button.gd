extends Button


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
#func _ready():

func _on_Button_pressed():
	get_node("Label").text = "HELLO!";
# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
