using Godot;
using System;

public partial class ExportProperties : TabBar
{
    //Signals
    [Signal] public delegate void SaveImageEventHandler();

    //Nodes
    private Button save_image_button;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        SetupNodes();
        SetupConnections();
    }

    private void SetupNodes()
    {
        save_image_button = GetNode<Button>("VBoxContainer/SaveImageButton");
    }

    private void SetupConnections()
    {
        save_image_button.Pressed += SaveImagePressed;
    }

    //Connections
    private void SaveImagePressed()
    {
        EmitSignal(SignalName.SaveImage);
    }

}
