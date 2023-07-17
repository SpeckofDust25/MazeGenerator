using Godot;
using System;


public class Cell {

	//Grid Open Directions
	public bool north, south, east, west;
	public Vector2I index;

	public Cell() {
		north = false;
		south = false;
		east = false;
		west = false;
		index = new Vector2I(0, 0);
	}

	public Cell(Vector2I _index)
	{
        north = false;
        south = false;
        east = false;
        west = false;
        index = new Vector2I(_index.X, _index.Y);
    }

	public bool IsVisited() {
		if (north || south || east || west) {
			return true;
		}

		return false;
	}
 }