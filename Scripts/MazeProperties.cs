using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using MazeGeneratorGlobal;
using System.Threading;

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

    //Points
    private EPoints point_type = EPoints.None;

    //Routing
    private float horizontal_bias = 0.5f;
    private float braid_value;
    private bool is_pathfinding = false;
    private bool is_unicursal;
    private List<Vector2I> path;

    //Nodes
    private Button generate_maze_button;
    private OptionButton maze_type_option_button;
    private CheckButton pathfinding_button;
    private SpinBox cells_x_spin_box, cells_y_spin_box;
    private SpinBox cell_size_spin_box, wall_size_spin_box;

	private CheckButton draw_button;

    private OptionButton points_option_button;
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
        pathfinding_button = GetNode<CheckButton>(start_path + "PathfindingHBoxContainer/CheckButton");
		cells_x_spin_box = GetNode<SpinBox>(start_path + "WidthHBoxContainer/CellsXSpinBox");
		cells_y_spin_box = GetNode<SpinBox>(start_path + "HeightHBoxContainer/CellsYSpinBox");
		cell_size_spin_box = GetNode<SpinBox>(start_path + "CellSizeHBoxContainer/CellSizeSpinBox");
		wall_size_spin_box = GetNode<SpinBox>(start_path + "WallSizeHBoxContainer/WallSizeSpinBox");
		draw_button = GetNode<CheckButton>(start_path + "DrawHBoxContainer/CheckButton");

        //Points
        points_option_button = GetNode<OptionButton>(start_path + "PointsHBoxContainer/OptionButton");

        //Maze Modifications
        draw_button = GetNode<CheckButton>(start_path + "DrawHBoxContainer/CheckButton");
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
        pathfinding_button.Toggled += PathfindingChanged;
		cells_x_spin_box.ValueChanged += GridWidthChanged;
		cells_y_spin_box.ValueChanged += GridHeightChanged;
		cell_size_spin_box.ValueChanged += CellSizeChanged;
		wall_size_spin_box.ValueChanged += WallSizeChanged;


        //Points
        points_option_button.ItemSelected += PointsSelected;

        //Maze Modifications
        draw_button.Toggled += DrawButtonToggled;
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
            UpdatePoints(point_type);

            if (point_type != EPoints.None) {
                Cell start = grid.GetStartCell();
                Cell end = grid.GetEndCell();
            
                path = PathFinding.AStar(ref grid, grid.GetStartCell(), grid.GetEndCell());
            }
            
            UpdateImage();  //Update Image            
        }
    }

    private void UpdateImage()
    {
        //Update Image
        if (is_pathfinding) {
            MazeImage.DrawRectangle(ref grid, Colors.Transparent, HasMaskSupport(), path);
        } else {
            MazeImage.DrawRectangle(ref grid, Colors.Transparent, HasMaskSupport(), null);
        }

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

    private void PathfindingChanged(bool value)
    {
        is_pathfinding = value;
        UpdateImage();
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

    //Points
    private void PointsSelected(long index)
    {
        switch(index)
        {
            case (long)EPoints.None:
                break;

            case (long)EPoints.Random:
                point_type = EPoints.Random;
                break;

            case (long)EPoints.Furthest:
                point_type = EPoints.Furthest;
                break;

            case (long)EPoints.Easy:
                point_type = EPoints.Easy;
                break;

            case (long)EPoints.Medium:
                point_type = EPoints.Medium;
                break;

            case (long)EPoints.Hard:
                point_type = EPoints.Hard;
                break;
        }
    }

    public void UpdatePoints(EPoints point_type)
    {
        List<Vector2I> points = grid.GetAllPossiblePoints();
        Cell first = null;
        Cell second = null;

        Vector2I first_index, second_index;

        switch (point_type)
        {
            case EPoints.None:
                break;

            case EPoints.Random:
                if (points.Count > 0)
                {
                    first_index = points[(int)(GD.Randi() % points.Count)];
                    first = grid.cells[first_index.X, first_index.Y];
                    points.Remove(first_index);
                }

                if (points.Count > 0)
                {
                    second_index = points[(int)(GD.Randi() % points.Count)];
                    second = grid.cells[second_index.X, second_index.Y];
                }

                grid.start_end_points = new Points(ref first, ref second, grid.GetInvalidNeighbors(first.index), grid.GetInvalidNeighbors(second.index));
                break;

            case EPoints.Furthest:
                if (points.Count > 0)
                {
                    first_index = points[(int)(GD.Randi() % points.Count)];
                    first = grid.cells[first_index.X, first_index.Y];
                    points.Remove(first_index);
                }

                PGrid furthest_grid = PathFinding.GetDistancesFromCell(ref grid, first);
                Vector2I furthest_index = furthest_grid.GetFurthestIndex(first, grid.GetAllPossiblePoints());

                second = grid.cells[furthest_index.X, furthest_index.Y];
                grid.start_end_points = new Points(ref first, ref second, grid.GetInvalidNeighbors(first.index), grid.GetInvalidNeighbors(second.index));
                break;

            case EPoints.Easy:
                break;

            case EPoints.Medium:
                break;

            case EPoints.Hard:
                break;
        }
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