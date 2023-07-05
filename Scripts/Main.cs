using Godot;
using System;
using System.ComponentModel.Design;

public partial class Main : CanvasLayer
{
	private Grid grid;

    //Nodes
    private TextureRect texture_rect;
	private Panel maze_properties;

	//Image
    private Image image;

	public override void _Ready()
	{
		SetupNodes();
		SetupConnections();
      
		grid = new Grid(20, 20, 1, 5);

		MazeGenerator.AldousBroderAlgorithm(ref grid);
		GenerateMazeImage();
	}

	private void GenerateMazeImage() {

		if (image != null) { image.Dispose(); }

		//Setup Variables
		image = Image.Create((grid.GetWidth() * grid.GetCellSize()) + grid.GetThickness(), (grid.GetHeight() * grid.GetCellSize()) + grid.GetThickness(), false, Image.Format.Rgb8);
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
        int thickness = grid.GetThickness();

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

        Callable c_save_image = new Callable(this, "SaveImage");
        maze_properties.Connect("SaveImage", c_save_image);

		Callable c_generate_maze = new Callable(this, "GenerateMaze");
		maze_properties.Connect("GenerateMaze", c_generate_maze);
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
        grid = new Grid(20, 20, 1, 5);

        MazeGenerator.AldousBroderAlgorithm(ref grid);
        GenerateMazeImage();
    }
}
