using System;
using System.Threading;
public class Base_maze
{
    public int[,] maze;//迷宫数组 0 通路 1 墙 2 寻找出的路径点 3 起点 4 终点
    public int row_num, col_num, startX, startY, endX, endY, road_length, dir, last_direction;
    public int[] pathx, pathy;
    Random r = new Random();
    double[] possibility; //向各方向生成迷宫的概率
    double pos_rand, keep_rand, keep_factor = 0; //keep_factor是保持当前方向的概率
    public double calculated_difficulty; //计算出的难度
    bool has_find_route = false;

    public Base_maze(int r, int c)
    {
        row_num = r * 2 + 1;
        col_num = c * 2 + 1;
        maze = new int[row_num, col_num];
        pathx = new int[5000];
        pathy = new int[5000];
        possibility = new double[4];
    }
    public void create_maze()
    {
        for (int i = 0; i < row_num; ++i)
            for (int j = 0; j < col_num; ++j) maze[i, j] = 1;//初始化全部为墙
        maze[1, 0] = 3;//起点设置
        maze[row_num - 2, col_num - 1] = 4;//终点设置
        startX = 1; startY = 1; //用于dfs
        endX = row_num - 2; endY = col_num - 2;
        dfs_create_maze(endX, endY); //从终点开始逆向生成迷宫
        //System.Console.Write("Created\n");
        dfs_find_path(startX, startY); //开始寻路
        //System.Console.Write("Route Finded\n");
        for (int i = 0; i < road_length; i++)
            maze[pathx[i], pathy[i]] = 2;
    }
    public void dfs_create_maze(int x, int y)
    {
        maze[x, y] = 0; //放置道路  
        int[] t = { 0, 0, 0, 0 };// 已走方向标记  
        // 下方遍历四个方向 
        while (t[0] != 1 || t[1] != 1 || t[2] != 1 || t[3] != 1)
        {
            pos_rand = r.NextDouble();
            keep_rand = r.NextDouble();
            if (keep_rand < keep_factor)// 保持方向
            {
                t[last_direction] = 1;
                move(last_direction, x, y);
            }
            else //重新选择方向
            {
                dir = 0;
                if (pos_rand > possibility[0]) dir = 1;
                if (pos_rand > possibility[1]) dir = 2;
                if (pos_rand > possibility[2]) dir = 3;
                if (t[dir] != 1)
                {
                    t[dir] = 1;
                    move(dir, x, y);
                }
            }
        }
    } //随机遍历四个方向拆墙
    public void dfs_find_path(int x, int y) //搜索路径
    {
        if (!has_find_route)
        {
            //System.Console.Write("deepth = {0},x = {1},y={2}\n", road_length, x, y);
            // 放置路径  
            maze[x, y] = 2;
            pathx[road_length] = x;
            pathy[road_length] = y;
            road_length++;
            // 搜索到目的地  
            if (x == endX && y == endY)
            {
                has_find_route = true;
                return;
            }
            else  // 搜索并回溯  
            {
                if (x + 1 < row_num && maze[x + 1, y] == 0) { dfs_find_path(x + 1, y); if (has_find_route) return; maze[x + 1, y] = 0; road_length--; }
                if (x > 1 && maze[x - 1, y] == 0) { dfs_find_path(x - 1, y); if (has_find_route) return; maze[x - 1, y] = 0; road_length--; }
                if (y + 1 < col_num && maze[x, y + 1] == 0) { dfs_find_path(x, y + 1); if (has_find_route) return; maze[x, y + 1] = 0; road_length--; }
                if (y > 1 && maze[x, y - 1] == 0) { dfs_find_path(x, y - 1); if (has_find_route) return; maze[x, y - 1] = 0; road_length--; }
            }
        }
    }
    public void move(int direction, int x, int y)
    {
        //System.Console.Write("dir = {0},x = {1},y={2}\n", direction,x,y);
        switch (direction)
        {
            case 0:
                {// 向左 
                    if (y - 2 > 0 && maze[x, y - 2] > 0 && maze[x - 1, y - 2] > 0 && maze[x + 1, y - 2] > 0)
                    {
                        maze[x, y - 2] = 0;
                        maze[x, y - 1] = 0;
                        dfs_create_maze(x, y - 2);
                        last_direction = 0;
                    }
                    break;
                }
            case 1:
                {// 向右  
                    if (y + 2 < col_num && maze[x, y + 2] > 0 && maze[x - 1, y + 2] > 0 && maze[x + 1, y + 2] > 0)
                    {
                        maze[x, y + 2] = 0;
                        maze[x, y + 1] = 0;
                        dfs_create_maze(x, y + 2);
                        last_direction = 1;
                    }
                    break;
                }
            case 2:
                { // 向上
                    if (x - 2 > 0 && maze[x - 2, y] > 0 && maze[x - 2, y + 1] > 0 && maze[x - 2, y - 1] > 0)
                    {
                        maze[x - 1, y] = 0;
                        maze[x - 2, y] = 0;
                        dfs_create_maze(x - 2, y);
                        last_direction = 2;
                    }
                    break;
                }
            case 3:
                {// 向下
                    if (x + 2 < row_num && maze[x + 2, y] > 0 && maze[x + 2, y + 1] > 0 && maze[x + 2, y + 1] > 0)
                    {
                        maze[x + 1, y] = 0;
                        maze[x + 2, y] = 0;
                        dfs_create_maze(x + 2, y);
                        last_direction = 3;
                    }
                    break;
                }
        }
    } //用于生成迷宫的拆墙算法
    public void set_difficulty(double difficulty) //difficulty从0-1,   0最简单,1最难
    {
        double easy_index = 1.0 - difficulty;
        if (easy_index > 0.3) keep_factor = (easy_index - 0.3) * 1.3;
        else easy_index = 0;
        possibility[0] = 0.25 + 0.24 * easy_index;
        possibility[1] = 0.25 - 0.24 * easy_index;
        possibility[2] = 0.25 + 0.24 * easy_index;
        possibility[3] = 0.25 - 0.24 * easy_index;
        possibility[1] += possibility[0];
        possibility[2] += possibility[1];
        possibility[3] += possibility[2];
    }
    public void display_in_console()
    {
        for (int i = 0; i < row_num; ++i)
        {
            for (int j = 0; j < col_num; ++j) switch (maze[i, j])
                {
                    case 0:
                        System.Console.Write('□');
                        break;
                    case 1:
                        System.Console.Write('■');
                        break;
                    case 2:
                        System.Console.Write('☆');
                        break;
                    case 3:
                        System.Console.Write("S ");
                        break;
                    case 4:
                        System.Console.Write("E ");
                        break;
                }
            System.Console.Write('\n');
        }

    }
    public double get_total_difficulty() //计算该迷宫总难度分,需要迭代更新根据分支数计算
    {
        int direction_changes = 0, last_direction, this_direction;
        last_direction = 2 * (pathx[1] - pathx[0]) + pathy[1] - pathy[0];
        for (int i = 2; i < road_length; ++i)
        {
            this_direction = 2 * (pathx[i] - pathx[i - 1]) + pathy[i] - pathy[i - 1];
            if (this_direction != last_direction)
            {
                direction_changes++;
                //需要加入判断分支的逻辑
                last_direction = this_direction;
            }
        }
        //System.Console.Write("length = {0},dir_change = {1}\n", road_length, direction_changes);
        calculated_difficulty = System.Math.Log10(road_length) * direction_changes;
        return calculated_difficulty;
    }
    public uint hash() //保证迷宫唯一性，采用BKDRHash算法
    {
        uint hash = 5381;
        for (int i = 0; i < row_num; ++i)
            for (int j = 0; j < col_num; ++j)
            {
                hash += (hash << 5) + (uint)maze[i, j];
            }
        return (hash & 0x7FFFFFFFu);
    }
    public int[] get_hint(int now_X, int now_Y, int hint_length) //获得提示,还没写完
    {
        int[] hint = new int[hint_length];
        road_length = 0; //清空两个路径数组，假如未来仍然需要起点到终点的路径，则进行重构
        dfs_find_path(now_X, now_Y);
        return hint;
    }
    public int[,] to_maze_graph()
    {   // 左下角是(0,0)右上角是（n，m）
        // 0bit向上 1bit向右 2bit向下 3bit向左
        int real_row_num = (row_num - 1) / 2; //真实的迷宫高度
        int real_col_num = (col_num - 1) / 2; //真实的迷宫宽度
        int[,] graph = new int[real_row_num, real_col_num];
        for (int i = 0; i < real_row_num; ++i)
            for (int j = 0; j < real_col_num; ++j) graph[i, j] = 15;


        for (int i = 1, ii = 0; i < row_num; i += 2, ii += 1)
            for (int j = 1, jj = 0; j < col_num; j += 2, jj += 1)
            {

                if (maze[i, j + 1] != 1 && ((graph[ii, jj] & 1) == 1)) graph[ii, jj] -= 1;
                if (maze[i + 1, j] != 1 && ((graph[ii, jj] & 2) == 2)) graph[ii, jj] -= 2;
                if (maze[i, j - 1] != 1 && ((graph[ii, jj] & 4) == 4)) graph[ii, jj] -= 4;
                if (maze[i - 1, j] != 1 && ((graph[ii, jj] & 8) == 8)) graph[ii, jj] -= 8;
            }
        graph[0, 0] += 4;
        graph[real_row_num - 1, real_col_num - 1] += 1;//封起始和终止点的外墙
        return graph;
    }
    /*public static void Main(String[] args)
    {
        System.Console.Write("Start building...\n");
        Maze engine = new Maze(3,3); //构造长宽为指定值的迷宫
        for (int i = 0; i < 10; ++i)
        {
            engine = new Maze(2, 2);
            engine.set_difficulty(i/50.0); //设置难度
            System.Console.WriteLine(i / 50.0);
            engine.create_maze();
            System.Console.WriteLine(engine.get_total_difficulty());
            engine.display_in_console();
        }       
        Thread.Sleep(100000); //防止关屏
    }*/


}