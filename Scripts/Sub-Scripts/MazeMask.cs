using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
}


public static class MazeMask
{
    public static Image image;
    public static Mask mask;
    public static Vector2I size;

    public static void CreateNewMask(ref Grid grid)
    {
        /*
        //Create Image
        image = Image.Create(image_width, image_height, false, Image.Format.Rgba8);
        image.Fill(Colors.SlateGray);   //Fill Image

        if (width != size.X || height != size.Y)
        {
            mask = new Mask(width, height); //New Mask
        }

        size = new Vector2I(width, height);*/
    }

    public static void UpdateImage(ref Grid grid, Vector2I local_mouse_position)
    {
        bool pressed = Input.IsActionJustPressed("right_click");

        if (grid != null)
        {
            //Grid Lines
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

            UpdateNewMask(ref grid, local_mouse_position, pressed);
        }
    }

    public static void UpdateNewMask(ref Grid grid, Vector2I local_mouse_position, bool pressed)
    {
        if (pressed) {
            //Get Cell Index
            Vector2I index = grid.GetCellIndexAtImagePosition(local_mouse_position);
            Rect2I rect = grid.GetInsideCellSizePx(index.X, index.Y);
            bool is_dead_cell = mask.dead_cells[index.X, index.Y];

            if (!is_dead_cell) {
                mask.dead_cells[index.X, index.Y] = true;
                image.FillRect(rect, Colors.Red);
            } else {
                mask.dead_cells[index.X, index.Y] = false;
                image.FillRect(rect, Colors.SlateGray);
            }
        }
    }
}
