using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Godot;
using MazeGeneratorGlobal;

public class Grid {

	private int cell_size = 10;
	private int wall_size = 10;
	private int width = 10;
	private int height = 10;
	private Points start_end_points;

	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1, int _thickness = 1, int _cell_size = 1) {
		width = _width;
		height = _height;
		cells = new Cell[width, height];
        wall_size = _thickness;
        cell_size = _cell_size;

		//Populate Grid
		for (int x = 0; x < cells.GetLength(0); x++) {
			for (int y = 0; y < cells.GetLength(1); y++) {
				cells[x, y] = new Cell();
				cells[x, y].index = new Vector2I(x, y);
			}
		}
	}

	public void Braid(float chance)	{ //Remove All Dead Ends
	
		List<Cell> deadends = GetAllValidDeadends();
        
        for (int i = deadends.Count - 1; i >= 0; i--) { //Iterate Through Deadends
            
            if (GD.Randf() > chance) { continue; } //Deadend Removal Chance

            Cell current_cell = deadends[i];
            List<ERectangleDirections> dir = GetValidNeighbors(current_cell.index);

            int count = 0;

            if (!current_cell.north) { count += 1; }
            if (!current_cell.south) { count += 1; }
            if (!current_cell.east) { count += 1; }
            if (!current_cell.west) { count += 1; }

            if (count < 3) { continue; }

            //Only Get Valid Directions
            for (int l = dir.Count - 1; l >= 0; l--)
            {
                switch (dir[l])
                {
                    case ERectangleDirections.North:
                        if (current_cell.north)
                        {
                            dir.RemoveAt(l);
                        }
                        break;

                    case ERectangleDirections.South:
                        if (current_cell.south)
                        {
                            dir.RemoveAt(l);
                        }
                        break;

                    case ERectangleDirections.East:
                        if (current_cell.east)
                        {
                            dir.RemoveAt(l);
                        }
                        break;

                    case ERectangleDirections.West:
                        if (current_cell.west)
                        {
                            dir.RemoveAt(l);
                        }
                        break;
                }
            }

            //Carve Random Path
            if (dir.Count > 0) {

                ERectangleDirections chosen_direction = dir[(int)(GD.Randi() % dir.Count)];

                //Prioritize going to another dead end
                for (int g = 0; g < dir.Count; g++)
                {
                    if (deadends.Contains(GetCellInDirection(current_cell.index, dir[g]))) {
                        chosen_direction = dir[g];
                    }
                }

                Cell neighbor_cell = GetCellInDirection(current_cell.index, chosen_direction);

                switch(chosen_direction)
                {
                    case ERectangleDirections.North:
                        current_cell.north = true;
                        neighbor_cell.south = true;
                        break;

                    case ERectangleDirections.South:
                        current_cell.south = true;
                        neighbor_cell.north = true;
                        break;

                    case ERectangleDirections.East:
                        current_cell.east = true;
                        neighbor_cell.west = true;
                        break;

                    case ERectangleDirections.West:
                        current_cell.west = true;
                        neighbor_cell.east = true;
                        break;
                }
            }

            deadends.RemoveAt(i);
        }
    }

	public void Unicursal()
	{

	}

	public void PartialBraid()	//Remove Some Dead Ends
	{

	}

	public void Rooms()
	{

	}

	//Setters
	public void SetMask(Mask mask)
	{
        //Populate DeadCells
        if (MazeMask.mask != null)
        {
			if (mask.dead_cells.GetLength(0) == GetWidth() && mask.dead_cells.GetLength(1) == GetHeight()) {
				for (int x = 0; x < mask.dead_cells.GetLength(0); x++)
				{
					for (int y = 0; y < mask.dead_cells.GetLength(1); y++)
					{
						cells[x, y].dead_cell = mask.dead_cells[x, y];
					}
				}
			}
        }
    }
	
	public void UpdatePoints(EPoints point_type)
	{
        switch(point_type)
        {
            case EPoints.None:
                break;

            case EPoints.Random:
                List<Cell> points = GetAllPossiblePoints();

                Cell first = null;
                Cell second = null;

                if (points.Count > 0) {
                    first = points[(int)(GD.Randi() % points.Count)];
                }

                points.Remove(first);

                if (points.Count > 0) {
                    second = points[(int)(GD.Randi() % points.Count)];
                }

                start_end_points = new Points(ref first, ref second, GetInvalidNeighbors(first.index), GetInvalidNeighbors(second.index));
                break;

            case EPoints.Furthest:
                break;

            case EPoints.Easy:
                break;

            case EPoints.Medium:
                break;

            case EPoints.Hard:
                break;
        }
	}

	public void SetCellSize(int _cell_size)
	{
		cell_size = _cell_size;
	}

	public void SetWallSize(int _wall_size) {
		wall_size = _wall_size;
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

    public Vector2I GetStartPoint()
    {
        Vector2I result = Vector2I.Zero;

        if (start_end_points != null) {
            if (start_end_points.start != null)
            {
                result = start_end_points.start.index;
            }
        }

        return result;
    }

    public Vector2I GetEndPoint()
    {
        Vector2I result = Vector2I.Zero;

        if (start_end_points != null) {
            if (start_end_points.end != null)
            {
                result = start_end_points.end.index;
            }
        }

        return result;
    }

    //-------------------------------------------

	//Image Drawing Methods ---------------------
	public int GetImageWidth()
	{
		int wall_width = (GetWidth() * wall_size) + wall_size;
		int cell_width = GetWidth() * GetCellSize();
		return wall_width + cell_width;
	}

	public int GetImageHeight()
	{
		int wall_height = (GetHeight() * wall_size) + wall_size;
		int cell_height = GetHeight() * GetCellSize();
        return wall_height + cell_height;
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

    public List<Cell> GetAllPossiblePoints()
    {
        List<Cell> points = new List<Cell>();

        for (int x = 0; x < GetWidth(); x++)
        {
            for (int y = 0; y < GetHeight(); y++)
            {
                //Outside Bounds
                if (x == 0 || x >= (GetWidth() - 1) || y == 0 || y >= (GetHeight() - 1))
                {
                    if (!cells[x, y].dead_cell)
                    {
                        points.Add(cells[x, y]);
                    }

                } else {
                    bool can_add = false;

                    if (!cells[x, y].dead_cell) {
                        if (cells[x - 1, y].dead_cell) { can_add = true; }
                        if (cells[x + 1, y].dead_cell) { can_add = true; }
                        if (cells[x, y - 1].dead_cell) { can_add = true; }
                        if (cells[x, y + 1].dead_cell) { can_add = true; }
                    }

                    if (can_add) { points.Add(cells[x, y]); }
                }
            }
        }

        return points;
    }

	//-------------------------------------------

	//Routing Getters----------------------------
    public EBias GetBias(float horizontal_bias)
    {
        EBias bias_direction = EBias.None;

        if (horizontal_bias == 0) { return EBias.Vertical; }

        float rand_value = GD.Randf();

        if (rand_value <= horizontal_bias)
        {
            bias_direction = EBias.Horizontal;
        } else {
            bias_direction = EBias.Vertical;
        }

        return bias_direction;
    }
    //-------------------------------------------

}