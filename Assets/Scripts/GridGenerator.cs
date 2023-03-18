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
    [SerializeField] private float worldCellPerlinMinimumValue = .1f;
    [SerializeField] private float worldCellNoiseScale = 10f;
    [SerializeField] private float wallHeightScale = 2f;
    [SerializeField] private int numRows;
    [SerializeField] private int numColumns;
    [SerializeField] private int safeAreaRows = 3;
    [SerializeField] private int safeAreaColumns = 3;
    private Vector2 perlinRandomOffset;

    [Header("Powerups")]
    [SerializeField] private int numFoodSpawned = 2;
    [SerializeField] private int numCoinsSpawned = 3;
    [SerializeField] private int numBombsSpawned = 3;
    [SerializeField] private int numPowerupsSpawned = 1;
    [SerializeField] private int numObstaclesSpawned = 1;

    public GridCell[,] GridCells { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GridCell gridCellPrefab;
    [SerializeField] private SnakeBehaviour snakePrefab;
    [SerializeField] private GridCellOccupant wallPrefab;
    [SerializeField] private GridCellOccupant foodPrefab;
    [SerializeField] private GridCellOccupant coinPrefab;
    [SerializeField] private GridCellOccupant bombPrefab;

    [SerializeField] private GridCellOccupant teleporterEntrancePrefab;
    [SerializeField] private GridCellOccupant teleporterExitPrefab;

    [SerializeField] private GridCellOccupant[] powerupPrefabs;
    [SerializeField] private GridCellOccupant[] obstaclePrefabs;
    [SerializeField] private ArrowPointer arrowPointer;

    [Header("Game Related")]
    [SerializeField] private float snakeStartDelay = 2.5f;

    [Header("Time")]
    [SerializeField] private float alterTimeScaleSpeed = 1f;
    [SerializeField] private FloatStore alterTimeTimer;
    private float targetTimeScale;

    [Header("Double Events")]
    [SerializeField] private FloatStore doubleEventTriggersTimer;
    private int eventTriggerRepeats = 1;
    public int EventTriggerRepeats => eventTriggerRepeats;

    private bool paused;
    public bool IsPaused => paused;

    [Header("References")]
    [SerializeField] private IntStore coins;

    [SerializeField] private CinemachineVirtualCamera vCam;

    private List<ArrowPointer> spawnedFoodPointers = new List<ArrowPointer>();
    private string cardAlterationsResourcePath = "CardAlterations/";

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
        spawnedSnake = (SnakeBehaviour)(FindUnoccupiedCell()).SpawnOccupant(snakePrefab);
        vCam.m_Follow = spawnedSnake.transform;

        for (int i = 0; i < numFoodSpawned; i++)
        {
            SpawnFood();
        }

        for (int i = 0; i < numCoinsSpawned; i++)
        {
            SpawnCoin();
        }

        for (int i = 0; i < numBombsSpawned; i++)
        {
            SpawnBomb();
        }

        for (int i = 0; i < numPowerupsSpawned; i++)
        {
            SpawnPowerup();
        }

        for (int i = 0; i < numObstaclesSpawned; i++)
        {
            SpawnObstacle();
        }

        StartCoroutine(StartSnake());

        // Set Time
        targetTimeScale = 1;
        Time.timeScale = 1;
    }

    private IEnumerator StartSnake()
    {
        yield return new WaitForSeconds(snakeStartDelay);
        spawnedSnake.StartMoving();
    }

    private void Update()
    {
        if (paused) return;

        ChangeTime();
        ChangeDoubleEventTriggers();
    }

    [ContextMenu("Spawn Teleporter")]
    public void SpawnTeleporter()
    {
        GridCell start = FindUnoccupiedCell();
        Teleporter teleporterSpawned = (Teleporter)(start.SpawnOccupant(teleporterEntrancePrefab));
        GridCell exit = FindUnoccupiedCell();
        GridCellOccupant spawnedExit = exit.SpawnOccupant(teleporterExitPrefab);
        teleporterSpawned.Link(exit, spawnedExit);
    }

    private void Generate()
    {
        // Generate new offset for perlin noise
        perlinRandomOffset = new Vector2(UnityEngine.Random.Range(0.0f, 1000), UnityEngine.Random.Range(0.0f, 1000));

        // Create array
        GridCells = new GridCell[numRows, numColumns];

        // Sample Noise for Cells
        float[,] celllNoiseMap = new float[numRows, numColumns];
        for (int x = 0; x < numRows; x++)
        {
            for (int y = 0; y < numColumns; y++)
            {
                celllNoiseMap[x, y] = SamplePerlinNoise(x, y, worldCellNoiseScale);
            }
        }

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
                // Don't spawn cell if not supposed to
                if (celllNoiseMap[i, p] < worldCellPerlinMinimumValue) continue;

                // Spawn cell
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
                    Wall wall = (Wall)spawned.SpawnOccupant(wallPrefab);
                    wall.transform.SetParent(transform, true);
                    wall.transform.localScale = new Vector3(wall.transform.localScale.x, 1 * wallHeightScale, wall.transform.localScale.z);
                    wall.IsBorderWall = true;
                    wall.SetWallType(WallType.BORDER);
                    continue;
                }

                // Spawn Regular Wall
                if (wallNoiseMap[i, p] > wallSamplePerlinMinimumValue)
                {
                    // Spawn wall
                    Wall wall = (Wall)spawned.SpawnOccupant(wallPrefab);
                    wall.SetWallType(WallType.NORMAL);

                    // Add it as a child under the generator to reduce clutter
                    wall.transform.SetParent(transform, true);

                    // Set if wall should drop coin on break
                    if (coinNoiseMap[i, p] > coinSamplePerlinMinimumValue)
                    {
                        wall.SetWallType(WallType.VALUABLE);
                        wall.AddOnDestroyCallback(delegate
                        {
                            wall.CurrentCell.SpawnOccupant(coinPrefab);
                            spawnedOres.Remove(wall);
                        });
                        spawnedOres.Add(wall);
                    }
                }
            }
        }
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
        if (!selectedCell
            || selectedCell.IsOccupiedByObstruction())
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
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(coinPrefab);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnCoin();
        });
    }

    public void SpawnBomb()
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(bombPrefab);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnBomb();
        });
    }

    public void SpawnPowerup()
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(powerupPrefabs[UnityEngine.Random.Range(0, powerupPrefabs.Length)]);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnPowerup();
        });
    }

    public void SpawnObstacle()
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)]);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnObstacle();
        });
    }

    public void SlowTime(float changeTimeTo, float duration)
    {
        targetTimeScale = changeTimeTo;
        alterTimeTimer.Value += duration;
    }

    private void ChangeTime()
    {
        if (alterTimeTimer.Value > 0)
        {
            alterTimeTimer.Value -= Time.unscaledDeltaTime;
        }
        else
        {
            targetTimeScale = 1;
        }

        Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * alterTimeScaleSpeed);
    }


    public void DoubleEventTriggers(float duration)
    {
        eventTriggerRepeats = 2;
        doubleEventTriggersTimer.Value += duration;
    }

    private void ChangeDoubleEventTriggers()
    {
        if (doubleEventTriggersTimer.Value > 0)
        {
            doubleEventTriggersTimer.Value -= Time.unscaledDeltaTime;

        }
        else
        {
            eventTriggerRepeats = 1;
        }
    }

    public void Pause()
    {
        Time.timeScale = 0;
        paused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1;
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
}
