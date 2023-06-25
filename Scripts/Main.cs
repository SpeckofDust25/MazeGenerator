using Godot;
using System;

public partial class Main : CanvasLayer
{
	private Vector2 vec;
	private Grid grid;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		grid = new Grid();
		vec = new Vector2(0, 2);
		GD.Print(vec);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
