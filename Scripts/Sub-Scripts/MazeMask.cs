using Godot;

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
    private static bool draw_open;

    //Returns If Updated
    public static bool Update(ref Maze maze, Vector2I local_mouse_position)
    {
        bool did_update = false;
        bool just_pressed = Input.IsActionJustPressed("right_click");
        bool pressed = Input.IsActionPressed("right_click");

        //Input
        if (Input.IsActionJustReleased("right_click"))
        {
            draw_open = false;
        }

        NewMask(maze.GetWidth(), maze.GetHeight());
        did_update = NewImage(ref maze);

        //Set Draw Type
        if (just_pressed)
        {
            Vector2I index = maze.GetCellIndexAtImagePosition(local_mouse_position);
            bool is_dead_cell = mask.dead_cells[index.X, index.Y];

            if (is_dead_cell)
            {
                draw_open = false;
            } else {
                draw_open = true;
            }
        }

        //Update Cell Center
        if (pressed)
        {
            //Get Cell Index
            Vector2I index = maze.GetCellIndexAtImagePosition(local_mouse_position);
            Rect2I rect = maze.GetInsideCellSizePx(index.X, index.Y);

            if (draw_open)
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

    private static bool NewImage(ref Maze maze)
    {
        bool update_image = true;

        //Create a New Image
        if (maze != null) {

            if (image != null)
            {
                if (maze.GetImageWidth() == image.GetWidth() && maze.GetImageHeight() == image.GetHeight())
                {
                    update_image = false;
                }
            }

            if (update_image)
            {
                image = Image.Create(maze.GetImageWidth(), maze.GetImageHeight(), false, Image.Format.Rgba8);
                image.Fill(Colors.SlateGray);

                //Create Lines
                for (int x = 0; x < maze.GetWidth(); x++)
                {
                    Rect2I rect = maze.GetHorizontalWall(x, 0, false);
                    rect.Size = new Vector2I(rect.Size.X, rect.Size.Y * maze.GetHeight());
                    image.FillRect(rect, Colors.DarkSlateGray);

                    if (x == maze.GetWidth() - 1)
                    {
                        rect = maze.GetHorizontalWall(x, 0, true);
                        rect.Size = new Vector2I(rect.Size.X, rect.Size.Y * maze.GetHeight());
                        image.FillRect(rect, Colors.DarkSlateGray);
                    }
                }

                for (int y = 0; y < maze.GetHeight(); y++)
                {
                    Rect2I rect = maze.GetVerticalWallFull(0, y, false);
                    rect.Size = new Vector2I(rect.Size.X * maze.GetWidth(), rect.Size.Y);
                    image.FillRect(rect, Colors.DarkSlateGray);

                    if (y == maze.GetHeight() - 1)
                    {
                        rect = maze.GetVerticalWall(0, y, true);
                        rect.Size = new Vector2I(rect.Size.X * maze.GetWidth(), rect.Size.Y);
                        image.FillRect(maze.GetVerticalWallFull(0, y, true), Colors.DarkSlateGray);
                    }
                }

                //Create Image and Fill 
                for (int x = 0; x < mask.dead_cells.GetLength(0); x++)
                {
                    for (int y = 0; y < mask.dead_cells.GetLength(1); y++)
                    {
                        if (mask.dead_cells[x, y] == true)
                        {
                            Rect2I rect = maze.GetInsideCellSizePx(x, y);
                            image.FillRect(rect, Colors.Red);
                        }
                    }
                }
            }
        }

        return update_image;
    }

    public static void ClearMask(ref Maze maze)
    {
        mask = new Mask(size.X, size.Y);

        for (int x = 0; x < size.X; x++) {
            for (int y = 0; y < size.Y; y++) {
                Rect2I rect = maze.GetInsideCellSizePx(x, y);
                mask.dead_cells[x, y] = false;
                image.FillRect(rect, Colors.SlateGray);
            }
        }
    }
}
