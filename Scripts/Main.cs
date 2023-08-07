using Godot;
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
    private TextureRect texture_rect;
    private TabBar maze_properties;
    private TabBar points_properties;
    private TabBar pathfinding_properties;
    private TabBar animation_properties;
    private TabBar export_properties;

    private MazeInterface maze_interface;

    //Image
    private Image image;

    //Points
    private EPointType first_point_type = EPointType.None, second_point_type = EPointType.None;
    private Cell start_point, end_point;
    private Color start_point_color, end_point_color;
    private bool can_draw_points = false;

    bool can_expand = false;

    public override void _Ready()
    {
        x_cells = 10;
        y_cells = 10;
        wall_size = 10;
        cell_size = 10;
        maze_type = EMazeType.BinaryTree;

        //start_point = new Vector2I(-1, -1);
        //end_point = new Vector2I(-1, -1);

        start_point_color = Colors.Green;
        end_point_color = Colors.Red;

        SetupNodes();
        SetupConnections();
        GenerateMaze();
    }

    public override void _Process(double delta)
    {
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

    private void GenerateMazeImage()
    {
        int exterior_size = grid.GetExteriorSize();
        image = Image.Create(grid.GetTotalWidthPx() + (exterior_size * 2), grid.GetTotalHeightPx() + (exterior_size * 2), false, Image.Format.Rgb8);
        image.Fill(Colors.White);   //Fill Image

        //Draw Points
        //DrawStartEndPoints();

        //Draw Maze
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                SetMazeColors(Colors.Black, ref image, x, y);
            }
        }

        texture_rect.Texture = ImageTexture.CreateFromImage(image);
    }

    /*private void DrawStartEndPoints()
    {
        if (can_draw_points) {
            Rect2I start_rect = new Rect2I(start_point.X * (grid.GetCellSize() + grid.GetWallSize()), start_point.Y * (grid.GetCellSize() + grid.GetWallSize()), grid.GetCellSizePx());
            image.FillRect(start_rect, start_point_color);

            Rect2I end_rect = new Rect2I(end_point.X * (grid.GetCellSize() + grid.GetWallSize()), end_point.Y * (grid.GetCellSize() + grid.GetWallSize()), grid.GetCellSizePx());
            image.FillRect(end_rect, end_point_color);
        }
    }*/

    private void SetMazeColors(Color wall_color, ref Image _image, int _x, int _y)
    {
        int exterior_offset = grid.GetExteriorSize();
        Cell cell = grid.cells[_x, _y];

        //Wall Drawing
        if (!cell.north) { // North
            Rect2I north_wall = grid.GetNorthWall(_x, _y);
            north_wall.Position += new Vector2I(exterior_offset, exterior_offset);

            image.FillRect(north_wall, wall_color);
        }

        if (!cell.south) {  //South
            Rect2I south_wall = grid.GetSouthWall(_x, _y);
            south_wall.Position += new Vector2I(exterior_offset, exterior_offset);

            image.FillRect(south_wall, wall_color);
        }
        
		if (!cell.east) {   //East
            Rect2I east_wall = grid.GetEastWall(_x, _y);
            east_wall.Position += new Vector2I(exterior_offset, exterior_offset);

            image.FillRect(east_wall, wall_color);
        }

		if (!cell.west) { //West
            Rect2I west_wall = grid.GetWestWall(_x, _y);
            west_wall.Position += new Vector2I(exterior_offset, exterior_offset);

            image.FillRect(west_wall, wall_color);
        }
    }


    //Setup Methods
    private void SetupNodes()
    {
        tab_container = GetNode<TabContainer>("Interface/TabContainer");
        panel = GetNode<Panel>("Interface/MazePanel");
        maze_interface = (MazeInterface)panel;

        if (maze_interface.HasMethod("IsExpanding"))
        {
            maze_interface.IsExpanding(false);
        }

        texture_rect = GetNode<TextureRect>("Interface/MazePanel/MazeImage");
        maze_properties = GetNode<MazeProperties>("Interface/TabContainer/Maze");

        //pathfinding_properties = GetNode<PathFindingProperties>();
        //animation_properties = GetNode<AnimationProperties>();
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

        Callable c_generate_maze = new Callable(this, "GenerateMaze");
        Callable c_maze_type = new Callable(this, "MazeType");
        Callable c_cells_x = new Callable(this, "CellsXChanged");
        Callable c_cells_y = new Callable(this, "CellsYChanged");
        Callable c_cell_size = new Callable(this, "CellSizeChanged");
        Callable c_wall_size = new Callable(this, "WallSizeChanged");
        Callable c_exterior_size = new Callable(this, "ExteriorSizeChanged");

        maze_properties.Connect("GenerateMaze", c_generate_maze);
        maze_properties.Connect("MazeType", c_maze_type);
        maze_properties.Connect("CellsX", c_cells_x);
        maze_properties.Connect("CellsY", c_cells_y);
        maze_properties.Connect("CellSize", c_cell_size);
        maze_properties.Connect("WallSize", c_wall_size);
        maze_properties.Connect("ExteriorSize", c_exterior_size);

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

    //Connections

    //Maze Properties
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

    private void GenerateMaze()
    {
        //Create New Grid
        grid = new Grid(x_cells, y_cells, wall_size, cell_size, exterior_size);

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

        //Set Points
        NewStartPoint();
        NewEndPoint();


        GenerateMazeImage();
    }

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
        GenerateMazeImage();
    }

    private void WallSizeChanged(double value)
    {
        wall_size = (int)value;
        grid.SetWallSize(wall_size);
        GenerateMazeImage();
    }

    private void ExteriorSizeChanged(double value)
    {
        exterior_size = (int)value;
        grid.SetExteriorSize(exterior_size);
        GenerateMazeImage();
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

        FillInCell(ref start_point);

        switch (first_point_type)
        {
            case EPointType.None:
                break;

            case EPointType.Open:
                start_point = CreateOpenCell();
                break;
        }

        GenerateMazeImage();
    }

    private void NewEndPoint()
    {
        GD.Randomize();

        FillInCell(ref end_point);

        switch (second_point_type)
        {
            case EPointType.None:
                break;

            case EPointType.Open:
                end_point = CreateOpenCell();
                break;
        }

        GenerateMazeImage();
    }

    //Helper Methods
    /*private void SetRandomPoint(bool isStart)
    {
        GD.Randomize();

        if (grid.GetWidth() >= 2 || grid.GetHeight() >= 2) {
            bool again = true;

            do {
                again = true;
                int walls = 0;
                Vector2I point = new Vector2I((int)(GD.Randi() % grid.GetWidth()), (int)(GD.Randi() % grid.GetHeight()));
                Cell cell = grid.cells[point.X, point.Y];

                //Count walls
                if (!cell.north) { walls += 1; }
                if (!cell.south) { walls += 1; }
                if (!cell.east) { walls += 1; }
                if (!cell.west) { walls += 1; }

                if (walls == 3) {
                    again = false;
                }

                //Which Point
                if (isStart)
                {
                    start_point = point;
                }
                else
                {
                    end_point = point;
                }

            } while (start_point == end_point || again);
        }
    }*/

    private Cell CreateOpenCell()
    {
        Cell open_cell = null;
        List<Cell> edge_cells = new List<Cell>();

        //Create a List of Edge Cells
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                if (x == 0 || y == 0 || x >= grid.GetWidth() - 1 || y >= grid.GetHeight() - 1)
                {
                    edge_cells.Add(grid.cells[x, y]);
                }
            }
        }

        //Get Random Cell
        if (edge_cells.Count > 0)
        {
            int count = 0;  //Can't have a start and end point on the same point

            while (count <= 1) {
                count = 0;

                int rand = (int)(GD.Randi() % edge_cells.Count);
                open_cell = edge_cells[rand];

                if (start_point != null)
                {
                    if (start_point.index.X == open_cell.index.X && start_point.index.Y == open_cell.index.Y)
                    {
                        continue;
                    } else {
                        count += 1;
                    }
                } else {
                    count += 1;
                }

                if (end_point != null)
                {
                    if (end_point.index.X == open_cell.index.X && end_point.index.Y == open_cell.index.Y)
                    {
                        continue;
                    } else
                    {
                        count += 1;
                    }

                } else {
                    count += 1;
                }
            }
        }

        //Open Wall
        if (open_cell != null)
        {
            List<MazeGenerator.Direction> directions = new List<MazeGenerator.Direction>();
            Vector2I index = open_cell.index;
            
            //Add Possible Directions
            if (index.X == 0) { directions.Add(MazeGenerator.Direction.west); }
            if (index.Y == 0) { directions.Add(MazeGenerator.Direction.north); }
            if (index.X == grid.GetWidth() - 1) { directions.Add(MazeGenerator.Direction.east); }
            if (index.Y == grid.GetHeight() - 1) { directions.Add(MazeGenerator.Direction.south); }

            MazeGenerator.Direction dir = directions[(int)(GD.Randi() % directions.Count)];

            switch(dir)
            {
                case MazeGenerator.Direction.north:
                    open_cell.north = true;
                    break;
                case MazeGenerator.Direction.south:
                    open_cell.south = true;
                    break;
                case MazeGenerator.Direction.east:
                    open_cell.east = true;
                    break;
                case MazeGenerator.Direction.west:
                    open_cell.west = true;
                    break;
            }
        }

        return open_cell;
    }

    private void FillInCell(ref Cell open_cell)
    {
        //Fill in Cell
        if (open_cell != null)
        {
            Vector2I index = open_cell.index;

            //Add Possible Directions
            if (index.X == 0) { open_cell.west = false; }
            if (index.Y == 0) { open_cell.north = false; }
            if (index.X == grid.GetWidth() - 1) { open_cell.east = false; }
            if (index.Y == grid.GetHeight() - 1) { open_cell.south = false; }
        }
    }
}
