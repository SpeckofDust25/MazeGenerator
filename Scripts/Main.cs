using Godot;
using System;
using System.ComponentModel.Design;

public partial class Main : CanvasLayer
{
	private Vector2 vec;
	private Grid grid;

	//Image Properties
	private Image image;
	private TextureRect texture_rect;

	public override void _Ready()
	{
        texture_rect = GetNode<TextureRect>("Interface/MazePanel/MazeImage");
      
		grid = new Grid(20, 20, 1, 5);
		vec = new Vector2(0, 2);

		MazeGenerator.AldousBroderAlgorithm(ref grid);
		GenerateMazeImage();
	}

	private void GenerateMazeImage() {

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
}
