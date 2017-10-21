using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//管理整个迷宫
public class Maze : MonoBehaviour {

    public MyCanvas canvas;//UI幕布

    public IntVector2 size;//地图长宽

    public MazeCell cellPrefab;
    public MazePassage passagePrefab;
    public MazeWall[] wallPrefabs;
    public MazeDoor doorPrefab;

    [Range(0f, 1f)]
    public float doorProbability;//渲染门的概率

    private MazeCell[,] cells;//整个地图

    public float generationStepDelay;

    public MazeRoomSettings[] roomSettings;

    private List<MazeRoom> rooms = new List<MazeRoom>();

    [Range(0f, 1f)]
    public float difficulty;//难度
    private float[] dirProbability;//走四个方向的概率
    private float keepProbability;//直走概率
    static private int[] dx = new int[] { 0, 1, 0, -1 };
    static private int[] dy = new int[] { 1, 0, -1, 0 };

    public Box winPrefab;
    private Box winInstance;

    public WinFlag winFlagPrefab;
    private WinFlag winFlagInstance;

    private int[,] _graph;

    private float durationTime;

    private void Awake()
    {
        //从UI获取信息，并显示
        canvas = GameObject.Find("Canvas").GetComponent<MyCanvas>();

        difficulty = canvas.difficultSlider.value;
        size = new IntVector2((int)canvas.sizeXSlider.value, (int)canvas.sizeYSlider.value);

        canvas.difficultText.text = "Difficult: " + difficulty.ToString("F2");
        canvas.sizeXText.text = "SizeX: " + size.x.ToString();
        canvas.sizeYText.text = "SizeY: " + size.z.ToString();

        canvas.difficultSlider.gameObject.SetActive(false);
        canvas.sizeXSlider.gameObject.SetActive(false);
        canvas.sizeYSlider.gameObject.SetActive(false);
        canvas.difficultText.gameObject.SetActive(false);
        canvas.sizeXText.gameObject.SetActive(false);
        canvas.sizeYSlider.gameObject.SetActive(false);
    }


    private void Update()
    {
        durationTime += Time.deltaTime;
    }

    public float GetDurationTime
    {
        get
        {
            return durationTime;
        }
    }

    public void DrawGraph(Maze_engine mazeEngine)
    {
        int[,] graph = mazeEngine.next_maze();
        //int[,] graph = MazeGenerator.next_maze();
        //int[,] graph = _graph;
        size.x = graph.GetLength(0);
        size.z = graph.GetLength(1);
        cells = new MazeCell[size.x, size.z];
        MazeRoom newRoom = CreateRoom(-1);
        for (int i = 0; i < size.x; i ++ )
            for (int j = 0; j < size.z; j ++ )
            {
                MazeCell newCell = CreateCell(new IntVector2(i, j));
                newCell.Initialize(newRoom);
            }
        for (int i = 0; i < size.x; i ++ )
            for (int j = 0; j < size.z; j ++ )
            {
                MazeCell cell = cells[i, j];
                for (int k = 0; k < 4; k ++ )
                {
                    int x = i + dx[k], y = j + dy[k];
                    MazeCell otherCell = null;
                    if (ContainsCoordinates(new IntVector2(x, y)))
                        otherCell = cells[x, y];
                    if ((graph[i,j] >> k & 1) == 1)
                    {
                        MazeWall wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
                        wall.Initialize(cell, otherCell, (MazeDirection)k);
                    }
                    else
                    {
                        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
                        passage.Initialize(cell, otherCell, (MazeDirection)k);
                    }
                }
            }

        winInstance = Instantiate(winPrefab) as Box;
        winInstance.name = "Win Box";
        winInstance.transform.parent = transform;
        winInstance.transform.localPosition = new Vector3(size.x * 0.5f, 0.5f, size.z * 0.5f);

        winFlagInstance = Instantiate(winFlagPrefab) as WinFlag;
        winFlagInstance.name = "Win Flag";
        winFlagInstance.transform.parent = transform;
        winFlagInstance.transform.localPosition = new Vector3(size.x * 0.5f - 0.5f, 0f, size.z * 0.5f - 0.5f);

        durationTime = 0;
    }

    public void Win(int mission)
    {
        canvas.winText.text = "Mission " + mission.ToString();
    }

    //创建一个房间，房间定义为用门隔开的连通块
    private MazeRoom CreateRoom(int indexToExclude)
    {
        MazeRoom newRoom = ScriptableObject.CreateInstance<MazeRoom>();
        newRoom.settingsIndex = Random.Range(0, roomSettings.Length);
        if (newRoom.settingsIndex == indexToExclude)
        {
            newRoom.settingsIndex = (newRoom.settingsIndex + 1) % roomSettings.Length;
        }
        newRoom.settings = roomSettings[newRoom.settingsIndex];
        rooms.Add(newRoom);
        return newRoom;
    }

    public MazeCell GetCell(IntVector2 coordinates)
    {
        return cells[coordinates.x, coordinates.z];
    }

    //根据难度设立各种概率
    private void SetDifficulty()
    {
        keepProbability = 0.75f * (1 - difficulty);
        dirProbability = new float[4];
        dirProbability[0] = 0.25f + 0.24f * (1 - difficulty);
        dirProbability[1] = 0.25f + 0.24f * (1 - difficulty);
        dirProbability[2] = 0.25f - 0.24f * (1 - difficulty);
        dirProbability[3] = 0.25f - 0.24f * (1 - difficulty);

        for (int i = 1; i < 3; i++)
            dirProbability[i] += dirProbability[i - 1];
    }

    //向某个方向移动一步，如果可以，则深度优先搜索
    private void Move(IntVector2 coor, int dir)
    {
        IntVector2 pos = new IntVector2(coor.x + dx[dir], coor.z + dy[dir]);
        if (pos.x >= 0 && pos.x < size.x && pos.z >= 0 && pos.z < size.z)
        {
            if (_graph[pos.x, pos.z] == -1)
            {
                _graph[pos.x, pos.z] = 0;
                dfs_maze(pos, dir);
            }
            else
            {
                _graph[coor.x, coor.z] += 1 << dir;
            }
        }
        else
        {
            _graph[coor.x, coor.z] += 1 << dir;
        }
    }

    //递归生成迷宫
    private void dfs_maze(IntVector2 coor, int lastDir)
    {
        bool[] used = new bool[4]{ true, true, true, true };
        used[lastDir ^ 2] = false;
        while (true)
        {
            bool flag = false;
            for (int i = 0; i < 4; i++) flag |= used[i];
            if (!flag) break;
            if (Random.value < keepProbability)
            {
                if (used[lastDir])
                {
                    used[lastDir] = false;
                    Move(coor, lastDir);
                }
            }
            else
            {
                float prob = Random.value;
                int dir = 0;
                for (int i = 0; i < 3; i++)
                    if (prob > dirProbability[i])
                        dir = i + 1;
                if (used[dir])
                {
                    used[dir] = false;
                    Move(coor, dir);
                }
            }
        }
    }

    //用家栋的方法创建迷宫
    public void GenerateAlgorithm(Maze_engine mazeEngine)
    {
        _graph = new int[size.x, size.z];
        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.z; j++)
                _graph[i, j] = -1;
        SetDifficulty();
        int dir = Random.Range(0, 2);
        _graph[0, 0] = (1 << (dir^2));
        dfs_maze(new IntVector2(0, 0), dir);

        DrawGraph(mazeEngine);
    }

    //生成第一人称地图
    public void Generate()
    {
        cells = new MazeCell[size.x, size.z];
        List<MazeCell> activeCells = new List<MazeCell>();
        DoFirstGenerationStep(activeCells);
        //类似Prime算法，维护更新集合，每次选末尾元素，枚举各种方向，如果未遍历，加入更新集合；否则如果各个方向都已遍历，则pop
        while(activeCells.Count > 0)
        {
            DoNextGenerationStep(activeCells);
        }
        for (int i = 0; i < rooms.Count; i++)
        {
            //rooms[i].Hide();
        }
    }

    //渲染一个空格
    public MazeCell CreateCell(IntVector2 coordinates)
    {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        cells[coordinates.x, coordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
        return newCell;
    }

    //在地图中随机一个坐标
    public IntVector2 RandomCoordinates
    {
        get
        {
            return new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
        }
    }

    //判断坐标是否在地图中
    private bool ContainsCoordinates(IntVector2 coordinate)
    {
        return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
    }

    //第一人称地图初始化，随机选择初始点
    private void DoFirstGenerationStep(List<MazeCell> activeCells)
    {
        MazeCell newCell = CreateCell(RandomCoordinates);
        newCell.Initialize(CreateRoom(-1));
        activeCells.Add(newCell);
    }
    //第一人称地图每步迭代
    private void DoNextGenerationStep(List<MazeCell> activeCells)
    {
        int currentIndex = activeCells.Count - 1;
        MazeCell currentCell = activeCells[currentIndex];
        if (currentCell.IsFullyInitialized)
        {
            activeCells.RemoveAt(currentIndex);
            return;
        }
        MazeDirection direction = currentCell.RandomUninitializedDirection;
        IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
        if (ContainsCoordinates(coordinates))
        {
            MazeCell neighbor = GetCell(coordinates);
            if (neighbor == null)
                //如果未遍历，则添加一条通路
            {
                neighbor = CreateCell(coordinates);
                CreatePassage(currentCell, neighbor, direction);
                activeCells.Add(neighbor);
            }
            else if (currentCell.room.settingsIndex == neighbor.room.settingsIndex)
                //如果两侧属于同一房间，则合并两个room集合
            {
                 CreatePassageInSameRoom(currentCell, neighbor, direction);
            }
            else
                //如果已遍历，则添加一堵墙
            {
                CreateWall(currentCell, neighbor, direction);
            }
        }
        else
        //如果处于边界，则添加一堵墙
        {
            CreateWall(currentCell, null, direction);
        }
    }

    private void CreatePassageInSameRoom(MazeCell cell, MazeCell otherCell, MazeDirection direction)
        //添加一条通路，并合并两侧格子所在的room集合
    {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
        if (cell.room != otherCell.room)
        {
            MazeRoom roomToAssimilate = otherCell.room;
            cell.room.Assimilate(roomToAssimilate);
            rooms.Remove(roomToAssimilate);
            Destroy(roomToAssimilate);
        }
    }

    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction)
        //添加一条通路
    {
        MazePassage prefab = Random.value < doorProbability ? doorPrefab : passagePrefab;
        MazePassage passage = Instantiate(prefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(prefab) as MazePassage;
        if (passage is MazeDoor)
        {
            otherCell.Initialize(CreateRoom(cell.room.settingsIndex));
        }
        else
        {
            otherCell.Initialize(cell.room);
        }
        passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction)
        //创建一堵墙
    {
        MazeWall wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null)
        {
            wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
        }
    }
}
