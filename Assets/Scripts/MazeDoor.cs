using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeDoor : MazePassage {

    public Transform hinge;//获得门的轴的引用，方便开关门

    private static Quaternion normalRotation = Quaternion.Euler(0f, 90f, 0f);
    private static Quaternion mirroredRotation = Quaternion.Euler(0f, -90f, 0f);//用来处理两侧门方向相反的情况

    private MazeDoor OtherSideOfDoor
        //墙上的门都是成对的一组，获得另一个门
    {
        get
        {
            return otherCell.GetEdge(direction.GetOpposite()) as MazeDoor;
        }
    }

    public override void Initialize(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        base.Initialize(cell, otherCell, direction);
        if (OtherSideOfDoor != null)
            //如果是渲染另一侧的门，需要镜像操作
        {
            Vector3 angel = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(angel.x, angel.y, angel.z + 180);
            Vector3 position = transform.position;
            transform.position = new Vector3(position.x, 1f, position.z);
        }
        
        //为门渲染不同颜色
        for (int i = 0; i < transform.childCount; i ++)
        {
            Transform child = transform.GetChild(i);
            if (child != hinge)
                child.GetComponent<Renderer>().material = cell.room.settings.wallMaterial;
        }

        if (GameManager.personView == PersonView.ThirdPerson)
            gameObject.SetActive(false);
    }

    public override void OnPlayerEntered()
    {
        OtherSideOfDoor.hinge.localRotation =  normalRotation;
        hinge.localRotation = mirroredRotation;
        OtherSideOfDoor.cell.room.Show();
    }

    public override void OnPlayerExited()
    {
        OtherSideOfDoor.hinge.localRotation = hinge.localRotation = Quaternion.identity;
        OtherSideOfDoor.cell.room.Hide();
    }
}
