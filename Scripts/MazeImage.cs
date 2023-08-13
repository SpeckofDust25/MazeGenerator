using Godot;
using System;
using System.Runtime.InteropServices;

public static class MazeImage
{
    public static Image image;

    //Shapes-------------------------------------
    public static void DrawRectangle(ref Grid grid)
    {
        SetupImage(grid.GetTotalWidthPx(), grid.GetTotalHeightPx());

        //Draw Maze
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                SetMazeWalls(ref grid, ref image, Colors.Black, x, y);
            }
        }
    }
    //-------------------------------------------

    //Common Methods-----------------------------
    private static void SetupImage(int total_width, int total_height)
    {
        image = Image.Create(total_width, total_height, false, Image.Format.Rgb8);
        image.Fill(Colors.White);   //Fill Image
    }

    private static void SetMazeWalls(ref Grid grid, ref Image image, Color wall_color, int x, int y)
    {
        bool is_dead_cell = grid.cells[x, y].dead_cell;
        bool is_visited = grid.cells[x, y].IsVisited();
        
        //Create Walls For Cell
        if (!is_dead_cell || is_visited) {
            int exterior_offset = grid.GetExteriorSize();
            Cell cell = grid.cells[x, y];

            //Wall Drawing
            if (!cell.north)
            { // North
                Rect2I north_wall = grid.GetVerticalWall(x, y, false);
                north_wall.Position += new Vector2I(exterior_offset, exterior_offset);

                image.FillRect(north_wall, wall_color);
            }

            if (!cell.south)
            {  //South
                Rect2I south_wall = grid.GetVerticalWall(x, y, true);
                south_wall.Position += new Vector2I(exterior_offset, exterior_offset);

                image.FillRect(south_wall, wall_color);
            }

            if (!cell.east)
            {   //East
                Rect2I east_wall = grid.GetHorizontalWall(x, y, true);
                east_wall.Position += new Vector2I(exterior_offset, exterior_offset);

                image.FillRect(east_wall, wall_color);
            }

            if (!cell.west)
            { //West
                Rect2I west_wall = grid.GetHorizontalWall(x, y, false);
                west_wall.Position += new Vector2I(exterior_offset, exterior_offset);

                image.FillRect(west_wall, wall_color);
            }
        } else {    //Cover Cell
            Rect2I whole_cell = grid.GetCellSizePx(x, y);
            whole_cell.Position += new Vector2I(grid.GetExteriorSize(), grid.GetExteriorSize());
            image.FillRect(whole_cell, wall_color);
        }
    }
    //-------------------------------------------
}
