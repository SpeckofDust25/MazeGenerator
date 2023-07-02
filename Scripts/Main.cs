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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        texture_rect = GetNode<TextureRect>("Interface/MazePanel/MazeImage");
      
		grid = new Grid(40, 40);
		vec = new Vector2(0, 2);

		MazeGenerator.AldousBroderAlgorithm(ref grid);
		GenerateMazeImage();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		
	}

	private void GenerateMazeImage() {

		//Setup Variables
		image = Image.Create(grid.GetWidth() * grid.GetCellSize(), grid.GetHeight() * grid.GetCellSize(), false, Image.Format.Rgb8);

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
		Color color = background_color;
        int thickness = grid.GetThickness();

        for (int x = 0; x < grid.GetCellSize(); x++) { 
			for(int y = 0; y < grid.GetCellSize(); y++)
			{
                if (!cell.east && x > grid.GetCellSize() - thickness - 1) { //Right
					color = wall_color;

				} else if (!cell.west && x < thickness)	{ //Left
                    color = wall_color;

                } else if (!cell.north && y < thickness) {  //Up
                    color = wall_color;

                } else if (!cell.south && y > grid.GetCellSize() - thickness - 1) { //Down
					color = wall_color;

                } else {
					color = background_color;
				}

				//Set Colors Based on Cells
                image.SetPixelv(new Vector2I(x + (_x * grid.GetCellSize()), y + (_y * grid.GetCellSize())), color);
            }
		}
	}
}
