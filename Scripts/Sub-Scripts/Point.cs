using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MazeGeneratorGlobal;
using System.Globalization;

public class Points
{
    public Cell start;
    public Cell end;

    private ERectangleDirections start_direction;
    private ERectangleDirections end_direction;

    public Points(ref Cell _start, ref Cell _end, List<ERectangleDirections> start_dirs, List<ERectangleDirections> end_dirs)
    {
        start = _start;
        end = _end;

        //Choose Randomly
        if (start_dirs.Count > 0) {
            start_direction = start_dirs[(int)(GD.Randi() % start_dirs.Count)];
        }

        if (end_dirs.Count > 0) {
            end_direction = end_dirs[(int)(GD.Randi() % end_dirs.Count)];
        }

        //Remove Cell Wall in that direction
        Vector2I s_index = start.index;
        Vector2I e_index = end.index;

        switch (start_direction)
        {
            case ERectangleDirections.North:
                start.north = true;
                break;

            case ERectangleDirections.South:
                start.south = true;
                break;

            case ERectangleDirections.East:
                start.east = true;
                break;

            case ERectangleDirections.West:
                start.west = true;
                break;
        }

        switch (end_direction)
        {
            case ERectangleDirections.North:
                end.north = true;
                break;

            case ERectangleDirections.South:
                end.south = true;
                break;

            case ERectangleDirections.East:
                end.east = true;
                break;

            case ERectangleDirections.West:
                end.west = true;
                break;
        }
    }
}
