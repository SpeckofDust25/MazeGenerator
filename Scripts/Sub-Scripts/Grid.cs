using System;
using System.Runtime.InteropServices;
using Godot;

public class Grid {

	private int cell_size = 5;
	private int wall_size = 1;
	private int width = 0;
	private int height = 0;

	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1, int _thickness = 1, int _cell_size = 2) {
		width = _width;
		height = _height;
		cells = new Cell[width, height];

		//Populate Grid
		for (int x = 0; x < cells.GetLength(0); x++) {
			for (int y = 0; y < cells.GetLength(1); y++) {
				cells[x, y] = new Cell();
				cells[x, y].index = new Vector2I(x, y);
			}
		}

		wall_size = _thickness;
		cell_size = _cell_size;
	}

	//Setters
	public void SetWallSize(int _wall_size) {
		if (_wall_size > cell_size)
		{
			wall_size = cell_size - 1;
		} else {
			wall_size = _wall_size;
        }
	}

	//Getters
	public int GetTotalCells() {
		return width * height;
	}

	public int GetWidth() {
		return width;
	}

	public int GetHeight() {
		return height;
	}

	public int GetCellSize() {
		return cell_size;
	}

	public int GetWallSize() {
		return wall_size;
	}
}