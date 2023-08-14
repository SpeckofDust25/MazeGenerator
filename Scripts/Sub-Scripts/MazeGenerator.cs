using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class MazeGenerator
{
    public enum Direction { none, north, south, east, west }

    //Move North or East on each cell: No Mask Support
    public static Grid BinaryTreeAlgorithm(ref Grid grid)
    {
        GD.Randomize();

        //Create Maze 
        for (int x = 0; x < grid.GetWidth(); ++x)
        {
            for (int y = 0; y < grid.GetHeight(); ++y)
            {
                Grid.Direction dir = Grid.Direction.none;
                List<Grid.Direction> directions = grid.GetNeighbors(new Vector2I(x, y), true, false, true, false);

                //Remove Possible Walls
                if (directions.Count > 1) {
                    int rand = (int)(GD.Randi() % 2);

                    if (rand == 0) { 
                        dir = Grid.Direction.north;
                    } else {
                        dir = Grid.Direction.east;
                    }

                } else if (directions.Contains(Grid.Direction.north)) {
                    dir = Grid.Direction.north;

                } else if (directions.Contains(Grid.Direction.east)) {
                    dir = Grid.Direction.east;
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
                List<Grid.Direction> directions = grid.GetNeighbors(new Vector2I(x, y), true, false, true, false);
                bool is_north = false;
                
                //Choose Direction
                if (directions.Count > 1) {
                    int num = (int)(GD.Randi() % 2);
                    
                    if (num == 0) {
                        is_north = true;
                    }

                } else if (directions.Contains(Grid.Direction.north)) {
                    is_north = true;
                }

                //Carve Path
                if (is_north) {
                    int rand = 0;
                    if (east_count > 0) { rand = (int)(GD.Randi() % east_count); }
                    CarvePathIndex(ref grid, x - rand, y, Grid.Direction.north);
                    east_count = 0;

                } else if (directions.Contains(Grid.Direction.east)) {
                    east_count += 1;
                    CarvePathIndex(ref grid, x, y, Grid.Direction.east);
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
                do {
                    cell = grid.GetRandomCell();
                } while (cell.dead_cell);
            }

            Cell next_cell = cell;
            Grid.Direction move_dir = Grid.Direction.none;
            List<Grid.Direction> directions = grid.GetValidNeighbors(cell.index);

            //Get Direction
            if (directions.Count > 0)
            {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            //Get Next Cell
            if (move_dir != Grid.Direction.none)
            {
                next_cell = grid.GetCellInDirection(cell.index, move_dir);
            }

            //Carve path
            if (directions.Count > 0 && !cell.dead_cell) {
                if (!next_cell.IsVisited()) {
                    CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
                    visited_count += 1;
                }

                cell = next_cell;
            } else {
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
            Grid.Direction move_dir = Grid.Direction.none;
            List<Grid.Direction> directions = grid.GetValidNeighbors(cell.index);

            //Get Direction
            if (directions.Count > 0)
            {
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
            }

            //Carve Path
            if (move_dir != Grid.Direction.none)
            {
                next_cell = grid.GetCellInDirection(cell.index, move_dir);

                if (next_cell.IsVisited() || next_cell.IsSameCell(v_end_cell)) 
                {
                    l_cell_index.Add(next_cell);
                    CarvePathLoop(ref grid, ref l_cell_index);
                    visited_count += l_cell_index.Count() - 1;
                    has_new_path = false;

                } else {    //Check For Loop
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

        int count = 0;

        while (!(visited_count >= (grid.GetWidth() * grid.GetHeight())))
        {
            if (count > 10000) { break; }
            count += 1;

            //Get Valid Cell
            if (cell.dead_cell) {
                do {
                    cell = grid.GetRandomCell();
                } while (cell.dead_cell);
            }

            Cell next_cell = cell;
            Grid.Direction move_dir = Grid.Direction.none;
            List<Grid.Direction> directions = grid.GetValidUnvisitedNeighbors(cell.index);

            //Get Direction
            if (directions.Count > 0) { //Move
                int num = (int)(GD.Randi() % directions.Count);
                move_dir = directions[num];
                next_cell = grid.GetCellInDirection(cell.index, move_dir);

                if (!next_cell.IsVisited()) {
                    CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
                    visited_count += 1;
                    cell = next_cell;
                }

            } else {
                cell = Hunt(ref grid, cell);
                visited_count += 1;
            }
        }
        
        return grid;
    }

    public static Grid RecursiveBacktracker(ref Grid grid) {
        
        GD.Randomize();
        int visited_count = 1;
        Cell cell = grid.GetValidRandomCell();
        Stack<Cell> s_cells = new Stack<Cell>();
        visited_count += grid.GetTotalDeadCells();

        while (!(visited_count >= (grid.GetWidth() * grid.GetHeight())))
        {
            Cell next_cell = cell;
            Grid.Direction move_dir = Grid.Direction.none;
            s_cells.Push(cell);

            //Check For Valid Adjacent Cells
            List<Grid.Direction> directions = grid.GetValidUnvisitedNeighbors(cell.index);

            if (directions.Count > 0)
            {
                move_dir = directions[(int)(GD.Randi() % directions.Count)];
                next_cell = grid.GetCellInDirection(cell.index, move_dir);
            }

            //Carve path
            if (move_dir != Grid.Direction.none)
            {
                if (!next_cell.IsVisited()) {   //Not Visited
                    CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
                    visited_count += 1;
                    cell = next_cell;
                }
            } else {
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
        for (int y = 0; y < grid.GetHeight(); y++) {
            cell_set_table.Add(new List<Identifier>());

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                cell_set_table[y].Add(new Identifier(iteration));
                iteration += 1;
            }
        }

        //Loop
        while (y_index < grid.GetHeight()) {

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

        do {
            temp_cell = grid.GetRandomCell();
        } while (temp_cell.dead_cell);

        active_list.Add(temp_cell);

        while(active_list.Count > 0)
        {
            int index = (int)(GD.Randi() % active_list.Count);  //Get a Active Cell
            Cell cell = active_list[index];
            List<Grid.Direction> directions = grid.GetValidUnvisitedNeighbors(cell.index);
            Grid.Direction move_dir = Grid.Direction.none;

            //Random
            if (type == 0)
            {
                if (directions.Count > 0)
                {
                    move_dir = move_dir = directions[(int)(GD.Randi() % directions.Count)];
                }
            }

            //Last
            if (type == 1) {    
                index = active_list.Count - 1;
                cell = active_list[index];

                directions = grid.GetValidUnvisitedNeighbors(cell.index);

                if (directions.Count > 0)
                {
                    move_dir = directions[(int)(GD.Randi() % directions.Count)];
                }

            }

            //Mixed
            if (type == 2) { 
                if ((int)(GD.Randi() % 2) == 0) {
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
            if (move_dir != Grid.Direction.none)
            {
                Cell new_cell = CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);

                if (!new_cell.IsSameCell(cell))
                {
                    active_list.Add(new_cell);
                }
            } else { //Remove path
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
        for (int y = 0; y < grid.GetHeight(); y++) {
            set.Add(new List<int>());

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                if (!grid.cells[x, y].dead_cell) {
                    set[y].Add(iteration);
                    iteration += 1;
                } else {
                    set[y].Add(-1);
                }
            }
        }

        while (!SetComplete(set))
        {
            Cell cell = grid.GetValidRandomCell();
            List<Grid.Direction> directions = grid.GetValidNeighbors(cell.index);

            Grid.Direction move_dir = Grid.Direction.none;

            if (directions.Count > 0)
            {
                move_dir = directions[(int)(GD.Randi() % directions.Count)];
            }

            int first = set[cell.index.Y][cell.index.X];
            int second = 0;

            if (move_dir == Grid.Direction.none)  //No Usable Adjacent Cell
            {
                continue;
            }

            switch (move_dir)
            {
                case Grid.Direction.north:
                    second = set[cell.index.Y - 1][cell.index.X];
                    break;

                case Grid.Direction.south:
                    second = set[cell.index.Y + 1][cell.index.X];
                    break;

                case Grid.Direction.east:
                    second = set[cell.index.Y][cell.index.X + 1];
                    break;

                case Grid.Direction.west:
                    second = set[cell.index.Y][cell.index.X - 1];
                    break;
            }

            if (first != second) {
                MergeSetManual(ref set, first, second);
                CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
            }
        }

        return grid;
    }

    public static Grid Prims_Simple(ref Grid grid) {
        
        List<Cell> active = new List<Cell>();
        active.Add(grid.GetValidRandomCell());

        while(active.Count > 0)
        {
            int index = (int)(GD.Randi() % active.Count);
            Cell cell = active[index];
            List<Grid.Direction> directions = grid.GetValidUnvisitedNeighbors(cell.index);
            Grid.Direction move_dir = Grid.Direction.none;

            if (directions.Count > 0) {
                move_dir = directions[(int)(GD.Randi() % directions.Count)];
                active.Add(grid.GetCellInDirection(cell.index, move_dir));
                CarvePathIndex(ref grid, cell.index.X, cell.index.Y, move_dir);
            } else {
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

            for (int y = 0; y < grid.GetHeight(); y++) {
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
            Grid.Direction move_dir = Grid.Direction.none;
            List<Grid.Direction> directions = grid.GetValidUnvisitedNeighbors(lowest_cost_cell.index);
            Cell neighbor = active[0];
            Cell temp = null;

            //Get Lowest Cost Neighbor
            if (directions.Contains(Grid.Direction.north))  //North
            {
                temp = grid.GetCellInDirection(lowest_cost_cell.index, Grid.Direction.north);
                
                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = Grid.Direction.north;
                    neighbor = temp;
                }
            }

            if (directions.Contains(Grid.Direction.south))  //South
            {
                temp = grid.GetCellInDirection(lowest_cost_cell.index, Grid.Direction.south);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = Grid.Direction.south;
                    neighbor = temp;
                }
            }

            if (directions.Contains(Grid.Direction.east))  //East
            {
                temp = grid.GetCellInDirection(lowest_cost_cell.index, Grid.Direction.east);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = Grid.Direction.east;
                    neighbor = temp;
                }
            }

            if (directions.Contains(Grid.Direction.west))  //West
            {
                temp = grid.GetCellInDirection(lowest_cost_cell.index, Grid.Direction.west);

                if (cell_cost[temp.index.X][temp.index.Y] < lowest)
                {
                    move_dir = Grid.Direction.west;
                    neighbor = temp;
                }
            }

            if (move_dir != Grid.Direction.none) {
                active.Add(neighbor);
                CarvePathIndex(ref grid, lowest_cost_cell.index.X, lowest_cost_cell.index.Y, move_dir);
            } else {
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
    private static Cell CarvePathIndex(ref Grid _grid, int x, int y, Grid.Direction direction)
    {
        Cell cell = _grid.cells[x, y];
        Vector2I index = cell.index;

        switch (direction)
        {
            case Grid.Direction.north:
                _grid.cells[cell.index.X, cell.index.Y].north = true;
                _grid.cells[cell.index.X, cell.index.Y - 1].south = true;
                index = new Vector2I(cell.index.X, cell.index.Y - 1);
                break;

            case Grid.Direction.south:
                _grid.cells[cell.index.X, cell.index.Y].south = true;
                _grid.cells[cell.index.X, cell.index.Y + 1].north = true;
                index = new Vector2I(cell.index.X, cell.index.Y + 1);
                break;

            case Grid.Direction.east:
                _grid.cells[cell.index.X, cell.index.Y].east = true;
                _grid.cells[cell.index.X + 1, cell.index.Y].west = true;
                index = new Vector2I(cell.index.X + 1, cell.index.Y);
                break;

            case Grid.Direction.west:
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
                if (is_loop || list[y_index][i].uid != list[y_index][i + 1].uid) {
                    MergeSets(ref list, list[y_index][i].uid, list[y_index][i + 1].uid);
                    CarvePathIndex(ref _grid, i, y_index, Grid.Direction.east);
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
                CarvePathIndex(ref _grid, i, y_index, Grid.Direction.east);
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
            if (i < list[y_index].Count - 1) {  //Not At End
                if (list[y_index][i + 1].uid != number)
                {
                    if (is_loop) {
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
                                    CarvePathIndex(ref _grid, i - l, y_index, Grid.Direction.south);
                                }
                            }
                        }

                        if (!carved)
                        {
                            if (!_grid.cells[i - num, y_index].south)
                            { //Go South
                                MergeSets(ref list, number, list[y_index + 1][i - num].uid, true);
                                CarvePathIndex(ref _grid, i - num, y_index, Grid.Direction.south);
                            }
                        }

                    } else {    //No Loop

                        while (south_count > 0) {
                            num = (int)(GD.Randi() % count);

                            if (!_grid.cells[i - num, y_index].south) { //Go South
                                MergeSets(ref list, number, list[y_index + 1][i - num].uid, true);
                                CarvePathIndex(ref _grid, i - num, y_index, Grid.Direction.south);
                                south_count -= 1;
                            }
                        }
                    }
                    
                    number = list[y_index][i + 1].uid;
                    count = 0;
                }
            } else { //At End of Row
                MergeSets(ref list, number, list[y_index + 1][i - num].uid, true);
                CarvePathIndex(ref _grid, i - num, y_index, Grid.Direction.south);
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
                if (set[y][x] == second) {
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
    
    private static bool SetComplete(List<List<int>> set) {
        bool complete = true;
        int number = -1;

        for (int y = 0; y < set.Count; y++)
        {
            for (int x = 0; x < set[y].Count; x++)
            {
                if (set[y][x] >= 0) { //Skip -1
                    if (number == -1) {
                        number = set[y][x];
                    } else if (number != set[y][x]) {
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
        List<Grid.Direction> neighbors = new List<Grid.Direction>();

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
                            neighbors.Add(Grid.Direction.north);
                        }
                    }

                    if (new_cell.index.X > 0)
                    {
                        temp_cell = grid.cells[new_cell.index.X - 1, new_cell.index.Y];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //West
                            neighbors.Add(Grid.Direction.west);
                        }
                    }

                    if (new_cell.index.Y < grid.GetHeight() - 1)
                    {
                        temp_cell = grid.cells[new_cell.index.X, new_cell.index.Y + 1];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //South
                            neighbors.Add(Grid.Direction.south);
                        }
                    }

                    if (new_cell.index.X < grid.GetWidth() - 1)
                    {
                        temp_cell = grid.cells[new_cell.index.X + 1, new_cell.index.Y];
                        if (temp_cell.IsVisited() && !temp_cell.dead_cell)
                        {  //East
                            neighbors.Add(Grid.Direction.east);
                        }
                    }
                }

                if (neighbors.Count > 0) { break; }
            }

            if (neighbors.Count > 0) { break; }
        }

        //Pick Random Cell
        if (neighbors.Count > 0) {
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
            if (cells.TryPop(out target)) {

                List<Grid.Direction> neighbors = grid.GetValidUnvisitedNeighbors(target.index);

                if (neighbors.Count > 0)
                {
                    break;
                }

            } else {
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
