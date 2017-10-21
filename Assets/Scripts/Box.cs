using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {
    //放终点，判断玩家是否走到终点

    public GameManager gameManager;

    private void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name == "Capsule")
        {
            gameManager.EndGame();
        }
    }
}
