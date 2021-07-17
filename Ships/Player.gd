extends KinematicBody2D


# Declare member variables here. Examples:
var Bullet = preload("res://Bullets//Bullet.tscn");
export var speed: int = 670;
var screen_size;

# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	screen_size = get_viewport_rect().size;

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(delta):
	var velocity = Vector2()  # The player's movement vector.
	if InputEventMouseMotion:
		rotation = get_global_mouse_position().angle_to_point(position);
	if Input.is_action_just_pressed("shift"):
		if speed == 670:
			speed = 1000;
			print(speed);
		elif speed == 1000:
			speed = 670;
			print(speed);
	if Input.is_action_pressed("right"):
		velocity.x += 1;
	if Input.is_action_pressed("left"):
		velocity.x -= 1;
	if Input.is_action_pressed("down"):
		velocity.y += 1;
	if Input.is_action_pressed("up"):
		velocity.y -= 1;
	if velocity.length() > 0:
		velocity = velocity.normalized() * speed;
	if Input.is_action_pressed("space"):
		shoot();

	position += velocity * delta;
	position.x = clamp(position.x, 0, screen_size.x)
	position.y = clamp(position.y, 0, screen_size.y)

	
func rotate_player():
	var coord: Vector2 = get_viewport().get_mouse_position(); 
	return atan2((coord.y) - position.y, (coord.x) - position.x);
		
	

func shoot():
	#spawn a projectile
	var bullet = Bullet.instance();
	bullet.start(position - Vector2(-10,0).rotated(rotation), rotation);
	get_parent().add_child(bullet);

