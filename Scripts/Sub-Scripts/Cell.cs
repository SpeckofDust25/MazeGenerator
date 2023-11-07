using Godot;

public class Cell {

	//Grid Open Directions
	public bool north, south, east, west;	//Is Open
	public Vector2I index;
	public bool dead_cell;

	public Cell() {
		dead_cell = false;
		north = false;
		south = false;
		east = false;
		west = false;
		index = new Vector2I(0, 0);
	}

	public Cell(Vector2I _index)
	{
		dead_cell = false;
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

	public bool IsSameCell(Cell cell)
	{
		bool result = false;

		if (cell != null) {
			if (index.X == cell.index.X && index.Y == cell.index.Y)
			{
				result = true;
			}
		}

		return result;
	}
}