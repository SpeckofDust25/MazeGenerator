using Godot;
using System;


public struct Cell {

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


/*
public class Cell
{
    private Cell north, south, east, west, adjoined;
    private int x, y;

    public Cell(int _x, int _y) {
        _x = x;
        _y = y;
    }

	public void SetAdjoined() {

	}

	public void SetNeighbors(Cell n, Cell s, Cell e, Cell w) {
	}
}
*/