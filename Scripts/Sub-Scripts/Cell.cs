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