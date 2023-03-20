using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSnakeButton : MonoBehaviour
{
    [SerializeField] private GameObject destroyOnceStarted;
    [SerializeField] private float snakeStartDelay = 2.5f;

    public void StartGame()
    {
        GridGenerator._Instance.StartGame(snakeStartDelay);

        Destroy(destroyOnceStarted);
    }
}
