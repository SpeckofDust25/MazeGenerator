using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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
            if (dir != Direction.none) {
                if (!next_cell.IsVisited()) {
                    CarvePathManual(ref _grid, cell, dir);
                    visited_count += 1;
                }

                cell = next_cell;
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

        bool has_new_path = false;

        //Already Visited cells will not be modified
        while (!(visited_count >= (_grid.GetWidth() * _grid.GetHeight())))
        {
            //Get Random Unvisited cell
            if (!has_new_path) {

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
            switch (num) {
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

                } else {    //Check For Loop
                    CheckForLoop(ref l_cell_index, next_cell);
                    l_cell_index.Add(next_cell);
                }

                cell = next_cell;
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
        for (int i = 0; i < cells.Count - 1; i++) {
            Vector2I direction = new Vector2I(cells[i + 1].index.X - cells[i].index.X, cells[i + 1].index.Y - cells[i].index.Y);

            if (direction.X != 0) {
                if (direction.X > 0) {  //East
                    _grid.cells[cells[i].index.X, cells[i].index.Y].east = true;
                    _grid.cells[cells[i].index.X + 1, cells[i].index.Y].west = true;
                } else {    //West
                    _grid.cells[cells[i].index.X, cells[i].index.Y].west = true;
                    _grid.cells[cells[i].index.X - 1, cells[i].index.Y].east = true;
                }
            } 

            if (direction.Y != 0) {
                if (direction.Y > 0) {  //South
                    _grid.cells[cells[i].index.X, cells[i].index.Y].south = true;
                    _grid.cells[cells[i].index.X, cells[i].index.Y + 1].north = true;
                } else {    //North
                    _grid.cells[cells[i].index.X, cells[i].index.Y].north = true;
                    _grid.cells[cells[i].index.X, cells[i].index.Y - 1].south = true;
                }
            }
        }
    }


    //Helper Methods

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
}