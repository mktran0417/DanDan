extends KinematicBody2D

export var speed:int = 1000;
var velocity: Vector2 = Vector2()


func start(pos, dir):
	rotation = dir;
	position = pos;
	velocity = Vector2(speed, 0).rotated(rotation)

func _physics_process(_delta):
	var collision = move_and_collide(velocity)
	if collision:
		velocity = velocity.bounce(collision.normal)
		if collision.collider.has_method("hit"):
			collision.collider.hit()

func _on_VisibilityNotifier2D_screen_exited():
	queue_free()
