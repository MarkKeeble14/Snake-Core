using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBehaviour : GridCellOccupant
{
    public static SnakeBehaviour _Instance { get; private set; }

    private void Awake()
    {
        _Instance = this;
    }

    [Header("Settings")]
    [SerializeField] private Vector2Int direction = Vector2Int.right;
    [SerializeField] private float timeBetweenMoves = .25f;
    [SerializeField] private float spawnDelay = 1f;

    [Header("Attacks")]

    [Header("Break Ahead")]
    [SerializeField] private float minBreakAheadRange;
    [SerializeField] private float maxBreakAheadRange;
    [SerializeField] private float gainBreakAheadRate;
    [SerializeField] private float breakAheadCooldown;
    private float breakAheadCooldownTimer;
    [SerializeField] private FloatStore breakAheadCooldownPercent;

    [Header("Break Around")]
    [SerializeField] private float minBreakAroundRange;
    [SerializeField] private float maxBreakAroundRange;
    [SerializeField] private float gainBreakAroundRate;
    [SerializeField] private float breakAroundCooldown;
    private float breakAroundCooldownTimer;
    [SerializeField] private FloatStore breakAroundCooldownPercent;

    [Header("References")]
    [SerializeField] private GridCellOccupant snakeTailPrefab;
    private LinkedList<GridCellOccupant> snakeSegments = new LinkedList<GridCellOccupant>();

    [Header("UI")]
    [SerializeField] private IntStore segmentsCounter;
    [SerializeField] private IntStore allowedCollisions;

    private Vector2Int lastDirectionMoved;
    private bool snakeEnabled;
    private bool moving;
    private float moveTimer;

    private bool teleportFlag;

    private bool isPreppingPierce;
    private bool isPreppingAoE;

    private void Start()
    {
        // Set forward vector
        transform.forward = new Vector3(direction.x, 0, direction.y);

        // Reset values
        segmentsCounter.Reset();
        allowedCollisions.Reset();
        moveTimer = timeBetweenMoves;

        // Add head to linked list
        snakeSegments.AddFirst(this);

        StartCoroutine(Movement());
    }

    public override void ChangeCell(GridCell nextCell)
    {
        base.ChangeCell(nextCell);

        // Propagate movement to segments
        // if there is only a head, no need to propogate
        if (snakeSegments.Count > 1)
        {
            for (LinkedListNode<GridCellOccupant> node = snakeSegments.First.Next; node != null; node = node.Next)
            {
                node.Value.ChangeCell(node.Previous.Value.PreviousCell);
            }
        }
    }

    private IEnumerator Movement()
    {
        yield return new WaitForSeconds(spawnDelay);

        snakeEnabled = true;
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();

        if (breakAheadCooldownTimer > 0)
        {
            breakAheadCooldownTimer -= Time.deltaTime;
        }
        else
        {
            breakAheadCooldownTimer = 0;
        }
        if (breakAroundCooldownTimer > 0)
        {
            breakAroundCooldownTimer -= Time.deltaTime;
        }
        else
        {
            breakAroundCooldownTimer = 0;
        }
        breakAheadCooldownPercent.Value = breakAheadCooldownTimer / breakAheadCooldown;
        breakAroundCooldownPercent.Value = breakAroundCooldownTimer / breakAroundCooldown;

        // Update forward
        transform.forward = new Vector3(direction.x, 0, direction.y);

        // Track movement
        if (moveTimer > 0)
        {
            moveTimer -= Time.deltaTime;
            moving = false;
        }
        else
        {
            moving = true;
            moveTimer = timeBetweenMoves;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (lastDirectionMoved != Vector2Int.down)
            {
                direction = Vector2Int.up;
                if (snakeEnabled && lastDirectionMoved != direction)
                {
                    moving = true;
                    moveTimer = timeBetweenMoves;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (lastDirectionMoved != Vector2Int.right)
            {
                direction = Vector2Int.left;
                if (snakeEnabled && lastDirectionMoved != direction)
                {
                    moving = true;
                    moveTimer = timeBetweenMoves;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (lastDirectionMoved != Vector2Int.up)
            {
                direction = Vector2Int.down;
                if (snakeEnabled && lastDirectionMoved != direction)
                {
                    moving = true;
                    moveTimer = timeBetweenMoves;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (lastDirectionMoved != Vector2Int.left)
            {
                direction = Vector2Int.right;
                if (snakeEnabled && lastDirectionMoved != direction)
                {
                    moving = true;
                    moveTimer = timeBetweenMoves;
                }
            }
        }

        if (!snakeEnabled)
        {
            return;
        }

        // Pierce Shoot (Temporary)
        if (Input.GetKeyDown(KeyCode.Alpha1) && breakAheadCooldownTimer <= 0 && !isPreppingPierce && !isPreppingAoE)
        {
            StartCoroutine(BreakAhead());
        }

        // AoE (Temporary)
        if (Input.GetKeyDown(KeyCode.Alpha2) && breakAroundCooldownTimer <= 0 && !isPreppingAoE && !isPreppingPierce)
        {
            StartCoroutine(BreakAround());
        }

        if (!moving)
        {
            return;
        }

        if (isPreppingPierce)
            SetSelectedCellBreakAhead(setRange);
        else if (isPreppingAoE)
            SetSelectedCellBreakAround(setRange);
        Move();
    }

    private List<GridCell> selectedCells = new List<GridCell>();
    private int setRange;

    private IEnumerator BreakAhead()
    {
        isPreppingPierce = true;

        float currentRange = minBreakAheadRange;
        setRange = Mathf.FloorToInt(currentRange);

        while (Input.GetKey(KeyCode.Alpha1))
        {
            // 
            if (currentRange <= maxBreakAheadRange)
                currentRange += Time.deltaTime * gainBreakAheadRate;
            else
                currentRange = maxBreakAheadRange;

            if (currentRange > setRange)
            {
                setRange = Mathf.FloorToInt(currentRange);
                SetSelectedCellBreakAhead(setRange);
            }
            yield return null;
        }

        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;

            cell.BreakDestroyables();
            cell.SetSelected(false);
        }

        setRange = 0;
        isPreppingPierce = false;

        breakAheadCooldownTimer = breakAheadCooldown;
    }

    private void SetSelectedCellBreakAhead(int range)
    {
        // Deselect previous cells
        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;
            cell.SetSelected(false);
        }

        // Determine new cells to select
        List<Vector2Int> pathDirections = new List<Vector2Int>();
        for (int i = 0; i < range; i++)
        {
            pathDirections.Add(direction);
        }
        selectedCells = currentCell.GetPath(pathDirections, new List<GridCell>());

        // Select new cells
        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;
            cell.SetSelected(true);
        }
    }

    private IEnumerator BreakAround()
    {
        isPreppingAoE = true;

        float currentRange = minBreakAroundRange;
        setRange = Mathf.FloorToInt(currentRange);

        while (Input.GetKey(KeyCode.Alpha2))
        {
            // 
            if (currentRange <= maxBreakAroundRange)
                currentRange += Time.deltaTime * gainBreakAroundRate;
            else
                currentRange = maxBreakAroundRange;

            if (currentRange > setRange)
            {
                setRange = Mathf.FloorToInt(currentRange);
                SetSelectedCellBreakAround(setRange);
            }
            yield return null;
        }

        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;

            cell.BreakDestroyables();
            cell.SetSelected(false);
        }

        setRange = 0;
        isPreppingAoE = false;
        breakAroundCooldownTimer = breakAroundCooldown;
    }

    private void SetSelectedCellBreakAround(int range)
    {
        // Deselect previous cells
        foreach (GridCell cell in selectedCells)
        {
            if (!cell) continue;
            cell.SetSelected(false);
        }

        // Determine new cells to select
        selectedCells = currentCell.GetNeighbours(false, true, Mathf.RoundToInt(range));

        // Select new cells
        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;
            cell.SetSelected(true);
        }
    }

    private void Move()
    {
        GridCell nextCell;
        if (teleportFlag)
        {
            nextCell = GridGenerator._Instance.FindUnoccupiedCell();
            teleportFlag = false;
        }
        else
        {
            nextCell = currentCell.GetNeighbour(direction);
        }

        // Restart scene if player loses
        if (nextCell.IsBorderWall())
        {
            UIManager._Instance.OpenLoseScreen();
            snakeEnabled = false;
        }
        else if (nextCell.IsOccupiedByObstruction())
        {
            if (allowedCollisions.Value > 0)
            {
                allowedCollisions.Value--;
            }
            else
            {
                UIManager._Instance.OpenLoseScreen();
                snakeEnabled = false;
            }
        }

        lastDirectionMoved = direction;
        ChangeCell(nextCell);
    }

    public void Grow()
    {
        // Track number of segments
        segmentsCounter.Value++;

        // Add new segments
        GridCell spawnCell = snakeSegments.Last.Value.PreviousCell;
        if (spawnCell == null)
            spawnCell = snakeSegments.Last.Value.CurrentCell.GetNeighbour(false, false);
        GridCellOccupant spawnedSegment = spawnCell.SpawnOccupant(snakeTailPrefab);
        snakeSegments.AddLast(spawnedSegment);
    }

    public void AddAllowedCollision()
    {
        allowedCollisions.Value++;
    }

    public void Teleport()
    {
        teleportFlag = true;
    }
}
