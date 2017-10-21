using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MouseOrbit : MonoBehaviour
{
    #region Variables

    public Transform target;
    public VirtualJoystick vj;
    public bool isMoveTarget = false;

    public float distance = 10f;
    float xSpeed = 1f;
    float ySpeed = 1f;
    float sx, x;
    float sy, y;

    Vector2[] touches_tmp = { Vector2.zero, Vector2.zero, Vector2.zero };

    #endregion

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        sx = x = angles.y;
        sy = y = angles.x;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    // Update is called once per frame

    public float getAngleX()
    {
        return x - sx;
    }

    public float getAngleY()
    {
        return y - sy;
    }

    Vector2 getAverage(Vector2[] tmp, int size)
    {
        Vector2 res = Vector2.zero;
        for (int i = 0; i < size; i++)
        {
            res.x += tmp[i].x / size;
            res.y += tmp[i].y / size;
        }

        return res;
    }

#if UNITY_STANDALONE_WIN
    void LateUpdate()
    {
        distance += Input.GetAxis("Mouse ScrollWheel") * 5;

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetMouseButton(1))
            {
                distance += Input.GetAxis("Mouse Y") * 0.5f;
            }

            if (Input.GetMouseButton(0))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 3;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 15;
                y = ClampAngle(y);
                x = ClampAngle(x);
                transform.rotation = Quaternion.Euler(y, x, 0f);
            }


            if (Input.GetMouseButton(2))
            {
                float x2 = Input.GetAxis("Mouse X");
                float y2 = Input.GetAxis("Mouse Y");
                target.transform.position += transform.right * (-x2 * 0.2f);
                target.transform.position += transform.up * (-y2 * 0.2f);
                isMoveTarget = true;
            }

        }
        transform.position = target.transform.position - (transform.forward * distance);
    }
#else
    void LateUpdate () 
	{
        int index = vj.getTouchIndex();
        if (index % 10 == 2)
        {
            for (int i = 0, k = index; i < 2; i++)
            {
                touches_tmp[i] = Input.GetTouch(k / 10 % 10).deltaPosition;
                k /= 10;
            }

            //zoom
            Vector2 t0 = Input.GetTouch(index / 10 % 10).position, t1 = Input.GetTouch(index / 100 % 10).position;
            Vector2 pt0 = t0 - touches_tmp[0], pt1 = t1 - touches_tmp[1];

            distance -= ((t0 - t1).magnitude - (pt0 - pt1).magnitude) * 0.05f;

            //rotate
            Vector2 ap_tmp = getAverage(touches_tmp, 2);
           
            x += ap_tmp.x * xSpeed;
            y -= ap_tmp.y * ySpeed;
            y = ClampAngle(y);
            x = ClampAngle(x);
            transform.rotation = Quaternion.Euler(y, x, 0.0f);
        }

        if (index % 10 == 3)
        {
            for (int i = 0, k = index; i < 3; i++)
            {
                touches_tmp[i] = Input.GetTouch(k / 10 % 10).deltaPosition;
                k /= 10;
            }
            //shift
            Vector2 ap_tmp = getAverage(touches_tmp, 3);

            float x2 = ap_tmp.x;
            float y2 = ap_tmp.y;
            target.transform.position += transform.right * (-x2 * 0.02f);
            target.transform.position += transform.up * (-y2 * 0.02f);
            isMoveTarget = true;
        }

		transform.position = target.transform.position - (transform.forward * distance);
	}
#endif

    float ClampAngle(float angle)
    {
        while (angle < -360)
            angle += 360;
        while (angle > 360)
            angle -= 360;
        return angle;
    }
}
