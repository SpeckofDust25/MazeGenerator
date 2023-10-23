using Godot;
using MazeGeneratorGlobal;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;

public static class MazeGenerator
{
    public static bool GenerateMaze(ref Maze maze, EMazeType type, float horizontal_bias)
    {
        bool successful = false;
        int section_count = GetSectionCount(ref maze);

        if (section_count <= 1) {
            successful = true;

            //Conditions
            if (maze.GetTotalDeadCells() == maze.GetTotalCells())
            {
                MazeMask.ClearMask(ref maze);
                maze.SetMask(MazeMask.mask);
            }

            //Generate Maze
            switch (type)
            {
                case EMazeType.BinaryTree:
                    BinaryTreeAlgorithm(ref maze, horizontal_bias);
                    break;

                case EMazeType.Sidewinder:
                    SidewinderAlgorithm(ref maze, horizontal_bias);
                    break;

                case EMazeType.Aldous_Broder:
                    AldousBroderAlgorithm(ref maze, horizontal_bias);
                    break;

                case EMazeType.Wilsons:
                    WilsonsAlgorithm(ref maze, horizontal_bias);
                    break;

                case EMazeType.HuntandKill:
                    HuntandKill(ref maze, horizontal_bias);
                    break;

                case EMazeType.Recursive_Backtracker:
                    RecursiveBacktracker(ref maze, horizontal_bias);
                    break;

                case EMazeType.Ellers:
                    Ellers(ref maze, false, horizontal_bias);
                    break;

                case EMazeType.Ellers_Loop:
                    Ellers(ref maze, true, horizontal_bias);
                    break;

                case EMazeType.GrowingTree_Random:
                    GrowingTree(ref maze, 0, horizontal_bias);
                    break;

                case EMazeType.GrowingTree_Last:
                    GrowingTree(ref maze, 1, horizontal_bias);
                    break;

                case EMazeType.GrowingTree_Mix:
                    GrowingTree(ref maze, 2, horizontal_bias);
                    break;

                case EMazeType.Kruskals_Random:
                    Kruskals(ref maze, horizontal_bias);
                    break;

                case EMazeType.Prims_Simple:
                    Prims_Simple(ref maze, horizontal_bias);
                    break;

                case EMazeType.Prims_True:
                    Prims_True(ref maze, horizontal_bias);
                    break;
            }
        }

        return successful;
    }

    public static int GetSectionCount(ref Maze maze)
    {
        //No Dead Cells
        if (maze.GetTotalDeadCells() == 0) {
            return 1;
        } 

        //Get Sections
        List<List<Cell>> sections = new List<List<Cell>>();

        //Get Sections
        for (int x = 0; x < maze.GetWidth(); x++)
        {
            for (int y = 0; y < maze.GetHeight(); y++)
            {
                if (!maze.cells[x, y].dead_cell) {  //Not a Dead Cell

                    List<Cell> neighbors = maze.GetValidNeighborCells(new Vector2I(x, y));
                    List<int> section_merge_indexes = new List<int>();

                    //Add index to section_merge_indexes to be processed
                    for (int i = 0; i < sections.Count; i++)
                    {
                        for (int l = 0; l < neighbors.Count; l++)
                        {
                            if (sections[i].Contains(neighbors[l])) //Section Contains Neighbor Cell
                            {
                                if (!section_merge_indexes.Contains(i)) {   //Only Add New Indexes
                                    section_merge_indexes.Add(i);
                                }
                            }
                        }
                    }

                    //Add to Section, Merge or Create New Section
                    if (section_merge_indexes.Count == 1) { //Add To Section
                        sections[section_merge_indexes[0]].Add(maze.cells[x,y]);

                    } else if (section_merge_indexes.Count > 1) {   //Merge

                        List<Cell> merge_sections = new List<Cell>();

                        //Move Cells to Merge Section
                        for (int i = section_merge_indexes.Count - 1; i >= 0; i--)
                        {
                            merge_sections.AddRange(sections[section_merge_indexes[i]].ToList<Cell>());
                            sections.RemoveAt(section_merge_indexes[i]);
                        }

                        sections.Add(new List<Cell>()); //Create New Section

                        //Move all Merge Sections to New Section
                        sections[sections.Count - 1].AddRange(merge_sections.ToList<Cell>());
                        sections[sections.Count - 1].Add(maze.cells[x,y]);

                    } else {    //Create New Section
                        sections.Add(new List<Cell>());
                        sections[sections.Count - 1].Add(maze.cells[x, y]);
                    }
                }
            }
        }

        return sections.Count;
    }

    //Move North or East on each cell: No Mask Support
    public static Maze BinaryTreeAlgorithm(ref Maze maze, float horizontal_bias)
    {
        GD.Randomize();

        //Create Maze 
        for (int x = 0; x < maze.GetWidth(); ++x)
        {
            for (int y = 0; y < maze.GetHeight(); ++y)
            {
                ERectangleDirections dir = ERectangleDirections.None;
                List<ERectangleDirections> directions = maze.GetNeighbors(new Vector2I(x, y), true, false, true, false);

                //Remove Possible Walls
                if (directions.Count > 1)
                {
                    EBias bias_direction = maze.GetBias(horizontal_bias);

                    if (bias_direction == EBias.Horizontal)
                    {
                        dir = ERectangleDirections.East;
                    } else {
                        dir = ERectangleDirections.North;
                    }
                }
                else if (directions.Contains(ERectangleDirections.North))
                {
                    dir = ERectangleDirections.North;

                }
                else if (directions.Contains(ERectangleDirections.East))
                {
                    dir = ERectangleDirections.East;
                }

                CarvePathIndex(ref maze, x, y, dir);
            }
        }

        return maze;
    }

    //East-North: Count Previous East Moves, When North Pick Randomly: No Mask Support
    public static Maze SidewinderAlgorithm(ref Maze maze, float horizontal_bias)
    {
        GD.Randomize();

        //Create Maze
        for (int y = 0; y < maze.GetHeight(); y++)
        {
            int east_count = 0;

            for (int x = 0; x < maze.GetWidth(); x++)
            {
                List<ERectangleDirections> directions = maze.GetNeighbors(new Vector2I(x, y), true, false, true, false);
                bool is_north = false;

                //Choose Direction
                if (directions.Count > 1)
                {
                    EBias bias_direction = maze.GetBias(horizontal_bias);

                    if (bias_direction == EBias.Vertical)
                    {
                        is_north = true;
                    }

                }
                else if (directions.Contains(ERectangleDirections.North))
                {
                    is_north = true;
                }

                //Carve Path
                if (is_north)
                {
                    int rand = 0;
                    if (east_count > 0) { rand = (int)(GD.Randi() % east_count); }
                    CarvePathIndex(ref maze, x - rand, y, ERectangleDirections.North);
                    east_count = 0;

                }
                else if (directions.Contains(ERectangleDirections.East))
                {
                    east_count += 1;
                    CarvePathIndex(ref maze, x, y, ERectangleDirections.East);
                }
            }
        }

        return maze;
    }

    //Pick a Random point and goes in any random direction till all are visited
    public static Maze AldousBroderAlgorithm(ref Maze maze, float horizontal_bias)
    {
        //Horizontal-Vertical Bias cannot be 1 or 0: Fixing this within the algorithm would cause 
        //unnecessary Bias that Aldous Broder's is not meant to create nor works properly with
        if (horizontal_bias <= 0)
        {
            horizontal_bias = 0.1f;
        }

        if (horizontal_bias >= 1)
        {
            horizontal_bias = 0.9f;
        }

        GD.Randomize();
        int visited_count = 1;

        Cell cell = maze.GetRandomCell();

        //Count DeadCells
        visited_count += maze.GetTotalDeadCells();

        //Carve Path
        while (!(visited_count >= maze.GetTotalCells()))
        {
            //Get Valid Cell
            if (cell.dead_cell) {
                do
                {
                    cell = maze.GetRandomCell();
                } while (cell.dead_cell);
            }

            Cell next_cell = cell;
            ERectangleDirections move_dir = ERectangleDirections.None;
            List<ERectangleDirections> directions = maze.GetValidNeighbors(cell.index);

            //Get Direction
            if (directions.Count >= 3)
            {
                EBias bias_direction = maze.GetBias(horizontal_bias);

                if (bias_direction == EBias.Horizontal)
                {
                    directions.Remove(ERectangleDirections.North);
                    directions.Remove(ERectangleDirections.South);
                } else {
                    directions.Remove(ERectangleDirections.East);
                    directions.Remove(ERectangleDirections.West);
                }

                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];

            } else if (directions.Count > 0) {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            //Get Next Cell
            if (move_dir != ERectangleDirections.None)
            {
                next_cell = maze.GetCellInDirection(cell.index, move_dir);
            }

            //Carve path
            if (directions.Count > 0 && !cell.dead_cell)
            {
                if (!next_cell.IsVisited() || !cell.IsVisited())
                {
                    CarvePathIndex(ref maze, cell.index.X, cell.index.Y, move_dir);
                    visited_count += 1;
                }

                cell = next_cell;
            }
        }

        return maze;
    }

    //Pick a Random point and go in any random direction till all are visited reset when a loop occurs
    public static Maze WilsonsAlgorithm(ref Maze maze, float horizontal_bias)
    {
        //Horizontal/Vertical Bias cannot be 1 or 0: Fixing this within the algorithm would cause 
        //unnecessary Bias that Wilson's is not meant to create nor works properly with

        if (horizontal_bias <= 0)
        {
            horizontal_bias = 0.1f;
        }

        if (horizontal_bias >= 1)
        {
            horizontal_bias = 0.9f;
        }

        GD.Randomize();

        int visited_count = 1;
        visited_count += maze.GetTotalDeadCells();
        Cell cell = maze.GetRandomCell();

        List<Cell> l_cell_index = new List<Cell>(); //Loop List
        Cell v_end_cell = maze.GetRandomCell();

        //Get valid cell
        while (v_end_cell.dead_cell)
        {
            v_end_cell = maze.GetRandomCell();
        }

        bool has_new_path = false;

        //Already Visited cells will not be modified
        while (!(visited_count >= (maze.GetWidth() * maze.GetHeight())))
        {
            //Get Valid Cell
            if (cell.dead_cell)
            {
                do
                {
                    cell = maze.GetRandomCell();
                } while (cell.dead_cell);
            }

            //Get Random Unvisited cell
            if (!has_new_path)
            {
                while (cell.IsVisited() || cell.IsSameCell(v_end_cell) || cell.dead_cell)
                {
                    cell = maze.GetRandomCell();
                }

                l_cell_index.Clear();
                l_cell_index.Add(cell);
                has_new_path = true;
            }

            //Starting Values: Get Direction
            Cell next_cell = cell;
            ERectangleDirections move_dir = ERectangleDirections.None;
            List<ERectangleDirections> directions = maze.GetValidNeighbors(cell.index);

            //Get Direction
            if (directions.Count == 4)
            {
                EBias bias_direction = maze.GetBias(horizontal_bias);

                if (bias_direction == EBias.Horizontal)
                {
                    directions.Remove(ERectangleDirections.North);
                    directions.Remove(ERectangleDirections.South);
                }
                else
                {
                    directions.Remove(ERectangleDirections.East);
                    directions.Remove(ERectangleDirections.West);
                }

                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];

            }
            else if (directions.Count > 0)
            {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            //Carve Path
            if (move_dir != ERectangleDirections.None)
            {
                next_cell = maze.GetCellInDirection(cell.index, move_dir);

                if (next_cell.IsVisited() || next_cell.IsSameCell(v_end_cell))
                {
                    l_cell_index.Add(next_cell);
                    CarvePathLoop(ref maze, ref l_cell_index);
                    visited_count += l_cell_index.Count - 1;
                    has_new_path = false;

                }
                else
                {    //Check For Loop
                    CheckForLoop(ref l_cell_index, next_cell);
                    l_cell_index.Add(next_cell);
                }

                cell = next_cell;
            }
        }

        return maze;
    }

    //Random Walk, when closed in get a cell with at least 1 visited cell
    public static Maze HuntandKill(ref Maze maze, float horizontal_bias)
    {
        GD.Randomize();
        int visited_count = 1;
        visited_count += maze.GetTotalDeadCells();

        Cell cell = maze.GetRandomCell();

        while (!(visited_count >= (maze.GetWidth() * maze.GetHeight())))
        {
            //Get Valid Cell
            if (cell.dead_cell)
            {
                do
                {
                    cell = maze.GetRandomCell();
                } while (cell.dead_cell);
            }

            Cell next_cell = cell;
            ERectangleDirections move_dir = ERectangleDirections.None;
            List<ERectangleDirections> directions = maze.GetValidUnvisitedNeighbors(cell.index);

            //Get Direction
            if (directions.Count >= 3)
            {
                EBias bias_direction = maze.GetBias(horizontal_bias);

                if (bias_direction == EBias.Horizontal)
                {
                    directions.Remove(ERectangleDirections.North);
                    directions.Remove(ERectangleDirections.South);
                } else {
                    directions.Remove(ERectangleDirections.East);
                    directions.Remove(ERectangleDirections.West);
                }

                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];

            } else if (directions.Count > 0) {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            } 

            if (directions.Count == 0) {    //Only Hunt when we have nowhere to go
                cell = Hunt(ref maze, cell);
                visited_count += 1;

            } else {    //Carve Path
                next_cell = maze.GetCellInDirection(cell.index, move_dir);
                CarvePathIndex(ref maze, cell.index.X, cell.index.Y, move_dir);
                visited_count += 1;
                cell = next_cell;
            }
        }

        return maze;
    }

    public static Maze RecursiveBacktracker(ref Maze maze, float horizontal_bias)
    {

        GD.Randomize();
        int visited_count = 1;
        Cell cell = maze.GetValidRandomCell();
        Stack<Cell> s_cells = new Stack<Cell>();

        s_cells.Push(cell);
        visited_count += maze.GetTotalDeadCells();

        while (!(visited_count >= (maze.GetWidth() * maze.GetHeight())))
        {

            Cell next_cell = cell;
            ERectangleDirections move_dir = ERectangleDirections.None;
            s_cells.Push(cell);

            //Check For Valid Adjacent Cells
            List<ERectangleDirections> directions = maze.GetValidUnvisitedNeighbors(cell.index);

            if (directions.Count >= 3)
            {
                EBias bias_direction = maze.GetBias(horizontal_bias);

                if (bias_direction == EBias.Horizontal)
                {
                    directions.Remove(ERectangleDirections.North);
                    directions.Remove(ERectangleDirections.South);
                }
                else
                {
                    directions.Remove(ERectangleDirections.East);
                    directions.Remove(ERectangleDirections.West);
                }

                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            } else if (directions.Count > 0) {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            //Carve path
            if (move_dir != ERectangleDirections.None)
            {
                next_cell = maze.GetCellInDirection(cell.index, move_dir);
                CarvePathIndex(ref maze, cell.index.X, cell.index.Y, move_dir);
                visited_count += 1;
                cell = next_cell;
            } else {
                cell = Backtrack(ref maze, s_cells);
            }
        }

        return maze;
    }

    //No Mask Support
    public static Maze Ellers(ref Maze maze, bool is_loop, float horizontal_bias)
    {
        GD.Randomize();
        int iteration = 1;
        int y_index = 0;

        List<List<Identifier>> cell_set_table = new List<List<Identifier>>();

        //Set Cell Table
        for (int y = 0; y < maze.GetHeight(); y++)
        {
            cell_set_table.Add(new List<Identifier>());

            for (int x = 0; x < maze.GetWidth(); x++)
            {
                cell_set_table[y].Add(new Identifier(iteration));
                iteration += 1;
            }
        }

        //Loop
        while (y_index < maze.GetHeight())
        {

            //Link, Go South and Merge Sets
            if (y_index < maze.GetHeight() - 1)
            {
                Link(ref maze, ref cell_set_table, y_index, is_loop);    //Link Row
                RandomSouth(ref maze, ref cell_set_table, y_index, is_loop);    //Get South
            }
            else
            {    //Last 
                LinkLast(ref maze, ref cell_set_table);
            }

            y_index += 1;
        }

        return maze;
    }

    // 0 - Prim's (Random), 1 - Recursive Backtracker (Last), 2 - Mix
    public static Maze GrowingTree(ref Maze maze, int type, float horizontal_bias)
    {
        List<Cell> active_list = new List<Cell>();

        Cell temp_cell;

        do
        {
            temp_cell = maze.GetRandomCell();
        } while (temp_cell.dead_cell);

        active_list.Add(temp_cell);

        while (active_list.Count > 0)
        {
            int index = (int)(GD.Randi() % active_list.Count);  //Get a Active Cell
            Cell cell = active_list[index];
            List<ERectangleDirections> directions = maze.GetValidUnvisitedNeighbors(cell.index);
            ERectangleDirections move_dir = ERectangleDirections.None;

            //Random: Do Nothing

            //Last
            if (type == 1)
            {
                index = active_list.Count - 1;
                cell = active_list[index];

                directions = maze.GetValidUnvisitedNeighbors(cell.index);
            }

            //Mixed
            if (type == 2)
            {
                if ((int)(GD.Randi() % 2) == 0)
                {
                    index = active_list.Count - 1;
                    cell = active_list[index];

                    directions = maze.GetValidUnvisitedNeighbors(cell.index);
                }
            }

            //Get Direction
            if (directions.Count >= 3)
            {
                EBias bias_direction = maze.GetBias(horizontal_bias);

                if (bias_direction == EBias.Horizontal)
                {
                    directions.Remove(ERectangleDirections.North);
                    directions.Remove(ERectangleDirections.South);
                }
                else
                {
                    directions.Remove(ERectangleDirections.East);
                    directions.Remove(ERectangleDirections.West);
                }

                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];

            }
            else if (directions.Count > 0)
            {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            //Carve path
            if (move_dir != ERectangleDirections.None)
            {
                Cell new_cell = CarvePathIndex(ref maze, cell.index.X, cell.index.Y, move_dir);

                if (!new_cell.IsSameCell(cell))
                {
                    active_list.Add(new_cell);
                }
            }
            else
            { //Remove path
                active_list.RemoveAt(index);
            }
        }

        return maze;
    }

    public static Maze Kruskals(ref Maze maze, float horizontal_bias)
    {
        int iteration = 1;
        List<List<int>> set = new List<List<int>>();

        //Populate sets
        for (int y = 0; y < maze.GetHeight(); y++)
        {
            set.Add(new List<int>());

            for (int x = 0; x < maze.GetWidth(); x++)
            {
                if (!maze.cells[x, y].dead_cell)
                {
                    set[y].Add(iteration);
                    iteration += 1;
                }
                else
                {
                    set[y].Add(-1);
                }
            }
        }

        while (!SetComplete(set))
        {
            Cell cell = maze.GetValidRandomCell();
            List<ERectangleDirections> directions = maze.GetValidNeighbors(cell.index);

            ERectangleDirections move_dir = ERectangleDirections.None;

            //Get Direction
            if (directions.Count == 4)
            {
                EBias bias_direction = maze.GetBias(horizontal_bias);

                if (bias_direction == EBias.Horizontal)
                {
                    directions.Remove(ERectangleDirections.North);
                    directions.Remove(ERectangleDirections.South);
                }
                else
                {
                    directions.Remove(ERectangleDirections.East);
                    directions.Remove(ERectangleDirections.West);
                }

                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];

            }
            else if (directions.Count > 0)
            {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            int first = set[cell.index.Y][cell.index.X];
            int second = 0;

            if (move_dir == ERectangleDirections.None)  //No Usable Adjacent Cell
            {
                continue;
            }

            switch (move_dir)
            {
                case ERectangleDirections.North:
                    second = set[cell.index.Y - 1][cell.index.X];
                    break;

                case ERectangleDirections.South:
                    second = set[cell.index.Y + 1][cell.index.X];
                    break;

                case ERectangleDirections.East:
                    second = set[cell.index.Y][cell.index.X + 1];
                    break;

                case ERectangleDirections.West:
                    second = set[cell.index.Y][cell.index.X - 1];
                    break;
            }

            if (first != second)
            {
                MergeSetManual(ref set, first, second);
                CarvePathIndex(ref maze, cell.index.X, cell.index.Y, move_dir);
            }
        }

        return maze;
    }

    public static Maze Prims_Simple(ref Maze maze, float horizontal_bias)
    {
        List<Cell> active = new List<Cell>();
        active.Add(maze.GetValidRandomCell());

        while (active.Count > 0)
        {
            int index = (int)(GD.Randi() % active.Count);
            Cell cell = active[index];
            List<ERectangleDirections> directions = maze.GetValidUnvisitedNeighbors(cell.index);
            ERectangleDirections move_dir = ERectangleDirections.None;

            //Get Direction
            if (directions.Count >= 3)
            {
                EBias bias_direction = maze.GetBias(horizontal_bias);

                if (bias_direction == EBias.Horizontal)
                {
                    directions.Remove(ERectangleDirections.North);
                    directions.Remove(ERectangleDirections.South);
                }
                else
                {
                    directions.Remove(ERectangleDirections.East);
                    directions.Remove(ERectangleDirections.West);
                }

                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];

            }
            else if (directions.Count > 0)
            {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }


            if (directions.Count > 0)
            {
                active.Add(maze.GetCellInDirection(cell.index, move_dir));
                CarvePathIndex(ref maze, cell.index.X, cell.index.Y, move_dir);
            }
            else
            {
                active.RemoveAt(index);
            }
        }

        return maze;
    }

    public static Maze Prims_True(ref Maze maze, float horizontal_bias)
    {
        List<List<int>> cell_cost = new List<List<int>>();
        List<Cell> active = new List<Cell>();

        //Get Starting Point
        active.Add(maze.GetValidRandomCell());

        //Populate Cell Cost List: 0 - 99
        for (int x = 0; x < maze.GetWidth(); x++)
        {
            cell_cost.Add(new List<int>());

            for (int y = 0; y < maze.GetHeight(); y++)
            {
                cell_cost[x].Add((int)(GD.Randi() % 100));
            }
        }

        //Find the Lowest Cost Cell, and it's Lowest Cost Neighbor
        while (active.Count > 0)
        {
            int index = 0;

            //Get Lowest Cost Cell
            int lowest = 100;
            Cell lowest_cost_cell = active[0];

            for (int i = 0; i < active.Count; i++)
            {
                int cost = cell_cost[active[i].index.X][active[i].index.Y];
                if (cost < lowest)
                {
                    lowest = cost;
                    lowest_cost_cell = active[i];
                    index = i;
                }
            }

            lowest = 100;
            ERectangleDirections move_dir = ERectangleDirections.None;
            List<ERectangleDirections> directions = maze.GetValidUnvisitedNeighbors(lowest_cost_cell.index);
            Cell neighbor = active[0];
            Cell temp;

            //Get Lowest Cost Neighbor
            if (directions.Contains(ERectangleDirections.North))  //North
            {
                temp = maze.GetCellInDirection(lowest_cost_cell.index, ERectangleDirections.North);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = ERectangleDirections.North;
                    neighbor = temp;
                }
            }

            if (directions.Contains(ERectangleDirections.South))  //South
            {
                temp = maze.GetCellInDirection(lowest_cost_cell.index, ERectangleDirections.South);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = ERectangleDirections.South;
                    neighbor = temp;
                }
            }

            if (directions.Contains(ERectangleDirections.East))  //East
            {
                temp = maze.GetCellInDirection(lowest_cost_cell.index, ERectangleDirections.East);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = ERectangleDirections.East;
                    neighbor = temp;
                }
            }

            if (directions.Contains(ERectangleDirections.West))  //West
            {
                temp = maze.GetCellInDirection(lowest_cost_cell.index, ERectangleDirections.West);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = ERectangleDirections.West;
                    neighbor = temp;
                }
            }

            if (move_dir != ERectangleDirections.None)
            {
                active.Add(neighbor);
                CarvePathIndex(ref maze, lowest_cost_cell.index.X, lowest_cost_cell.index.Y, move_dir);
            }
            else
            {
                active.RemoveAt(index);
            }
        }

        return maze;
    }

    public static Maze GrowingForest(ref Maze maze, float horizontal_bias)
    {
        return maze;
    }

    public static Maze Recursive_Division(ref Maze maze, float horizontal_bias)
    {
        return maze;
    }


    //Carve Path Methods
    private static Cell CarvePathIndex(ref Maze maze, int x, int y, ERectangleDirections direction)
    {
        Cell cell = maze.cells[x, y];
        Vector2I index = cell.index;

        switch (direction)
        {
            case ERectangleDirections.North:
                maze.cells[cell.index.X, cell.index.Y].north = true;
                maze.cells[cell.index.X, cell.index.Y - 1].south = true;
                index = new Vector2I(cell.index.X, cell.index.Y - 1);
                break;

            case ERectangleDirections.South:
                maze.cells[cell.index.X, cell.index.Y].south = true;
                maze.cells[cell.index.X, cell.index.Y + 1].north = true;
                index = new Vector2I(cell.index.X, cell.index.Y + 1);
                break;

            case ERectangleDirections.East:
                maze.cells[cell.index.X, cell.index.Y].east = true;
                maze.cells[cell.index.X + 1, cell.index.Y].west = true;
                index = new Vector2I(cell.index.X + 1, cell.index.Y);
                break;

            case ERectangleDirections.West:
                maze.cells[cell.index.X, cell.index.Y].west = true;
                maze.cells[cell.index.X - 1, cell.index.Y].east = true;
                index = new Vector2I(cell.index.X - 1, cell.index.Y);
                break;
        }

        return maze.cells[index.X, index.Y];
    }

    private static void CarvePathLoop(ref Maze maze, ref List<Cell> cells)
    {
        for (int i = 0; i < cells.Count - 1; i++)
        {
            Vector2I direction = new Vector2I(cells[i + 1].index.X - cells[i].index.X, cells[i + 1].index.Y - cells[i].index.Y);

            if (direction.X != 0)
            {
                if (direction.X > 0)
                {  //East
                    maze.cells[cells[i].index.X, cells[i].index.Y].east = true;
                    maze.cells[cells[i].index.X + 1, cells[i].index.Y].west = true;
                }
                else
                {    //West
                    maze.cells[cells[i].index.X, cells[i].index.Y].west = true;
                    maze.cells[cells[i].index.X - 1, cells[i].index.Y].east = true;
                }
            }

            if (direction.Y != 0)
            {
                if (direction.Y > 0)
                {  //South
                    maze.cells[cells[i].index.X, cells[i].index.Y].south = true;
                    maze.cells[cells[i].index.X, cells[i].index.Y + 1].north = true;
                }
                else
                {    //North
                    maze.cells[cells[i].index.X, cells[i].index.Y].north = true;
                    maze.cells[cells[i].index.X, cells[i].index.Y - 1].south = true;
                }
            }
        }
    }

    //Helper Methods

    /* Used for Eller's Algorithm
     * Links Up the passed in Identifiers Horizontally based on the unique id of each cell
    */
    private static void Link(ref Maze maze, ref List<List<Identifier>> list, int y_index, bool is_loop)
    {
        List<Identifier> row_list = list[y_index];

        for (int i = 0; i < row_list.Count - 1; i++)
        {
            int no_wall = (int)(GD.Randi() % 2);

            if (no_wall == 0)
            {
                if (is_loop || list[y_index][i].uid != list[y_index][i + 1].uid)
                {
                    MergeSets(ref list, list[y_index][i].uid, list[y_index][i + 1].uid);
                    CarvePathIndex(ref maze, i, y_index, ERectangleDirections.East);
                }
            }
        }
    }

    private static void LinkLast(ref Maze maze, ref List<List<Identifier>> list)
    {
        int y_index = maze.GetHeight() - 1;

        for (int i = 0; i < list[y_index].Count - 1; i++)
        {
            if (list[y_index][i].uid != list[y_index][i + 1].uid)
            {
                MergeSets(ref list, list[y_index][i].uid, list[y_index][i + 1].uid);
                CarvePathIndex(ref maze, i, y_index, ERectangleDirections.East);
            }
        }
    }

    private static void RandomSouth(ref Maze maze, ref List<List<Identifier>> list, int y_index, bool is_loop)
    {
        int count = 0;
        int number = list[y_index][0].uid;

        //Iterate Through Row
        for (int i = 0; i < list[0].Count; i++)
        {
            //Found Another Number
            if (list[y_index][i].uid == number)
            {
                count += 1;
            }

            int num = count;
            int south_count = 1;

            //Set Random Number: No Loop
            if (count > 0)
            {
                num = (int)(GD.Randi() % count);

                //Number of Times we Carve South
                for (int s = 0; s < count - 1; s++)
                {
                    int sn = (int)(GD.Randi() % 3);

                    if (sn == 0)
                    {
                        south_count += 1;
                    }
                }
            }

            //Carve South: Loop and No Loop
            if (i < list[y_index].Count - 1)
            {  //Not At End
                if (list[y_index][i + 1].uid != number)
                {
                    if (is_loop)
                    {
                        south_count = 0;
                        bool carved = false;

                        for (int l = 0; l < count; l++)
                        {
                            bool can_carve = false;

                            if ((int)(GD.Randi() % 2) == 0)
                            {
                                Cell middle = maze.cells[i - l, y_index];
                                Cell right = maze.cells[i - l + 1, y_index];
                                Cell left = middle;

                                if (middle.index.X != 0)
                                {
                                    left = maze.cells[i - l - 1, y_index];
                                }

                                if (!middle.south && !right.south && !left.south) //&& !right.south)
                                {
                                    can_carve = true;
                                }

                                //Carve South
                                if (can_carve)
                                {
                                    carved = true;
                                    MergeSets(ref list, number, list[y_index + 1][i - l].uid, true);
                                    CarvePathIndex(ref maze, i - l, y_index, ERectangleDirections.South);
                                }
                            }
                        }

                        if (!carved)
                        {
                            if (!maze.cells[i - num, y_index].south)
                            { //Go South
                                MergeSets(ref list, number, list[y_index + 1][i - num].uid, true);
                                CarvePathIndex(ref maze, i - num, y_index, ERectangleDirections.South);
                            }
                        }

                    }
                    else
                    {    //No Loop

                        while (south_count > 0)
                        {
                            num = (int)(GD.Randi() % count);

                            if (!maze.cells[i - num, y_index].south)
                            { //Go South
                                MergeSets(ref list, number, list[y_index + 1][i - num].uid, true);
                                CarvePathIndex(ref maze, i - num, y_index, ERectangleDirections.South);
                                south_count -= 1;
                            }
                        }
                    }

                    number = list[y_index][i + 1].uid;
                    count = 0;
                }
            }
            else
            { //At End of Row
                MergeSets(ref list, number, list[y_index + 1][i - num].uid, true);
                CarvePathIndex(ref maze, i - num, y_index, ERectangleDirections.South);
            }
        }
    }

    //Set Methods
    private static void MergeSetManual(ref List<List<int>> set, int first, int second)
    {
        for (int y = 0; y < set.Count; y++)
        {
            for (int x = 0; x < set[y].Count; x++)
            {
                if (set[y][x] == second)
                {
                    set[y][x] = first;
                }
            }
        }
    }

    private static void MergeSets(ref List<List<Identifier>> cell_set_table, int first, int second, bool vertical = false)
    {
        for (int y = 0; y < cell_set_table.Count; y++)
        {
            for (int x = 0; x < cell_set_table[y].Count; x++)
            {
                if (cell_set_table[y][x].uid == second)
                {
                    cell_set_table[y][x].uid = first;
                }
            }
        }
    }

    private static bool SetComplete(List<List<int>> set)
    {
        bool complete = true;
        int number = -1;

        for (int y = 0; y < set.Count; y++)
        {
            for (int x = 0; x < set[y].Count; x++)
            {
                if (set[y][x] >= 0)
                { //Skip -1
                    if (number == -1)
                    {
                        number = set[y][x];
                    }
                    else if (number != set[y][x])
                    {
                        complete = false;
                        break;
                    }
                }
            }
        }

        return complete;
    }

    private static Cell Hunt(ref Maze maze, Cell new_cell)
    {
        List<ERectangleDirections> neighbors = new List<ERectangleDirections>();

        //Find a Cell with at least 1 adjacent visited cell
        for (int y = 0; y < maze.GetHeight(); y++)
        {
            neighbors.Clear();

            for (int x = 0; x < maze.GetWidth(); x++)
            {
                Cell temp_cell;
                new_cell = maze.cells[x, y];

                if (!new_cell.IsVisited() && !new_cell.dead_cell)
                {
                    if (new_cell.index.Y > 0)
                    {
                        temp_cell = maze.cells[new_cell.index.X, new_cell.index.Y - 1];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //North
                            neighbors.Add(ERectangleDirections.North);
                        }
                    }

                    if (new_cell.index.X > 0)
                    {
                        temp_cell = maze.cells[new_cell.index.X - 1, new_cell.index.Y];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //West
                            neighbors.Add(ERectangleDirections.West);
                        }
                    }

                    if (new_cell.index.Y < maze.GetHeight() - 1)
                    {
                        temp_cell = maze.cells[new_cell.index.X, new_cell.index.Y + 1];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //South
                            neighbors.Add(ERectangleDirections.South);
                        }
                    }

                    if (new_cell.index.X < maze.GetWidth() - 1)
                    {
                        temp_cell = maze.cells[new_cell.index.X + 1, new_cell.index.Y];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //East
                            neighbors.Add(ERectangleDirections.East);
                        }
                    }
                }

                if (neighbors.Count > 0) { break; }
            }

            if (neighbors.Count > 0) { break; }
        }

        //Pick Random Cell
        if (neighbors.Count > 0)
        {
            int new_dir = (int)(GD.Randi() % (int)neighbors.Count);
            CarvePathIndex(ref maze, new_cell.index.X, new_cell.index.Y, neighbors[new_dir]);
        }

        return new_cell;
    }

    private static Cell Backtrack(ref Maze maze, Stack<Cell> cells)
    {
        Cell target = null;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells.TryPop(out target))
            {

                List<ERectangleDirections> neighbors = maze.GetValidUnvisitedNeighbors(target.index);

                if (neighbors.Count > 0)
                {
                    break;
                }

            }
            else
            {
                break;
            }
        }

        return target;
    }

    //Used for the Wilson's Algorithm: Checks For a Loop 
    private static void CheckForLoop(ref List<Cell> cells, Cell current_cell)
    {
        bool is_loop = false;
        int index = 0;

        for (int i = 0; i < cells.Count; i++)
        {
            //Identify Loop
            if (cells[i].index.X == current_cell.index.X && cells[i].index.Y == current_cell.index.Y)
            {
                is_loop = true;
                index = i;
                break;
            }
        }

        if (is_loop)
        {
            cells.RemoveRange(index, cells.Count - index); //Get rid of Loop
        }
    }
}

