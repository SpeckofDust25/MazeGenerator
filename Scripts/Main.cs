using Godot;
using System;
using System.Collections.Generic;

public partial class Main : CanvasLayer
{
    //Event Handlers
    [Signal] public delegate void ExpandingEventHandler(bool _expanding);

    //Grid Properties
    bool is_draw_mode = false;
    bool can_expand = false;

    //Nodes
    private TabContainer tab_container;
    private Panel panel;
    private TabBar maze_properties;
    private TabBar points_properties;
    private TabBar pathfinding_properties;
    private TabBar animation_properties;
    private TabBar export_properties;
    private TextureRect n_maze_image;
    private MazeInterface maze_interface;

    //Image
    private Image maze_image;
    private Image mask_image;

    public override void _EnterTree()
    {
        SetupNodes();
        SetupConnections();
    }

    public override void _Process(double delta)
    {
        /*if (is_draw_mode)
        {
            //MazeMask.UpdateImage(ref grid, (Vector2I)maze_image.GetLocalMousePosition());
            SetImage(MazeMask.image);
        }*/

        //Expand UI
        Vector2 start_position = new Vector2(tab_container.Size.X, 0);
        Vector2 size = new Vector2((panel.Position.X - tab_container.Size.X) + 2, panel.Size.Y);
        Rect2 middle_bar = new Rect2(start_position, size);

        if (middle_bar.HasPoint(GetViewport().GetMousePosition()))
        {
           if (Input.IsActionJustPressed("panning")) {
               can_expand = true;
           }
        }

        if (can_expand)
        {
            tab_container.CustomMinimumSize = new Vector2(GetViewport().GetMousePosition().X, 0);

            if (Input.IsActionJustReleased("panning"))
            {
                can_expand = false;
            }
        }

        EmitSignal(SignalName.Expanding, can_expand);
    }

    //Setup Methods------------------------------
    private void SetupNodes()
    {
        n_maze_image = GetNode<TextureRect>("Interface/MazePanel/MazeImage");
        tab_container = GetNode<TabContainer>("Interface/TabContainer");
        panel = GetNode<Panel>("Interface/MazePanel");
        maze_interface = (MazeInterface)panel;

        if (maze_interface.HasMethod("IsExpanding"))
        {
            maze_interface.IsExpanding(false);
        }

        maze_properties = GetNode<MazeProperties>("Interface/TabContainer/Maze");
        export_properties = GetNode<ExportProperties>("Interface/TabContainer/Export");
    }

    private void SetupConnections()
    {
        //Maze Properties
        if (!maze_properties.HasSignal("GenerateMaze")) { GD.PrintErr("Can't Find GenerateMaze Signal!"); }
        if (!maze_properties.HasSignal("DrawToggled")) { GD.PrintErr("Can't Find DrawButtonToggled! "); }

        Callable c_generate_maze = new Callable(this, "SetImage");
        Callable c_draw_toggled = new Callable(this, "DrawButtonToggle");

        maze_properties.Connect("GenerateMaze", c_generate_maze);
        maze_properties.Connect("DrawToggled", c_draw_toggled);

        //Export Properties
        if (!export_properties.HasSignal("SaveImage")) { GD.PrintErr("Can't Find SaveImage Signal!"); }
        Callable c_save_image = new Callable(this, "SaveImage");
        export_properties.Connect("SaveImage", c_save_image);

        //Maze Interface
        if (!maze_interface.HasMethod("IsExpanding")) { GD.PrintErr("Can't Find IsExpanding Method! ");  }
        Callable c_expanding = new Callable(maze_interface, "IsExpanding");
        Connect("Expanding", c_expanding);
    }
    //-------------------------------------------

    //Connections--------------------------------
    private void SetImage(Image image)
    {
        maze_image = image;
        n_maze_image.Texture = ImageTexture.CreateFromImage(maze_image);
    }

    private void DrawButtonToggle(bool toggle)
    {
        Image new_image = null;

        if (toggle)
        {
            is_draw_mode = true;
            new_image = mask_image;
        }
        else
        {
            is_draw_mode = false;
            new_image = maze_image;
        }

        SetImage(new_image);
    }

    //Export Properties
    private void SaveImage()
    {
        string save_path;

        if (OS.HasFeature("editor"))
        {
            save_path = "res://Images/" + Time.GetDatetimeStringFromSystem(false, false).Replace(":", "_") + ".png";

            if (!DirAccess.DirExistsAbsolute("res://Images"))
            {
                DirAccess.MakeDirAbsolute("res://Images");
            }

        }
        else
        {
            save_path = OS.GetExecutablePath().GetBaseDir() + "/Images/" + Time.GetDatetimeStringFromSystem(false, false).Replace(":", "_") + ".png";

            if (!DirAccess.DirExistsAbsolute(OS.GetExecutablePath().GetBaseDir() + "/Images"))
            {
                DirAccess.MakeDirAbsolute(OS.GetExecutablePath().GetBaseDir() + "/Images");
            }
        }

        maze_image.SavePng(save_path);
    }
    //-------------------------------------------
}
