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
            Cell next_cell = cell;
            Direction dir = Direction.none;

            //Get Direction and Next Cell
            switch (num)
            {
                case 0: //North
                    if (cell.index.Y != 0)
                    {
                        next_cell = _grid.cells[cell.index.X, cell.index.Y - 1];
                        dir = Direction.north;
                    }
                    break;

                case 1: //South
                    if (cell.index.Y != _grid.GetHeight() - 1)
                    {
                        next_cell = _grid.cells[cell.index.X, cell.index.Y + 1];
                        dir = Direction.south;
                    }
                    break;

                case 2: //East
                    if (cell.index.X != _grid.GetWidth() - 1)
                    {
                        next_cell = _grid.cells[cell.index.X + 1, cell.index.Y];
                        dir = Direction.east;
                    }
                    break;

                case 3: //West
                    if (cell.index.X != 0)
                    {
                        next_cell = _grid.cells[cell.index.X - 1, cell.index.Y];
                        dir = Direction.west;
                    }
                    break;
            }

            //Carve path
            if (dir != Direction.none)
            {
                if (!next_cell.IsVisited())
                {
                    CarvePathManual(ref _grid, cell, dir);
                    visited_count += 1;
                }

                cell = next_cell;
            }
        }

        return _grid;
    }

    //Pick a Random point and go in any random direction till all are visited reset when a loop occurs
    public static Grid WilsonsAlgorithm(ref Grid _grid)
    {
        GD.Randomize();

        int visited_count = 1;
        Cell cell = _grid.cells[(int)(GD.Randi() % _grid.GetWidth()), (int)(GD.Randi() % _grid.GetHeight())];

        List<Cell> l_cell_index = new List<Cell>(); //Loop List
        Vector2I v_end_cell = new Vector2I((int)(GD.Randi() % _grid.GetWidth()), (int)(GD.Randi() % _grid.GetHeight()));

        bool has_new_path = false;

        //Already Visited cells will not be modified
        while (!(visited_count >= (_grid.GetWidth() * _grid.GetHeight())))
        {
            //Get Random Unvisited cell
            if (!has_new_path)
            {

                while (cell.IsVisited() || (cell.index.X == v_end_cell.X && cell.index.Y == v_end_cell.Y))
                {
                    cell = _grid.cells[(int)(GD.Randi() % _grid.GetWidth()), (int)(GD.Randi() % _grid.GetHeight())];
                }

                l_cell_index.Clear();
                l_cell_index.Add(cell);
                has_new_path = true;
            }

            //Starting Values: Get Direction
            uint num = GD.Randi() % 4;
            Cell next_cell = cell;
            Direction dir = Direction.none;

            //Get Direction
            switch (num)
            {
                case 0: //North
                    if (cell.index.Y != 0)
                    {
                        next_cell = _grid.cells[cell.index.X, cell.index.Y - 1];
                        dir = Direction.north;
                    }
                    break;

                case 1: //South
                    if (cell.index.Y != _grid.GetHeight() - 1)
                    {
                        next_cell = _grid.cells[cell.index.X, cell.index.Y + 1];
                        dir = Direction.south;
                    }
                    break;

                case 2: //East
                    if (cell.index.X != _grid.GetWidth() - 1)
                    {
                        next_cell = _grid.cells[cell.index.X + 1, cell.index.Y];
                        dir = Direction.east;
                    }
                    break;

                case 3: //West
                    if (cell.index.X != 0)
                    {
                        next_cell = _grid.cells[cell.index.X - 1, cell.index.Y];
                        dir = Direction.west;
                    }
                    break;
            }

            //Carve Path
            if (dir != Direction.none)
            {
                if (next_cell.IsVisited() || (v_end_cell.X == next_cell.index.X && v_end_cell.Y == next_cell.index.Y))
                {

                    l_cell_index.Add(next_cell);
                    CarvePathLoop(ref _grid, ref l_cell_index);
                    visited_count += l_cell_index.Count() - 1;
                    has_new_path = false;

                }
                else
                {    //Check For Loop
                    CheckForLoop(ref l_cell_index, next_cell);
                    l_cell_index.Add(next_cell);
                }

                cell = next_cell;
            }
        }

        return _grid;
    }

    public static Grid HuntandKill(ref Grid _grid)
    {
        GD.Randomize();
        int visited_count = 1;
        Cell cell = _grid.cells[(int)(GD.Randi() % (uint)(_grid.GetWidth())), (int)(GD.Randi() % (uint)(_grid.GetHeight()))];

        while (!(visited_count >= (_grid.GetWidth() * _grid.GetHeight())))
        {
            uint num = GD.Randi() % 4;
            Cell next_cell = cell;
            Direction dir = Direction.none;

            //Boxed In: Check for Adjacent cells
            if (dir == Direction.none) { 
                dir = GetAdjacentUnvisited(ref _grid, cell);
                next_cell = GetCellInDirection(ref _grid, cell, dir);
            }

            //Carve path
            if (dir != Direction.none)
            {
                if (!next_cell.IsVisited())
                {   //Not Visited
                    CarvePathManual(ref _grid, cell, dir);
                    visited_count += 1;
                    cell = next_cell;
                }
                
            } else {
                cell = Hunt(ref _grid, cell);
                visited_count += 1;
            }
        }

        return _grid;
    }

    public static Grid RecursiveBacktracker(ref Grid _grid) {
        GD.Randomize();
        int visited_count = 1;
        Cell cell = _grid.cells[(int)(GD.Randi() % (uint)(_grid.GetWidth())), (int)(GD.Randi() % (uint)(_grid.GetHeight()))];
        Stack<Cell> s_cells = new Stack<Cell>();

        while (!(visited_count >= (_grid.GetWidth() * _grid.GetHeight())))
        {
            uint num = GD.Randi() % 4;
            Cell next_cell = cell;
            Direction dir = Direction.none;
            s_cells.Push(cell);

            //Boxed In: Check for Adjacent cells
            if (dir == Direction.none)
            {
                dir = GetAdjacentUnvisited(ref _grid, cell);
                next_cell = GetCellInDirection(ref _grid, cell, dir);
            }

            //Carve path
            if (dir != Direction.none)
            {
                if (!next_cell.IsVisited())
                {   //Not Visited
                    CarvePathManual(ref _grid, cell, dir);
                    visited_count += 1;
                    cell = next_cell;
                }
            }
            else
            {
                cell = Backtrack(ref _grid, s_cells);
                //visited_count += 1;
            }
        }

        return _grid;
    }

    //Carve Path Methods
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

    private static void CarvePathLoop(ref Grid _grid, ref List<Cell> cells)
    {
        for (int i = 0; i < cells.Count - 1; i++)
        {
            Vector2I direction = new Vector2I(cells[i + 1].index.X - cells[i].index.X, cells[i + 1].index.Y - cells[i].index.Y);

            if (direction.X != 0)
            {
                if (direction.X > 0)
                {  //East
                    _grid.cells[cells[i].index.X, cells[i].index.Y].east = true;
                    _grid.cells[cells[i].index.X + 1, cells[i].index.Y].west = true;
                }
                else
                {    //West
                    _grid.cells[cells[i].index.X, cells[i].index.Y].west = true;
                    _grid.cells[cells[i].index.X - 1, cells[i].index.Y].east = true;
                }
            }

            if (direction.Y != 0)
            {
                if (direction.Y > 0)
                {  //South
                    _grid.cells[cells[i].index.X, cells[i].index.Y].south = true;
                    _grid.cells[cells[i].index.X, cells[i].index.Y + 1].north = true;
                }
                else
                {    //North
                    _grid.cells[cells[i].index.X, cells[i].index.Y].north = true;
                    _grid.cells[cells[i].index.X, cells[i].index.Y - 1].south = true;
                }
            }
        }
    }


    //Helper Methods
    private static Direction GetAdjacentUnvisited(ref Grid _grid, Cell cell)
    {
        Direction dir = Direction.none;
        List<Direction> neighbors = new List<Direction>();

        if (cell.index.Y > 0)
        {
            if (!_grid.cells[cell.index.X, cell.index.Y - 1].IsVisited())
            {  //North
                neighbors.Add(Direction.north);
            }
        }

        if (cell.index.X > 0)
        {
            if (!_grid.cells[cell.index.X - 1, cell.index.Y].IsVisited())
            {  //West
                neighbors.Add(Direction.west);
            }
        }

        if (cell.index.Y < _grid.GetHeight() - 1)
        {
            if (!_grid.cells[cell.index.X, cell.index.Y + 1].IsVisited())
            {  //South
                neighbors.Add(Direction.south);
            }
        }

        if (cell.index.X < _grid.GetWidth() - 1)
        {
            if (!_grid.cells[cell.index.X + 1, cell.index.Y].IsVisited())
            {  //East
                neighbors.Add(Direction.east);
            }
        }

        //Get Random Direction
        if (neighbors.Count > 0)
        {
            int new_dir = (int)(GD.Randi() % (uint)neighbors.Count);
            dir = neighbors[new_dir];
        }

        return dir;
    }

    private static Cell GetCellInDirection(ref Grid _grid, Cell cell, Direction dir)
    {
        Cell next_cell = null;

        if (dir == Direction.north) { next_cell = _grid.cells[cell.index.X, cell.index.Y - 1]; }
        if (dir == Direction.south) { next_cell = _grid.cells[cell.index.X, cell.index.Y + 1]; }
        if (dir == Direction.east) { next_cell = _grid.cells[cell.index.X + 1, cell.index.Y]; }
        if (dir == Direction.west) { next_cell = _grid.cells[cell.index.X - 1, cell.index.Y]; }

        return next_cell;
    }

    private static Cell Hunt(ref Grid _grid, Cell new_cell)
    {
        List<Direction> neighbors = new List<Direction>();

        //Find a Cell with at least 1 adjacent visited cell
        for (int y = 0; y < _grid.GetHeight(); y++)
        {
            for (int x = 0; x < _grid.GetWidth(); x++)
            {
                new_cell = _grid.cells[x, y];

                if (!new_cell.IsVisited())
                {
                    if (new_cell.index.Y > 0)
                    {
                        if (_grid.cells[new_cell.index.X, new_cell.index.Y - 1].IsVisited())
                        {  //North
                            neighbors.Add(Direction.north);
                        }
                    }

                    if (new_cell.index.X > 0)
                    {
                        if (_grid.cells[new_cell.index.X - 1, new_cell.index.Y].IsVisited())
                        {  //West
                            neighbors.Add(Direction.west);
                        }
                    }

                    if (new_cell.index.Y < _grid.GetHeight() - 1)
                    {
                        if (_grid.cells[new_cell.index.X, new_cell.index.Y + 1].IsVisited())
                        {  //South
                            neighbors.Add(Direction.south);
                        }
                    }

                    if (new_cell.index.X < _grid.GetWidth() - 1)
                    {
                        if (_grid.cells[new_cell.index.X + 1, new_cell.index.Y].IsVisited())
                        {  //East
                            neighbors.Add(Direction.east);
                        }
                    }
                }

                if (neighbors.Count > 0) { break; }
            }

            if (neighbors.Count > 0) { break; }
        }

        //Pick Random Cell
        if (neighbors.Count > 0)
        {
            int new_dir = (int)(GD.Randi() % (uint)neighbors.Count);
            CarvePathManual(ref _grid, new_cell, neighbors[new_dir]);
        }

        return new_cell;
    }

    private static Cell Backtrack(ref Grid _grid, Stack<Cell> cells)
    {
        Cell target = null;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells.TryPop(out target)) {

                Direction dir = GetAdjacentUnvisited(ref _grid, target);

                if (dir != Direction.none)
                {
                    break;
                }

            } else {
                break;
            }
        }

        return target;
    }
    
    //Used for the Wilson's Algorithm: Checks For a Loop 
    private static void CheckForLoop(ref List<Cell> cells, Cell current_cell)
    {
        bool is_loop = false;
        int index = 0;

        for (int i = 0; i < cells.Count; i++)
        {
            //Identify Loop
            if (cells[i].index.X == current_cell.index.X && cells[i].index.Y == current_cell.index.Y)
            {
                is_loop = true;
                index = i;
                break;
            }
        }

        if (is_loop)
        {
            cells.RemoveRange(index, cells.Count - index); //Get rid of Loop
        }
    }

    private static uint GetDeadends(Grid _grid)
    {
        uint deadends = 0;
        for (int x = 0; x < _grid.GetWidth(); x++)
        {
            for (int y = 0; y < _grid.GetHeight(); y++)
            {
                uint count = 0;
                Cell cell = _grid.cells[x, y];
                if (cell.north) { count += 1; }
                if (cell.south) { count += 1; }
                if (cell.east) { count += 1; }
                if (cell.west) { count += 1; }


                if (count == 3)
                {
                    deadends += 1;
                }
            }
        }

        return deadends;
    }
}