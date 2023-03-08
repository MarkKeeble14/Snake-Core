using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Cinemachine;

public class GridGenerator : MonoBehaviour
{
    // Also serves as a game manager
    public static GridGenerator _Instance { get; set; }
    [Header("Settings")]
    [Header("Generation")]
    [SerializeField] private float wallSamplePerlinMinimumValue = .45f;
    [SerializeField] private float wallPerlinNoiseScale = 20f;
    [SerializeField] private int numRows;
    [SerializeField] private int numColumns;

    [Header("Powerups")]
    [SerializeField] private int maxNumCoinsSpawned;
    [SerializeField] private int maxNumPowerupsSpawned;
    [SerializeField] private int maxNumObstaclesSpawned;

    public GridCell[,] GridCells { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GridCell gridCellPrefab;
    [SerializeField] private GridCellOccupant snakePrefab;
    [SerializeField] private GridCellOccupant wallPrefab;
    [SerializeField] private GridCellOccupant foodPrefab;
    [SerializeField] private GridCellOccupant coinPrefab;
    [SerializeField] private GridCellOccupant[] powerupPrefabs;
    [SerializeField] private GridCellOccupant[] obstaclePrefabs;

    [Header("Time")]
    [SerializeField] private float alterTimeScaleSpeed = 1f;
    [SerializeField] private TextMeshProUGUI slowDownTimeScaleText;
    private float slowDownTimeTimer;
    private float targetTimeScale;

    [Header("Double Events")]
    [SerializeField] private TextMeshProUGUI doubleEventsText;
    private bool doublingEventTriggers;
    public bool DoublingEventTriggers => doublingEventTriggers;
    private float doubleEventTriggersTimer;

    [Header("References")]
    [SerializeField] private IntStore coins;
    private List<GridCellOccupant> spawnedCoins;
    private List<GridCellOccupant> spawnedPowerups;
    private List<GridCellOccupant> spawnedObstacles;

    [SerializeField] private CinemachineVirtualCamera vCam;

    private void Awake()
    {
        // Set instance
        _Instance = this;

        // Reset Coins
        coins.Reset();

        // Create Lists
        spawnedCoins = new List<GridCellOccupant>();
        spawnedPowerups = new List<GridCellOccupant>();
        spawnedObstacles = new List<GridCellOccupant>();

        // Create array
        GridCells = new GridCell[numRows, numColumns];

        float[,] wallNoiseMap = new float[numRows, numColumns];
        // Create noise texture
        for (int x = 0; x < numRows; x++)
        {
            for (int y = 0; y < numColumns; y++)
            {
                wallNoiseMap[x, y] = SamplePerlinNoise(x, y);
            }
        }

        // Spawn cells
        for (int i = 0; i < numRows; i++)
        {
            for (int p = 0; p < numColumns; p++)
            {
                GridCell spawned = Instantiate(gridCellPrefab, new Vector3(i, 0, p), Quaternion.identity, transform);
                spawned.name += "[" + i + ", " + p + "]";
                GridCells[i, p] = spawned;
                spawned.Set(i, p);

                if (i == 0 || i == numRows - 1 || p == 0 || p == numColumns - 1)
                {
                    GridCellOccupant occupant = spawned.SpawnOccupant(wallPrefab);
                    occupant.transform.SetParent(transform, true);
                    occupant.IsInstantLoss = true;
                    continue;
                }

                if (wallNoiseMap[i, p] > wallSamplePerlinMinimumValue)
                {
                    spawned.SpawnOccupant(wallPrefab).transform.SetParent(transform, true);
                }
            }
        }

        // Spawn first food
        SpawnFood();

        // Spawn snake
        GridCellOccupant snake = FindUnoccupiedCell().SpawnOccupant(snakePrefab);
        vCam.m_Follow = snake.transform;
    }

    private float SamplePerlinNoise(int x, int y)
    {
        float xCoord = (float)x / numRows;
        float yCoord = (float)y / numColumns;
        float noiseValue = Mathf.PerlinNoise(xCoord * wallPerlinNoiseScale, yCoord * wallPerlinNoiseScale);
        // Debug.Log("x: " + x + ", xCoord: " + xCoord + ", y: " + y + ", yCoord: " + yCoord + ", noiseValue: " + noiseValue);
        return noiseValue;
    }

    public GridCell FindUnoccupiedCell()
    {
        GridCell selectedCell = GridCells[UnityEngine.Random.Range(1, numRows - 2), UnityEngine.Random.Range(1, numColumns - 2)];
        if (selectedCell.IsOccupiedByObstruction())
        {
            return FindUnoccupiedCell();
        }
        else
        {
            return selectedCell;
        }
    }

    public void SpawnFood()
    {
        FindUnoccupiedCell().SpawnOccupant(foodPrefab);
    }

    public void SpawnCoin()
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(coinPrefab);
        spawnedCoins.Add(occupant);
        if (occupant.TryGetComponent(out DestroySelfTriggerEvent destroySelf))
        {
            destroySelf.AddOnDestroyCallback(() => spawnedCoins.Remove(occupant));
        }
    }

    public void SpawnPowerup()
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(powerupPrefabs[UnityEngine.Random.Range(0, powerupPrefabs.Length)]);
        spawnedPowerups.Add(occupant);
        if (occupant.TryGetComponent(out DestroySelfTriggerEvent destroySelf))
        {
            destroySelf.AddOnDestroyCallback(() => spawnedPowerups.Remove(occupant));
        }
    }

    public void SpawnObstacle()
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)]);
        spawnedObstacles.Add(occupant);
        if (occupant.TryGetComponent(out DestroySelfTriggerEvent destroySelf))
        {
            destroySelf.AddOnDestroyCallback(() => spawnedObstacles.Remove(occupant));
        }
    }

    public void SlowTime(float changeTimeTo, float duration)
    {
        targetTimeScale = changeTimeTo;
        slowDownTimeTimer += duration;
    }

    private void ChangeTime()
    {
        if (slowDownTimeTimer > 0)
        {
            slowDownTimeTimer -= Time.unscaledDeltaTime;

            slowDownTimeScaleText.text = Math.Round(slowDownTimeTimer, 1).ToString();
            slowDownTimeScaleText.gameObject.SetActive(true);
        }
        else
        {
            targetTimeScale = 1;

            slowDownTimeScaleText.gameObject.SetActive(false);
        }

        Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * alterTimeScaleSpeed);
    }

    public void DoubleEventTriggers(float duration)
    {
        doubleEventTriggersTimer += duration;
    }

    private void ChangeDoubleEventTriggers()
    {
        if (doubleEventTriggersTimer > 0)
        {
            doubleEventTriggersTimer -= Time.deltaTime;

            doubleEventsText.text = "x2 Events: " + Math.Round(doubleEventTriggersTimer, 1).ToString();
            doubleEventsText.gameObject.SetActive(true);

            doublingEventTriggers = true;
        }
        else
        {
            doublingEventTriggers = false;

            doubleEventsText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        ChangeTime();
        ChangeDoubleEventTriggers();

        if (spawnedCoins.Count < maxNumCoinsSpawned)
        {
            SpawnCoin();
        }

        if (spawnedPowerups.Count < maxNumPowerupsSpawned)
        {
            SpawnPowerup();
        }

        if (spawnedObstacles.Count < maxNumObstaclesSpawned)
        {
            SpawnObstacle();
        }
    }
}
