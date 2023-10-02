using Godot;
using MazeGeneratorGlobal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;


//Grid is contained of Pathfinding Cells instead of regular cells
//Cells can be opened or closed
//They have a g and h cost and the neighbor that opened that cell
public class PCell
{
    private int g_cost;
    private int h_cost;
    private Vector2I index;
    private Vector2I creator_index;

    public PCell() { }
    public PCell(int _g_cost, int _h_cost, Vector2I _index, Vector2I _creator_index)
    {
        g_cost = _g_cost;
        h_cost = _h_cost;

        index = _index;
        creator_index = _creator_index;
    }

    //Setters
    public void SetCost(int _g_cost, int _h_cost)
    {
        g_cost = _g_cost;
        h_cost = _h_cost;
    }

    public void SetIndex(Vector2I _index)
    {
        index = _index;
    }

    public void SetCreatorIndex(Vector2I _creator_index)
    {
        creator_index = _creator_index;
    }

    //Getters
    public int GetFCost()
    {
        return g_cost + h_cost;
    }

    public int GetGCost()
    {
        return g_cost;
    }

    public int GetHCost()
    {
        return h_cost;
    }

    public Vector2I GetIndex()
    {
        return index;
    }

    public Vector2I GetCreatorIndex()
    {
        return creator_index;
    }
}

public class DistanceCell
{
    public Vector2I index;
    public int distance;

    public DistanceCell(Vector2I _index, int _distance)
    {
        index = _index;
        distance = _distance;
    }
}

public class PGrid
{
    public PCell[,] pCells;
    int width, height;
    public List<PCell> opened_cells;
    public List<PCell> closed_cells;

    Cell start_cell;
    Cell end_cell;

    public PGrid(int _width, int _height, Cell _start_cell, Cell _end_cell)
    {
        width = _width;
        height = _height;

        start_cell = _start_cell;
        end_cell = _end_cell;

        pCells = new PCell[width, height];
        opened_cells = new List<PCell>();
        closed_cells = new List<PCell>();

        //Populate Grid
        for (int x = 0; x < pCells.GetLength(0); x++)
        {
            for (int y = 0; y < pCells.GetLength(1); y++)
            {
                pCells[x, y] = new PCell();
                pCells[x, y].SetIndex(new Vector2I(x, y));
            }
        }
    }

    public PGrid(int _width, int _height, Cell _start_cell)
    {
        width = _width;
        height = _height;

        start_cell = _start_cell;
        end_cell = null;

        pCells = new PCell[width, height];
        opened_cells = new List<PCell>();
        closed_cells = new List<PCell>();

        //Populate Grid
        for (int x = 0; x < pCells.GetLength(0); x++)
        {
            for (int y = 0; y < pCells.GetLength(1); y++)
            {
                pCells[x, y] = new PCell();
                pCells[x, y].SetIndex(new Vector2I(x, y));
            }
        }
    }

    //Setters
    public void OpenCell(PCell cell, Vector2I creator_index)
    {
        if (!IsClosedCell(cell))
        {
            opened_cells.Add(cell); //Set 
            cell.SetCreatorIndex(creator_index); //Set Creator
        }
    }

    public void CloseCell(PCell cell)
    {
        opened_cells.Remove(cell);
        closed_cells.Add(cell);
    }

    //Getters
    public PCell GetLowestCostCell()
    {
        PCell lowest_cost_cell = null;

        if (opened_cells.Count > 0) {
            lowest_cost_cell = opened_cells[0];

            for (int i = 0; i < opened_cells.Count; i++)
            {
                bool is_equal = (opened_cells[i].GetFCost() == lowest_cost_cell.GetFCost());

                if (is_equal && opened_cells[i].GetHCost() < lowest_cost_cell.GetHCost())
                {
                    lowest_cost_cell = opened_cells[i];
                } else if (opened_cells[i].GetFCost() < lowest_cost_cell.GetFCost())
                {
                    lowest_cost_cell = opened_cells[i];
                }
            }
        }
       
        return lowest_cost_cell;
    }

    public PCell GetLowestGCostCell()
    {
        PCell lowest_cost_cell = null;

        if (opened_cells.Count > 0)
        {
            lowest_cost_cell = opened_cells[0];

            for (int i = 0; i < opened_cells.Count; i++)
            {
                bool is_equal = (opened_cells[i].GetFCost() == lowest_cost_cell.GetFCost());

                if (opened_cells[i].GetGCost() < lowest_cost_cell.GetGCost()) {
                    lowest_cost_cell = opened_cells[i];
                }
            }
        }

        return lowest_cost_cell;
    }


    public List<DistanceCell> GetCellDistances()
    {
        List<DistanceCell> cell_distances = new List<DistanceCell>();

        //Get Cell Distances
        for (int x = 0; x < pCells.GetLength(0); x++)
        {
            for (int y = 0; y < pCells.GetLength(1); y++)
            {
                if (pCells[x, y].GetGCost() != 0) {
                    cell_distances.Add(new DistanceCell(pCells[x, y].GetIndex(), pCells[x, y].GetGCost()));
                }
            }
        }

        //Bubble Sort Cells
        for (int i = 0; i < cell_distances.Count - 1; i++)
        {
            for (int l = 0; l < cell_distances.Count - i - 1; l++)
            {
                if (cell_distances[l].distance > cell_distances[l + 1].distance)
                {
                    DistanceCell temp_d_cell = cell_distances[l + 1];
                    cell_distances[l + 1] = cell_distances[l];
                    cell_distances[l] = temp_d_cell;
                }
            }
        }

        return cell_distances;
    }

    public Vector2I GetEndIndex(float value, List<Vector2I> points) {
        List<DistanceCell> distances = GetCellDistances();
        Vector2I index = Vector2I.Zero;

        if (points != null) {
            for (int i = distances.Count - 1; i >= 0; i--)
            {
                if (!points.Contains(distances[i].index))
                {
                    distances.Remove(distances[i]);
                }
            }

            if (distances.Count > 0)
            {
                index = distances[(int)(value * distances.Count)].index;
            }
        }

        return index;
    }

    public Vector2I GetFurthestIndex(List<Vector2I> points = null) {
        List<DistanceCell> distances = GetCellDistances();
        int highest = 0;
        Vector2I index = Vector2I.Zero;

        for (int i = 0; i < distances.Count; i++)
        {
            if (points == null) {
                if (distances[i].distance > highest)
                {
                    highest = distances[i].distance;
                    index = distances[i].index;
                }
            } else {
                if (distances[i].distance > highest)
                {
                    if (points.Contains(distances[i].index))
                    {
                        highest = distances[i].distance;
                        index = distances[i].index;
                    }
                }
            }
        }

        return index;
    }

    public bool IsClosedCell(PCell cell)
    {
        return closed_cells.Contains(cell);
    }
}

public static class PathFinding
{
    public static List<Vector2I> AStar(ref Maze maze, Cell start, Cell end)
    {
        //Setup Starting Variables
        bool found_solution = false;
        PGrid pGrid = new PGrid(maze.GetWidth(), maze.GetHeight(), start, end);

        //Set Start Cell
        int x_distance = Mathf.Abs(start.index.X - end.index.X);
        int y_distance = Mathf.Abs(start.index.Y - end.index.Y);

        int g_cost = 0;
        int h_cost = x_distance + y_distance;
        
        pGrid.pCells[start.index.X, start.index.Y].SetCost(g_cost, h_cost);
        pGrid.OpenCell(pGrid.pCells[start.index.X, start.index.Y], start.index);

        //Set Costs until finish is found
        while (!found_solution) {

            PCell lowest_cost_cell = pGrid.GetLowestCostCell();
            List<ERectangleDirections> directions = maze.GetValidNeighborsNoWalls(lowest_cost_cell.GetIndex());

            //Go Through Each Possible Direction and Open New Cells
            for (int d = 0; d < directions.Count; d++) {
                
                //Get Open Cell Details
                Cell dir_cell = maze.GetCellInDirection(lowest_cost_cell.GetIndex(), directions[d]);
                pGrid.CloseCell(lowest_cost_cell);  //Close Our Checked Cell

                int g_dir_cost = lowest_cost_cell.GetGCost() + 2;
                int h_dir_cost = Mathf.Abs(dir_cell.index.X - end.index.X) + Mathf.Abs(dir_cell.index.Y - end.index.Y);

                //Set Pathfinding Cell Cost
                PCell p_cell = pGrid.pCells[dir_cell.index.X, dir_cell.index.Y];

                if (!pGrid.closed_cells.Contains(p_cell))
                {
                    p_cell.SetCost(g_dir_cost, h_dir_cost);
                    pGrid.OpenCell(p_cell, lowest_cost_cell.GetIndex());
                }

                //Check Finish Condition
                if (p_cell.GetIndex() == end.index)
                {
                    found_solution = true;
                    break;
                }
            }
        }

        //Get Path: Via Cells
        List<Vector2I> path = new List<Vector2I>();
        path.Add(end.index);
        Vector2I current_index = pGrid.pCells[end.index.X, end.index.Y].GetCreatorIndex();
        path.Add(current_index);

        while(current_index != start.index)
        {
            current_index = pGrid.pCells[current_index.X, current_index.Y].GetCreatorIndex();

            path.Add(current_index);
        }

        List<Vector2I> test_path = new List<Vector2I>();
        for(int i = 0; i < pGrid.closed_cells.Count; i++)
        {
            test_path.Add(pGrid.closed_cells[i].GetIndex());
        }

        for (int i = 0; i < pGrid.opened_cells.Count; i++)
        {
            test_path.Add(pGrid.opened_cells[i].GetIndex());
        }

        return path;
    }

    private static List<ERectangleDirections> MoveDirFromPoint(ref Grid grid, Cell cell)
    {
        List<ERectangleDirections> moves = new List<ERectangleDirections>();
        
        //Check Open 
        if (cell.north) { moves.Add(ERectangleDirections.North); }
        if (cell.south) { moves.Add(ERectangleDirections.South); }
        if (cell.east) { moves.Add(ERectangleDirections.East); }
        if (cell.west) { moves.Add(ERectangleDirections.West); }

        return moves;
    }

    public static PGrid GetDistancesFromCell(ref Maze maze, Cell start)
    {
        PGrid pGrid = null;

        if (start != null) {
            int valid_cell_count = maze.GetValidCellCount();

            //Setup Starting Variables
            pGrid = new PGrid(maze.GetWidth(), maze.GetHeight(), start);

            //Set Start Cell
            pGrid.pCells[start.index.X, start.index.Y].SetCost(0, 0);
            pGrid.OpenCell(pGrid.pCells[start.index.X, start.index.Y], start.index);

            //Set Costs until finish is found
            while (pGrid.closed_cells.Count < (valid_cell_count))
            {
                PCell lowest_cost_cell = pGrid.GetLowestGCostCell();
                List<ERectangleDirections> directions = maze.GetValidNeighborsNoWalls(lowest_cost_cell.GetIndex());
                pGrid.CloseCell(lowest_cost_cell);  //Close Our Checked Cell

                //Go Through Each Possible Direction and Open New Cells
                for (int d = 0; d < directions.Count; d++)
                {
                    //Get Open Cell Details
                    Cell dir_cell = maze.GetCellInDirection(lowest_cost_cell.GetIndex(), directions[d]);
                    int g_dir_cost = lowest_cost_cell.GetGCost() + 1;

                    //Set Pathfinding Cell Cost
                    PCell p_cell = pGrid.pCells[dir_cell.index.X, dir_cell.index.Y];

                    //Only add if we don't already have it
                    if (!pGrid.closed_cells.Contains(p_cell)) {

                        p_cell.SetCost(g_dir_cost, 0);
                        pGrid.OpenCell(p_cell, lowest_cost_cell.GetIndex());
                    }
                }
            }
        }

        return pGrid;
    }
    
}
