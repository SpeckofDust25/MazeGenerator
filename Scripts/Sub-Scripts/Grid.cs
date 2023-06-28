using System;

public class Grid {

	private int cell_size = 8;
	private int thickness = 1;
	private int width = 0;
	private int height = 0;

	public Cell[,] cells;

	public Grid(int _width = 1, int _height = 1) {
		width = _width;
		height = _height;
		cells = new Cell[width, height];
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