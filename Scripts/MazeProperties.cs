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

    //Nodes
    private Button generate_maze_button;
    private OptionButton maze_type_option_button;
    private SpinBox cells_x_spin_box, cells_y_spin_box;
    private SpinBox cell_size_spin_box, wall_size_spin_box;
	private SpinBox exterior_size_spin_box;
	private OptionButton start_option_button, end_option_button;
	private Button new_start_point_button, new_end_point_button;
	private CheckButton draw_button;

	//Default Methods----------------------------
    public override void _Ready() {
		SetupNodes();
		SetupConnections();
        //points = new Point();

        maze_type = EMazeType.BinaryTree;
        UpdateMaze();
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
		cells_x_spin_box.ValueChanged += GridWidthChanged;
		cells_y_spin_box.ValueChanged += GridHeightChanged;
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
    //-------------------------------------------

    //Update Methods-----------------------------
    private void UpdateMaze()
    {
        //Create New Grid and Mask
        //MazeMask.UpdateImage(ref grid, (Vector2I)maze_image.GetLocalMousePosition());
        //MazeMask.CreateNewMask(ref grid);
        //grid.SetMask(MazeMask.mask);

        if (grid != null)
        {
            grid = new Grid(grid.GetWidth(), grid.GetHeight(), grid.GetWallSize(), grid.GetCellSize(), grid.GetExteriorSize());
        } else {
            grid = new Grid(10, 10, 10, 10, 0);
        }
        
        //Generate Maze
        switch (maze_type)
        {
            case EMazeType.BinaryTree:
                MazeGenerator.BinaryTreeAlgorithm(ref grid);
                break;

            case EMazeType.Sidewinder:
                MazeGenerator.SidewinderAlgorithm(ref grid);
                break;

            case EMazeType.Aldous_Broder:
                MazeGenerator.AldousBroderAlgorithm(ref grid);
                break;

            case EMazeType.Wilsons:
                MazeGenerator.WilsonsAlgorithm(ref grid);
                break;

            case EMazeType.HuntandKill:
                MazeGenerator.HuntandKill(ref grid);
                break;

            case EMazeType.Recursive_Backtracker:
                MazeGenerator.RecursiveBacktracker(ref grid);
                break;

            case EMazeType.Ellers:
                MazeGenerator.Ellers(ref grid, false);
                break;

            case EMazeType.Ellers_Loop:
                MazeGenerator.Ellers(ref grid, true);
                break;

            case EMazeType.GrowingTree_Random:
                MazeGenerator.GrowingTree(ref grid, 0);
                break;

            case EMazeType.GrowingTree_Last:
                MazeGenerator.GrowingTree(ref grid, 1);
                break;

            case EMazeType.GrowingTree_Mix:
                MazeGenerator.GrowingTree(ref grid, 2);
                break;

            case EMazeType.Kruskals_Random:
                MazeGenerator.Kruskals(ref grid);
                break;

            case EMazeType.Prims_Simple:
                MazeGenerator.Prims_Simple(ref grid);
                break;

            case EMazeType.Prims_True:
                MazeGenerator.Prims_True(ref grid);
                break;

            case EMazeType.GrowingForest:
                MazeGenerator.GrowingForest(ref grid);
                break;

            case EMazeType.Recursive_Division:
                MazeGenerator.Recursive_Division(ref grid);
                break;
        }

        UpdateImage();
    }

    private void UpdateImage()
    {
        //Update Image
        MazeImage.DrawRectangle(ref grid, Colors.Transparent, HasMaskSupport());

        if (!is_draw_mode)
        {
            EmitSignal(SignalName.GenerateMaze, MazeImage.image);
        }
    }

    //-------------------------------------------

    //Connections

    //Maze Properties
    private void GenerateMazePressed() {
        UpdateMaze();
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

	private void ExteriorSizeChanged(double value)
	{
        grid.SetExteriorSize((int)value);
        UpdateImage();
    }

    //Point Properties
    private void StartPointTypeChanged(long index)
    {
        /*
        switch (index)
        {
            case 0: //None
                first_point_type = EPointType.None;
                break;

            case 1: //Open
                first_point_type = EPointType.Open;
                break;
        }

        //Set Points
        //points.ResetPoints();
        NewStartPressed();
        NewEndPressed();


        UpdateImage();
        */
    }

    private void EndPointTypeChanged(long index)
    {
        /*
        switch (index)
        {
            case 0: //None
                second_point_type = EPointType.None;
                break;

            case 1: //Open
                second_point_type = EPointType.Open;
                break;
        }

        UpdateImage();*/
    }

	private void NewStartPressed()
	{
        /*GD.Randomize();

        points.FillInCell(ref start_point, true);

        switch (first_point_type)
        {
            case EPointType.None:
                break;

            case EPointType.Open:
                start_point = points.CreateOpenCell(ref start_point_dir, ref grid);
                break;
        }
        */
        //UpdateImage();
    }

	private void NewEndPressed()
	{
        /*GD.Randomize();

        points.FillInCell(ref end_point, false);

        switch (second_point_type)
        {
            case EPointType.None:
                break;

            case EPointType.Open:
                end_point = points.CreateOpenCell(ref end_point_dir, ref grid);
                break;
        }*/

        //UpdateImage();
    }

	private void DrawButtonToggled(bool toggle)
	{
        EmitSignal(SignalName.DrawToggled, toggle);
    }

    //Point Methods

    private bool HasMaskSupport()
    {
        return (maze_type == EMazeType.BinaryTree || maze_type == EMazeType.Sidewinder || maze_type == EMazeType.Ellers || maze_type == EMazeType.Ellers_Loop);
    }


}