using Godot;
using System;

public partial class MazeProperties : TabBar
{
	//Signals
	[Signal] public delegate void GenerateMazeEventHandler();
	[Signal] public delegate void MazeTypeEventHandler();
	[Signal] public delegate void CellsXEventHandler();
	[Signal] public delegate void CellsYEventHandler();
	[Signal] public delegate void CellSizeEventHandler();
	[Signal] public delegate void WallSizeEventHandler();

    //Nodes
    private Button generate_maze_button;
    private OptionButton maze_type_option_button;
    private SpinBox cells_x_spin_box, cells_y_spin_box;
    private SpinBox cell_size_spin_box, wall_size_spin_box;

    public override void _Ready() {
		SetupNodes();
		SetupConnections();
	}

	//Setup Methods
	private void SetupNodes() {

		//Maze Properties
		generate_maze_button = GetNode<Button>("VBoxContainer/GenerateMazeButton");
		maze_type_option_button = GetNode<OptionButton>("VBoxContainer/TypeHBoxContainer/MazeTypeOptionButton");
		cells_x_spin_box = GetNode<SpinBox>("VBoxContainer/WidthHBoxContainer/CellsXSpinBox");
		cells_y_spin_box = GetNode<SpinBox>("VBoxContainer/HeightHBoxContainer/CellsYSpinBox");
		cell_size_spin_box = GetNode<SpinBox>("VBoxContainer/CellSizeHBoxContainer/CellSizeSpinBox");
		wall_size_spin_box = GetNode<SpinBox>("VBoxContainer/WallSizeHBoxContainer/WallSizeSpinBox");
	}
	
	private void SetupConnections() {

		//Maze Properties
		generate_maze_button.Pressed += GenerateMazePressed;
		maze_type_option_button.ItemSelected += MazeTypeSelected;
		cells_x_spin_box.ValueChanged += CellsXChanged;
		cells_y_spin_box.ValueChanged += CellsYChanged;
		cell_size_spin_box.ValueChanged += CellSizeChanged;
		wall_size_spin_box.ValueChanged += WallSizeChanged;
    }


	//Connections
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
