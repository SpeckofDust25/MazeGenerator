using Godot;
using System;

public partial class PointProperties : TabBar
{
    //Signals
    [Signal] public delegate void StartPointTypeEventHandler();
    [Signal] public delegate void EndPointTypeEventHandler();

    //Nodes
    private OptionButton start_option_button, end_option_button;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        start_option_button = GetNode<OptionButton>("VBoxContainer/StartPointHBoxContainer/OptionButton");
        end_option_button = GetNode<OptionButton>("VBoxContainer/EndPointHBoxContainer/OptionButton");

        start_option_button.ItemSelected += StartPointTypeSelected;
        end_option_button.ItemSelected += EndPointTypeSelected;
    }

    //Connections
    private void StartPointTypeSelected(long index)
    {
        EmitSignal("StartPointType", index);
    }

    private void EndPointTypeSelected(long index)
    {
        EmitSignal("EndPointType", index);
    }
}
