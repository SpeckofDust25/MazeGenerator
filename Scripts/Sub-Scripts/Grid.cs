using System;

public class Grid {

	public int cell_size = 8;
	public int width = 0;
	public int height = 0;
	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1) {
		width = _width;
		height = _height;
		cells = new Cell[width, height];
	}

	//Getters
	public int GetTotalCells() {
		return width * height;
	}
}