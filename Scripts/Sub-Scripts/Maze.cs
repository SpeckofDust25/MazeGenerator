using Godot;
using MazeGeneratorGlobal;
using System.Collections.Generic;

public class Maze: Grid
{
    //Constructor 
    public Maze(int _width, int _height, int _wall_size, int _cell_size) : base(_width, _height)
    {
        wall_size = _wall_size;
        cell_size = _cell_size;
    }

    private int cell_size = 10;
    private int wall_size = 10;
    public Points start_end_points;

    //Maze Modification: Removes all dead ends
    public void Braid(float chance) { 

        List<Cell> deadends = GetAllValidDeadends();

        for (int i = deadends.Count - 1; i >= 0; i--)
        { //Iterate Through Deadends

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
            if (dir.Count > 0)
            {

                ERectangleDirections chosen_direction = dir[(int)(GD.Randi() % dir.Count)];

                //Prioritize going to another dead end
                for (int g = 0; g < dir.Count; g++)
                {
                    if (deadends.Contains(GetCellInDirection(current_cell.index, dir[g])))
                    {
                        chosen_direction = dir[g];
                    }
                }

                Cell neighbor_cell = GetCellInDirection(current_cell.index, chosen_direction);

                switch (chosen_direction)
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

    //Setters------------------------------------
    public void SetCellSize(int _cell_size)
    {
        cell_size = _cell_size;
    }

    public void SetWallSize(int _wall_size)
    {
        wall_size = _wall_size;
    }

    public void SetMask(Mask mask)
    {
        //Populate DeadCells
        if (MazeMask.mask != null)
        {
            if (mask.dead_cells.GetLength(0) == GetWidth() && mask.dead_cells.GetLength(1) == GetHeight())
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
    }
    //-------------------------------------------

    //Getters------------------------------------
    public int GetCellSize()
    {
        return cell_size;
    }

    public int GetWallSize()
    {
        return wall_size;
    }

    public Cell GetStartCell()
    {
        Cell start = null;

        if (start_end_points != null)
        {
            if (start_end_points.start != null)
            {
                start = start_end_points.start;
            }
        }

        return start;
    }

    public Cell GetEndCell()
    {
        Cell end = null;

        if (start_end_points != null)
        {
            if (start_end_points.end != null)
            {
                end = start_end_points.end;
            }
        }

        return end;
    }

    public ERectangleDirections GetStartDirection()
    {
        ERectangleDirections direction = ERectangleDirections.None;

        if (start_end_points != null)
        {
            direction = start_end_points.start_direction;
        }

        return direction;
    }

    public ERectangleDirections GetEndDirection()
    {
        ERectangleDirections direction = ERectangleDirections.None;

        if (start_end_points != null)
        {
            direction = start_end_points.end_direction;
        }

        return direction;
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

        Vector2I size = new Vector2I(GetWallSize(), (GetWallSize() * 2) + GetCellSize());
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
        return new Rect2I(x * (GetCellSize() + GetWallSize()), y * (GetCellSize() + GetWallSize()), size);
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
        for (int x = 0; x < GetWidth(); x++)
        {
            for (int y = 0; y < GetHeight(); y++)
            {
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


    //Routing Getters----------------------------
    public EBias GetBias(float horizontal_bias)
    {
        EBias bias_direction = EBias.None;

        if (horizontal_bias == 0) { return EBias.Vertical; }

        float rand_value = GD.Randf();

        if (rand_value <= horizontal_bias)
        {
            bias_direction = EBias.Horizontal;
        }
        else
        {
            bias_direction = EBias.Vertical;
        }

        return bias_direction;
    }
    //-------------------------------------------

}