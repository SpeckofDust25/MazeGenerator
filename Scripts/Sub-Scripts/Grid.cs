using System;

public class Grid {

	private int cell_size = 5;
	private int thickness = 1;
	private int width = 0;
	private int height = 0;

	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1, int _thickness = 1, int _cell_size = 2) {
		width = _width;
		height = _height;
		cells = new Cell[width, height];
		thickness = _thickness;
		cell_size = _cell_size;
	}

	//Setters
	public void SetThickness(int _thickness) {
		if (_thickness > cell_size)
		{
			thickness = cell_size - 1;
		} else {
			thickness = _thickness;
		}
	}

	//Getters
	public int GetTotalCells() {
		return width * height;
	}

	public int GetWidth()
	{
		return width;
	}

	public int GetHeight()
	{
		return height;
	}

	public int GetCellSize()
	{
		return cell_size;
	}

	public int GetThickness() {
		return thickness;
	}

}