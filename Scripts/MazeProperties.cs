using Godot;
using System;

public partial class MazeProperties : Panel
{
	private Button save_image_button;
	private Button generate_maze_button;

	[Signal] public delegate void SaveImageEventHandler();
	[Signal] public delegate void GenerateMazeEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		SetupNodes();
		SetupConnections();
	}

	//Setup Methods
	private void SetupNodes() {
		save_image_button = GetNode<Button>("VBoxContainer/SaveImageButton");
		generate_maze_button = GetNode<Button>("VBoxContainer/GenerateMazeButton");
	}

	private void SetupConnections() {
        save_image_button.Pressed += SaveImagePressed;
		generate_maze_button.Pressed += GenerateMazePressed;
    }

	//Connections
	private void SaveImagePressed() {
		EmitSignal(SignalName.SaveImage);
	}

	private void GenerateMazePressed() {
		EmitSignal(SignalName.GenerateMaze);
	}
}
