using Godot;
using MazeGeneratorGlobal;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;

public static class MazeGenerator
{
    public static bool GenerateMaze(ref Grid grid, EMazeType type)
    {
        bool successful = false;
        int section_count = GetSectionCount(ref grid);

        if (section_count <= 1) {
            successful = true;

            //Conditions
            if (grid.GetTotalDeadCells() == grid.GetTotalCells())
            {
                MazeMask.ClearMask(ref grid);
                grid.SetMask(MazeMask.mask);
            }

            //Generate Maze
            switch (type)
            {
                case EMazeType.BinaryTree:
                    BinaryTreeAlgorithm(ref grid);
                    break;

                case EMazeType.Sidewinder:
                    SidewinderAlgorithm(ref grid);
                    break;

                case EMazeType.Aldous_Broder:
                    AldousBroderAlgorithm(ref grid);
                    break;

                case EMazeType.Wilsons:
                    WilsonsAlgorithm(ref grid);
                    break;

                case EMazeType.HuntandKill:
                    HuntandKill(ref grid);
                    break;

                case EMazeType.Recursive_Backtracker:
                    RecursiveBacktracker(ref grid);
                    break;

                case EMazeType.Ellers:
                    Ellers(ref grid, false);
                    break;

                case EMazeType.Ellers_Loop:
                    Ellers(ref grid, true);
                    break;

                case EMazeType.GrowingTree_Random:
                    GrowingTree(ref grid, 0);
                    break;

                case EMazeType.GrowingTree_Last:
                    GrowingTree(ref grid, 1);
                    break;

                case EMazeType.GrowingTree_Mix:
                    GrowingTree(ref grid, 2);
                    break;

                case EMazeType.Kruskals_Random:
                    Kruskals(ref grid);
                    break;

                case EMazeType.Prims_Simple:
                    Prims_Simple(ref grid);
                    break;

                case EMazeType.Prims_True:
                    Prims_True(ref grid);
                    break;

                case EMazeType.GrowingForest:
                    GrowingForest(ref grid);
                    break;

                case EMazeType.Recursive_Division:
                    Recursive_Division(ref grid);
                    break;
            }
        }

        return successful;
    }

    public static int GetSectionCount(ref Grid grid)
    {
        //No Dead Cells
        if (grid.GetTotalDeadCells() == 0) {
            return 1;
        } 

        //Get Sections
        List<List<Cell>> sections = new List<List<Cell>>();

        //Get Sections
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                if (!grid.cells[x, y].dead_cell) {

                    //Start
                    if (sections.Count == 0) {
                        sections.Add(new List<Cell>());
                    }

                    List<int> index_added = new List<int>();    //Section index's that Contain the same cell
                    List<Cell> neighbors = grid.GetValidNeighborCells(new Vector2I(x, y));

                    //Add Cell To Existing List
                    for(int l = 0; l < sections.Count; l++)
                    {
                        for (int i = 0; i < neighbors.Count; i++)
                        {
                            if (sections[l].Contains(neighbors[i]))
                            {
                                sections[l].Add(grid.cells[x, y]);
                                index_added.Add(l);
                                break;
                            }
                        }
                    }

                    //Create a New List
                    if (index_added.Count == 0)
                    {
                        sections.Add(new List<Cell>());
                        sections[sections.Count - 1].Add(grid.cells[x, y]);

                    } else if (index_added.Count > 1) { //Merge
                        
                        //Merge
                        for (int i = 1; i < index_added.Count; i++) //Get Section Index
                        {
                            for (int l = 0; l < sections[index_added[i]].Count; l++) //Add To index 0: No Duplicates
                            {
                                if (!sections[0].Contains(sections[index_added[i]][l]))
                                {
                                    sections[0].Add(sections[index_added[i]][l]);
                                }
                            }
                        }

                        //Remove
                        for (int i = index_added.Count - 1; i > 0; i--)
                        {
                            sections.RemoveAt(index_added[i]);
                        }
                    }
                }
            }
        }

        return sections.Count;
    }


    //Move North or East on each cell: No Mask Support
    public static Grid BinaryTreeAlgorithm(ref Grid grid)
    {
        GD.Randomize();

        //Create Maze 
        for (int x = 0; x < grid.GetWidth(); ++x)
        {
            for (int y = 0; y < grid.GetHeight(); ++y)
            {
                ERectangleDirections dir = ERectangleDirections.None;
                List<ERectangleDirections> directions = grid.GetNeighbors(new Vector2I(x, y), true, false, true, false);

                //Remove Possible Walls
                if (directions.Count > 1)
                {
                    int rand = (int)(GD.Randi() % 2);

                    if (rand == 0)
                    {
                        dir = ERectangleDirections.North;
                    }
                    else
                    {
                        dir = ERectangleDirections.East;
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

                CarvePathIndex(ref grid, x, y, dir);
            }
        }

        return grid;
    }

    //East-North: Count Previous East Moves, When North Pick Randomly: No Mask Support
    public static Grid SidewinderAlgorithm(ref Grid grid)
    {
        GD.Randomize();

        //Create Maze
        for (int y = 0; y < grid.GetHeight(); y++)
        {
            int east_count = 0;

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                List<ERectangleDirections> directions = grid.GetNeighbors(new Vector2I(x, y), true, false, true, false);
                bool is_north = false;

                //Choose Direction
                if (directions.Count > 1)
                {
                    int num = (int)(GD.Randi() % 2);

                    if (num == 0)
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
                    CarvePathIndex(ref grid, x - rand, y, ERectangleDirections.North);
                    east_count = 0;

                }
                else if (directions.Contains(ERectangleDirections.East))
                {
                    east_count += 1;
                    CarvePathIndex(ref grid, x, y, ERectangleDirections.East);
                }
            }
        }

        return grid;
    }

    //Pick a Random point and goes in any random direction till all are visited
    public static Grid AldousBroderAlgorithm(ref Grid grid)
    {
        GD.Randomize();
        int visited_count = 1;

        Cell cell = grid.GetRandomCell();

        //Count DeadCells
        visited_count += grid.GetTotalDeadCells();

        //Carve Path
        while (!(visited_count >= (grid.GetWidth() * grid.GetHeight())))
        {
            //Get Valid Cell
            if (cell.dead_cell)
            {
                do
                {
                    cell = grid.GetRandomCell();
                } while (cell.dead_cell);
            }

            Cell next_cell = cell;
            ERectangleDirections move_dir = ERectangleDirections.None;
            List<ERectangleDirections> directions = grid.GetValidNeighbors(cell.index);

            //Get Direction
            if (directions.Count > 0)
            {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            //Get Next Cell
            if (move_dir != ERectangleDirections.None)
            {
                next_cell = grid.GetCellInDirection(cell.index, move_dir);
            }

            //Carve path
            if (directions.Count > 0 && !cell.dead_cell)
            {
                if (!next_cell.IsVisited())
                {
                    CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
                    visited_count += 1;
                }

                cell = next_cell;
            }
            else
            {
                cell = grid.GetRandomCell();
            }
        }

        return grid;
    }

    //Pick a Random point and go in any random direction till all are visited reset when a loop occurs
    public static Grid WilsonsAlgorithm(ref Grid grid)
    {
        GD.Randomize();

        int visited_count = 1;
        visited_count += grid.GetTotalDeadCells();
        Cell cell = grid.GetRandomCell();

        List<Cell> l_cell_index = new List<Cell>(); //Loop List
        Cell v_end_cell = grid.GetRandomCell();

        //Get valid cell
        while (v_end_cell.dead_cell)
        {
            v_end_cell = grid.GetRandomCell();
        }

        bool has_new_path = false;

        //Already Visited cells will not be modified
        while (!(visited_count >= (grid.GetWidth() * grid.GetHeight())))
        {
            //Get Valid Cell
            if (cell.dead_cell)
            {
                do
                {
                    cell = grid.GetRandomCell();
                } while (cell.dead_cell);
            }

            //Get Random Unvisited cell
            if (!has_new_path)
            {
                while (cell.IsVisited() || cell.IsSameCell(v_end_cell) || cell.dead_cell)
                {
                    cell = grid.GetRandomCell();
                }

                l_cell_index.Clear();
                l_cell_index.Add(cell);
                has_new_path = true;
            }

            //Starting Values: Get Direction
            Cell next_cell = cell;
            ERectangleDirections move_dir = ERectangleDirections.None;
            List<ERectangleDirections> directions = grid.GetValidNeighbors(cell.index);

            //Get Direction
            if (directions.Count > 0)
            {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            //Carve Path
            if (move_dir != ERectangleDirections.None)
            {
                next_cell = grid.GetCellInDirection(cell.index, move_dir);

                if (next_cell.IsVisited() || next_cell.IsSameCell(v_end_cell))
                {
                    l_cell_index.Add(next_cell);
                    CarvePathLoop(ref grid, ref l_cell_index);
                    visited_count += l_cell_index.Count() - 1;
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

        return grid;
    }

    //Random Walk, when closed in get a cell with at least 1 visited cell
    public static Grid HuntandKill(ref Grid grid)
    {
        GD.Randomize();
        int visited_count = 1;
        visited_count += grid.GetTotalDeadCells();

        Cell cell = grid.GetRandomCell();

        while (!(visited_count >= (grid.GetWidth() * grid.GetHeight())))
        {
            //Get Valid Cell
            if (cell.dead_cell)
            {
                do
                {
                    cell = grid.GetRandomCell();
                } while (cell.dead_cell);
            }

            Cell next_cell = cell;
            ERectangleDirections move_dir = ERectangleDirections.None;
            List<ERectangleDirections> directions = grid.GetValidUnvisitedNeighbors(cell.index);

            //Get Direction
            if (directions.Count > 0)
            { //Move
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
                next_cell = grid.GetCellInDirection(cell.index, move_dir);

                if (!next_cell.IsVisited())
                {
                    CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
                    visited_count += 1;
                    cell = next_cell;
                }

            }
            else
            {
                cell = Hunt(ref grid, cell);
                visited_count += 1;
            }
        }

        return grid;
    }

    public static Grid RecursiveBacktracker(ref Grid grid)
    {

        GD.Randomize();
        int visited_count = 1;
        Cell cell = grid.GetValidRandomCell();
        Stack<Cell> s_cells = new Stack<Cell>();

        s_cells.Push(cell);
        visited_count += grid.GetTotalDeadCells();

        while (!(visited_count >= (grid.GetWidth() * grid.GetHeight())))
        {

            Cell next_cell = cell;
            ERectangleDirections move_dir = ERectangleDirections.None;
            s_cells.Push(cell);

            //Check For Valid Adjacent Cells
            List<ERectangleDirections> directions = grid.GetValidUnvisitedNeighbors(cell.index);

            if (directions.Count > 0)
            {
                move_dir = directions[(int)(GD.Randi() % directions.Count)];
                next_cell = grid.GetCellInDirection(cell.index, move_dir);
            }

            //Carve path
            if (move_dir != ERectangleDirections.None)
            {
                if (!next_cell.IsVisited())
                {   //Not Visited
                    CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
                    visited_count += 1;
                    cell = next_cell;
                }
            }
            else
            {
                cell = Backtrack(ref grid, s_cells);
            }
        }

        return grid;
    }

    //No Mask Support
    public static Grid Ellers(ref Grid grid, bool is_loop)
    {
        GD.Randomize();
        int iteration = 1;
        int y_index = 0;

        List<List<Identifier>> cell_set_table = new List<List<Identifier>>();

        //Set Cell Table
        for (int y = 0; y < grid.GetHeight(); y++)
        {
            cell_set_table.Add(new List<Identifier>());

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                cell_set_table[y].Add(new Identifier(iteration));
                iteration += 1;
            }
        }

        //Loop
        while (y_index < grid.GetHeight())
        {

            //Link, Go South and Merge Sets
            if (y_index < grid.GetHeight() - 1)
            {
                Link(ref grid, ref cell_set_table, y_index, is_loop);    //Link Row
                RandomSouth(ref grid, ref cell_set_table, y_index, is_loop);    //Get South
            }
            else
            {    //Last 
                LinkLast(ref grid, ref cell_set_table);
            }

            y_index += 1;
        }

        return grid;
    }

    // 0 - Prim's (Random), 1 - Recursive Backtracker (Last), 2 - Mix
    public static Grid GrowingTree(ref Grid grid, int type = 0)
    {
        List<Cell> active_list = new List<Cell>();

        Cell temp_cell = null;

        do
        {
            temp_cell = grid.GetRandomCell();
        } while (temp_cell.dead_cell);

        active_list.Add(temp_cell);

        while (active_list.Count > 0)
        {
            int index = (int)(GD.Randi() % active_list.Count);  //Get a Active Cell
            Cell cell = active_list[index];
            List<ERectangleDirections> directions = grid.GetValidUnvisitedNeighbors(cell.index);
            ERectangleDirections move_dir = ERectangleDirections.None;

            //Random
            if (type == 0)
            {
                if (directions.Count > 0)
                {
                    move_dir = move_dir = directions[(int)(GD.Randi() % directions.Count)];
                }
            }

            //Last
            if (type == 1)
            {
                index = active_list.Count - 1;
                cell = active_list[index];

                directions = grid.GetValidUnvisitedNeighbors(cell.index);

                if (directions.Count > 0)
                {
                    move_dir = directions[(int)(GD.Randi() % directions.Count)];
                }

            }

            //Mixed
            if (type == 2)
            {
                if ((int)(GD.Randi() % 2) == 0)
                {
                    index = active_list.Count - 1;
                    cell = active_list[index];

                    directions = grid.GetValidUnvisitedNeighbors(cell.index);
                }

                if (directions.Count > 0)
                {
                    move_dir = directions[(int)(GD.Randi() % directions.Count)];
                }
            }

            //Carve path
            if (move_dir != ERectangleDirections.None)
            {
                Cell new_cell = CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);

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

        return grid;
    }

    public static Grid Kruskals(ref Grid grid)
    {
        int iteration = 1;
        List<List<int>> set = new List<List<int>>();

        //Populate sets
        for (int y = 0; y < grid.GetHeight(); y++)
        {
            set.Add(new List<int>());

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                if (!grid.cells[x, y].dead_cell)
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
            Cell cell = grid.GetValidRandomCell();
            List<ERectangleDirections> directions = grid.GetValidNeighbors(cell.index);

            ERectangleDirections move_dir = ERectangleDirections.None;

            if (directions.Count > 0)
            {
                move_dir = directions[(int)(GD.Randi() % directions.Count)];
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
                CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
            }
        }

        return grid;
    }

    public static Grid Prims_Simple(ref Grid grid)
    {
        List<Cell> active = new List<Cell>();
        active.Add(grid.GetValidRandomCell());

        while (active.Count > 0)
        {
            int index = (int)(GD.Randi() % active.Count);
            Cell cell = active[index];
            List<ERectangleDirections> directions = grid.GetValidUnvisitedNeighbors(cell.index);
            ERectangleDirections move_dir = ERectangleDirections.None;

            if (directions.Count > 0)
            {
                move_dir = directions[(int)(GD.Randi() % directions.Count)];
                active.Add(grid.GetCellInDirection(cell.index, move_dir));
                CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
            }
            else
            {
                active.RemoveAt(index);
            }
        }

        return grid;
    }

    public static Grid Prims_True(ref Grid grid)
    {
        List<List<int>> cell_cost = new List<List<int>>();
        List<Cell> active = new List<Cell>();

        //Get Starting Point
        active.Add(grid.GetValidRandomCell());

        //Populate Cell Cost List: 0 - 99
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            cell_cost.Add(new List<int>());

            for (int y = 0; y < grid.GetHeight(); y++)
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
            List<ERectangleDirections> directions = grid.GetValidUnvisitedNeighbors(lowest_cost_cell.index);
            Cell neighbor = active[0];
            Cell temp = null;

            //Get Lowest Cost Neighbor
            if (directions.Contains(ERectangleDirections.North))  //North
            {
                temp = grid.GetCellInDirection(lowest_cost_cell.index, ERectangleDirections.North);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = ERectangleDirections.North;
                    neighbor = temp;
                }
            }

            if (directions.Contains(ERectangleDirections.South))  //South
            {
                temp = grid.GetCellInDirection(lowest_cost_cell.index, ERectangleDirections.South);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = ERectangleDirections.South;
                    neighbor = temp;
                }
            }

            if (directions.Contains(ERectangleDirections.East))  //East
            {
                temp = grid.GetCellInDirection(lowest_cost_cell.index, ERectangleDirections.East);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = ERectangleDirections.East;
                    neighbor = temp;
                }
            }

            if (directions.Contains(ERectangleDirections.West))  //West
            {
                temp = grid.GetCellInDirection(lowest_cost_cell.index, ERectangleDirections.West);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = ERectangleDirections.West;
                    neighbor = temp;
                }
            }

            if (move_dir != ERectangleDirections.None)
            {
                active.Add(neighbor);
                CarvePathIndex(ref grid, lowest_cost_cell.index.X, lowest_cost_cell.index.Y, move_dir);
            }
            else
            {
                active.RemoveAt(index);
            }
        }

        return grid;
    }

    public static Grid GrowingForest(ref Grid _grid)
    {
        return _grid;
    }

    public static Grid Recursive_Division(ref Grid _grid)
    {

        return _grid;
    }



    //Carve Path Methods
    private static Cell CarvePathIndex(ref Grid _grid, int x, int y, ERectangleDirections direction)
    {
        Cell cell = _grid.cells[x, y];
        Vector2I index = cell.index;

        switch (direction)
        {
            case ERectangleDirections.North:
                _grid.cells[cell.index.X, cell.index.Y].north = true;
                _grid.cells[cell.index.X, cell.index.Y - 1].south = true;
                index = new Vector2I(cell.index.X, cell.index.Y - 1);
                break;

            case ERectangleDirections.South:
                _grid.cells[cell.index.X, cell.index.Y].south = true;
                _grid.cells[cell.index.X, cell.index.Y + 1].north = true;
                index = new Vector2I(cell.index.X, cell.index.Y + 1);
                break;

            case ERectangleDirections.East:
                _grid.cells[cell.index.X, cell.index.Y].east = true;
                _grid.cells[cell.index.X + 1, cell.index.Y].west = true;
                index = new Vector2I(cell.index.X + 1, cell.index.Y);
                break;

            case ERectangleDirections.West:
                _grid.cells[cell.index.X, cell.index.Y].west = true;
                _grid.cells[cell.index.X - 1, cell.index.Y].east = true;
                index = new Vector2I(cell.index.X - 1, cell.index.Y);
                break;
        }

        return _grid.cells[index.X, index.Y];
    }

    private static void CarvePathLoop(ref Grid _grid, ref List<Cell> cells)
    {
        for (int i = 0; i < cells.Count - 1; i++)
        {
            Vector2I direction = new Vector2I(cells[i + 1].index.X - cells[i].index.X, cells[i + 1].index.Y - cells[i].index.Y);

            if (direction.X != 0)
            {
                if (direction.X > 0)
                {  //East
                    _grid.cells[cells[i].index.X, cells[i].index.Y].east = true;
                    _grid.cells[cells[i].index.X + 1, cells[i].index.Y].west = true;
                }
                else
                {    //West
                    _grid.cells[cells[i].index.X, cells[i].index.Y].west = true;
                    _grid.cells[cells[i].index.X - 1, cells[i].index.Y].east = true;
                }
            }

            if (direction.Y != 0)
            {
                if (direction.Y > 0)
                {  //South
                    _grid.cells[cells[i].index.X, cells[i].index.Y].south = true;
                    _grid.cells[cells[i].index.X, cells[i].index.Y + 1].north = true;
                }
                else
                {    //North
                    _grid.cells[cells[i].index.X, cells[i].index.Y].north = true;
                    _grid.cells[cells[i].index.X, cells[i].index.Y - 1].south = true;
                }
            }
        }
    }

    //Helper Methods

    /* Used for Eller's Algorithm
     * Links Up the passed in Identifiers Horizontally based on the unique id of each cell
    */
    private static void Link(ref Grid _grid, ref List<List<Identifier>> list, int y_index, bool is_loop)
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
                    CarvePathIndex(ref _grid, i, y_index, ERectangleDirections.East);
                }
            }
        }
    }

    private static void LinkLast(ref Grid _grid, ref List<List<Identifier>> list)
    {
        int y_index = _grid.GetHeight() - 1;

        for (int i = 0; i < list[y_index].Count - 1; i++)
        {
            if (list[y_index][i].uid != list[y_index][i + 1].uid)
            {
                MergeSets(ref list, list[y_index][i].uid, list[y_index][i + 1].uid);
                CarvePathIndex(ref _grid, i, y_index, ERectangleDirections.East);
            }
        }
    }

    private static void RandomSouth(ref Grid _grid, ref List<List<Identifier>> list, int y_index, bool is_loop)
    {
        int count = 0;
        int number = list[y_index][0].uid;

        //Iterate Through Row
        for (int i = 0; i < list[0].Count(); i++)
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
                                Cell middle = _grid.cells[i - l, y_index];
                                Cell right = _grid.cells[i - l + 1, y_index];
                                Cell left = middle;

                                if (middle.index.X != 0)
                                {
                                    left = _grid.cells[i - l - 1, y_index];
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
                                    CarvePathIndex(ref _grid, i - l, y_index, ERectangleDirections.South);
                                }
                            }
                        }

                        if (!carved)
                        {
                            if (!_grid.cells[i - num, y_index].south)
                            { //Go South
                                MergeSets(ref list, number, list[y_index + 1][i - num].uid, true);
                                CarvePathIndex(ref _grid, i - num, y_index, ERectangleDirections.South);
                            }
                        }

                    }
                    else
                    {    //No Loop

                        while (south_count > 0)
                        {
                            num = (int)(GD.Randi() % count);

                            if (!_grid.cells[i - num, y_index].south)
                            { //Go South
                                MergeSets(ref list, number, list[y_index + 1][i - num].uid, true);
                                CarvePathIndex(ref _grid, i - num, y_index, ERectangleDirections.South);
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
                CarvePathIndex(ref _grid, i - num, y_index, ERectangleDirections.South);
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

    private static Cell Hunt(ref Grid grid, Cell new_cell)
    {
        List<ERectangleDirections> neighbors = new List<ERectangleDirections>();

        //Find a Cell with at least 1 adjacent visited cell
        for (int y = 0; y < grid.GetHeight(); y++)
        {
            neighbors.Clear();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                Cell temp_cell = null;
                new_cell = grid.cells[x, y];

                if (!new_cell.IsVisited() && !new_cell.dead_cell)
                {
                    if (new_cell.index.Y > 0)
                    {
                        temp_cell = grid.cells[new_cell.index.X, new_cell.index.Y - 1];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //North
                            neighbors.Add(ERectangleDirections.North);
                        }
                    }

                    if (new_cell.index.X > 0)
                    {
                        temp_cell = grid.cells[new_cell.index.X - 1, new_cell.index.Y];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //West
                            neighbors.Add(ERectangleDirections.West);
                        }
                    }

                    if (new_cell.index.Y < grid.GetHeight() - 1)
                    {
                        temp_cell = grid.cells[new_cell.index.X, new_cell.index.Y + 1];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //South
                            neighbors.Add(ERectangleDirections.South);
                        }
                    }

                    if (new_cell.index.X < grid.GetWidth() - 1)
                    {
                        temp_cell = grid.cells[new_cell.index.X + 1, new_cell.index.Y];
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
            CarvePathIndex(ref grid, new_cell.index.X, new_cell.index.Y, neighbors[new_dir]);
        }

        return new_cell;
    }

    private static Cell Backtrack(ref Grid grid, Stack<Cell> cells)
    {
        Cell target = null;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells.TryPop(out target))
            {

                List<ERectangleDirections> neighbors = grid.GetValidUnvisitedNeighbors(target.index);

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


//May Use: Determines sections and sets them up into their own grid: INCOMPLETE
//List<Grid> list = new List<Grid>();
//list.Add(grid)
//Create Grid using it's ranging index from min to max in both x and y coordinates
/*List<Vector2I> min_values = new List<Vector2I>();
List<Vector2I> max_values = new List<Vector2I>();

//Get Min, Max values for x and y
for (int i = 0; i < sections.Count; i++)
{
    min_values.Add(new Vector2I());
    max_values.Add(new Vector2I());

    for (int l = 0; l < sections[i].Count; l++)
    {
        Vector2I index = sections[i][l].index;

        if (l == 0) //Start Index
        {
            min_values[i] = index;
            max_values[i] = index;
        } else {
            //X and Y; Min Max values
            min_values[i] = new Vector2I(Mathf.Min(min_values[i].X, index.X), Mathf.Min(min_values[i].Y, index.Y));
            max_values[i] = new Vector2I(Mathf.Max(max_values[i].X, index.X), Mathf.Max(max_values[i].Y, index.Y));
        }
    }
}

List<Grid> grids = new List<Grid>();

//Create Grid 
for (int i = 0; i < min_values.Count; i++)
{
    int width = max_values[i].X - min_values[i].X;
    int height = max_values[i].Y - min_values[i].Y;
    grids.Add(new Grid(width, height, 1, 1));
}

//Set All Cells to Dead Cells
for (int i = 0; i < grids.Count; i++)
{
    for (int x = 0; x < grids[i].GetWidth(); x++)
    {
        for (int y = 0; y < grids[i].GetHeight(); y++)
        {
            grids[i].cells[x, y].dead_cell = true;
        }
    }
}

//Assign Corresponding Cells
for (int i = 0; i < grids.Count; i++)
{
    for (int l = 0; l < sections[i].Count; l++)
    {
        Cell assigned_cell = sections[i][l];
        //grids[i].cells[]

        grids[i].cells[l];
        //sections[i][l].index;
    }
}


//TODO: May need to make an entirely new cell



//Finish Setting up Sections
GD.Print("Sections: " + sections.Count.ToString());*/