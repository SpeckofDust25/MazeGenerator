using Godot;
using System;

public partial class MazeProperties : Panel
{
	private Button save_image_button, generate_maze_button;
	private OptionButton maze_type_option_button;
	private SpinBox cells_x_spin_box, cells_y_spin_box;
	private SpinBox cell_size_spin_box, wall_size_spin_box;

	[Signal] public delegate void SaveImageEventHandler();
	[Signal] public delegate void GenerateMazeEventHandler();
	[Signal] public delegate void MazeTypeEventHandler();
	[Signal] public delegate void CellsXEventHandler();
	[Signal] public delegate void CellsYEventHandler();
	[Signal] public delegate void CellSizeEventHandler();
	[Signal] public delegate void WallSizeEventHandler();

	public override void _Ready() {
		SetupNodes();
		SetupConnections();
	}

	//Setup Methods
	private void SetupNodes() {
		save_image_button = GetNode<Button>("VBoxContainer/SaveImageButton");
		generate_maze_button = GetNode<Button>("VBoxContainer/GenerateMazeButton");
		maze_type_option_button = GetNode<OptionButton>("VBoxContainer/TypeHBoxContainer/MazeTypeOptionButton");
		cells_x_spin_box = GetNode<SpinBox>("VBoxContainer/CellsHBoxContainer/CellsXSpinBox");
		cells_y_spin_box = GetNode<SpinBox>("VBoxContainer/CellsHBoxContainer/CellsYSpinBox");
		cell_size_spin_box = GetNode<SpinBox>("VBoxContainer/CellSizeHBoxContainer/CellSizeSpinBox");
		wall_size_spin_box = GetNode<SpinBox>("VBoxContainer/WallSizeHBoxContainer/WallSizeSpinBox");
	}
	
	private void SetupConnections() {
        save_image_button.Pressed += SaveImagePressed;
		generate_maze_button.Pressed += GenerateMazePressed;
		maze_type_option_button.ItemSelected += MazeTypeSelected;
		cells_x_spin_box.ValueChanged += CellsXChanged;
		cells_y_spin_box.ValueChanged += CellsYChanged;
		cell_size_spin_box.ValueChanged += CellSizeChanged;
		wall_size_spin_box.ValueChanged += WallSizeChanged;
    }

    //Signal Emission
    private void SaveImagePressed() {
		EmitSignal(SignalName.SaveImage);
	}

	private void GenerateMazePressed() {
		EmitSignal(SignalName.GenerateMaze);
	}
	
	private void MazeTypeSelected(long index) {
		EmitSignal(SignalName.MazeType, index);
	}

	private void CellsXChanged(double value)
	{
		EmitSignal(SignalName.CellsX, value);
	}

	private void CellsYChanged(double value)
	{
		EmitSignal(SignalName.CellsY, value);
	}

	private void CellSizeChanged(double value)
	{
		EmitSignal(SignalName.CellSize, value);
	}

	private void WallSizeChanged(double value)
	{
		EmitSignal(SignalName.WallSize, value);
	}

}
