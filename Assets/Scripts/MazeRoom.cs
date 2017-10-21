using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRoom : ScriptableObject {
    //房间集合，定义为用门隔开的连通块

    public int settingsIndex;//材质编号

    public MazeRoomSettings settings;

    private List<MazeCell> cells = new List<MazeCell>();//房间里的空格集合

    public void Add(MazeCell cell)
    {
        cell.room = this;
        cells.Add(cell);
    }

    public void Assimilate(MazeRoom room)
        //合并两个房间
    {
        for (int i = 0; i < room.cells.Count; i ++)
        {
            Add(room.cells[i]);
        }
    }

    public void Hide()
    {
        for (int i = 0; i < cells.Count; i++)
            cells[i].Hide();
    }

    public void Show()
    {
        for (int i = 0; i < cells.Count; i++)
            cells[i].Show();
    }
}
