using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
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
