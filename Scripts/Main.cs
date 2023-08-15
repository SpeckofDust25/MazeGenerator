using Godot;
using System;
using System.Collections.Generic;

public partial class Main : CanvasLayer
{
    enum EMazeType { BinaryTree, Sidewinder, Aldous_Broder, Wilsons, HuntandKill, Recursive_Backtracker, Ellers, Ellers_Loop,
        GrowingTree_Random, GrowingTree_Last, GrowingTree_Mix, Kruskals_Random, Prims_Simple, Prims_True, GrowingForest, Recursive_Division }
    enum EPointType { None = 0, Open };

    //Event Handlers
    [Signal] public delegate void ExpandingEventHandler(bool _expanding);

    //Grid Properties
    private Grid grid;
    private EMazeType maze_type;
    private int x_cells, y_cells;
    private int wall_size, cell_size;
    private int exterior_size;

    //Nodes
    private TabContainer tab_container;
    private Panel panel;
    private TabBar maze_properties;
    private TabBar points_properties;
    private TabBar pathfinding_properties;
    private TabBar animation_properties;
    private TabBar export_properties;
    private TextureRect maze_image;
    private MazeInterface maze_interface;

    //Image
    private Image image;

    //Points
    private EPointType first_point_type = EPointType.None, second_point_type = EPointType.None;
    private Cell start_point, end_point;
    private MazeGenerator.Direction start_point_dir, end_point_dir;
    private Color start_point_color, end_point_color;
    private bool is_draw_mode = false;

    bool can_expand = false;

    public override void _Ready()
    {
        x_cells = 10;
        y_cells = 10;
        wall_size = 10;
        cell_size = 10;
        maze_type = EMazeType.BinaryTree;

        start_point_color = Colors.Green;
        end_point_color = Colors.Red;

        SetupNodes();
        SetupConnections();
        CreateNewMaze();
    }

    public override void _Process(double delta)
    {
        if (is_draw_mode)
        {
            MazeMask.UpdateImage(ref grid, (Vector2I)maze_image.GetLocalMousePosition(), Input.IsActionJustPressed("right_click"));
            SetImage(MazeMask.image);
        }

        //Expand UI
        Vector2 start_position = new Vector2(tab_container.Size.X, 0);
        Vector2 size = new Vector2((panel.Position.X - tab_container.Size.X) + 2, panel.Size.Y);
        Rect2 middle_bar = new Rect2(start_position, size);

        if (middle_bar.HasPoint(GetViewport().GetMousePosition()))
        {
           if (Input.IsActionJustPressed("panning")) {
               can_expand = true;
           }
        }

        if (can_expand)
        {
            tab_container.CustomMinimumSize = new Vector2(GetViewport().GetMousePosition().X, 0);

            if (Input.IsActionJustReleased("panning"))
            {
                can_expand = false;
            }
        }

        EmitSignal(SignalName.Expanding, can_expand);
    }

    //Setup Methods------------------------------
    private void SetupNodes()
    {
        maze_image = GetNode<TextureRect>("Interface/MazePanel/MazeImage");
        tab_container = GetNode<TabContainer>("Interface/TabContainer");
        panel = GetNode<Panel>("Interface/MazePanel");
        maze_interface = (MazeInterface)panel;

        if (maze_interface.HasMethod("IsExpanding"))
        {
            maze_interface.IsExpanding(false);
        }

        maze_properties = GetNode<MazeProperties>("Interface/TabContainer/Maze");
        export_properties = GetNode<ExportProperties>("Interface/TabContainer/Export");
    }

    private void SetupConnections()
    {
        //Maze Properties
        if (!maze_properties.HasSignal("GenerateMaze")) { GD.PrintErr("Can't Find GenerateMaze Signal!"); }
        if (!maze_properties.HasSignal("MazeType")) { GD.PrintErr("Can't Find MazeType Signal!"); }
        if (!maze_properties.HasSignal("CellsX")) { GD.PrintErr("Can't Find CellsX Signal!"); }
        if (!maze_properties.HasSignal("CellsY")) { GD.PrintErr("Can't Find CellsY Signal!"); }
        if (!maze_properties.HasSignal("CellSize")) { GD.PrintErr("Can't Find CellSize Signal!"); }
        if (!maze_properties.HasSignal("WallSize")) { GD.PrintErr("Can't Find WallSize Signal! "); }
        if (!maze_properties.HasSignal("ExteriorSize")) { GD.PrintErr("Can't Find ExteriorSize Signal! "); }
        if (!maze_properties.HasSignal("DrawToggled")) { GD.PrintErr("Can't Find DrawButtonToggled! "); }

        Callable c_generate_maze = new Callable(this, "CreateNewMaze");
        Callable c_maze_type = new Callable(this, "MazeType");
        Callable c_cells_x = new Callable(this, "CellsXChanged");
        Callable c_cells_y = new Callable(this, "CellsYChanged");
        Callable c_cell_size = new Callable(this, "CellSizeChanged");
        Callable c_wall_size = new Callable(this, "WallSizeChanged");
        Callable c_exterior_size = new Callable(this, "ExteriorSizeChanged");
        Callable c_draw_toggled = new Callable(this, "DrawButtonToggle");

        maze_properties.Connect("GenerateMaze", c_generate_maze);
        maze_properties.Connect("MazeType", c_maze_type);
        maze_properties.Connect("CellsX", c_cells_x);
        maze_properties.Connect("CellsY", c_cells_y);
        maze_properties.Connect("CellSize", c_cell_size);
        maze_properties.Connect("WallSize", c_wall_size);
        maze_properties.Connect("ExteriorSize", c_exterior_size);
        maze_properties.Connect("DrawToggled", c_draw_toggled);

        //Point Properties
        if (!maze_properties.HasSignal("StartPointType")) { GD.PrintErr(" Can't Find StartPointType Signal! ");  }
        if (!maze_properties.HasSignal("EndPointType")) { GD.PrintErr(" Can't Find EndPointType Signal! "); }
        if (!maze_properties.HasSignal("NewStartPoint")) { GD.PrintErr(" Can't Find NewStartPoint Signal! "); }
        if (!maze_properties.HasSignal("NewEndPoint")) { GD.PrintErr(" Can't Find NewEndPoint Signal! "); }

        Callable c_start_point_type = new Callable(this, "StartPointType");
        Callable c_end_point_type = new Callable(this, "EndPointType");
        Callable c_new_start_point = new Callable(this, "NewStartPoint");
        Callable c_new_end_point = new Callable(this, "NewEndPoint");

        maze_properties.Connect("StartPointType", c_start_point_type);
        maze_properties.Connect("EndPointType", c_end_point_type);
        maze_properties.Connect("NewStartPoint", c_new_start_point);
        maze_properties.Connect("NewEndPoint", c_new_end_point);

        //Export Properties
        if (!export_properties.HasSignal("SaveImage")) { GD.PrintErr("Can't Find SaveImage Signal!"); }
        Callable c_save_image = new Callable(this, "SaveImage");
        export_properties.Connect("SaveImage", c_save_image);

        //Maze Interface
        if (!maze_interface.HasMethod("IsExpanding")) { GD.PrintErr("Can't Find IsExpanding Method! ");  }
        Callable c_expanding = new Callable(maze_interface, "IsExpanding");
        Connect("Expanding", c_expanding);
    }
    //-------------------------------------------

    //Generate Maze and Image--------------------
    private void CreateNewMaze()
    {
        GenerateMaze();
        GenerateMazeImage();
    }

    private void GenerateMaze()
    {

        //Create New Grid and Mask
        MazeMask.UpdateImage(ref grid, (Vector2I)maze_image.GetLocalMousePosition(), Input.IsActionJustPressed("right_click"));
        grid = new Grid(x_cells, y_cells, wall_size, cell_size, exterior_size);

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
    }

    private void GenerateMazeImage()
    {
        //Set Points
        ResetPoints();
        NewStartPoint();
        NewEndPoint();

        //Update Image
        UpdateMazeImage();
    }

    private void UpdateMazeImage() {

        MazeImage.DrawRectangle(ref grid, Colors.Transparent, HasMaskSupport());
        
        if (!is_draw_mode) {
            image = MazeImage.image;
            maze_image.Texture = ImageTexture.CreateFromImage(image);
        }
    }
    
    private void SetImage(Image _image)
    {
        maze_image.Texture = ImageTexture.CreateFromImage(_image);
    }
    //-------------------------------------------


    //Connections--------------------------------
    private void MazeType(long index)
    {
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

    private void CellsXChanged(double value)
    {
        x_cells = (int)value;
    }

    private void CellsYChanged(double value)
    {
        y_cells = (int)value;
    }

    private void CellSizeChanged(double value)
    {
        cell_size = (int)value;
        grid.SetCellSize(cell_size);
        UpdateMazeImage();
    }

    private void WallSizeChanged(double value)
    {
        wall_size = (int)value;
        grid.SetWallSize(wall_size);
        UpdateMazeImage();
    }

    private void ExteriorSizeChanged(double value)
    {
        exterior_size = (int)value;
        grid.SetExteriorSize(exterior_size);
        UpdateMazeImage();
    }

    private void DrawButtonToggle(bool toggle)
    {
        if (toggle) {
            is_draw_mode = true;
        } else {
            SetImage(MazeImage.image);
            is_draw_mode = false;
        }
    }

    //Point Properties
    private void StartPointType(long index)
    {
        switch(index)
        {
            case 0: //None
                first_point_type = EPointType.None;
                break;

            case 1: //Open
                first_point_type = EPointType.Open;
                break;
        }
    }

    private void EndPointType(long index)
    {
        switch(index)
        {
            case 0: //None
                second_point_type = EPointType.None;
                break;

            case 1: //Open
                second_point_type = EPointType.Open;
                break;
        }
    }

    private void NewStartPoint()
    {
        GD.Randomize();

        FillInCell(ref start_point, true);

        switch (first_point_type)
        {
            case EPointType.None:
                break;

            case EPointType.Open:
                start_point = CreateOpenCell(ref start_point_dir);
                break;
        }

        UpdateMazeImage();
    }

    private void NewEndPoint()
    {
        GD.Randomize();

        FillInCell(ref end_point, false);

        switch (second_point_type)
        {
            case EPointType.None:
                break;

            case EPointType.Open:
                end_point = CreateOpenCell(ref end_point_dir);
                break;
        }

        UpdateMazeImage();
    }

    //Export Properties
    private void SaveImage()
    {
        string save_path;

        if (OS.HasFeature("editor"))
        {
            save_path = "res://Images/" + Time.GetDatetimeStringFromSystem(false, false).Replace(":", "_") + ".png";

            if (!DirAccess.DirExistsAbsolute("res://Images"))
            {
                DirAccess.MakeDirAbsolute("res://Images");
            }

        }
        else
        {
            save_path = OS.GetExecutablePath().GetBaseDir() + "/Images/" + Time.GetDatetimeStringFromSystem(false, false).Replace(":", "_") + ".png";

            if (!DirAccess.DirExistsAbsolute(OS.GetExecutablePath().GetBaseDir() + "/Images"))
            {
                DirAccess.MakeDirAbsolute(OS.GetExecutablePath().GetBaseDir() + "/Images");
            }
        }

        image.SavePng(save_path);
    }
    //-------------------------------------------

    //Point Helper Methods
    private Cell CreateOpenCell(ref MazeGenerator.Direction assign_direction)
    {
        bool same_cell = false;
        Cell open_cell = null;

        List<Cell> deadend_cells = new List<Cell>();
        List<Cell> edge_cells = new List<Cell>();
        List<Cell> current_list = new List<Cell>();

        //Get Possible Cells
        deadend_cells = grid.GetAllValidEdgeDeadends();
        edge_cells = grid.GetAllEdgeCells();

        //List Conditions
        if (deadend_cells.Count > 0) {
            current_list = deadend_cells;
        } else {
            current_list = edge_cells;
        }

        //Get Random Cell
        if (current_list.Count > 0)
        {
            int rand = (int)(GD.Randi() % current_list.Count);
            open_cell = current_list[rand];
        }

        //Same Cell
        if (open_cell.IsSameCell(start_point) || open_cell.IsSameCell(end_point))
        {
            same_cell = true;
        }

        //Open Wall
        if (open_cell != null)
        {
            List<MazeGenerator.Direction> directions = new List<MazeGenerator.Direction>();
            Vector2I index = open_cell.index;
            
            //Add Possible Directions
            if (index.X == 0)   //West
            {
                if (!same_cell) {
                    directions.Add(MazeGenerator.Direction.west);

                } else  if (!open_cell.north && !open_cell.south && !open_cell.west) {
                    directions.Add(MazeGenerator.Direction.west);
                } 
            }

            if (index.Y == 0)   //North
            {
                if (!same_cell) {
                    directions.Add(MazeGenerator.Direction.north);

                } else if (!open_cell.west && !open_cell.east && !open_cell.north) {
                    directions.Add(MazeGenerator.Direction.north);
                } 
            }

            if (index.X == grid.GetWidth() - 1) //East
            {
                if (!same_cell) {
                    directions.Add(MazeGenerator.Direction.east);

                } else if (!open_cell.north && !open_cell.south && !open_cell.east) {
                    directions.Add(MazeGenerator.Direction.east);
                } 
            }

            if (index.Y == grid.GetHeight() - 1) //South
            {

                if (!same_cell) {
                    directions.Add(MazeGenerator.Direction.south);

                } else if (!open_cell.east && !open_cell.west && !open_cell.south) {
                    directions.Add(MazeGenerator.Direction.south);
                }

            }

            MazeGenerator.Direction dir = directions[(int)(GD.Randi() % directions.Count)];

            switch(dir)
            {
                case MazeGenerator.Direction.north:
                    open_cell.north = true;
                    assign_direction = MazeGenerator.Direction.north;
                    break;
                case MazeGenerator.Direction.south:
                    open_cell.south = true;
                    assign_direction = MazeGenerator.Direction.south;
                    break;
                case MazeGenerator.Direction.east:
                    open_cell.east = true;
                    assign_direction = MazeGenerator.Direction.east;
                    break;
                case MazeGenerator.Direction.west:
                    open_cell.west = true;
                    assign_direction = MazeGenerator.Direction.west;
                    break;
            }
        }

        return open_cell;
    }

    private void FillInCell(ref Cell open_cell, bool is_start)
    {
        MazeGenerator.Direction dir = MazeGenerator.Direction.none;

        //Fill in Cell
        if (open_cell != null)
        {
            Vector2I index = open_cell.index;

            //Get Direction
            if (is_start) {
                dir = start_point_dir;
                start_point_dir = MazeGenerator.Direction.none;
            } else {
                dir = end_point_dir;
                end_point_dir = MazeGenerator.Direction.none;
            }

            //Fill in
            switch(dir)
            {
                case MazeGenerator.Direction.north:
                    open_cell.north = false;
                    break;

                case MazeGenerator.Direction.south:
                    open_cell.south = false;
                    break;

                case MazeGenerator.Direction.east:
                    open_cell.east = false;
                    break;

                case MazeGenerator.Direction.west:
                    open_cell.west = false;
                    break;
            }
        }

        open_cell = null;
    }

    private bool HasMaskSupport()
    {
        return (maze_type == EMazeType.BinaryTree || maze_type == EMazeType.Sidewinder || maze_type == EMazeType.Ellers || maze_type == EMazeType.Ellers_Loop);
    }

    private void ResetPoints()
    {
        start_point = null;
        end_point = null;
        start_point_dir = MazeGenerator.Direction.none;
        end_point_dir = MazeGenerator.Direction.none;
    }
}
