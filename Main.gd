extends Node

export(PackedScene) var circles
# Called when the node enters the scene tree for the first time.
func _ready():
	$CircleTimer.start()	
	# Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass
	
func _on_CircleTimer_timeout():
	# Create a new instance of the Mob scene.	
	var circle = circles.instance()

	# Choose a random location on Path2D.
	var SpawnLoc = get_node("SpawnPath/SpawnLoc")
	SpawnLoc.offset = randi()
	
	circle.position = SpawnLoc.position
	
	
	add_child(circle)
