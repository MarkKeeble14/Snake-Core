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
    [SerializeField] private int numRows;
    [SerializeField] private int numColumns;
    // [SerializeField] private int safeAreaRows = 3;
    // [SerializeField] private int safeAreaColumns = 3;

    [Header("Wall Settings")]
    [SerializeField] private float wallSamplePerlinMinimumValue = .45f;
    [SerializeField] private float wallPerlinNoiseScale = 3f;
    [SerializeField] private Vector2 minMaxBorderWallHeightScale = new Vector2(3f, 5f);
    [SerializeField] private Vector2 minMaxNormalWallHeightScale = new Vector2(1f, 3f);

    [Header("Valuable Wall Settings")]
    [SerializeField] private float coinSamplePerlinMinimumValue = .55f;
    [SerializeField] private float coinPerlinNoiseScale = 5f;

    [Header("World Cell Settings")]
    [SerializeField] private float worldCellPerlinMinimumValue = .1f;
    [SerializeField] private float worldCellNoiseScale = 10f;
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

    [Header("Time")]
    [SerializeField] private FloatStore alterTimeTimer;

    [Header("Double Events")]
    [SerializeField] private FloatStore doubleEventTriggersTimer;
    private int eventTriggerRepeats = 1;
    public int EventTriggerRepeats => eventTriggerRepeats;

    [Header("Pausing")]
    private float timeBeforePause;
    private bool paused;
    public bool IsPaused => paused;
    private float targetTimeScale;

    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera vCam;

    private List<ArrowPointer> spawnedFoodPointers = new List<ArrowPointer>();

    private List<Wall> spawnedOres = new List<Wall>();

    [Header("Event Stack")]
    [SerializeField] private IntStore maxStackSize;
    private Queue<Action> eventStack = new Queue<Action>();
    [SerializeField] private EventStackDisplay eventStackDisplay;
    [SerializeField] private float delayBetweenEventStackTriggers = 1f;
    [SerializeField] private float delayAfterEventStackTriggers = 1f;
    [SerializeField] private IntStore popIn;
    private bool executingStack;

    private void Awake()
    {
        // Set instance
        _Instance = this;

        // Reset Stats
        NumStore[] stats = Resources.LoadAll<NumStore>("NumStores");
        foreach (NumStore store in stats)
        {
            store.Reset();
        }

        // Reset Coins
        popIn.Value = maxStackSize.Value;

        Generate();

        // Spawn snake
        spawnedSnake = (SnakeBehaviour)(FindUnoccupiedCell()).SpawnOccupant(snakePrefab);
        vCam.m_Follow = spawnedSnake.transform;

        SpawnFood(numFoodSpawned);
        SpawnCoin(numCoinsSpawned);
        SpawnBomb(numBombsSpawned);
        SpawnPowerup(numPowerupsSpawned);
        SpawnObstacle(numObstaclesSpawned);

        // Set Time
        Time.timeScale = 1;
    }

    private void Update()
    {
        if (paused) return;

        ChangeTime();
        ChangeDoubleEventTriggers();
    }

    public void StartGame(float delay)
    {
        StartCoroutine(UIManager._Instance.StartSnake(delay));
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
        // Vector2 safeAreaXBounds = new Vector2((numRows / 2) - (safeAreaRows / 2), (numRows / 2) + (safeAreaRows / 2));
        // Vector2 safeAreaYBounds = new Vector2((numColumns / 2) - (safeAreaColumns / 2), (numColumns / 2) + (safeAreaColumns / 2));

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
                // if (i > safeAreaXBounds.x && i < safeAreaXBounds.y && p > safeAreaYBounds.x && p < safeAreaYBounds.y)
                // {
                //    continue;
                // }

                // Spawn Border Walls
                if (i == 0 || i == numRows - 1 || p == 0 || p == numColumns - 1)
                {
                    Wall wall = (Wall)spawned.SpawnOccupant(wallPrefab);
                    wall.transform.SetParent(transform, true);
                    wall.transform.localScale = new Vector3(wall.transform.localScale.x, 1 * RandomHelper.RandomFloat(minMaxBorderWallHeightScale), wall.transform.localScale.z);
                    wall.IsBorderWall = true;
                    wall.SetWallType(WallType.BORDER);
                    continue;
                }

                // Spawn Regular Wall
                if (wallNoiseMap[i, p] > wallSamplePerlinMinimumValue)
                {
                    // Spawn wall
                    Wall wall = (Wall)spawned.SpawnOccupant(wallPrefab);
                    wall.transform.localScale = new Vector3(wall.transform.localScale.x, 1 * RandomHelper.RandomFloat(minMaxNormalWallHeightScale), wall.transform.localScale.z);
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
        return noiseValue;
    }

    public GridCell FindUnoccupiedCell()
    {
        GridCell selectedCell = GridCells[UnityEngine.Random.Range(1, numRows - 2), UnityEngine.Random.Range(1, numColumns - 2)];
        if (!selectedCell
            || selectedCell.isOccupied)
        {
            return FindUnoccupiedCell();
        }
        else
        {
            return selectedCell;
        }
    }

    public void SpawnFood(int num)
    {
        GridCellOccupant food = FindUnoccupiedCell().SpawnOccupant(foodPrefab);
        ArrowPointer arrow = Instantiate(arrowPointer, spawnedSnake.transform.position, Quaternion.identity);
        arrow.SetPointAt(spawnedSnake.transform, food.transform);
        spawnedFoodPointers.Add(arrow);
        food.AddOnDestroyCallback(delegate
        {
            spawnedFoodPointers.Remove(arrow);
            Destroy(arrow.gameObject);
            SpawnFood(1);
        });

        if (--num > 0)
            SpawnTeleporter(num);
    }

    public void SpawnCoin(int num)
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(coinPrefab);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnCoin(1);
        });

        if (--num > 0)
            SpawnCoin(num);
    }

    public void SpawnBomb(int num)
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(bombPrefab);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnBomb(1);
        });

        if (--num > 0)
            SpawnBomb(num);
    }

    public void SpawnPowerup(int num)
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(powerupPrefabs[UnityEngine.Random.Range(0, powerupPrefabs.Length)]);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnPowerup(1);
        });

        if (--num > 0)
            SpawnPowerup(num);
    }

    public void SpawnObstacle(int num)
    {
        GridCellOccupant occupant = FindUnoccupiedCell().SpawnOccupant(obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)]);
        occupant.AddOnDestroyCallback(delegate
        {
            SpawnObstacle(1);
        });

        if (--num > 0)
            SpawnObstacle(num);
    }

    public void SpawnWall(WallType type, int num)
    {
        Wall wall = (Wall)FindUnoccupiedCell().SpawnOccupant(wallPrefab);
        wall.transform.SetParent(transform, true);
        wall.SetWallType(type);
        switch (type)
        {
            case WallType.NORMAL:
                wall.transform.localScale = new Vector3(wall.transform.localScale.x, 1 * RandomHelper.RandomFloat(minMaxNormalWallHeightScale), wall.transform.localScale.z);
                break;
            case WallType.BORDER:
                wall.IsBorderWall = true;
                wall.transform.localScale = new Vector3(wall.transform.localScale.x, 1 * RandomHelper.RandomFloat(minMaxBorderWallHeightScale), wall.transform.localScale.z);
                break;
            case WallType.VALUABLE:
                wall.AddOnDestroyCallback(delegate
                {
                    wall.CurrentCell.SpawnOccupant(coinPrefab);
                    spawnedOres.Remove(wall);
                });
                spawnedOres.Add(wall);
                break;
        }

        if (--num > 0)
            SpawnWall(type, num);
    }

    [ContextMenu("Spawn Teleporter")]
    public void SpawnTeleporter()
    {
        SpawnTeleporter(0);
    }


    public void SpawnTeleporter(int num)
    {
        GridCell start = FindUnoccupiedCell();
        Teleporter teleporterSpawned = (Teleporter)(start.SpawnOccupant(teleporterEntrancePrefab));
        GridCell exit = FindUnoccupiedCell();
        GridCellOccupant spawnedExit = exit.SpawnOccupant(teleporterExitPrefab);
        teleporterSpawned.Link(exit, spawnedExit);

        if (--num > 0)
            SpawnTeleporter(num);
    }

    public void SlowTime(float addToTime, float duration)
    {
        targetTimeScale += addToTime;
        if (targetTimeScale < .25f)
            targetTimeScale = .25f;

        alterTimeTimer.Value += duration;
        if (executingStack) return;
        Time.timeScale = targetTimeScale;
    }

    private void ChangeTime()
    {
        if (paused || executingStack || !SnakeBehaviour._Instance.SnakeEnabled) return;
        if (alterTimeTimer.Value > 0)
        {
            alterTimeTimer.Value -= Time.unscaledDeltaTime;
        }
        else
        {
            targetTimeScale = 1;
            Time.timeScale = targetTimeScale;
        }
    }


    public void DoubleEventTriggers(float duration)
    {
        eventTriggerRepeats *= 2;
        doubleEventTriggersTimer.Value += duration;
    }

    private void ChangeDoubleEventTriggers()
    {
        if (paused || executingStack || !SnakeBehaviour._Instance.SnakeEnabled) return;
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
        timeBeforePause = Time.timeScale;
        Time.timeScale = 0;
        paused = true;
    }

    public void Resume()
    {
        Time.timeScale = timeBeforePause;
        timeBeforePause = 1;
        paused = false;
    }

    public void AddEventToStack(Action a, StoredTriggerEventDisplayInfo info)
    {
        // Add the action to the stack
        eventStack.Enqueue(a);

        // Add the corresponding UI element
        eventStackDisplay.Push(info);

        popIn.Value--;

        // if over the stack limit, call all functions in the stack
        if (eventStack.Count >= maxStackSize.Value)
        {
            StartCoroutine(ExecuteEventStack());
        }
    }

    private IEnumerator ExecuteEventStack()
    {
        executingStack = true;

        yield return new WaitUntil(() => !SnakeBehaviour._Instance.IsOnTargetCell);

        yield return new WaitUntil(() => SnakeBehaviour._Instance.IsOnTargetCell);

        SnakeBehaviour._Instance.StopMoving();
        Time.timeScale = 0;

        // For all of the displayed events in the stack
        while (eventStack.Count > 0)
        {
            yield return new WaitForSecondsRealtime(delayBetweenEventStackTriggers);

            // Pop topmost function
            Action a = eventStack.Dequeue();

            // Pop topmost display
            eventStackDisplay.Dequeue();

            // Call Function
            a?.Invoke();
        }

        yield return new WaitForSecondsRealtime(delayAfterEventStackTriggers);

        executingStack = false;

        StartCoroutine(UIManager._Instance.StartSnake());

        yield return new WaitUntil(() => SnakeBehaviour._Instance.SnakeEnabled);

        Time.timeScale = targetTimeScale;

        popIn.Value = maxStackSize.Value;
    }
}
