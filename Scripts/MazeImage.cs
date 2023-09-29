using Godot;
using MazeGeneratorGlobal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class MazeImage
{
    public static Image image;

    //Main Shape Methods-------------------------
    public static void DrawRectangle(ref Maze maze, Color dead_cell_color, bool draw_dead_cells, List<Vector2I> path)
    {
        SetupImage(maze.GetImageWidth(), maze.GetImageHeight());

        //Draw Dead Cells and Solid Cells
        if (!draw_dead_cells) {
            DrawDeadCells(ref maze, ref image, dead_cell_color);
            DrawSolidCells(ref maze, ref image, Colors.Black, false);
            DrawPath(ref maze, ref image, path, Colors.Red);
        } else {
            DrawSolidCells(ref maze, ref image, Colors.Black, true);
            DrawPath(ref maze, ref image, path, Colors.Red);
        }

        if (path == null && maze.GetStartCell() != null && maze.GetEndCell() != null)
        {
            //Fill in Start and End Points
            FillInCellInDirection(ref maze, maze.GetStartCell(), Colors.White, maze.GetStartDirection());
            FillInCellInDirection(ref maze, maze.GetEndCell(), Colors.White, maze.GetEndDirection());
        }
    }
    //-------------------------------------------

    //Common Methods-----------------------------
    private static void SetupImage(int total_width, int total_height)
    {
        image = Image.Create(total_width, total_height, false, Image.Format.Rgba8);
        image.Fill(Colors.White);   //Fill Image
    }

    private static void DrawSolidCells(ref Maze maze, ref Image image, Color wall_color, bool draw_dead_cell)
    {

        for (int x = 0; x < maze.GetWidth(); x++)
        {
            for (int y = 0; y < maze.GetHeight(); y++)
            {
                bool is_dead_cell = maze.cells[x, y].dead_cell;
                bool is_visited = maze.cells[x, y].IsVisited();

                //Always Draw Dead Cells
                if (draw_dead_cell)
                {
                    is_dead_cell = false;
                }
        
                //Create Walls For Cell
                if (!is_dead_cell) {
                    Cell cell = maze.cells[x, y];

                    //Wall Drawing
                    if (!cell.north)
                    { // North
                        Rect2I north_wall = maze.GetVerticalWall(x, y, false);

                        image.FillRect(north_wall, wall_color);
                    }

                    if (!cell.south)
                    {  //South
                        Rect2I south_wall = maze.GetVerticalWall(x, y, true);

                        image.FillRect(south_wall, wall_color);
                    }

                    if (!cell.east)
                    {   //East
                        Rect2I east_wall = maze.GetHorizontalWall(x, y, true);

                        image.FillRect(east_wall, wall_color);
                    }

                    if (!cell.west)
                    { //West
                        Rect2I west_wall = maze.GetHorizontalWall(x, y, false);

                        image.FillRect(west_wall, wall_color);
                    }
                }
            }
        }
    }
    
    private static void DrawDeadCells(ref Maze maze, ref Image image, Color fill_color)
    {
        for (int x = 0; x < maze.GetWidth(); x++)
        {
            for (int y = 0; y < maze.GetHeight(); y++)
            {
                if (maze.cells[x, y].dead_cell)
                {
                    Rect2I inside_cell = maze.GetCellSizePx(x, y);

                    Cell cell = maze.cells[x, y];
                    image.FillRect(inside_cell, fill_color);
                }
            }
        }
    }
    
    private static void DrawPath(ref Maze maze, ref Image image, List<Vector2I> path, Color path_color) 
    {
        if (path == null) { return; }

        for (int i = 0; i < path.Count; i++)
        {
            Cell path_cell = maze.cells[path[i].X, path[i].Y];
            ERectangleDirections cell_direction = ERectangleDirections.None;
            Rect2I inside_cell = maze.GetInsideCellSizePx(path[i].X, path[i].Y);

            //Fill Center
            image.FillRect(inside_cell, path_color);

            //Fill In Between 
            if (i != path.Count - 1) {
                Cell next_cell = maze.cells[path[i + 1].X, path[i + 1].Y];
                Vector2I difference = next_cell.index - path_cell.index;
                
                switch(difference.X)
                {
                    case -1:
                        cell_direction = ERectangleDirections.West;
                        break;

                    case 1:
                        cell_direction = ERectangleDirections.East;
                        break;
                }

                switch(difference.Y)
                {
                    case 1:
                        cell_direction = ERectangleDirections.South;
                        break;

                    case -1:
                        cell_direction = ERectangleDirections.North;
                        break;
                }
            }

            //Get Direction of Next Cell
            FillInCellInDirection(ref maze, path_cell, path_color, cell_direction);
        }

        //Fill in Start and End Points
        FillInCellInDirection(ref maze, maze.GetStartCell(), path_color, maze.GetStartDirection());
        FillInCellInDirection(ref maze, maze.GetEndCell(), path_color, maze.GetEndDirection());
    }

    //Used to Fill in Areas left Open where there is no wall
    private static void FillInCellInDirection(ref Maze maze, Cell cell, Color path_color, ERectangleDirections direction)
    {
        int wall_size = maze.GetWallSize();
        Rect2I inside_cell = maze.GetInsideCellSizePx(cell.index.X, cell.index.Y);

        switch (direction)
        {
            case ERectangleDirections.North:
                Vector2I north_position = new Vector2I(inside_cell.Position.X, inside_cell.Position.Y - wall_size);
                Rect2I north_rect = new Rect2I(north_position, inside_cell.Size);
                image.FillRect(north_rect, path_color);
                break;

            case ERectangleDirections.South:
                Vector2I south_position = new Vector2I(inside_cell.Position.X, inside_cell.Position.Y + wall_size);
                Rect2I south_rect = new Rect2I(south_position, inside_cell.Size);
                image.FillRect(south_rect, path_color);
                break;

            case ERectangleDirections.East:
                Vector2I east_position = new Vector2I(inside_cell.Position.X + wall_size, inside_cell.Position.Y);
                Rect2I east_rect = new Rect2I(east_position, inside_cell.Size);
                image.FillRect(east_rect, path_color);
                break;


            case ERectangleDirections.West:
                Vector2I west_position = new Vector2I(inside_cell.Position.X - wall_size, inside_cell.Position.Y);
                Rect2I west_rect = new Rect2I(west_position, inside_cell.Size);
                image.FillRect(west_rect, path_color);
                break;
        }
    }

    //-------------------------------------------
}
