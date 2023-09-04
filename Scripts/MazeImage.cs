using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class MazeImage
{
    public static Image image;

    //Main Shape Methods-------------------------
    public static void DrawRectangle(ref Grid grid, Color dead_cell_color, bool draw_dead_cells, List<Vector2I> path)
    {
        SetupImage(grid.GetImageWidth(), grid.GetImageHeight());

        //Draw Dead Cells and Solid Cells
        if (!draw_dead_cells) {
            DrawDeadCells(ref grid, ref image, dead_cell_color);
            DrawSolidCells(ref grid, ref image, Colors.Black, false);
            DrawPath(ref grid, ref image, path, Colors.Red);
        } else {
            DrawSolidCells(ref grid, ref image, Colors.Black, true);
            DrawPath(ref grid, ref image, path, Colors.Red);
        }
    }
    //-------------------------------------------

    //Common Methods-----------------------------
    private static void SetupImage(int total_width, int total_height)
    {
        image = Image.Create(total_width, total_height, false, Image.Format.Rgba8);
        image.Fill(Colors.White);   //Fill Image
    }

    private static void DrawSolidCells(ref Grid grid, ref Image image, Color wall_color, bool draw_dead_cell)
    {

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                bool is_dead_cell = grid.cells[x, y].dead_cell;
                bool is_visited = grid.cells[x, y].IsVisited();

                //Always Draw Dead Cells
                if (draw_dead_cell)
                {
                    is_dead_cell = false;
                }
        
                //Create Walls For Cell
                if (!is_dead_cell) {
                    Cell cell = grid.cells[x, y];

                    //Wall Drawing
                    if (!cell.north)
                    { // North
                        Rect2I north_wall = grid.GetVerticalWall(x, y, false);

                        image.FillRect(north_wall, wall_color);
                    }

                    if (!cell.south)
                    {  //South
                        Rect2I south_wall = grid.GetVerticalWall(x, y, true);

                        image.FillRect(south_wall, wall_color);
                    }

                    if (!cell.east)
                    {   //East
                        Rect2I east_wall = grid.GetHorizontalWall(x, y, true);

                        image.FillRect(east_wall, wall_color);
                    }

                    if (!cell.west)
                    { //West
                        Rect2I west_wall = grid.GetHorizontalWall(x, y, false);

                        image.FillRect(west_wall, wall_color);
                    }
                }
            }
        }
    }
    
    private static void DrawDeadCells(ref Grid grid, ref Image image, Color fill_color)
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                if (grid.cells[x, y].dead_cell)
                {
                    Rect2I inside_cell = grid.GetCellSizePx(x, y);

                    Cell cell = grid.cells[x, y];
                    image.FillRect(inside_cell, fill_color);
                }
            }
        }
    }
    
    private static void DrawPath(ref Grid grid, ref Image image, List<Vector2I> path, Color path_color) 
    {
        if (path == null) { return; }

        for (int i = 0; i < path.Count; i++)
        {
            Rect2I inside_cell = grid.GetInsideCellSizePx(path[i].X, path[i].Y);

            image.FillRect(inside_cell, path_color);
        }
    }

    //-------------------------------------------
}
