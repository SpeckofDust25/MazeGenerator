using Godot;
using System;
using System.Runtime.InteropServices;

public class MazeJson
{
    int json_width;
    int json_height;
    int maze_width;
    int maze_height;
    int[,] maze;

    public MazeJson(Maze _maze)
    {
        maze_width = _maze.GetWidth();
        maze_height = _maze.GetHeight();
        json_width = (_maze.GetWidth() * 2) + 1;
        json_height = (_maze.GetHeight() * 2) + 1;

        maze = new int[json_width, json_height];

        //Whole Maze
        for (int y = 0; y < maze_height; y++)
        {
            for (int x = 0; x < maze_width; x++)
            {
                Cell cell = _maze.cells[x, y];
                AddDataToCell(cell);
            }
        }

        //Maze Edges
        for (int x = 0; x < maze_width; x++)
        {
            Cell cell = _maze.cells[x, maze_height - 1];
            AddDataToCellEdge(cell);
        }

        for (int y = 0; y < maze_height; y++)
        {
            Cell cell = _maze.cells[maze_width - 1, y];
            AddDataToCellEdge(cell);
        }

        GD.Print(_maze.cells[maze_width - 1, maze_height - 1].index);
        GD.Print(maze_width.ToString() + ", " + maze_height.ToString());

        PrintMaze();
    }

    public void AddDataToCell(Cell cell)
    {
        int h = (int)Math.Floor(cell.index.X * 2f);
        int v = (int)Math.Floor(cell.index.Y * 2f);

        //Add Dead Cell
        if (cell.dead_cell)
        {
            maze[h, v + 1] = -1; //West
            maze[h + 1, v] = -1;    //North
            maze[h + 1, v + 1] = -1;    //Middle
            maze[h, v] = -1;    //North West
        }
        else
        {
            //Add Open and Walls
            maze[h + 1, v + 1] = 1; //Middle
            if (cell.north) { maze[h + 1, v] = 1; } //North
            if (cell.west) { maze[h, v + 1] = 1; }  //West
        }
    }

    public void AddDataToCellEdge(Cell cell)
    {
        int h = (int)(cell.index.X * 2f) + 1;
        int v = (int)(cell.index.Y * 2f) + 1;

        if (cell.dead_cell)
        {
            //Bottom Right Corner
            if (cell.index.X == maze_width - 1 && cell.index.Y == maze_height - 1)
            {
                maze[h + 1, v] = -1;
                maze[h, v + 1] = -1;
                maze[h + 1, v + 1] = -1;
            }
            else if (cell.index.X == maze_width - 1)
            {    //Right Wall
                maze[h + 1, v - 1] = -1;
                maze[h + 1, v] = -1;
                maze[h + 1, v + 1] = -1;
            }
            else if (cell.index.Y == maze_height - 1)
            {   //Bottom Wall
                maze[h, v + 1] = -1;
                maze[h - 1, v + 1] = -1;
                maze[h + 1, v + 1] = -1;
            }
        } else {

            //Bottom Right Corner
            if (cell.index.X == maze_width - 1 && cell.index.Y == maze_height - 1)
            {
                maze[h + 1, v] = 5;
                maze[h, v + 1] = 5;
                maze[h + 1, v + 1] = 5;
            } else if (cell.index.X == maze_width - 1) {    //Right Wall
                maze[h + 1, v - 1] = 5;
                maze[h + 1, v] = 5;
                maze[h + 1, v + 1] = 5;
            } else if (cell.index.Y == maze_height - 1) {   //Bottom Wall
                maze[h, v + 1] = 5;
                maze[h - 1, v + 1] = 5;
                maze[h + 1, v + 1] = 5;
            }
        }
    }

    public void PrintMaze()
    {
        for (int y = 0; y < maze.GetLength(1); y++) {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                GD.PrintRaw(maze[x, y] + ", ");
            }

            GD.PrintRaw('\n');
        }
    }
}