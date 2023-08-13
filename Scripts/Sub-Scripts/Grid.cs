using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Godot;

public class Grid {

	public enum Direction { none, north, south, east, west } 

	private int cell_size = 10;
	private int wall_size = 10;
	private int exterior_size = 0;
	private int width = 10;
	private int height = 10;

	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1, int _thickness = 1, int _cell_size = 1, int _exterior_size = 0) {
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
		exterior_size = _exterior_size;
	}

	//Setters
	public void SetCellSize(int _cell_size)
	{
		cell_size = _cell_size;
	}

	public void SetWallSize(int _wall_size) {
		wall_size = _wall_size;
	}

	public void SetExteriorSize(int _exterior_size)
	{
		exterior_size = _exterior_size;
	}

	//Getters

	//Grid Data----------------------------------
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

	public int GetExteriorSize()
	{
		return exterior_size;
	}
    //-------------------------------------------

	//Image Drawing Methods ---------------------
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
		return GetTotalWidthWallsPx() + GetTotalWidthCellsPx() + (exterior_size * 2);
	}

	public int GetTotalHeightPx()
	{
        return GetTotalHeightWallsPx() + GetTotalHeightCellsPx() + (exterior_size * 2);
    }

    //Image Size of Walls------------------------
    public Rect2I GetHorizontalWall(int x, int y, bool is_east)
    {
        if (is_east) { x += 1; }

        Vector2I size = new Vector2I(GetWallSize(), (GetWallSize() * 2) + GetCellSize() );
        Rect2I horizontal_wall = new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);

        return horizontal_wall;
    }

    public Rect2I GetVerticalWall(int x, int y, bool is_south)
    {
        if (is_south) { y += 1; }

        Vector2I size = new Vector2I((GetWallSize() * 2) + GetCellSize(), GetWallSize());
        Rect2I vertical_wall = new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);

        return vertical_wall;
    }

	public Rect2I GetCellSizePx(int x, int y)
	{
		Vector2I size = new Vector2I((GetWallSize() * 2) + GetCellSize(), (GetWallSize() * 2) + GetCellSize());
		return new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);
	}
    //-------------------------------------------


    //Single Cell Data---------------------------
	public Cell GetRandomCell()
	{
        return cells[(int)(GD.Randi() % GetWidth()), (int)(GD.Randi() % GetHeight())];
    }
	
	public List<Direction> GetNeighbors(Vector2I index, bool north, bool south, bool east, bool west)
	{
		List<Direction> directions = new List<Direction>();

        if (index.Y != 0 && north) { directions.Add(Direction.north); }
        if (index.Y < GetHeight() - 1 && south) { directions.Add(Direction.south); }
        if (index.X < GetWidth() - 1 && east) { directions.Add(Direction.east); }
        if (index.X != 0 && west) { directions.Add(Direction.west); }

        return directions;
	}

    public List<Direction> GetValidNeighbors(Vector2I index)
    {
		//Properties
		List<Direction> directions = new List<Direction>();
		Cell temp_cell = null;
        bool can_north = false;
        bool can_south = false;
        bool can_east = false;
        bool can_west = false;


        //Boundary
        if (index.X != 0) { can_west = true; }
        if (index.Y != 0) { can_north = true; }
        if (index.X < GetWidth() - 1) { can_east = true; }
        if (index.Y < GetHeight() - 1) { can_south = true; }

		//Valid Cell: North, South, East, West
		if (can_north)
		{
			temp_cell = cells[index.X, index.Y - 1];

			if (!temp_cell.dead_cell)
			{
				directions.Add(Direction.north);
			}
		}

		if (can_south)
		{
            temp_cell = cells[index.X, index.Y + 1];

			if (!temp_cell.dead_cell)
			{
				directions.Add(Direction.south);
			}
        }

        if (can_east)
        {
            temp_cell = cells[index.X + 1, index.Y];

			if (!temp_cell.dead_cell)
			{
				directions.Add(Direction.east);
			}
        }

        if (can_west)
		{
			temp_cell = cells[index.X - 1, index.Y];

			if (!temp_cell.dead_cell)
			{
				directions.Add(Direction.west);
			}
		}

		return directions;
    }
	//-------------------------------------------

	//Grid Sections------------------------------

	//-------------------------------------------

    //Total Grid Data----------------------------
    public List<Cell> GetAllEdgeDeadends()
	{
		List<Cell> deadends = new List<Cell>();

		for (int x = 0; x < cells.GetLength(0); x++)
		{
			for (int y = 0; y < cells.GetLength(1); y++)
			{
                if (x == 0 || y == 0 || x >= GetWidth() - 1 || y >= GetHeight() - 1)
                {
					int count = 0;

					//Count Walls
					if (!cells[x, y].north) { count += 1; }
					if (!cells[x, y].south) { count += 1; }
					if (!cells[x, y].east) { count += 1; }
					if (!cells[x, y].west) { count += 1; }

					//Add to List
					if (count >= 3)
					{
						deadends.Add(cells[x, y]);
					}
				}
			}
		}

		return deadends;
	}

	public List<Cell> GetAllEdgeCells() {

		List<Cell> edges = new List<Cell>();

        //Create a List of Edge Cells
        for (int x = 0; x < GetWidth(); x++)
        {
            for (int y = 0; y < GetHeight(); y++)
            {
                if (x == 0 || y == 0 || x >= GetWidth() - 1 || y >= GetHeight() - 1)
                {
                    edges.Add(cells[x, y]);
                }
            }
        }

		return edges;
    }

	public int GetTotalDeadCells()
	{
		int count = 0;

		for (int x = 0; x < GetWidth(); x++)
		{
			for (int y = 0; y < GetHeight(); y++)
			{
				if (cells[x, y].dead_cell) { count += 1; }
			}
		}

		return count;
	}
	//-------------------------------------------

	

}