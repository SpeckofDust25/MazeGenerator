
namespace MazeGeneratorGlobal
{

    //Maze Type
    enum EMazeType
    {
        BinaryTree, Sidewinder, Aldous_Broder, Wilsons, HuntandKill, Recursive_Backtracker, Ellers, Ellers_Loop,
        GrowingTree_Random, GrowingTree_Last, GrowingTree_Mix, Kruskals_Random, Prims_Simple, Prims_True, GrowingForest, Recursive_Division
    }

    //Points
    public enum EPoints { None, Open }

    //Directions
    public enum ERectangleDirections { None, North, South, East, West } 
}
