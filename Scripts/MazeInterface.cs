using Godot;

public partial class MazeInterface : Panel
{

	//Nodes
	private TextureRect n_maze_image;
	private Button n_maximize;
	private Button n_minimize;
	private Button n_center;
	private HSlider n_magnify_slider;
	private Label n_magnify_label;
    private TextureButton n_lock_button;
    private Label n_fps_label;
    private Panel maze_panel;

	//Load Resources
	Texture2D texture_lock_open = GD.Load<Texture2D>("Sprites/User_Interface/spr_lock_open.png");
	Texture2D texture_lock_closed = GD.Load<Texture2D>("Sprites/User_Interface/spr_lock_closed.png");

	//Variables
	bool is_canvas_locked = false;
	private bool panning = false;
	private Vector2 panning_vec;
	private bool is_focused;
	private float magnify_increment = 0.10f;
	private bool is_expanding = false;

	public override void _Ready()
	{
		SetupNodes();
		SetupConnections();
		n_maze_image.PivotOffset = n_maze_image.Size / 2;
		_center_pressed();

		panning_vec = new Vector2();
		FocusMode = FocusModeEnum.All;

		maze_panel = (Panel)this;
	}

	public override void _Process(double delta)
	{
		n_fps_label.Text = "Fps: " + Engine.GetFramesPerSecond().ToString();
        n_maze_image.PivotOffset = n_maze_image.Size / 2;
        HandleInput();
    }

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseMotion newEvent) //Mouse Movement
		{
			panning_vec.X = newEvent.Relative.X;
			panning_vec.Y = newEvent.Relative.Y;
		}
	}

	public void HandleInput()
	{
		//Zoom in and out 
		if (Input.IsActionJustPressed("zoom_in"))
		{
			if (maze_panel.GetRect().HasPoint(GetViewport().GetMousePosition())) {
				SetPivotToMousePosition();
                _maximize_pressed();
			}
		}

		if (Input.IsActionJustPressed("zoom_out"))
		{
            if (maze_panel.GetRect().HasPoint(GetViewport().GetMousePosition()))
            {
				SetPivotToMousePosition();
                _minimize_pressed();
            }
        }

		//Panning Image
		if (!is_canvas_locked) {
			if (Input.IsActionPressed("panning") && !is_expanding) {
				if (is_focused) { //Stay Focused Outside Panel Range
					n_maze_image.Position += panning_vec;

				} else if (GetGlobalRect().HasPoint(GetGlobalMousePosition())) {
					is_focused = true;
					n_maze_image.Position += panning_vec;
				} 
			} else {
				is_focused = false;
			}
		}

		panning_vec = Vector2.Zero;
	}

	private void SetupNodes()
	{
		n_maze_image = GetNode<TextureRect>("MazeImage");
		n_maximize = GetNode<Button>("MazeImageInterface/MaximizeButton");
		n_minimize = GetNode<Button>("MazeImageInterface/MinimizeButton");
		n_center = GetNode<Button>("MazeImageInterface/CenterButton");
		n_magnify_slider = GetNode<HSlider>("MazeImageInterface/MagnifySlider");
		n_magnify_label = GetNode<Label>("MazeImageInterface/MagnifyLabel");
		n_lock_button = GetNode<TextureButton>("LockButton");
		n_fps_label = GetNode<Label>("FpsLabel");
	}

	private void SetupConnections()
	{
		Callable c_maximize = new Callable(this, "_maximize_pressed");
		n_maximize.Connect("pressed", c_maximize);

		Callable c_minimize = new Callable(this, "_minimize_pressed");
		n_minimize.Connect("pressed", c_minimize);

		Callable c_center = new Callable(this, "_center_pressed");
		n_center.Connect("pressed", c_center);

		Callable c_magnify_slider = new Callable(this, "_magnify_slider_changed");
		n_magnify_slider.Connect("value_changed", c_magnify_slider);

		n_lock_button.Toggled += _toggle_lock_button;
	}

	public void _maximize_pressed()
	{
		if (!is_canvas_locked) {
			n_maze_image.Scale += new Vector2(magnify_increment, magnify_increment);
			UpdateMagnifyIndicators();
		}
	}

	public void _minimize_pressed()
	{
		if (!is_canvas_locked) {
			n_maze_image.Scale -= new Vector2(magnify_increment, magnify_increment);
			UpdateMagnifyIndicators();
		}
	}

	public void UpdateMagnifyIndicators(bool update_slider = true)
	{
        float clamp_value = Mathf.Clamp(n_maze_image.Scale.X, 0, 60);
        n_maze_image.Scale = new Vector2(clamp_value, clamp_value);
        n_magnify_label.Text = string.Format("{0:N0}", n_maze_image.Scale.X * 100) + "%";
		
		if (update_slider) {
			n_magnify_slider.Value = n_maze_image.Scale.X * 100;
		}
	}

	public void _center_pressed()
	{
		if (!is_canvas_locked) {
			Vector2 center = GetGlobalRect().GetCenter() - n_maze_image.Size / 2;
			n_maze_image.GlobalPosition = center;
		}
	}

	public void _magnify_slider_changed(float value)
	{
		if (!is_canvas_locked) {
			n_maze_image.Scale = new Vector2(value, value) * 0.01f;
			UpdateMagnifyIndicators(false);
		}
	}

    public void _toggle_lock_button(bool toggle)
	{
		if (toggle) { //Lock
			n_lock_button.TextureNormal = texture_lock_closed;
			n_magnify_slider.Editable = false;
			is_canvas_locked = true;

		} else { //Unlock
			n_lock_button.TextureNormal = texture_lock_open;
			n_magnify_slider.Editable = true;
			is_canvas_locked = false;
        }
	}


    public void IsExpanding(bool _expanding) {
		is_expanding = _expanding;
	}

	//TODO: Figure out how to Zoom in on mouse position
	private void SetPivotToMousePosition() {
        /*if (n_maze_image.Scale.X != 0)
		{
            Vector2 new_offset = n_maze_image.GetLocalMousePosition();
            GD.Print(new_offset.Round());
            n_maze_image.PivotOffset = new_offset.Round() + (n_maze_image.Scale * n_maze_image.Size);
		}*/
    }
}
