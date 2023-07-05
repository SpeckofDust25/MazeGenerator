using Godot;
using System;
using System.ComponentModel.Design;

public partial class Main : CanvasLayer
{
	enum EMazeType { BinaryTree, Sidewinder, Aldous_Broder }

    //Grid Properties
    private Grid grid;
    private EMazeType maze_type;
	private int x_cells, y_cells;
	private int wall_size, cell_size;

    //Nodes
    private TextureRect texture_rect;
	private Panel maze_properties;

	//Image
    private Image image;

	public override void _Ready() {
		x_cells = 2;
		y_cells = 2;
		wall_size = 1;
		cell_size = 2;
		maze_type = EMazeType.BinaryTree;
		SetupNodes();
		SetupConnections();
		GenerateMaze();
	}

	private void GenerateMazeImage() {

		//Setup Variables
		image = Image.Create((grid.GetWidth() * grid.GetCellSize()) + grid.GetWallSize(), (grid.GetHeight() * grid.GetCellSize()) + grid.GetWallSize(), false, Image.Format.Rgb8);
		image.Fill(Colors.White);   //Fill Image

		for (int x = 0; x < grid.GetWidth(); x++)
		{
			for (int y = 0; y < grid.GetHeight(); y++)
			{
				SetMazeColors(ref image, x, y);
			}
		}

		texture_rect.Texture = ImageTexture.CreateFromImage(image);
	}

	private void SetMazeColors(ref Image _image, int _x, int _y) {
		Cell cell = grid.cells[_x, _y];
        Color background_color = Colors.White;
        Color wall_color = Colors.Black;
        int thickness = grid.GetWallSize();

		Vector2 cell_position = new Vector2(_x * grid.GetCellSize(), _y * grid.GetCellSize());
		
		if (!cell.north) { // North
			image.FillRect(new Rect2I((int)cell_position.X, (int)cell_position.Y, grid.GetCellSize(), thickness), wall_color);
		} 

		if (!cell.south) {  //South
            image.FillRect(new Rect2I((int)cell_position.X, grid.GetCellSize() + (int)cell_position.Y, grid.GetCellSize() + thickness, thickness), wall_color);
        }

		if (!cell.east) {   //East
            image.FillRect(new Rect2I(grid.GetCellSize() + (int)cell_position.X, (int)cell_position.Y, thickness, grid.GetCellSize() + thickness), wall_color);
        }

		if (!cell.west) { //West
            image.FillRect(new Rect2I((int)cell_position.X, (int)cell_position.Y, thickness, grid.GetCellSize()), wall_color);
        }
	}


	//Setup Methods
	private void SetupNodes() {
        texture_rect = GetNode<TextureRect>("Interface/MazePanel/MazeImage");
		maze_properties = GetNode<MazeProperties>("Interface/MazeProperties");
    }

	private void SetupConnections()
	{
		if (!maze_properties.HasSignal("SaveImage")) { GD.PrintErr("Can't Find SaveImage Signal!");  }
		if (!maze_properties.HasSignal("GenerateMaze")) { GD.PrintErr("Can't Find GenerateMaze Signal!"); }
		if (!maze_properties.HasSignal("MazeType")) { GD.PrintErr("Can't Find MazeType Signal!"); }
		if (!maze_properties.HasSignal("CellsX")) { GD.PrintErr("Can't Find CellsX Signal!"); }
		if (!maze_properties.HasSignal("CellsY")) { GD.PrintErr("Can't Find CellsY Signal!"); }
		if (!maze_properties.HasSignal("CellSize")) { GD.PrintErr("Can't Find CellSize Signal!"); }
		if (!maze_properties.HasSignal("WallSize")) { GD.PrintErr("Can't Find WallSize Signal! "); }

        Callable c_save_image = new Callable(this, "SaveImage");
        Callable c_generate_maze = new Callable(this, "GenerateMaze");
        Callable c_maze_type = new Callable(this, "MazeType");
        Callable c_cells_x = new Callable(this, "CellsXChanged");
        Callable c_cells_y = new Callable(this, "CellsYChanged");
		Callable c_cell_size = new Callable(this, "CellSizeChanged");
		Callable c_wall_size = new Callable(this, "WallSizeChanged");

        maze_properties.Connect("SaveImage", c_save_image);
		maze_properties.Connect("GenerateMaze", c_generate_maze);
		maze_properties.Connect("MazeType", c_maze_type);
		maze_properties.Connect("CellsX", c_cells_x);
		maze_properties.Connect("CellsY", c_cells_y);
		maze_properties.Connect("CellSize", c_cell_size);
		maze_properties.Connect("WallSize", c_wall_size);
	}

	//Connections
	private void SaveImage() {
		string save_path;

		if (OS.HasFeature("editor"))
		{
			save_path = "res://Images/" + Time.GetDatetimeStringFromSystem(false, false).Replace(":", "_") + ".png";

			if (!DirAccess.DirExistsAbsolute("res://Images")) {
				DirAccess.MakeDirAbsolute("res://Images");
			}

		} else {
            save_path = OS.GetExecutablePath().GetBaseDir() + "/Images/" + Time.GetDatetimeStringFromSystem(false, false).Replace(":", "_") + ".png";

			if (!DirAccess.DirExistsAbsolute(OS.GetExecutablePath().GetBaseDir() + "/Images")) {
				DirAccess.MakeDirAbsolute(OS.GetExecutablePath().GetBaseDir() + "/Images");
			}
        }

		image.SavePng(save_path);
	}

	private void GenerateMaze() {

		//Create New Grid
        grid = new Grid(x_cells, y_cells, wall_size, cell_size);

		switch(maze_type)
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
		}

        GenerateMazeImage();
    }

	private void MazeType(long index)
	{ 
		switch(index) {
			case ((long)EMazeType.BinaryTree):
				maze_type = EMazeType.BinaryTree;
				break;

            case ((long)EMazeType.Sidewinder):
                maze_type = EMazeType.Sidewinder;
                break;
			
			case ((long)EMazeType.Aldous_Broder):
                maze_type = EMazeType.Aldous_Broder;
                break;
        }
	}

	private void CellsXChanged(double value) {
		x_cells = (int)value;
	}

	private void CellsYChanged(double value) {
		y_cells = (int)value;
	}
	
	private void CellSizeChanged(double value) {
		cell_size = (int)value;
	}

	private void WallSizeChanged(double value) {
		wall_size = (int)value;
	}
}
