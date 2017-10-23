using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    //控制玩家

    public float m_Speed;//移动速度
    public float m_TurnSpeed;//转动速度

    private string m_MovementAxisName;//用于获取玩家输入的移动信息，以下类似
    private string m_TurnAxisName;
    private float m_MovementInputValue;//径向速度
    private float m_TurnInputvalue;//转动速度,第三视角下表示横向速度

    public Rigidbody m_Rigidbody;

    private VirtualJoystick joystick;
    private MouseOrbit cameraAngle;

    private float routeLength;
    public float stayTime;

    private void OnEnable()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputvalue = 0f;
    }

    private void Start()
    {
        m_MovementAxisName = "Vertical";
        m_TurnAxisName = "Horizontal";

        if (GameManager.personView == PersonView.ThirdPerson) m_Speed *= 3;//第三视角下，玩家移动速度 * 3

        joystick = GameObject.Find("Canvas").transform.Find("BackgroundImage").GetComponent<VirtualJoystick>();
        cameraAngle = Camera.main.GetComponent<MouseOrbit>();

        routeLength = 0;
        stayTime = 0;
    }

    private Vector2 rotate_v2(float alpha, Vector2 p)
    {
        alpha = alpha * Mathf.PI / 180f;
        return new Vector2(Mathf.Cos(alpha) * p.x - Mathf.Sin(alpha) * p.y, Mathf.Sin(alpha) * p.x + Mathf.Cos(alpha) * p.y);
    }

    private void Update()
    {
#if !UNITY_STANDALONE_WIN
        int index = joystick.getTouchIndex();
        if (index % 10 == 1)
#else
        if (Input.GetMouseButton(0))
#endif
        {
            Vector2 p = rotate_v2(-cameraAngle.getAngleX(), new Vector2(-joystick.Horizontal(), -joystick.Vertical()));
            m_MovementInputValue = -p.y;
            m_TurnInputvalue = -p.x;
        }

#if UNITY_STANDALONE_WIN
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputvalue = Input.GetAxis(m_TurnAxisName);
#endif
    }

    private void FixedUpdate()
    {
        if (GameManager.personView == PersonView.FirstPerson)
        {
            Move();
            Turn();
        }
        else if (GameManager.personView == PersonView.ThirdPerson)
            Move_Horizontal();
    }

    private void Move()
    {
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }

    private void Move_Horizontal()
    {
        Vector3 movement = new Vector3(1f, 0f, 0f) * m_TurnInputvalue * m_Speed + new Vector3(0f, 0f, 1f) * m_MovementInputValue * m_Speed;
        if (movement.magnitude < 0.01f)
            stayTime += Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement * Time.deltaTime);
        routeLength += movement.magnitude * Time.deltaTime;
    }

    private void Turn()
    {
        m_Rigidbody.angularVelocity = Vector3.zero;

        float turn = m_TurnInputvalue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }

    public void SetLocation(MazeCell cell)
    {
        transform.localPosition = cell.transform.localPosition;
    }

    public float GetRouteLength
    {
        get
        {
            return routeLength;
        }
    }
}
