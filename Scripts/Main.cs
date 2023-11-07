using Godot;

public partial class Main : CanvasLayer
{
    //Event Handlers
    [Signal] public delegate void ExpandingEventHandler(bool _expanding);

    //Grid Properties
    bool is_draw_mode = false;
    bool can_expand = false;

    //Nodes
    private Panel panel;
    private PanelContainer panel_container;
    private MazeProperties maze_properties;
    private TextureRect n_maze_image;
    private MazeInterface maze_interface;

    public override void _EnterTree()
    {
        SetupNodes();
        SetupConnections();
    }

    public override void _Process(double delta)
    {
        //Set Local Mouse 
        //MazeProperties s_maze_properties = (MazeProperties) maze_properties;

        if (maze_properties != null )
        {
            maze_properties.SetLocalImageMousePosition((Vector2I)(n_maze_image.GetLocalMousePosition()));
        }

        //Expand UI
        Vector2 start_position = new Vector2(panel_container.Size.X, 0);
        Vector2 size = new Vector2((panel.Position.X - panel_container.Size.X) + 2, panel.Size.Y);
        Rect2 middle_bar = new Rect2(start_position, size);

        if (middle_bar.HasPoint(GetViewport().GetMousePosition()))
        {
           if (Input.IsActionJustPressed("panning")) {
               can_expand = true;
           }
        }

        if (can_expand)
        {
            panel_container.CustomMinimumSize = new Vector2(GetViewport().GetMousePosition().X, 0);

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
        panel_container = GetNode<PanelContainer>("Interface/MazeProperties");
        panel = GetNode<Panel>("Interface/MazePanel");
        maze_interface = (MazeInterface)panel;

        if (maze_interface.HasMethod("IsExpanding"))
        {
            maze_interface.IsExpanding(false);
        }

        maze_properties = GetNode<MazeProperties>("Interface/MazeProperties");
    }

    private void SetupConnections()
    {
        //Maze Properties
        if (!maze_properties.HasSignal("GenerateMaze")) { GD.PrintErr("Can't Find GenerateMaze Signal!"); }
        if (!maze_properties.HasSignal("DrawToggled")) { GD.PrintErr("Can't Find DrawButtonToggled! "); }

        Callable c_generate_maze = new Callable(this, "UpdateImage");
        Callable c_draw_toggled = new Callable(this, "DrawButtonToggle");

        maze_properties.Connect("GenerateMaze", c_generate_maze);
        maze_properties.Connect("DrawToggled", c_draw_toggled);

        //Export Properties
        if (!maze_properties.HasSignal("SaveImage")) { GD.PrintErr("Can't Find SaveImage Signal!"); }
        Callable c_save_image = new Callable(this, "SaveImage");
        maze_properties.Connect("SaveImage", c_save_image);

        //Maze Interface
        if (!maze_interface.HasMethod("IsExpanding")) { GD.PrintErr("Can't Find IsExpanding Method! ");  }
        Callable c_expanding = new Callable(maze_interface, "IsExpanding");
        Connect("Expanding", c_expanding);
    }
    //-------------------------------------------

    //Connections--------------------------------
    private void DrawButtonToggle(bool toggled)
    {
        is_draw_mode = toggled;
        UpdateImage();
    }

    private void UpdateImage()
    {  
        if (!is_draw_mode) { 
            SetImage(MazeImage.image);  
        } else {
            SetImage(MazeMask.image);
        }
    }

    private void SetImage(Image image)
    {
        if (image != null) {
            n_maze_image.Size = new Vector2(image.GetWidth(), image.GetHeight());
            n_maze_image.Texture = ImageTexture.CreateFromImage(image);
        }
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

        MazeImage.image.SavePng(save_path);
    }
    //-------------------------------------------
}
