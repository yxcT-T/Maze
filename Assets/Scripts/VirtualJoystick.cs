using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

public class VirtualJoystick : MonoBehaviour
{
    private Image bgImg;
    private Image joystickImg;
    private Vector3 inputVector;

    private bool isExist;
    private Vector3 notExist;

    public List<RectTransform> buttons;

    // Use this for initialization
    void Start()
    {
        bgImg = GetComponent<Image>();
        joystickImg = transform.Find("JoystickImage").GetComponent<Image>();

        isExist = false;

        transform.localScale = Vector3.zero;
    }

    public int getTouchIndex()
    {
        Vector2 pos;
        int index = 0, t = 1;

        for (int i = 0; i < Input.touchCount; i++)
        {
            bool flag = true;
            for (int j = 0; j < buttons.Count; j++)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(buttons[j].GetComponent<RectTransform>(), Input.GetTouch(i).position, GetComponentInParent<Camera>(), out pos))
                {
                    float width = buttons[j].GetComponent<RectTransform>().rect.width;
                    float height = buttons[j].GetComponent<RectTransform>().rect.height;

                    if (pos.x >= -width / 2 && pos.x <= width / 2 && pos.y >= -height / 2 && pos.y <= height / 2)
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag)
            {
                index++;
                index += t * i * 10;
                t *= 10;
            }
        }

        return index;
    }

    public bool isAttackButtonPressed()
    {
        Vector2 pos;

        for (int i = 0; i < Input.touchCount; i++)
        {
            for (int j = 0; j < 3; j += 2)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(buttons[j], Input.GetTouch(i).position, GetComponentInParent<Camera>(), out pos))
                {
                    float width = buttons[j].rect.width;
                    float height = buttons[j].rect.height;

                    if (pos.x >= -width / 2 && pos.x <= width / 2 && pos.y >= -height / 2 && pos.y <= height / 2)
                        return true;
                }
            }
        }

        return false;
    }

    public bool isScoreButtonPressed()
    {
        Vector2 pos;

        for (int i = 0; i < Input.touchCount; i++)
        {
            for (int j = 3; j < 4; j++)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(buttons[j], Input.GetTouch(i).position, GetComponentInParent<Camera>(), out pos))
                {
                    float width = buttons[j].rect.width;
                    float height = buttons[j].rect.height;

                    if (pos.x >= -width / 2 && pos.x <= width / 2 && pos.y >= -height / 2 && pos.y <= height / 2)
                        return true;
                }
            }
        }

        return false;
    }

    void Update()
    {
#if !UNITY_STANDALONE_LINUX

        Vector2 pos;
        int index = getTouchIndex();

#if UNITY_STANDALONE_WIN
        if (Input.GetMouseButton(0))
#else
        if (index % 10 == 1)
#endif
        {
            if (!isExist)
            {
                isExist = true;
                transform.localScale = new Vector3(1, 1, 1);
#if !UNITY_STANDALONE_WIN
                transform.position = Input.GetTouch(index / 10 % 10).position;
#else
                transform.position = Input.mousePosition;
#endif
            }
#if !UNITY_STANDALONE_WIN
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, Input.GetTouch(index / 10 % 10).position, GetComponentInParent<Camera>(), out pos))
#else
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, Input.mousePosition, GetComponentInParent<Camera>(), out pos))
#endif
            {
                pos.x = (pos.x / bgImg.rectTransform.sizeDelta.x);
                pos.y = (pos.y / bgImg.rectTransform.sizeDelta.y);

                inputVector = new Vector3(pos.x * 2, 0, pos.y * 2);
                inputVector = inputVector.magnitude > 1f ? inputVector.normalized : inputVector;

                joystickImg.rectTransform.anchoredPosition =
                    new Vector2(inputVector.x * (bgImg.rectTransform.sizeDelta.x / 4), inputVector.z * (bgImg.rectTransform.sizeDelta.y / 4));
            }
        }
        else
        {
            if (isExist)
            {
                isExist = false;
                transform.localScale = Vector3.zero;
            }
            joystickImg.rectTransform.anchoredPosition = Vector2.zero;
        }
#endif
    }

    public float Horizontal()
    {
        return inputVector.x;
    }

    public float Vertical()
    {
        return inputVector.z;
    }
}