using System;


public struct Cell {

	//Grid Open Directions
	public bool north, south, east, west;

	public Cell() {
		north = false;
		south = false;
		east = false;
		west = false;
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