using Godot;
using MazeGeneratorGlobal;
using System;
using System.Collections.Generic;


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

    public bool IsClosedCell(PCell cell)
    {
        return closed_cells.Contains(cell);
    }
}

public static class PathFinding
{
    public static List<Vector2I> AStar(ref Grid grid, Cell start, Cell end)
    {
        //Setup Starting Variables
        bool found_solution = false;
        PGrid pGrid = new PGrid(grid.GetWidth(), grid.GetHeight(), start, end);

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

            List<ERectangleDirections> directions = grid.GetValidNeighborsNoWalls(lowest_cost_cell.GetIndex());

            //Go Through Each Possible Direction and Open New Cells
            for (int d = 0; d < directions.Count; d++) {
                
                //Get Open Cell Details
                Cell dir_cell = grid.GetCellInDirection(lowest_cost_cell.GetIndex(), directions[d]);

                int g_dir_cost = lowest_cost_cell.GetGCost() + 2;
                int h_dir_cost = Mathf.Abs(dir_cell.index.X - end.index.X) + Mathf.Abs(dir_cell.index.Y - end.index.Y);

                //Set Pathfinding Cell Cost
                PCell p_cell = pGrid.pCells[dir_cell.index.X, dir_cell.index.Y];

                p_cell.SetCost(g_dir_cost, h_dir_cost);
                pGrid.OpenCell(p_cell, lowest_cost_cell.GetIndex());


                //Check Finish Condition
                if (p_cell.GetIndex() == end.index)
                {
                    found_solution = true;
                    break;
                }
            }

            pGrid.CloseCell(lowest_cost_cell);  //Close Our Checked Cell
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
}
