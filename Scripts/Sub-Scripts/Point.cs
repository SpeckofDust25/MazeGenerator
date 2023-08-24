using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MazeGeneratorGlobal;

public class Points
{
    /*
    public Cell start_cell;
    public Cell end_cell;

    EPoints point_type;
    ERectangleDirections direction;

    public Points(EPoints _type) {
        point_type = _type;
    }

    public void SetPointType(EPoints _type)
    {
        point_type = _type;
    }

    //Creates An Open Cell
    
    public Cell CreateOpenCell(ref ERectangleDirections assign_direction, ref Grid grid)   //Creates an Open Cell
    {
        Cell open_cell = null;
        /*bool same_cell = false;
        Cell open_cell = null;

        List<Cell> deadend_cells = new List<Cell>();
        List<Cell> edge_cells = new List<Cell>();
        List<Cell> current_list = new List<Cell>();

        //Get Possible Cells
        deadend_cells = grid.GetAllValidEdgeDeadends();
        edge_cells = grid.GetAllEdgeCells();

        //List Conditions
        if (deadend_cells.Count > 0)
        {
            current_list = deadend_cells;
        }
        else
        {
            current_list = edge_cells;
        }

        //Get Random Cell
        if (current_list.Count > 0)
        {
            int rand = (int)(GD.Randi() % current_list.Count);
            open_cell = current_list[rand];
        }

        //Same Cell
        if (open_cell.IsSameCell(start_cell) || open_cell.IsSameCell(end_cell))
        {
            same_cell = true;
        }

        //Open Wall
        if (open_cell != null)
        {
            List<MazeGenerator.Direction> directions = new List<MazeGenerator.Direction>();
            Vector2I index = open_cell.index;

            //Add Possible Directions
            if (index.X == 0)   //West
            {
                if (!same_cell)
                {
                    directions.Add(MazeGenerator.Direction.west);

                }
                else if (!open_cell.north && !open_cell.south && !open_cell.west)
                {
                    directions.Add(MazeGenerator.Direction.west);
                }
            }

            if (index.Y == 0)   //North
            {
                if (!same_cell)
                {
                    directions.Add(MazeGenerator.Direction.north);

                }
                else if (!open_cell.west && !open_cell.east && !open_cell.north)
                {
                    directions.Add(MazeGenerator.Direction.north);
                }
            }

            if (index.X == grid.GetWidth() - 1) //East
            {
                if (!same_cell)
                {
                    directions.Add(MazeGenerator.Direction.east);

                }
                else if (!open_cell.north && !open_cell.south && !open_cell.east)
                {
                    directions.Add(MazeGenerator.Direction.east);
                }
            }

            if (index.Y == grid.GetHeight() - 1) //South
            {

                if (!same_cell)
                {
                    directions.Add(MazeGenerator.Direction.south);

                }
                else if (!open_cell.east && !open_cell.west && !open_cell.south)
                {
                    directions.Add(MazeGenerator.Direction.south);
                }

            }

            Grid.Direction dir = directions[(int)(GD.Randi() % directions.Count)];

            switch (dir)
            {
                case Grid.Direction.north:
                    open_cell.north = true;
                    assign_direction = Grid.Direction.north;
                    break;
                case Grid.Direction.south:
                    open_cell.south = true;
                    assign_direction = Grid.Direction.south;
                    break;
                case MazeGeneGridrator.Direction.east:
                    open_cell.east = true;
                    assign_direction = Grid.Direction.east;
                    break;
                case MazeGenerator.Direction.west:
                    open_cell.west = true;
                    assign_direction = MazeGenerator.Direction.west;
                    break;
            }
        }

        return open_cell;
    }

    public void FillInCell(ref Cell open_cell)
    {
        ERectangleDirections dir = ERectangleDirections.None;

        //Fill in Cell
        if (open_cell != null)
        {
            Vector2I index = open_cell.index;

            dir = direction;
            direction = ERectangleDirections.None;

            //Fill in
            switch (dir)
            {
                case ERectangleDirections.North:
                    open_cell.north = false;
                    break;

                case ERectangleDirections.South:
                    open_cell.south = false;
                    break;

                case ERectangleDirections.East:
                    open_cell.east = false;
                    break;

                case ERectangleDirections.West:
                    open_cell.west = false;
                    break;
            }
        }

        open_cell = null;
    }

    public void ResetPoint()
    {
        start_cell = null;
        end_cell = null;
        direction = ERectangleDirections.None;
    }
*/
}
