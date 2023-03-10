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

    private SnakeBehaviour spawnedSnake;

    [Header("Settings")]
    [Header("Generation")]
    [SerializeField] private float wallSamplePerlinMinimumValue = .45f;
    [SerializeField] private float wallPerlinNoiseScale = 3f;
    [SerializeField] private float coinSamplePerlinMinimumValue = .55f;
    [SerializeField] private float coinPerlinNoiseScale = 5f;
    [SerializeField] private int numRows;
    [SerializeField] private int numColumns;
    [SerializeField] private int safeAreaRows = 3;
    [SerializeField] private int safeAreaColumns = 3;
    private Vector2 perlinRandomOffset;

    [Header("Regeneration")]
    [SerializeField] private int increaseRowsUponRegenerate = 5;
    [SerializeField] private int increaseColumnsUponRegenerate = 5;
    [SerializeField] private int increaseFoodUponRegenerate = 1;
    [SerializeField] private int increaseObstaclesUponRegenerate = 1;
    [SerializeField] private int increasePowerupsUponRegenerate = 1;

    [Header("Powerups")]
    [SerializeField] private int numFoodSpawned = 2;
    [SerializeField] private int numCoinsSpawned = 3;
    [SerializeField] private int numPowerupsSpawned = 1;
    [SerializeField] private int numObstaclesSpawned = 1;

    public GridCell[,] GridCells { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GridCell gridCellPrefab;
    [SerializeField] private SnakeBehaviour snakePrefab;
    [SerializeField] private GridCellOccupant wallPrefab;
    [SerializeField] private GridCellOccupant foodPrefab;
    [SerializeField] private GridCellOccupant coinPrefab;
    [SerializeField] private GridCellOccupant[] powerupPrefabs;
    [SerializeField] private GridCellOccupant[] obstaclePrefabs;
    [SerializeField] private ArrowPointer arrowPointer;

    [Header("Time")]
    [SerializeField] private float alterTimeScaleSpeed = 1f;
    [SerializeField] private TextMeshProUGUI slowDownTimeScaleText;
    private float slowDownTimeTimer;
    private float targetTimeScale;
    private bool paused;
    public bool IsPaused => paused;

    [Header("Double Events")]
    [SerializeField] private TextMeshProUGUI doubleEventsText;
    private float doubleEventTriggersTimer;
    private int eventTriggerRepeats = 1;
    public int EventTriggerRepeats => eventTriggerRepeats;

    [Header("References")]
    [SerializeField] private IntStore coins;

    [SerializeField] private CinemachineVirtualCamera vCam;

    private List<ArrowPointer> spawnedFoodPointers = new List<ArrowPointer>();
    private string cardAlterationsResourcePath = "CardAlterations/";

    private bool regenerating;

    private List<Wall> spawnedOres = new List<Wall>();

    private void Awake()
    {
        // Set instance
        _Instance = this;

        // Reset Coins
        coins.Reset();

        // Reset Cards
        ResetCardAlterations();

        Generate();

        // Spawn snake
        spawnedSnake = (SnakeBehaviour)GridCells[numRows / 2, numColumns / 2].SpawnOccupant(snakePrefab);
        vCam.m_Follow = spawnedSnake.transform;

        for (int i = 0; i < numFoodSpawned; i++)
        {
            SpawnFood();
        }

        for (int i = 0; i < numCoinsSpawned; i++)
        {
            SpawnCoin();
        }

        for (int i = 0; i < numPowerupsSpawned; i++)
        {
            SpawnPowerup();
        }

        for (int i = 0; i < numObstaclesSpawned; i++)
        {
            SpawnObstacle();
        }

        spawnedSnake.StartMoving(1f);
    }

    private void Generate()
    {
        // Generate new offset for perlin noise
        perlinRandomOffset = new Vector2(UnityEngine.Random.Range(0.0f, 1000), UnityEngine.Random.Range(0.0f, 1000));

        // Create array
        GridCells = new GridCell[numRows, numColumns];

        // Sample Noise for Walls
        float[,] wallNoiseMap = new float[numRows, numColumns];
        for (int x = 0; x < numRows; x++)
        {
            for (int y = 0; y < numColumns; y++)
            {
                wallNoiseMap[x, y] = SamplePerlinNoise(x, y, wallPerlinNoiseScale);
            }
        }

        // Sample Noise for Coins
        float[,] coinNoiseMap = new float[numRows, numColumns];
        for (int x = 0; x < numRows; x++)
        {
            for (int y = 0; y < numColumns; y++)
            {
                coinNoiseMap[x, y] = SamplePerlinNoise(x, y, coinPerlinNoiseScale);
            }
        }

        // Spawning safe area
        Vector2 safeAreaXBounds = new Vector2((numRows / 2) - (safeAreaRows / 2), (numRows / 2) + (safeAreaRows / 2));
        Vector2 safeAreaYBounds = new Vector2((numColumns / 2) - (safeAreaColumns / 2), (numColumns / 2) + (safeAreaColumns / 2));

        // Spawn cells
        for (int i = 0; i < numRows; i++)
        {
            for (int p = 0; p < numColumns; p++)
            {
                GridCell spawned = Instantiate(gridCellPrefab, new Vector3(i, 0, p), Quaternion.identity, transform);
                spawned.name += "[" + i + ", " + p + "]";
                GridCells[i, p] = spawned;
                spawned.Set(i, p);

                // inside of safe area
                if (i > safeAreaXBounds.x && i < safeAreaXBounds.y && p > safeAreaYBounds.x && p < safeAreaYBounds.y)
                {
                    continue;
                }

                // Spawn Border Walls
                if (i == 0 || i == numRows - 1 || p == 0 || p == numColumns - 1)
                {
                    GridCellOccupant occupant = spawned.SpawnOccupant(wallPrefab);
                    occupant.transform.SetParent(transform, true);
                    occupant.IsBorderWall = true;
                    occupant.GetComponent<Renderer>().material.color = Color.red;
                    continue;
                }

                // Spawn Regular Wall
                if (wallNoiseMap[i, p] > wallSamplePerlinMinimumValue)
                {
                    // Spawn wall
                    Wall wall = (Wall)spawned.SpawnOccupant(wallPrefab);
                    // Add it as a child under the generator to reduce clutter
                    wall.transform.SetParent(transform, true);

                    // Set if wall should drop coin on break
                    if (coinNoiseMap[i, p] > coinSamplePerlinMinimumValue)
                    {
                        wall.GetComponent<Renderer>().material.color = Color.yellow;
                        spawnedOres.Add(wall);
                        wall.AddOnDestroyCallback(delegate
                        {
                            wall.CurrentCell.SpawnOccupant(coinPrefab);
                            spawnedOres.Remove(wall);

                            // No more ore's, regenerate grid
                            if (spawnedOres.Count == 0)
                            {
                                Regenerate();
                            }
                        });
                    }
                }
            }
        }
    }

    private void Regenerate()
    {
        regenerating = true;
        spawnedSnake.StopMoving();

        while (spawnedFoodPointers.Count > 0)
        {
            Destroy(spawnedFoodPointers[0].gameObject);
            spawnedFoodPointers.RemoveAt(0);
        }

        for (int i = 0; i < numRows; i++)
        {
            for (int p = 0; p < numColumns; p++)
            {
                GridCell cell = GridCells[i, p];
                cell.Delete();
            }
        }

        numFoodSpawned += increaseFoodUponRegenerate;
        numPowerupsSpawned += increasePowerupsUponRegenerate;
        numObstaclesSpawned += increaseObstaclesUponRegenerate;
        numRows += increaseRowsUponRegenerate;
        numColumns += increaseColumnsUponRegenerate;

        Generate();
        regenerating = false;

        spawnedSnake.SetToCell(GridCells[numRows / 2, numColumns / 2]);

        for (int i = 0; i < numFoodSpawned; i++)
        {
            SpawnFood();
        }

        for (int i = 0; i < numCoinsSpawned; i++)
        {
            SpawnCoin();
        }

        for (int i = 0; i < numPowerupsSpawned; i++)
        {
            SpawnPowerup();
        }

        for (int i = 0; i < numObstaclesSpawned; i++)
        {
            SpawnObstacle();
        }

        spawnedSnake.StartMoving(1f);
    }

    private float SamplePerlinNoise(int x, int y, float scale)
    {
        float xCoord = (x + perlinRandomOffset.x) / numRows * scale;
        float yCoord = (y + perlinRandomOffset.y) / numColumns * scale;
        float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);
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
        if (regenerating) return;
        GridCellOccupant food = FindUnoccupiedCell().SpawnOccupant(foodPrefab);
        ArrowPointer arrow = Instantiate(arrowPointer, spawnedSnake.transform.position, Quaternion.identity);
        arrow.SetPointAt(spawnedSnake.transform, food.transform);
        spawnedFoodPointers.Add(arrow);
        food.AddOnDestroyCallback(delegate
        {
            spawnedFoodPointers.Remove(arrow);
            Destroy(arrow.gameObject);
            SpawnFood();
        });
    }

    public void SpawnCoin()
    {
        if (regenerating) return;
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(coinPrefab);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnCoin();
        });
    }

    public void SpawnPowerup()
    {
        if (regenerating) return;
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(powerupPrefabs[UnityEngine.Random.Range(0, powerupPrefabs.Length)]);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnPowerup();
        });
    }

    public void SpawnObstacle()
    {
        if (regenerating) return;
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)]);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnObstacle();
        });
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
        eventTriggerRepeats = 2;
        doubleEventTriggersTimer += duration;
    }

    private void ChangeDoubleEventTriggers()
    {
        if (doubleEventTriggersTimer > 0)
        {
            doubleEventTriggersTimer -= Time.deltaTime;

            doubleEventsText.text = "x" + eventTriggerRepeats + " Events: " + Math.Round(doubleEventTriggersTimer, 1).ToString();
            doubleEventsText.gameObject.SetActive(true);

        }
        else
        {
            eventTriggerRepeats = 1;

            doubleEventsText.gameObject.SetActive(false);
        }
    }

    public void Pause()
    {
        Time.timeScale = 0;
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    [ContextMenu("Reset Card Alterations")]
    private void ResetCardAlterations()
    {
        NumStore[] cardAlterations = Resources.LoadAll<NumStore>(cardAlterationsResourcePath);
        foreach (NumStore store in cardAlterations)
        {
            store.Reset();
        }
    }

    private void Update()
    {
        if (paused) return;
        ChangeTime();
        ChangeDoubleEventTriggers();
    }
}
