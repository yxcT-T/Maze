using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator
{

    static public int[,] next_maze()
    {
        Base_maze mz = new Base_maze(4, 4);
        mz.set_difficulty(0);
        mz.create_maze();
        return mz.to_maze_graph();
    }
}
