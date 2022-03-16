extends KinematicBody2D

# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var speed = 600
onready var target = get_node('/root/main/Player')
var arc: float
var velocity: Vector2
# Called when the node enters the scene tree for the first time.
func _ready():
	look_at(target.position)
	arc = self.atan(target.position)
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(_delta):
#	pass

func _physics_process(_delta):
	velocity = move_and_slide(Vector2(cos(arc), sin(arc)) * speed)
	for i in get_slide_count():
		var collision = get_slide_collision(i)
		if(collision.collider.name == 'Player'):
			queue_free()
		
func atan(coord):
	return atan2((coord.y) - self.position.y, (coord.x) - self.position.x);

func _on_VisibilityNotifier2D_screen_exited():
	queue_free()


