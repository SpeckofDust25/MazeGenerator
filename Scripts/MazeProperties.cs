using Godot;
using System.Collections.Generic;
using MazeGeneratorGlobal;

public partial class MazeProperties : PanelContainer
{
    //Signals
    [Signal] public delegate void GenerateMazeEventHandler();
    [Signal] public delegate void DrawToggledEventHandler();
    [Signal] public delegate void SaveImageEventHandler();

    //Properties

    //Grid Info
    Maze maze;
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
    private bool can_set_mask = false;
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

    private ColorPickerButton wall_color_button;
    private ColorPickerButton cell_color_button;
    private ColorPickerButton path_color_button;
    private ColorPickerButton distance_color_button;

    private Button save_image_button;

    //Default Methods----------------------------
    public override void _Ready() {
        local_mouse_position = new Vector2I(-1, -1);
		SetupNodes();
		SetupConnections();

        maze_type = EMazeType.BinaryTree;

        MazeImage.wall_color = wall_color_button.Color;
        MazeImage.cell_color = cell_color_button.Color;
        MazeImage.path_color = path_color_button.Color;
        MazeImage.distance_color = distance_color_button.Color;

        UpdateMaze();
    }

    public override void _Process(double delta)
    {
        bool did_update = MazeMask.Update(ref maze, local_mouse_position);

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

        //Colors
        wall_color_button = GetNode<ColorPickerButton>(start_path + "WallColorHBoxContainer/ColorPickerButton");
        cell_color_button = GetNode<ColorPickerButton>(start_path + "CellColorHBoxContainer/ColorPickerButton");
        path_color_button = GetNode<ColorPickerButton>(start_path + "PathColorHBoxContainer/ColorPickerButton");
        distance_color_button = GetNode<ColorPickerButton>(start_path + "DistanceColorHBoxContainer/ColorPickerButton");

        //Export
        save_image_button = GetNode<Button>(start_path + "SaveImageButton");
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

        //Colors
        wall_color_button.ColorChanged += WallColorChanged;
        cell_color_button.ColorChanged += CellColorChanged;
        path_color_button.ColorChanged += PathColorChanged;
        distance_color_button.ColorChanged += DistanceColorChanged;

        //Export
        save_image_button.Pressed += SaveImagePressed;
    }
    //-------------------------------------------

    //Update Methods-----------------------------
    private void UpdateMaze()
    {
        if (maze != null) {
            maze = new Maze(maze.GetWidth(), maze.GetHeight(), maze.GetWallSize(), maze.GetCellSize());
        } else {
            maze = new Maze(10, 10, 10, 10);
        }

        if (can_set_mask) {
            MazeMask.Update(ref maze, Vector2I.Zero);
            maze.SetMask(MazeMask.mask);
        }

        bool successful = MazeGenerator.GenerateMaze(ref maze, maze_type, horizontal_bias);  //Generate Maze

        if (successful) {
            ApplyMazeModifications();

            if ((maze.GetWidth() * maze.GetHeight()) > 1) {
                UpdatePoints(point_type);

                if (point_type != EPoints.None) {
                    Cell start = maze.GetStartCell();
                    Cell end = maze.GetEndCell();
            
                    path = PathFinding.AStar(ref maze, maze.GetStartCell(), maze.GetEndCell());
                }
            }
            
            UpdateImage();  //Update Image            
        }
    }

    private void UpdateImage()
    {
        //Update Image
        if (is_pathfinding && maze.GetStartCell() != null && maze.GetEndCell() != null) {
            MazeImage.DrawRectangle(ref maze, Colors.Transparent, HasMaskSupport(), path);
        } else {
            MazeImage.DrawRectangle(ref maze, Colors.Transparent, HasMaskSupport(), null);
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
        maze.Braid(braid_value);
    }

    private void MazeTypeSelected(long index) {
        switch (index)
        {
            case ((long)EMazeType.BinaryTree):
                maze_type = EMazeType.BinaryTree;
                can_set_mask = false;
                break;

            case ((long)EMazeType.Sidewinder):
                maze_type = EMazeType.Sidewinder;
                can_set_mask = false;
                break;

            case ((long)EMazeType.Aldous_Broder):
                maze_type = EMazeType.Aldous_Broder;
                can_set_mask = true;
                break;

            case ((long)EMazeType.Wilsons):
                maze_type = EMazeType.Wilsons;
                can_set_mask = true;
                break;

            case ((long)EMazeType.HuntandKill):
                maze_type = EMazeType.HuntandKill;
                can_set_mask = true;
                break;

            case ((long)EMazeType.Recursive_Backtracker):
                maze_type = EMazeType.Recursive_Backtracker;
                can_set_mask = true;
                break;

            case ((long)EMazeType.Ellers):
                maze_type = EMazeType.Ellers;
                can_set_mask = false;
                break;

            case ((long)EMazeType.Ellers_Loop):
                maze_type = EMazeType.Ellers_Loop;
                can_set_mask = false;
                break;

            case ((long)EMazeType.GrowingTree_Random):
                maze_type = EMazeType.GrowingTree_Random;
                can_set_mask = true;
                break;

            case ((long)EMazeType.GrowingTree_Last):
                maze_type = EMazeType.GrowingTree_Last;
                can_set_mask = true;
                break;

            case ((long)EMazeType.GrowingTree_Mix):
                maze_type = EMazeType.GrowingTree_Mix;
                can_set_mask = true;
                break;

            case ((long)EMazeType.Kruskals_Random):
                maze_type = EMazeType.Kruskals_Random;
                can_set_mask = true;
                break;

            case ((long)EMazeType.Prims_Simple):
                maze_type = EMazeType.Prims_Simple;
                can_set_mask = true;
                break;

            case ((long)EMazeType.Prims_True):
                maze_type = EMazeType.Prims_True;
                can_set_mask = true;
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
		maze.SetWidth((int)value);
        UpdateMaze();
    }

	private void GridHeightChanged(double value)
	{
		maze.SetHeight((int)value);
        UpdateMaze();
    }

	private void CellSizeChanged(double value)
	{
		maze.SetCellSize((int)value);
        UpdateImage();
    }

	private void WallSizeChanged(double value)
	{
		maze.SetWallSize((int)value);
        UpdateImage();
    }

    //Points
    private void PointsSelected(long index)
    {
        switch(index)
        {
            case (long)EPoints.None:
                point_type = EPoints.None;
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
        List<Vector2I> points = maze.GetAllPossiblePoints();
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
                    first = maze.cells[first_index.X, first_index.Y];
                    points.Remove(first_index);
                }

                if (points.Count > 0)
                {
                    second_index = points[(int)(GD.Randi() % points.Count)];
                    second = maze.cells[second_index.X, second_index.Y];
                }

                maze.start_end_points = new Points(ref first, ref second, maze.GetInvalidNeighbors(first.index), maze.GetInvalidNeighbors(second.index));
                break;

            case EPoints.Furthest:
                if (points.Count > 0)
                {
                    first_index = points[(int)(GD.Randi() % points.Count)];
                    first = maze.cells[first_index.X, first_index.Y];
                    points.Remove(first_index);
                }

                PGrid furthest_grid = PathFinding.GetDistancesFromCell(ref maze, first);
                second_index = furthest_grid.GetFurthestIndex(maze.GetAllPossiblePoints());

                second = maze.cells[second_index.X, second_index.Y];
                maze.start_end_points = new Points(ref first, ref second, maze.GetInvalidNeighbors(first.index), maze.GetInvalidNeighbors(second.index));
                break;

            case EPoints.Easy:
                if (points.Count > 0)
                {
                    first_index = points[(int)(GD.Randi() % points.Count)];
                    first = maze.cells[first_index.X, first_index.Y];
                    points.Remove(first_index);

                    PGrid easy_grid = PathFinding.GetDistancesFromCell(ref maze, first);
                    second_index = easy_grid.GetEndIndex(0.3f, points);

                    second = maze.cells[second_index.X, second_index.Y];

                    maze.start_end_points = new Points(ref first, ref second, maze.GetInvalidNeighbors(first.index), maze.GetInvalidNeighbors(second.index));
                }
                break;

            case EPoints.Medium:
                if (points.Count > 0)
                {
                    first_index = points[(int)(GD.Randi() % points.Count)];
                    first = maze.cells[first_index.X, first_index.Y];
                    points.Remove(first_index);

                    PGrid easy_grid = PathFinding.GetDistancesFromCell(ref maze, first);
                    second_index = easy_grid.GetEndIndex(0.6f, points);

                    second = maze.cells[second_index.X, second_index.Y];
                    maze.start_end_points = new Points(ref first, ref second, maze.GetInvalidNeighbors(first.index), maze.GetInvalidNeighbors(second.index));
                }
                break;

            case EPoints.Hard:
                if (points.Count > 0)
                {
                    first_index = points[(int)(GD.Randi() % points.Count)];
                    first = maze.cells[first_index.X, first_index.Y];
                    points.Remove(first_index);

                    PGrid easy_grid = PathFinding.GetDistancesFromCell(ref maze, first);
                    second_index = easy_grid.GetEndIndex(0.8f, points);

                    second = maze.cells[second_index.X, second_index.Y];
                    maze.start_end_points = new Points(ref first, ref second, maze.GetInvalidNeighbors(first.index), maze.GetInvalidNeighbors(second.index));
                }
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

    //Colors
    public void WallColorChanged(Color color) {
        MazeImage.wall_color = color;
        UpdateImage();
    }

    private void CellColorChanged(Color color) {
        MazeImage.cell_color = color;
        UpdateImage();
    }

    private void PathColorChanged(Color color) {
        MazeImage.path_color = color;
        UpdateImage();
    }

    private void DistanceColorChanged(Color color) {
        MazeImage.distance_color = color;
        UpdateImage();
    }

    //Export
    private void SaveImagePressed() {
        EmitSignal(SignalName.SaveImage);
    }
}