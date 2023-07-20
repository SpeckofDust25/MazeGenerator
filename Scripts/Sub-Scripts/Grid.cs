using System;
using System.Runtime.InteropServices;
using Godot;

public class Grid {

	private int cell_size = 10;
	private int wall_size = 10;
	private int width = 10;
	private int height = 10;

	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1, int _thickness = 1, int _cell_size = 1) {
		width = _width;
		height = _height;
		cells = new Cell[width, height];

		//Populate Grid
		for (int x = 0; x < cells.GetLength(0); x++) {
			for (int y = 0; y < cells.GetLength(1); y++) {
				cells[x, y] = new Cell();
				cells[x, y].index = new Vector2I(x, y);
			}
		}

		wall_size = _thickness;
		cell_size = _cell_size;
	}

	//Setters
	public void SetWallSize(int _wall_size) {
		if (_wall_size > cell_size)
		{
			wall_size = cell_size - 1;
		} else {
			wall_size = _wall_size;
        }
	}


	//Getters

	//Grid Data: Width, Height Getters in Grid Units
	public int GetTotalCells() {
		return width * height;
	}

	public int GetWidth() {
		return width;
	}

	public int GetHeight() {
		return height;
	}

	public int GetCellSize() {
		return cell_size;
	}

	public int GetWallSize() {
		return wall_size;
	}

	//Image Data: Width, Height Getters in Pixels
	public int GetTotalWidthWallsPx()
	{
		return (GetWidth() * wall_size) + wall_size;
	}

	public int GetTotalHeightWallsPx()
	{
        return (GetHeight() * wall_size) + wall_size;
	}

	public int GetTotalWidthCellsPx()
	{
		return GetWidth() * GetCellSize();
	}

	public int GetTotalHeightCellsPx()
	{
		return GetHeight() * GetCellSize();
	}

	public int GetTotalWidthPx()
	{
		return GetTotalWidthWallsPx() + GetTotalWidthCellsPx();
	}

	public int GetTotalHeightPx()
	{
        return GetTotalHeightWallsPx() + GetTotalHeightCellsPx();
    }

	public Vector2I GetCellSizePx()
	{
        return new Vector2I(GetCellSize() + (GetWallSize() * 2), GetCellSize() + (GetWallSize() * 2));
    }

	public Rect2I GetNorthWall(int x, int y)	
	{
		Vector2I size = new Vector2I((GetWallSize() * 2) + GetCellSize(), GetWallSize());
		Rect2I north_wall = new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);

		return north_wall;
	}

    public Rect2I GetSouthWall(int x, int y)
    {
		y += 1;
        Vector2I size = new Vector2I((GetWallSize() * 2) + GetCellSize(), GetWallSize());
        Rect2I south_wall = new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);

        return south_wall;
    }

	public Rect2I GetEastWall(int x, int y) 
    {
		x += 1;
        Vector2I size = new Vector2I(GetWallSize(), GetWallSize() + GetCellSize() + GetWallSize());
        Rect2I east_wall = new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);

        return east_wall;
    }

    public Rect2I GetWestWall(int x, int y)
    {
        Vector2I size = new Vector2I(GetWallSize(), GetWallSize() + GetCellSize() + GetWallSize());
        Rect2I west_wall = new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);

        return west_wall;
    } 
}