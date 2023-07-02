using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;

public static class MazeGenerator {

    enum Direction { none, north, south, east, west }

    public static Grid BinaryTreeAlgorithm(ref Grid _grid)
    {
        GD.Randomize();

        //Create Maze 
        for (int x = 0; x < _grid.GetWidth(); ++x)
        {
            for (int y = 0; y < _grid.GetHeight(); ++y)
            {
                uint num = GD.Randi() % 2;
                Direction dir = Direction.north;

                if (x == _grid.GetWidth() - 1 && y == 0) { //At Top Right: Skip
                    dir = Direction.none;

                } else if (y == 0) { //At North Wall
                    dir = Direction.east;

                } else if (x == _grid.GetWidth() - 1) { //At East Wall
                    dir = Direction.north;

                } else {    //Random Wall
                    if (num == 1) {
                        dir = Direction.east;
                    }
                }

                CarvePath(ref _grid, x, y, dir);
            }
        }

        return _grid;
    }



    //Step 1: Top Bias
    //Step 2: Top Right Corner
    //Step 3: Right Side Only North depending on Count
    //Step 4: Top Side Only goes East
    public static Grid SidewinderAlgorithm(ref Grid _grid) {
        GD.Randomize();

        //Create Maze
        for (int y = 0; y < _grid.GetHeight(); ++y) {
            int east_count = 0;

            for (int x = 0; x < _grid.GetWidth(); ++x) {
            
                uint num = GD.Randi() % 2;
                Direction dir = Direction.east;

                if (y == 0) {   //Top Bias
                    if (x == _grid.GetWidth() - 1) {
                        dir = Direction.none;
                    } else {
                        dir = Direction.east;
                    }

                } else if (x == _grid.GetWidth() - 1) { //Right Side
                    dir = Direction.north;

                } else {  //Random Choice

                    if (num == 1) { //East
                        east_count += 1;

                    } else {    //North
                        dir = Direction.north;
                    }
                }

                //Carve Path
                if (dir == Direction.north) {
                    uint temp_num = 0;
                    if (east_count != 0) { 
                        temp_num = (uint)(GD.Randi() % east_count);
                        east_count = 0;
                    }
                    CarvePath(ref _grid, x - (int)temp_num, y, Direction.north);

                } else {
                    CarvePath(ref _grid, x, y, dir);
                }
            }
        }

        return _grid;
    }

    public static Grid AldousBroderAlgorithm(ref Grid _grid)
    {
        GD.Randomize();
        bool all_visited = false;
        int visited_count = 1; 
        Vector2I cell_index = new Vector2I((int)(GD.Randi() % (uint)(_grid.GetWidth())), (int)(GD.Randi() % (uint)(_grid.GetHeight())));

        //Already Visited cells will not be modified
        while (!all_visited) {
            uint num = GD.Randi() % 4;
            Cell cell = _grid.cells[cell_index.X, cell_index.Y];
            Cell next_cell;

            // All Visited
            if (visited_count >= (_grid.GetWidth() * _grid.GetHeight())) { 
                all_visited = true;
                break;
            }

            //Carve a Path, Update cell, Update Visited Count
            switch(num) {
                case 0: //North
                    if (cell_index.Y != 0)
                    {
                        next_cell = _grid.cells[cell_index.X, cell_index.Y - 1];

                        if (!next_cell.north && !next_cell.south && !next_cell.east && !next_cell.west)
                        {
                            CarvePath(ref _grid, cell_index.X, cell_index.Y, Direction.north);
                            visited_count += 1;
                        }

                        cell_index = new Vector2I(cell_index.X, cell_index.Y - 1);
                    }
                    break;

                case 1: //South
                    if (cell_index.Y != _grid.GetHeight() - 1)
                    {
                        next_cell = _grid.cells[cell_index.X, cell_index.Y + 1];

                        if (!next_cell.north && !next_cell.south && !next_cell.east && !next_cell.west)
                        {
                            CarvePath(ref _grid, cell_index.X, cell_index.Y, Direction.south);
                            visited_count += 1;
                        }

                        cell_index = new Vector2I(cell_index.X, cell_index.Y + 1);
                    }
                    break;

                case 2: //East
                    if (cell_index.X != _grid.GetWidth() - 1)
                    {
                        next_cell = _grid.cells[cell_index.X + 1, cell_index.Y];

                        if (!next_cell.north && !next_cell.south && !next_cell.east && !next_cell.west)
                        {
                            CarvePath(ref _grid, cell_index.X, cell_index.Y, Direction.east);
                            visited_count += 1;
                        }

                        cell_index = new Vector2I(cell_index.X + 1, cell_index.Y);
                    }
                    break;

                case 3: //West
                    if (cell_index.X != 0)
                    {
                        next_cell = _grid.cells[cell_index.X - 1, cell_index.Y];

                        if (!next_cell.north && !next_cell.south && !next_cell.east && !next_cell.west)
                        {
                            CarvePath(ref _grid, cell_index.X, cell_index.Y, Direction.west);
                            visited_count += 1;
                        }

                        cell_index = new Vector2I(cell_index.X - 1, cell_index.Y);
                    }
                    break;
            }
        }

        return _grid;
    }

    public static Grid WilsonsAlgorithm(ref Grid _grid)
    {
        return _grid;
    }

    private static Grid CarvePath(ref Grid _grid, int _x, int _y, Direction _dir)
    {
        switch(_dir)
        {
            case Direction.none:
                break;

            case Direction.north:
                _grid.cells[_x, _y].north = true;

                if (_y > 0) {
                    _grid.cells[_x, _y - 1].south = true;
                }
                break;

            case Direction.south:
                _grid.cells[_x, _y].south = true;
                
                if (_y < _grid.GetHeight() - 1) {
                    _grid.cells[_x, _y + 1].north = true;
                }
                break;

            case Direction.east:
                _grid.cells[_x, _y].east = true;
               
                if (_x < _grid.GetWidth() - 1) {
                    _grid.cells[_x + 1, _y].west = true;
                }
                break;

            case Direction.west:
                _grid.cells[_x, _y].west = true;

                if (_x > 0)
                {
                    _grid.cells[_x - 1, _y].east = true;
                }
                break;
        }

        return _grid;
    }
}
