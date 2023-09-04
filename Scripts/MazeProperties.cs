using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using MazeGeneratorGlobal;

public partial class MazeProperties : TabBar
{
    //Signals 
    [Signal] public delegate void GenerateMazeEventHandler();
    [Signal] public delegate void DrawToggledEventHandler();

	//Properties

	//Grid Info
	Grid grid;
    private EMazeType maze_type;
    private bool is_draw_mode = false;
    private Vector2I local_mouse_position;

    //Routing
    private float horizontal_bias = 0.5f;
    private float braid_value;
    private bool is_unicursal;
    private List<Vector2I> path;

    //Nodes
    private Button generate_maze_button;
    private OptionButton maze_type_option_button;
    private SpinBox cells_x_spin_box, cells_y_spin_box;
    private SpinBox cell_size_spin_box, wall_size_spin_box;
	private CheckButton draw_button;

    private HSlider braid_slider, h_bias_slider;
    private Label braid_value_label, h_bias_value_label;
    private CheckButton unicursal_button;

	//Default Methods----------------------------
    public override void _Ready() {
        local_mouse_position = new Vector2I(-1, -1);
		SetupNodes();
		SetupConnections();

        maze_type = EMazeType.BinaryTree;
        UpdateMaze();
    }

    public override void _Process(double delta)
    {
        bool did_update = MazeMask.Update(ref grid, local_mouse_position);

        if (did_update)
        {
            EmitSignal(SignalName.GenerateMaze);
        }
    }
    //-------------------------------------------

    //Setup Methods------------------------------
    private void SetupNodes() {

		string start_path = "ScrollContainer/HBoxContainer/VBoxContainer/";

        //Maze Properties
        generate_maze_button = GetNode<Button>(start_path + "GenerateMazeButton");
		maze_type_option_button = GetNode<OptionButton>(start_path + "TypeHBoxContainer/MazeTypeOptionButton");
		cells_x_spin_box = GetNode<SpinBox>(start_path + "WidthHBoxContainer/CellsXSpinBox");
		cells_y_spin_box = GetNode<SpinBox>(start_path + "HeightHBoxContainer/CellsYSpinBox");
		cell_size_spin_box = GetNode<SpinBox>(start_path + "CellSizeHBoxContainer/CellSizeSpinBox");
		wall_size_spin_box = GetNode<SpinBox>(start_path + "WallSizeHBoxContainer/WallSizeSpinBox");
		draw_button = GetNode<CheckButton>(start_path + "DrawHBoxContainer/CheckButton");

        //Maze Modifications
        h_bias_slider = GetNode<HSlider>(start_path + "BiasHBoxContainer/HorizontalBiasHSlider");
        h_bias_value_label = GetNode<Label>(start_path + "BiasHBoxContainer/HorizontalBiasValueLabel");
        braid_slider = GetNode<HSlider>(start_path + "BraidHBoxContainer/BraidHSlider");
        braid_value_label = GetNode<Label>(start_path + "BraidHBoxContainer/BraidValueLabel");
        unicursal_button = GetNode<CheckButton>(start_path + "UnicursalHBoxContainer/UnicursalCheckButton");
    }
	
	private void SetupConnections() {

		//Maze Properties
		generate_maze_button.Pressed += GenerateMazePressed;
		maze_type_option_button.ItemSelected += MazeTypeSelected;
		cells_x_spin_box.ValueChanged += GridWidthChanged;
		cells_y_spin_box.ValueChanged += GridHeightChanged;
		cell_size_spin_box.ValueChanged += CellSizeChanged;
		wall_size_spin_box.ValueChanged += WallSizeChanged;
		draw_button.Toggled += DrawButtonToggled;

        //Maze Modifications
        h_bias_slider.ValueChanged += HBiasChanged;
        braid_slider.ValueChanged += BraidSliderChanged;
        unicursal_button.Toggled += UnicursalChanged;
    }
    //-------------------------------------------

    //Update Methods-----------------------------
    private void UpdateMaze()
    {
        if (grid != null) {
            grid = new Grid(grid.GetWidth(), grid.GetHeight(), grid.GetWallSize(), grid.GetCellSize());
        } else {
            grid = new Grid(10, 10, 10, 10);
        }

        MazeMask.Update(ref grid, Vector2I.Zero);
        grid.SetMask(MazeMask.mask);
        bool successful = MazeGenerator.GenerateMaze(ref grid, maze_type, horizontal_bias);  //Generate Maze

        if (successful) {
            ApplyMazeModifications();
            path = PathFinding.AStar(ref grid, grid.cells[0, 0], grid.cells[9, 9]);
            UpdateImage();  //Update Image            
        }
    }

    private void UpdateImage()
    {
        //Update Image
        MazeImage.DrawRectangle(ref grid, Colors.Transparent, HasMaskSupport(), path);
        EmitSignal(SignalName.GenerateMaze);
    }
    //-------------------------------------------


    //Setters
    public void SetLocalImageMousePosition(Vector2I _local_mouse_position)
    {
        local_mouse_position = _local_mouse_position;
    }

    //Connections

    //Maze Properties
    private void GenerateMazePressed() {
        UpdateMaze();
	}

    private void ApplyMazeModifications()
    {
        grid.Braid(braid_value);
    }

    private void MazeTypeSelected(long index) {
        switch (index)
        {
            case ((long)EMazeType.BinaryTree):
                maze_type = EMazeType.BinaryTree;
                break;

            case ((long)EMazeType.Sidewinder):
                maze_type = EMazeType.Sidewinder;
                break;

            case ((long)EMazeType.Aldous_Broder):
                maze_type = EMazeType.Aldous_Broder;
                break;

            case ((long)EMazeType.Wilsons):
                maze_type = EMazeType.Wilsons;
                break;

            case ((long)EMazeType.HuntandKill):
                maze_type = EMazeType.HuntandKill;
                break;

            case ((long)EMazeType.Recursive_Backtracker):
                maze_type = EMazeType.Recursive_Backtracker;
                break;

            case ((long)EMazeType.Ellers):
                maze_type = EMazeType.Ellers;
                break;

            case ((long)EMazeType.Ellers_Loop):
                maze_type = EMazeType.Ellers_Loop;
                break;

            case ((long)EMazeType.GrowingTree_Random):
                maze_type = EMazeType.GrowingTree_Random;
                break;

            case ((long)EMazeType.GrowingTree_Last):
                maze_type = EMazeType.GrowingTree_Last;
                break;

            case ((long)EMazeType.GrowingTree_Mix):
                maze_type = EMazeType.GrowingTree_Mix;
                break;

            case ((long)EMazeType.Kruskals_Random):
                maze_type = EMazeType.Kruskals_Random;
                break;

            case ((long)EMazeType.Prims_Simple):
                maze_type = EMazeType.Prims_Simple;
                break;

            case ((long)EMazeType.Prims_True):
                maze_type = EMazeType.Prims_True;
                break;

            case ((long)EMazeType.GrowingForest):
                maze_type = EMazeType.GrowingForest;
                break;

            case ((long)EMazeType.Recursive_Division):
                maze_type = EMazeType.Recursive_Division;
                break;
        }
    }



	private void GridWidthChanged(double value)
	{
		grid.SetWidth((int)value);
        UpdateMaze();
    }

	private void GridHeightChanged(double value)
	{
		grid.SetHeight((int)value);
        UpdateMaze();
    }

	private void CellSizeChanged(double value)
	{
		grid.SetCellSize((int)value);
        UpdateImage();
    }

	private void WallSizeChanged(double value)
	{
		grid.SetWallSize((int)value);
        UpdateImage();
    }

    //Maze Modifications
    private void HBiasChanged(double value)
    {
        horizontal_bias = (float)value;
        h_bias_value_label.Text = value.ToString("0.00");
    }

	private void DrawButtonToggled(bool toggle)
	{
        is_draw_mode = toggle;

        if (!is_draw_mode)
        {
            EmitSignal(SignalName.DrawToggled, toggle);
        }
        else
        {
            EmitSignal(SignalName.DrawToggled, toggle);
        }
    }

    private void BraidSliderChanged(double value)
    {
        braid_value = (float)value;
        braid_value_label.Text = value.ToString("0.00");
    }

    private void UnicursalChanged(bool is_on)
    {
        is_unicursal = is_on;
        braid_slider.Editable = !is_on;
    }


    //Point Methods

    private bool HasMaskSupport()
    {
        return (maze_type == EMazeType.BinaryTree || maze_type == EMazeType.Sidewinder || maze_type == EMazeType.Ellers || maze_type == EMazeType.Ellers_Loop);
    }
}