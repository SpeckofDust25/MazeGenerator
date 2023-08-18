using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class Mask
{
    public bool[,] dead_cells;

    public Mask(int width, int height)
    {
        dead_cells = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                dead_cells[x, y] = false;
            }
        }
    }

    public Mask(Mask mask, int width, int height)
    {
        dead_cells = new bool[width, height];

        for (int x = 0; x < dead_cells.GetLength(0); x++)
        {
            for (int y = 0; y < dead_cells.GetLength(1); y++)
            {
                bool x_valid = mask.dead_cells.GetLength(0) - 1 >= x;
                bool y_valid = mask.dead_cells.GetLength(1) - 1 >= y;

                if (x_valid && y_valid)
                {
                    dead_cells[x, y] = mask.dead_cells[x, y];
                }
            }
        }
    }
}


public static class MazeMask
{
    public static Image image;
    public static Mask mask;
    public static Vector2I size;

    //Returns If Updated
    public static bool Update(ref Grid grid, Vector2I local_mouse_position)
    {
        bool did_update = false;
        bool pressed = Input.IsActionJustPressed("right_click");

        NewMask(grid.GetWidth(), grid.GetHeight());
        did_update = NewImage(ref grid);

        //Update Cell Center
        if (pressed)
        {
            //Get Cell Index
            Vector2I index = grid.GetCellIndexAtImagePosition(local_mouse_position);
            Rect2I rect = grid.GetInsideCellSizePx(index.X, index.Y);
            bool is_dead_cell = mask.dead_cells[index.X, index.Y];

            if (!is_dead_cell)
            {
                mask.dead_cells[index.X, index.Y] = true;
                image.FillRect(rect, Colors.Red);
                did_update = true;
            }
            else
            {
                mask.dead_cells[index.X, index.Y] = false;
                image.FillRect(rect, Colors.SlateGray);
                did_update = true;
            }
        }

        return did_update;
    }

    private static void NewMask(int width, int height)
    {
        if (mask == null) {
            mask = new Mask(width, height);

        } else if (width != size.X || height != size.Y) {
            mask = new Mask(mask, width, height); 
        }

        size = new Vector2I(width, height);
    }

    private static bool NewImage(ref Grid grid)
    {
        bool update_image = true;

        //Create a New Image
        if (grid != null) {

            if (image != null)
            {
                if (grid.GetImageWidth() == image.GetWidth() && grid.GetImageHeight() == image.GetHeight())
                {
                    update_image = false;
                }
            }

            if (update_image)
            {
                //Create Image and Fill 
                for (int x = 0; x < mask.dead_cells.GetLength(0); x++)
                {
                    for (int y = 0; y < mask.dead_cells.GetLength(1); y++)
                    {
                        if (mask.dead_cells[x, y] == true)
                        {
                            Rect2I rect = grid.GetInsideCellSizePx(x, y);
                            image.FillRect(rect, Colors.Red);
                        }
                    }
                }

                image = Image.Create(grid.GetImageWidth(), grid.GetImageHeight(), false, Image.Format.Rgba8);
                image.Fill(Colors.SlateGray);

                //Create Lines
                for (int x = 0; x < grid.GetWidth(); x++)
                {
                    Rect2I rect = grid.GetHorizontalWall(x, 0, false);
                    rect.Size = new Vector2I(rect.Size.X, rect.Size.Y * grid.GetHeight());
                    image.FillRect(rect, Colors.DarkSlateGray);

                    if (x == grid.GetWidth() - 1)
                    {
                        rect = grid.GetHorizontalWall(x, 0, true);
                        rect.Size = new Vector2I(rect.Size.X, rect.Size.Y * grid.GetHeight());
                        image.FillRect(rect, Colors.DarkSlateGray);
                    }
                }

                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    Rect2I rect = grid.GetVerticalWallFull(0, y, false);
                    rect.Size = new Vector2I(rect.Size.X * grid.GetWidth(), rect.Size.Y);
                    image.FillRect(rect, Colors.DarkSlateGray);

                    if (y == grid.GetHeight() - 1)
                    {
                        rect = grid.GetVerticalWall(0, y, true);
                        rect.Size = new Vector2I(rect.Size.X * grid.GetWidth(), rect.Size.Y);
                        image.FillRect(grid.GetVerticalWallFull(0, y, true), Colors.DarkSlateGray);
                    }
                }
            }
        }

        return update_image;
    }
}
