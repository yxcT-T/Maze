using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
public class Maze_engine
{
    private SortedDictionary<double, Base_maze> mazes;
    public double user_ability; //度量用户的实力,调试完毕改为private
    private Base_maze this_maze; //当前呈现给用户的迷宫
    private const int START_ROW_NUM = 7, START_COL_NUM = 5;  //起始长宽
    private const int LIMIT_ROW_NUM = 45, LIMIT_COL_NUM = 35;  //最大长宽，理论上在win上超过55*53会堆栈溢出，安全和美观起见，设置小一些
    private const double DIFFICULTY_GROWTH_FACTOR = 1.16; //通关后的难度增长系数，这一超参数可以调节
    private const double DIFFICULTY_REDUCE_FACTOR = 0.75; //失败后的难度降低系数，这一超参数可以调节
    private const double BIGGER_MAP_ADAPTING_FACTOR = 0.8; //更大迷宫的难度适应系数，这一超参数可以调节。
    private int MAZE_COUNT = 80; //mazes中存储这么多个迷宫数组  这一值可以修改 这一值应当较大，一方面要考虑困难玩家的难度梯度较为平缓，另一方面也需要
    private int row_num, col_num;


    public Maze_engine(double _user_ability = 5)
    {
        row_num = START_ROW_NUM;
        col_num = START_COL_NUM;
        this.user_ability = _user_ability;
        generate_mazes();
    }
    public double get_calculated_difficulty()
    {
        return this_maze.calculated_difficulty;
    }
    public int[,] next_maze() //为用户生成下一关的迷宫，返回mazecell的二维数组
    {
        //是否需要加入新的迷宫增大逻辑？如连续3关后刷新？还是用户特异？
        if (mazes.Last().Key < user_ability) //现有迷宫集难度不足，增大难度
        {
            if (refresh_mazes(true)) //成功刷新迷宫集
            {
                user_ability *= BIGGER_MAP_ADAPTING_FACTOR; //使玩家适应新大小，降低迷宫难度
            }
            else //已经触碰到难度顶端
            {
                generate_mazes(); //刷新迷宫集,似乎在极端情况下会导致迷宫不再刷新（玩家能力大于所有游戏集）
                //日后加入其它难度机制
            }
        }
        bool has_change_maze = false; //在下面的foreach中是否选择了新迷宫
        foreach (KeyValuePair<double, Base_maze> kvp in mazes)
        {
            if (kvp.Key > user_ability)
            {
                this_maze = kvp.Value;
                has_change_maze = true;
                break;
            }
        }
        if (!has_change_maze) //没找到合适的迷宫
        {
            this_maze = mazes.Last().Value;
        }
        this_maze.display_in_console(); //调试用，待删除
        return this_maze.to_maze_graph();
    }
    public void finish_maze(double route_length, double total_time) //接口后期会变化，可能会引入其他信息
    {
        // refresh 用户实力
        user_ability = this_maze.calculated_difficulty * DIFFICULTY_GROWTH_FACTOR;
    }
    public void fail_maze() //点击投降按钮或退出游戏，视为游戏失败
    {
        user_ability *= DIFFICULTY_REDUCE_FACTOR; //降低难度
        refresh_mazes(false); //制造小一格的迷宫
    }
    private bool refresh_mazes(bool bigger_or_smaller)// 刷新迷宫，看是否需要更大或更小，初步计划三次通关同一难度迷宫则变化，放弃一次则变小
    {
        bool has_changed = true;
        if (bigger_or_smaller == true) //变大
        {
            row_num += 2;
            col_num += 2;
            if (row_num > LIMIT_ROW_NUM && col_num > LIMIT_COL_NUM) has_changed = false;
            row_num = Math.Min(row_num, LIMIT_ROW_NUM);
            col_num = Math.Min(col_num, LIMIT_COL_NUM);
        }
        else //变小
        {
            if (row_num > 2 && col_num > 2)  //防止出现小于2*2的迷宫
            {
                row_num -= 1;
                col_num -= 1;
            }
            else has_changed = false;
        }
        generate_mazes();
        return has_changed; //返回是否成功变更迷宫大小
    }
    private void generate_mazes()//生成迷宫
    {
        mazes = new SortedDictionary<double, Base_maze>(); //重构迷宫库
        for (int i = 0; i < MAZE_COUNT; ++i)
        {
            Base_maze m = new Base_maze(row_num, col_num);
            m.set_difficulty((double)i / MAZE_COUNT);
            m.create_maze();
            try
            {
                mazes.Add(m.get_total_difficulty(), m);
            }
            catch (ArgumentException)
            {
                //难度重复，假装无事发生过
            }

        }
    }
    /*public static void Main(String[] args)
    {
        Maze_engine mengine = new Maze_engine(10);
        for (int i = 0; i < 50; ++i)
        {
            System.Console.WriteLine("turn{0},ability{1}", i + 1, mengine.user_ability);
            System.Console.WriteLine("X{0},Y{1}", mengine.row_num, mengine.col_num);
            mengine.next_maze();
            mengine.finish_maze(0, 0);
            System.Console.WriteLine("");
            Thread.Sleep(100);
        }
        Thread.Sleep(2000000);
    }*/

}