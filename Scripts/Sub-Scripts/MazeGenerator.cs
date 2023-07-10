using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public static class MazeGenerator
{

    enum Direction { none, north, south, east, west }

    //Move North or East on each cell
    public static Grid BinaryTreeAlgorithm(ref Grid _grid)
    {
        GD.Randomize();

        //Create Maze 
        for (int x = 0; x < _grid.GetWidth(); ++x)
        {
            for (int y = 0; y < _grid.GetHeight(); ++y)
            {
                uint num = GD.Randi() % 2;

                if (x == _grid.GetWidth() - 1 && y == 0)    //Top Right
                {
                    continue;
                }
                else if (y == 0)
                { //At North Wall
                    _grid.cells[x, y].east = true;

                }
                else if (x == _grid.GetWidth() - 1)
                { //At East Wall
                    _grid.cells[x, y].north = true;
                }
                else
                {    //Random Wall
                    if (num == 1)
                    {
                        _grid.cells[x, y].east = true;
                    }
                    else
                    {
                        _grid.cells[x, y].north = true;
                    }
                }
            }
        }

        CarveFullPath(ref _grid);

        return _grid;
    }

    //East-North: Count Previous East Moves, When North Pick Randomly
    public static Grid SidewinderAlgorithm(ref Grid _grid)
    {
        GD.Randomize();

        //Create Maze
        for (int y = 0; y < _grid.GetHeight(); ++y)
        {
            int east_count = 0;

            for (int x = 0; x < _grid.GetWidth(); ++x)
            {
                bool is_north = false;
                uint num = GD.Randi() % 2;

                if (y == 0)
                {   //Top Bias
                    if (x == _grid.GetWidth() - 1)
                    {
                        continue;
                    }
                    else
                    {
                        _grid.cells[x, y].east = true;
                    }

                }
                else if (x == _grid.GetWidth() - 1)
                { //Right Side
                    is_north = true;
                }
                else
                {  //Random Choice

                    if (num == 1)
                    { //East
                        east_count += 1;
                        _grid.cells[x, y].east = true;
                    }
                    else
                    {    //North
                        is_north = true;
                    }
                }


                //Set Northern Path
                if (is_north)
                {
                    uint temp_num = 0;

                    if (east_count > 0)
                    {
                        temp_num = (uint)(GD.Randi() % east_count);
                        _grid.cells[x - temp_num, y].north = true;
                        east_count = 0;
                    }
                    else
                    {
                        _grid.cells[x, y].north = true;
                    }
                }
            }
        }

        CarveFullPath(ref _grid);

        return _grid;
    }

    //Pick a Random point and go in any random direction till all are visited
    public static Grid AldousBroderAlgorithm(ref Grid _grid)
    {
        GD.Randomize();
        int visited_count = 1;
        Cell cell = _grid.cells[(int)(GD.Randi() % (uint)(_grid.GetWidth())), (int)(GD.Randi() % (uint)(_grid.GetHeight()))];

        while (!(visited_count >= (_grid.GetWidth() * _grid.GetHeight())))
        {
            uint num = GD.Randi() % 4;
            Cell next_cell;

            switch (num)
            {
                case 0: //North
                    if (cell.index.Y != 0)
                    {
                        next_cell = _grid.cells[cell.index.X, cell.index.Y - 1];

                        if (!next_cell.IsVisited())
                        {
                            CarvePathManual(ref _grid, cell, Direction.north);
                            visited_count += 1;
                        }

                        cell = _grid.cells[cell.index.X, cell.index.Y - 1];
                    }
                    break;

                case 1: //South
                    if (cell.index.Y != _grid.GetHeight() - 1)
                    {
                        next_cell = _grid.cells[cell.index.X, cell.index.Y + 1];

                        if (!next_cell.IsVisited())
                        {
                            CarvePathManual(ref _grid, cell, Direction.south);
                            visited_count += 1;
                        }

                        cell = _grid.cells[cell.index.X, cell.index.Y + 1];
                    }
                    break;

                case 2: //East
                    if (cell.index.X != _grid.GetWidth() - 1)
                    {
                        next_cell = _grid.cells[cell.index.X + 1, cell.index.Y];

                        if (!next_cell.IsVisited())
                        {
                            CarvePathManual(ref _grid, cell, Direction.east);
                            visited_count += 1;
                        }

                        cell = _grid.cells[cell.index.X + 1, cell.index.Y];
                    }
                    break;

                case 3: //West
                    if (cell.index.X != 0)
                    {
                        next_cell = _grid.cells[cell.index.X - 1, cell.index.Y];

                        if (!next_cell.IsVisited())
                        {
                            CarvePathManual(ref _grid, cell, Direction.west);
                            visited_count += 1;
                        }

                        cell = _grid.cells[cell.index.X - 1, cell.index.Y];
                    }
                    break;
            }
        }

        return _grid;
    }


    //TODO: Start and End Cells can't be the same at the beginning stage
    //Pick a Random point and go in any random direction till all are visited reset when a loop occurs
    public static Grid WilsonsAlgorithm(ref Grid _grid)
    {
        GD.Randomize();

        int visited_count = 1;
        Cell cell = _grid.cells[(int)(GD.Randi() % _grid.GetWidth()), (int)(GD.Randi() % _grid.GetHeight())];

        List<Cell> l_cell_index = new List<Cell>(); //Loop List
        Vector2I v_end_cell = new Vector2I((int)(GD.Randi() % _grid.GetWidth()), (int)(GD.Randi() % _grid.GetHeight()));

        bool has_cells = false;

        //Already Visited cells will not be modified
        while (!(visited_count >= (_grid.GetWidth() * _grid.GetHeight())))
        {

            //Get Start cell
            if (!has_cells)
            {
                l_cell_index.Clear();

                //Make sure cell is unvisited
                while (!_grid.cells[cell.index.X, cell.index.Y].IsVisited())
                {
                    cell = _grid.cells[(int)(GD.Randi() % _grid.GetWidth()), (int)(GD.Randi() % _grid.GetHeight())];
                }

                has_cells = true;
            }

            //Starting Values: Get Direction
            uint num = GD.Randi() % 4;

            //Checks For a Loop and Resets it
            CheckForLoop(ref l_cell_index, cell, ref _grid);

            l_cell_index.Append(cell);    //Add Cell Index

            Cell next_cell;

            //Next Cell Direction: Update the Next Cell and see if it's Visited if it is then Carve a path
            switch (num)
            {
                case 0: //North
                    if (cell.index.Y != 0)
                    {
                        next_cell = _grid.cells[cell.index.X, cell.index.Y - 1];

                        if (!next_cell.IsVisited())
                        {
                            cell = _grid.cells[cell.index.X, cell.index.Y - 1];
                        }
                        else
                        {
                            CarvePathLoop(ref _grid, ref l_cell_index);
                            visited_count += l_cell_index.Count;
                            has_cells = false;
                        }
                    }
                    break;

                case 1: //South
                    if (cell.index.Y != _grid.GetHeight() - 1)
                    {
                        next_cell = _grid.cells[cell.index.X, cell.index.Y + 1];

                        if (!next_cell.IsVisited())
                        {
                            cell = _grid.cells[cell.index.X, cell.index.Y + 1];
                        }
                        else
                        {
                            CarvePathLoop(ref _grid, ref l_cell_index);
                            visited_count += l_cell_index.Count;
                            has_cells = false;
                        }
                    }
                    break;

                case 2: //East
                    if (cell.index.X != _grid.GetWidth() - 1)
                    {
                        next_cell = _grid.cells[cell.index.X + 1, cell.index.Y];

                        if (!next_cell.IsVisited())
                        {
                            cell = _grid.cells[cell.index.X + 1, cell.index.Y];
                        }
                        else
                        {
                            CarvePathLoop(ref _grid, ref l_cell_index);
                            visited_count += l_cell_index.Count;
                            has_cells = false;
                        }
                    }
                    break;

                case 3: //West
                    if (cell.index.X != 0)
                    {
                        next_cell = _grid.cells[cell.index.X - 1, cell.index.Y];

                        if (!next_cell.IsVisited())
                        {
                            cell = _grid.cells[cell.index.X - 1, cell.index.Y];
                        }
                        else
                        {
                            CarvePathLoop(ref _grid, ref l_cell_index);
                            visited_count += l_cell_index.Count;
                            has_cells = false;
                        }
                    }
                    break;
            }
        }

        return _grid;
    }

    //Used for the Wilson's Algorithm: Checks For a Loop 
    private static void CheckForLoop(ref List<Cell> cells, Cell current_cell, ref Grid _grid)
    {
        bool is_loop = false;
        int index = 0;

        for (int i = 0; i < cells.Count; i++)
        {

            if (is_loop) { index = i; }

            //Identify Loop
            if (cells[i].index.X == current_cell.index.X && cells[i].index.Y == current_cell.index.Y)
            {
                is_loop = true;
            }
        }
    }

    private static void CarvePathManual(ref Grid _grid, Cell cell, Direction direction)
    {
        switch (direction)
        {
            case Direction.north:
                _grid.cells[cell.index.X, cell.index.Y].north = true;
                _grid.cells[cell.index.X, cell.index.Y - 1].south = true;
                break;

            case Direction.south:
                _grid.cells[cell.index.X, cell.index.Y].south = true;
                _grid.cells[cell.index.X, cell.index.Y + 1].north = true;
                break;

            case Direction.east:
                _grid.cells[cell.index.X, cell.index.Y].east = true;
                _grid.cells[cell.index.X + 1, cell.index.Y].west = true;
                break;

            case Direction.west:
                _grid.cells[cell.index.X, cell.index.Y].west = true;
                _grid.cells[cell.index.X - 1, cell.index.Y].east = true;
                break;
        }
    }


    private static void CarveFullPath(ref Grid _grid)
    {
        //Carve Path
        for (int x = 0; x < _grid.GetWidth(); x++)
        {
            for (int y = 0; y < _grid.GetHeight(); y++)
            {
                Cell cell = _grid.cells[x, y];

                if (cell.IsVisited())
                {
                    if (cell.north && cell.south && cell.east && cell.west)
                    {
                        GD.Print("Cell is completely open");
                    }

                    if (cell.north && y > 0)
                    { //North
                        _grid.cells[x, y - 1].south = true;

                    }

                    if (cell.east && x < _grid.GetWidth() - 1)
                    {  //East
                        _grid.cells[x + 1, y].west = true;

                    }

                    if (cell.south && y < _grid.GetHeight() - 1)
                    { //South
                        _grid.cells[x, y + 1].north = true;

                    }

                    if (cell.west && x > 0)
                    {    //West
                        _grid.cells[x - 1, y].east = true;

                    }

                }
                else
                {
                    Debug.Print("Cell Not Visited! %s : %s", x, y);
                }
            }
        }
    }

    //TODO: Finish
    private static Grid CarvePathLoop(ref Grid _grid, ref List<Cell> cells)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            _grid.cells[cells[i].index.X, cells[i].index.Y] = cells[i];
        }

        return _grid;
    }
}



/*


*/