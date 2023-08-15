using Godot;
using System;

public partial class MazeProperties : TabBar
{
	//Maze Signals
	[Signal] public delegate void GenerateMazeEventHandler();
	[Signal] public delegate void MazeTypeEventHandler();
	[Signal] public delegate void CellsXEventHandler();
	[Signal] public delegate void CellsYEventHandler();
	[Signal] public delegate void CellSizeEventHandler();
	[Signal] public delegate void WallSizeEventHandler();
	[Signal] public delegate void ExteriorSizeEventHandler();

	//Point Signals
	[Signal] public delegate void StartPointTypeEventHandler();
	[Signal] public delegate void EndPointTypeEventHandler();
	[Signal] public delegate void NewStartPointEventHandler();
	[Signal] public delegate void NewEndPointEventHandler();
	[Signal] public delegate void DrawToggledEventHandler();

    //Nodes
    private Button generate_maze_button;
    private OptionButton maze_type_option_button;
    private SpinBox cells_x_spin_box, cells_y_spin_box;
    private SpinBox cell_size_spin_box, wall_size_spin_box;
	private SpinBox exterior_size_spin_box;
	private OptionButton start_option_button, end_option_button;
	private Button new_start_point_button, new_end_point_button;
	private CheckButton draw_button;

    public override void _Ready() {
		SetupNodes();
		SetupConnections();
	}

	//Setup Methods
	private void SetupNodes() {

		string start_path = "ScrollContainer/HBoxContainer/VBoxContainer/";

        //Maze Properties
        generate_maze_button = GetNode<Button>(start_path + "GenerateMazeButton");
		maze_type_option_button = GetNode<OptionButton>(start_path + "TypeHBoxContainer/MazeTypeOptionButton");
		cells_x_spin_box = GetNode<SpinBox>(start_path + "WidthHBoxContainer/CellsXSpinBox");
		cells_y_spin_box = GetNode<SpinBox>(start_path + "HeightHBoxContainer/CellsYSpinBox");
		cell_size_spin_box = GetNode<SpinBox>(start_path + "CellSizeHBoxContainer/CellSizeSpinBox");
		wall_size_spin_box = GetNode<SpinBox>(start_path + "WallSizeHBoxContainer/WallSizeSpinBox");
		exterior_size_spin_box = GetNode<SpinBox>(start_path + "ExteriorSizeHBoxContainer/ExteriorSizeSpinBox");
		draw_button = GetNode<CheckButton>(start_path + "DrawHBoxContainer/CheckButton");

		//Point Properties
		start_option_button = GetNode<OptionButton>(start_path + "StartPointHBoxContainer/OptionButton");
        end_option_button = GetNode<OptionButton>(start_path + "EndPointHBoxContainer/OptionButton");
		new_start_point_button = GetNode<Button>(start_path + "NewPointHBoxContainer/NewStartPointButton");
		new_end_point_button = GetNode<Button>(start_path + "NewPointHBoxContainer/NewEndPointButton");
    }
	
	private void SetupConnections() {

		//Maze Properties
		generate_maze_button.Pressed += GenerateMazePressed;
		maze_type_option_button.ItemSelected += MazeTypeSelected;
		cells_x_spin_box.ValueChanged += CellsXChanged;
		cells_y_spin_box.ValueChanged += CellsYChanged;
		cell_size_spin_box.ValueChanged += CellSizeChanged;
		wall_size_spin_box.ValueChanged += WallSizeChanged;
		exterior_size_spin_box.ValueChanged += ExteriorSizeChanged;
		draw_button.Toggled += DrawButtonToggled;

		//Point Properties
		start_option_button.ItemSelected += StartPointTypeChanged;
		end_option_button.ItemSelected += EndPointTypeChanged;
		new_start_point_button.Pressed += NewStartPressed;
		new_end_point_button.Pressed += NewEndPressed;
    }

	//Connections

	//Maze Properties
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

	private void ExteriorSizeChanged(double value)
	{
		EmitSignal(SignalName.ExteriorSize, value);
	}

    //Point Properties
    private void StartPointTypeChanged(long index)
    {
		EmitSignal(SignalName.StartPointType, index);
    }

    private void EndPointTypeChanged(long index)
    {
		EmitSignal(SignalName.EndPointType, index);
    }

	private void NewStartPressed()
	{
		EmitSignal(SignalName.NewStartPoint);
	}

	private void NewEndPressed()
	{
		EmitSignal(SignalName.NewEndPoint);
	}

	private void DrawButtonToggled(bool toggle)
	{
		EmitSignal(SignalName.DrawToggled, toggle);
	}
}
