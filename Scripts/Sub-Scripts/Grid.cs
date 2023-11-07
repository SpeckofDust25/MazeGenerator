using System.Collections.Generic;
using Godot;
using MazeGeneratorGlobal;

public class Grid {

	private int width = 10;
	private int height = 10;

	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1) {
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
	}

	//Setters
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
    //-------------------------------------------


    //Single Cell Data---------------------------
	public Cell GetRandomCell()
	{
        return cells[(int)(GD.Randi() % GetWidth()), (int)(GD.Randi() % GetHeight())];
    }
	
	public Cell GetValidRandomCell()
	{
		Cell temp_cell;

		do {
			temp_cell = cells[(int)(GD.Randi() % GetWidth()), (int)(GD.Randi() % GetHeight())];
		} while (temp_cell.dead_cell);

		return temp_cell;
    }

	public List <Cell> GetValidNeighborCells(Vector2I index)
	{
        //Properties
        List<Cell> neighbor_cells = new List<Cell>();
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
			Cell temp_cell = cells[index.X, index.Y - 1];

            if (!temp_cell.dead_cell)
            {
                neighbor_cells.Add(temp_cell);
            }
        }

        if (can_south)
        {
            Cell temp_cell = cells[index.X, index.Y + 1];

            if (!temp_cell.dead_cell)
            {
				neighbor_cells.Add(temp_cell);
            }
        }

        if (can_east)
        {
            Cell temp_cell = cells[index.X + 1, index.Y];

            if (!temp_cell.dead_cell)
            {
				neighbor_cells.Add(temp_cell);
            }
        }

        if (can_west)
        {
            Cell temp_cell = cells[index.X - 1, index.Y];

            if (!temp_cell.dead_cell)
            {
				neighbor_cells.Add(temp_cell);
            }
        }

        return neighbor_cells;
    }

	public List<ERectangleDirections> GetNeighbors(Vector2I index, bool north, bool south, bool east, bool west)
	{
        //Get Neighbor Directions Within Bounds
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
			Cell temp_cell = cells[index.X, index.Y - 1];

			if (!temp_cell.dead_cell)
			{
				directions.Add(ERectangleDirections.North);
			}
		}

		if (can_south)
		{
            Cell temp_cell = cells[index.X, index.Y + 1];

			if (!temp_cell.dead_cell)
			{
				directions.Add(ERectangleDirections.South);
			}
        }

        if (can_east)
        {
            Cell temp_cell = cells[index.X + 1, index.Y];

			if (!temp_cell.dead_cell)
			{
				directions.Add(ERectangleDirections.East);
			}
        }

        if (can_west)
		{
			Cell temp_cell = cells[index.X - 1, index.Y];

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

        Cell temp_cell;
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

    public List<ERectangleDirections> GetValidNeighborsNoWalls(Vector2I index)
    {
        //Properties
        List<ERectangleDirections> directions = new List<ERectangleDirections>();
        Cell c_cell = cells[index.X, index.Y];
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
        if (can_north && c_cell.north)
        {
            Cell temp_cell = cells[index.X, index.Y - 1];

            if (!temp_cell.dead_cell)
            {
                directions.Add(ERectangleDirections.North);
            }
        }

        if (can_south && c_cell.south)
        {
            Cell temp_cell = cells[index.X, index.Y + 1];

            if (!temp_cell.dead_cell)
            {
                directions.Add(ERectangleDirections.South);
            }
        }

        if (can_east && c_cell.east)
        {
            Cell temp_cell = cells[index.X + 1, index.Y];

            if (!temp_cell.dead_cell)
            {
                directions.Add(ERectangleDirections.East);
            }
        }

        if (can_west && c_cell.west)
        {
            Cell temp_cell = cells[index.X - 1, index.Y];

            if (!temp_cell.dead_cell)
            {
                directions.Add(ERectangleDirections.West);
            }
        }

        return directions;
    }

    public List<ERectangleDirections> GetInvalidNeighbors(Vector2I index)
    {
        //Properties
        List<ERectangleDirections> directions = new List<ERectangleDirections>();
        bool can_north = false;
        bool can_south = false;
        bool can_east = false;
        bool can_west = false;

        //Boundary
        if (index.X != 0) { can_west = true; }
        if (index.Y != 0) { can_north = true; }
        if (index.X < GetWidth() - 1) { can_east = true; }
        if (index.Y < GetHeight() - 1) { can_south = true; }

        //Invalid Directions: North, South, East, West
        if (can_north)
        {
            Cell temp_cell = cells[index.X, index.Y - 1];

            if (temp_cell.dead_cell)
            {
                directions.Add(ERectangleDirections.North);
            }
        } else {
            directions.Add(ERectangleDirections.North);
        }

        if (can_south)
        {
            Cell temp_cell = cells[index.X, index.Y + 1];

            if (temp_cell.dead_cell)
            {
                directions.Add(ERectangleDirections.South);
            }
        } else {
            directions.Add(ERectangleDirections.South);
        }

        if (can_east)
        {
            Cell temp_cell = cells[index.X + 1, index.Y];

            if (temp_cell.dead_cell)
            {
                directions.Add(ERectangleDirections.East);
            }
        } else {
            directions.Add(ERectangleDirections.East);
        }

        if (can_west)
        {
            Cell temp_cell = cells[index.X - 1, index.Y];

            if (temp_cell.dead_cell)
            {
                directions.Add(ERectangleDirections.West);
            }
        } else {
            directions.Add(ERectangleDirections.West);
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
    public int GetValidCellCount()
    {
        int count = 0;
        for (int x = 0; x < GetWidth(); x++)
        {
            for (int y = 0; y < GetHeight(); y++)
            {
                if (!cells[x, y].dead_cell)
                {
                    count += 1;
                }
            }
        }

        return count;
    }

    public List<Cell> GetAllValidDeadends()
	{
		List<Cell> deadends = new List<Cell>();

		for (int x = 0; x < cells.GetLength(0); x++)
		{
			for (int y = 0; y < cells.GetLength(1); y++)
			{
				if (!cells[x, y].dead_cell) {
					int count = 0;

					//Count Walls
					if (!cells[x, y].north) { count += 1; }
					if (!cells[x, y].south) { count += 1; }
					if (!cells[x, y].east) { count += 1; }
					if (!cells[x, y].west) { count += 1; }

					//Add to List
					if (count == 3)
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

    public List<Vector2I> GetAllPossiblePoints()
    {
        List<Vector2I> points = new List<Vector2I>();

        for (int x = 0; x < GetWidth(); x++)
        {
            for (int y = 0; y < GetHeight(); y++)
            {
                //Outside Bounds
                if (x == 0 || x >= (GetWidth() - 1) || y == 0 || y >= (GetHeight() - 1))
                {
                    if (!cells[x, y].dead_cell)
                    {
                        points.Add(cells[x, y].index);
                    }

                } else {
                    bool can_add = false;

                    if (!cells[x, y].dead_cell) {
                        if (cells[x - 1, y].dead_cell) { can_add = true; }
                        if (cells[x + 1, y].dead_cell) { can_add = true; }
                        if (cells[x, y - 1].dead_cell) { can_add = true; }
                        if (cells[x, y + 1].dead_cell) { can_add = true; }
                    }

                    if (can_add) { points.Add(cells[x, y].index); }
                }
            }
        }

        return points;
    }

	//-------------------------------------------
}