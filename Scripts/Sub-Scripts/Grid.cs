using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Godot;
using MazeGeneratorGlobal;

public class Grid {

	private int cell_size = 10;
	private int wall_size = 10;
	private int exterior_size = 0;
	private int width = 10;
	private int height = 10;
	private List<Point> points = new List<Point>();

	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1, int _thickness = 1, int _cell_size = 1, int _exterior_size = 0) {
		width = _width;
		height = _height;
		cells = new Cell[width, height];
        wall_size = _thickness;
        cell_size = _cell_size;
        exterior_size = _exterior_size;

		points.Add(new Point(EPoints.None));
		points.Add(new Point(EPoints.None));

		//Populate Grid
		for (int x = 0; x < cells.GetLength(0); x++) {
			for (int y = 0; y < cells.GetLength(1); y++) {
				cells[x, y] = new Cell();
				cells[x, y].index = new Vector2I(x, y);
			}
		}
	}


	//Setters
	public void SetMask(Mask mask)
	{
        //Populate DeadCells
        if (MazeMask.mask != null)
        {
            for (int x = 0; x < mask.dead_cells.GetLength(0); x++)
            {
                for (int y = 0; y < mask.dead_cells.GetLength(1); y++)
                {
                    cells[x, y].dead_cell = mask.dead_cells[x, y];
                }
            }
        }
    }
	
	public void SetPointType(bool is_first, EPoints type)
	{
		if (is_first)
		{
			points[0].SetPointType(type);
		} else {
			points[1].SetPointType(type);
		}
	}

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

	public void SetWidth(int _width)
	{
		width = _width;
	}

	public void SetHeight(int _height)
	{
		height = _height;
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
	public int GetImageWidth()
	{
		int wall_width = (GetWidth() * wall_size) + wall_size;
		int cell_width = GetWidth() * GetCellSize();
		return wall_width + cell_width + (exterior_size * 2);
	}

	public int GetImageHeight()
	{
		int wall_height = (GetHeight() * wall_size) + wall_size;
		int cell_height = GetHeight() * GetCellSize();
        return wall_height + cell_height + (exterior_size * 2);
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

	public Rect2I GetHorizontalWallFull(int x, int y, bool is_east)
	{
        if (is_east) { x += 1; }

        Vector2I size = new Vector2I(GetWallSize(), ((GetWallSize() * 2) + GetCellSize()) * GetHeight());
        Rect2I horizontal_wall = new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);

        return horizontal_wall;
    }

	public Rect2I GetVerticalWallFull(int x, int y, bool is_south)
	{
        if (is_south) { y += 1; }

        Vector2I size = new Vector2I(((GetWallSize() * 2) + GetCellSize()) * GetWidth(), GetWallSize());
        Rect2I vertical_wall = new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);

        return vertical_wall;
    }

	public Rect2I GetCellSizePx(int x, int y)
	{
		Vector2I size = new Vector2I((GetWallSize() * 2) + GetCellSize(), (GetWallSize() * 2) + GetCellSize());
		return new Rect2I(x * (GetCellSize() + wall_size), y * (GetCellSize() + wall_size), size);
	}

	public Rect2I GetInsideCellSizePx(int x, int y)
	{
        Vector2I size = new Vector2I(GetCellSize(), GetCellSize());
        return new Rect2I(x * (GetCellSize() + GetWallSize()) + GetWallSize(), y * (GetCellSize() + GetWallSize()) + GetWallSize(), size);
    }
    
	public Vector2I GetCellIndexAtImagePosition(Vector2I position)
	{
		int x_position = 0;
		int y_position = 0;

		//Get X and Y Cells
		for (int x = 0; x < GetWidth(); x++) {
            for (int y = 0; y < GetHeight(); y++) {
				Rect2I rect = GetCellSizePx(x, y);

				if (rect.HasPoint(position))
				{
					x_position = x;
					y_position = y;
					break;
				}
			}
        }

		return new Vector2I(x_position, y_position);
	}
	//-------------------------------------------


    //Single Cell Data---------------------------
	public Cell GetRandomCell()
	{
        return cells[(int)(GD.Randi() % GetWidth()), (int)(GD.Randi() % GetHeight())];
    }
	
	public Cell GetValidRandomCell()
	{
		Cell temp_cell = null;

		do {
			temp_cell = cells[(int)(GD.Randi() % GetWidth()), (int)(GD.Randi() % GetHeight())];
		} while (temp_cell.dead_cell);

		return temp_cell;
    }

	public List<ERectangleDirections> GetNeighbors(Vector2I index, bool north, bool south, bool east, bool west)
	{
		List<ERectangleDirections> directions = new List<ERectangleDirections>();

        if (index.Y != 0 && north) { directions.Add(ERectangleDirections.North); }
        if (index.Y < GetHeight() - 1 && south) { directions.Add(ERectangleDirections.South); }
        if (index.X < GetWidth() - 1 && east) { directions.Add(ERectangleDirections.East); }
        if (index.X != 0 && west) { directions.Add(ERectangleDirections.West); }

        return directions;
	}

    public List<ERectangleDirections> GetValidNeighbors(Vector2I index)
    {
		//Properties
		List<ERectangleDirections> directions = new List<ERectangleDirections>();
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
				directions.Add(ERectangleDirections.North);
			}
		}

		if (can_south)
		{
            temp_cell = cells[index.X, index.Y + 1];

			if (!temp_cell.dead_cell)
			{
				directions.Add(ERectangleDirections.South);
			}
        }

        if (can_east)
        {
            temp_cell = cells[index.X + 1, index.Y];

			if (!temp_cell.dead_cell)
			{
				directions.Add(ERectangleDirections.East);
			}
        }

        if (can_west)
		{
			temp_cell = cells[index.X - 1, index.Y];

			if (!temp_cell.dead_cell)
			{
				directions.Add(ERectangleDirections.West);
			}
		}

		return directions;
    }
	
	public List<ERectangleDirections> GetValidUnvisitedNeighbors(Vector2I index)
	{
        //Properties
        List<ERectangleDirections> directions = new List<ERectangleDirections>();

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

            if (!temp_cell.dead_cell && !temp_cell.IsVisited())
            {
                directions.Add(ERectangleDirections.North);
            }
        }

        if (can_south)
        {
            temp_cell = cells[index.X, index.Y + 1];

            if (!temp_cell.dead_cell && !temp_cell.IsVisited())
            {
                directions.Add(ERectangleDirections.South);
            }
        }

        if (can_east)
        {
            temp_cell = cells[index.X + 1, index.Y];

            if (!temp_cell.dead_cell && !temp_cell.IsVisited())
            {
                directions.Add(ERectangleDirections.East);
            }
        }

        if (can_west)
        {
            temp_cell = cells[index.X - 1, index.Y];

            if (!temp_cell.dead_cell && !temp_cell.IsVisited())
            {
                directions.Add(ERectangleDirections.West);
            }
        }

        return directions;
    }

	public Cell GetCellInDirection(Vector2I index, ERectangleDirections direction)
	{
		Cell cell = null;

        //Get Next Cell
        switch (direction)
        {
            case ERectangleDirections.North: //North
                cell = cells[index.X, index.Y - 1];
                break;

            case ERectangleDirections.South: //South
                cell = cells[index.X, index.Y + 1];
                break;

            case ERectangleDirections.East: //East
				cell = cells[index.X + 1, index.Y];
                break;

            case ERectangleDirections.West: //West
                cell = cells[index.X - 1, index.Y];
                break;
        }

		return cell;
    }
	//-------------------------------------------


    //Total Grid Data----------------------------
    public List<Cell> GetAllValidEdgeDeadends()
	{
		List<Cell> deadends = new List<Cell>();

		for (int x = 0; x < cells.GetLength(0); x++)
		{
			for (int y = 0; y < cells.GetLength(1); y++)
			{
                if (x == 0 || y == 0 || x >= GetWidth() - 1 || y >= GetHeight() - 1)
                {
					if (!cells[x, y].dead_cell) {
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