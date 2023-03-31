using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSnakeButton : MonoBehaviour
{
    [SerializeField] private GameObject destroyOnceStarted;
    [SerializeField] private float snakeStartDelay = 2.5f;

    [SerializeField] private Difficulty repDifficulty;
    [SerializeField] private float snakeSpeed;
    [SerializeField] private FloatStore snakeSpeedStore;

    public void StartGame()
    {
        GridGenerator._Instance.Difficulty = repDifficulty;
        snakeSpeedStore.Value = snakeSpeed;

        if (UIManager._Instance.UseButtons)
            SnakeBehaviour._Instance.AddButtonControls();

        GridGenerator._Instance.StartGame(snakeStartDelay);

        Destroy(destroyOnceStarted);
    }
}
