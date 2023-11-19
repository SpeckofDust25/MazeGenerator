using Godot;
using MazeGeneratorGlobal;
using System;
using System.Collections.Generic;

public class MazeJson
{
    int json_width;
    int json_height;
    int maze_width;
    int maze_height;

    int[,] maze;
    int[,] solution;

    public MazeJson(Maze _maze, List<Vector2I> path)
    {
        maze_width = _maze.GetWidth();
        maze_height = _maze.GetHeight();
        json_width = (_maze.GetWidth() * 2) + 1;
        json_height = (_maze.GetHeight() * 2) + 1;

        maze = new int[json_width, json_height];
        solution = new int[json_width, json_height];

        //Whole Maze
        for (int y = 0; y < maze_height; y++)
        {
            for (int x = 0; x < maze_width; x++)
            {
                Cell cell = _maze.cells[x, y];
                Cell north_cell = null;
                Cell west_cell = null;

                if (cell.index.X != 0)
                {
                    west_cell = _maze.cells[x - 1, y];
                }

                if (cell.index.Y != 0)
                {
                    north_cell = _maze.cells[x, y - 1];
                }

                AddDataToMaze(cell, north_cell, west_cell, 1);
            }
        }

        //Maze Edges
        for (int x = 0; x < maze_width; x++)
        {
            Cell cell = _maze.cells[x, maze_height - 1];
            AddDataToMazeEdge(cell, 0);
        }

        for (int y = 0; y < maze_height; y++)
        {
            Cell cell = _maze.cells[maze_width - 1, y];
            AddDataToMazeEdge(cell, 0);
        }

        //Solution
        for (int i = 0; i < path.Count; i++) {
            AddDataToSolution(_maze, path[i], 1);
        }

        //Start and Finish
        if (_maze.start_end_points != null)
        {
            Cell start_cell = _maze.start_end_points.start;
            Cell end_cell = _maze.start_end_points.end;

            ERectangleDirections start_direction = _maze.start_end_points.start_direction;
            ERectangleDirections end_direction = _maze.start_end_points.end_direction;

            if (start_cell != null && end_cell != null)
            {
                AddStartAndEndMaze(start_cell, start_direction, 1);
                AddStartAndEndMaze(end_cell, end_direction, 1);
            }
        }

        PrintMaze();
        PrintSolution();
    }

    public void AddDataToMaze(Cell cell, Cell north_cell, Cell west_cell, int number)
    {
        int h = (int)Math.Floor(cell.index.X * 2f);
        int v = (int)Math.Floor(cell.index.Y * 2f);

        //Add Dead Cell
        if (cell.dead_cell)
        {
            maze[h + 1, v + 1] = 3;    //Middle

            if (west_cell.dead_cell) {
                maze[h, v + 1] = 3; //West
            }

            if (north_cell.dead_cell) {
                maze[h + 1, v] = 3;    //North
            }

            if (north_cell.dead_cell && west_cell.dead_cell) {
                maze[h, v] = 3;    //North West
            }
        }
        else
        {
            //Add Open and Walls
            maze[h + 1, v + 1] = number; //Middle
            if (cell.north) { maze[h + 1, v] = number; } //North
            if (cell.west) { maze[h, v + 1] = number; }  //West
        }
    }

    public void AddDataToMazeEdge(Cell cell, int number)
    {
        int h = (int)(cell.index.X * 2f) + 1;
        int v = (int)(cell.index.Y * 2f) + 1;

        if (cell.dead_cell)
        {
            //Bottom Right Corner
            if (cell.index.X == maze_width - 1 && cell.index.Y == maze_height - 1)
            {
                maze[h + 1, v] = 3;
                maze[h, v + 1] = 3;
                maze[h + 1, v + 1] = 3;
            }
            else if (cell.index.X == maze_width - 1)
            {    //Right Wall
                maze[h + 1, v - 1] = 3;
                maze[h + 1, v + 1] = 3;
                maze[h + 1, v] = 3;
            }
            else if (cell.index.Y == maze_height - 1)
            {   //Bottom Wall
                maze[h, v + 1] = 3;
                maze[h - 1, v + 1] = 3;
                maze[h + 1, v + 1] = 3;
            }
        }
        else
        {

            //Bottom Right Corner
            if (cell.index.X == maze_width - 1 && cell.index.Y == maze_height - 1)
            {
                maze[h + 1, v] = number;
                maze[h, v + 1] = number;
                maze[h + 1, v + 1] = number;
            }
            else if (cell.index.X == maze_width - 1)
            {    //Right Wall
                maze[h + 1, v - 1] = number;
                maze[h + 1, v] = number;
                maze[h + 1, v + 1] = number;
            }
            else if (cell.index.Y == maze_height - 1)
            {   //Bottom Wall
                maze[h, v + 1] = number;
                maze[h - 1, v + 1] = number;
                maze[h + 1, v + 1] = number;
            }
        }
    }

    public void AddStartAndEndMaze(Cell cell, ERectangleDirections direction, int number)
    {
        int h = (int)(cell.index.X * 2f);
        int v = (int)(cell.index.Y * 2f);

        //Check Edges
        if (cell.index.X == maze_width && cell.index.Y == maze_height)
        { //Bottom Right
            h += 1;
            v += 1;

            if (cell.east)
            {
                maze[h + 1, v] = number;
            }
            else
            {
                maze[h, v + 1] = number;
            }

        }
        else if (cell.index.X == maze_width)
        {    //Right
            h += 1;
            v += 1;

            maze[h + 1, v] = number;

        }
        else if (cell.index.Y == maze_height)
        {   //Bottom
            h += 1;
            v += 1;

            maze[h, v + 1] = number;

        }
        else
        {

            switch (direction)
            {
                case ERectangleDirections.North:
                    maze[h + 1, v] = number;
                    break;

                case ERectangleDirections.South:
                    maze[h + 1, v + 2] = number;
                    break;

                case ERectangleDirections.East:
                    maze[h + 2, v + 1] = number;
                    break;

                case ERectangleDirections.West:
                    maze[h, v + 1] = number;
                    break;
            }
        }
    }

    public void AddDataToSolution(Maze _maze, Vector2I index, int symbol)
    {
        Cell start_cell;
        Cell end_cell;

        ERectangleDirections start_direction = ERectangleDirections.None;
        ERectangleDirections end_direction = ERectangleDirections.None;

        int h = index.X * 2;
        int v = index.Y * 2;

        bool is_start = false;
        bool is_end = false;

        solution[h + 1, v + 1] = symbol;

        if (_maze.start_end_points != null)
        {
            start_cell = _maze.start_end_points.start;
            end_cell = _maze.start_end_points.end;

            start_direction = _maze.start_end_points.start_direction;
            end_direction = _maze.start_end_points.end_direction;

            is_start = (index == start_cell.index);
            is_end = (index == end_cell.index);
        }

        //Regular Cell
        if (!is_start && !is_end) {
            if (_maze.cells[index.X, index.Y].north)
            {
                solution[h + 1, v] = symbol;
            }

            if (_maze.cells[index.X, index.Y].west)
            {
                solution[h, v + 1] = symbol;
            }

            return;
        }

        //Start Or End Cell
        if (is_start)
        {
            switch(start_direction)
            {
                case ERectangleDirections.North:
                    solution[h + 1, v] = symbol;
                    break;

                case ERectangleDirections.South:
                    solution[h + 1, v + 2] = symbol;
                    break;

                case ERectangleDirections.East:
                    solution[h + 2, v + 1] = symbol;
                    break;

                case ERectangleDirections.West:
                    solution[h, v + 1] = symbol;
                    break;
            }
        }

        if (is_end)
        {
            switch (end_direction)
            {
                case ERectangleDirections.North:
                    solution[h + 1, v] = symbol;
                    break;

                case ERectangleDirections.South:
                    solution[h + 1, v + 2] = symbol;
                    break;

                case ERectangleDirections.East:
                    solution[h + 2, v + 1] = symbol;
                    break;

                case ERectangleDirections.West:
                    solution[h, v + 1] = symbol;
                    break;
            }
        }
    }

    //Printing-----------------------------------
    public void PrintMaze()
    {
        GD.Print("Maze: ");
        for (int y = 0; y < maze.GetLength(1); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                GD.PrintRaw(maze[x, y] + ", ");
            }

            GD.PrintRaw('\n');
        }
    }

    public void PrintSolution()
    {
        GD.Print("Solution: ");

        for (int y = 0; y < maze.GetLength(1); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                GD.PrintRaw(solution[x, y] + ", ");
            }

            GD.PrintRaw('\n');
        }
    }
}