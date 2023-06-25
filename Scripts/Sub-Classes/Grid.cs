using System;

public class Grid {
	public int width = 0;
	public int height = 0;
	public Cell[,] cells;

	public Grid() {
		cells = new Cell[width, height];
	}
}