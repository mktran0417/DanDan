extends Node

#var file: String;
var file: String = "res://assets/audio/Logic-No-Pressure-Freestyle-.mp3.mp3"
var audio_player: AudioStreamPlayer = AudioStreamPlayer.new();
# Declare member variables here. Examples:
# var a = 2
# var b = "text"

# Called when the node enters the scene tree for the first time.
func _ready():
	var loaded = load(file)

	var audio_file = loaded.get_data();
	var save_file = File.new()
	save_file.open("res://Output//save.txt", File.WRITE);
	for data in audio_file:
		save_file.store_line(String(data));
	save_file.close();
	print(audio_file.size());
	audio_player.set_stream(loaded);
	add_child(audio_player);
	audio_player.play();
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

func load(path):
	var load_audio_stream;
	var exists = File.new();
	if(exists.file_exists(path)):
		load_audio_stream = load(path);
	return load_audio_stream;
