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

    [Header("Powerups")]
    [Header("Ghosting")]
    [SerializeField] private float ghostingOpacity;

    [Header("Bombs")]
    [Header("Break Ahead")]
    [SerializeField] private float breakAheadRange;
    [SerializeField] private float gainBreakAheadRate;

    [Header("Break Around")]
    [SerializeField] private float breakAroundRange;
    [SerializeField] private float gainBreakAroundRate;

    [Header("References")]
    [SerializeField] private GridCellOccupant snakeTailPrefab;
    private LinkedList<GridCellOccupant> snakeSegments = new LinkedList<GridCellOccupant>();
    private Material snakeMat;

    [Header("Scriptable Objects")]
    [SerializeField] private IntStore segmentsCounter;
    [SerializeField] private IntStore bombStore;
    [SerializeField] private FloatStore isGhostedTimer;

    // Tracking Variables
    private bool hasLost;
    private Vector2Int lastDirectionMoved;
    private bool snakeEnabled;
    private bool moving;
    private float moveTimer;

    private GridCell teleportToCell;

    private bool isGhosted;

    // Swiping
    Vector2 firstPressPos;
    Vector2 secondPressPos;
    Vector2 currentSwipe;

    [SerializeField] private int foodBetweenCards;

    private void Start()
    {
        // Set forward vector
        transform.forward = new Vector3(direction.x, 0, direction.y);

        // Reset values
        segmentsCounter.Reset();
        bombStore.Reset();
        moveTimer = timeBetweenMoves;

        // Get a reference to the snakes renderer
        snakeMat = GetComponent<Renderer>().material;

        // Add head to linked list
        snakeSegments.AddFirst(this);
    }

    public void StartMoving()
    {
        snakeEnabled = true;
    }

    public void StopMoving()
    {
        snakeEnabled = false;
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

    // Update is called once per frame
    protected new void Update()
    {
        if (hasLost) return;

        base.Update();

        // Update forward
        transform.forward = new Vector3(direction.x, 0, direction.y);

        // Change Directions
        GameControls();
        MobileControls();

        if (!snakeEnabled)
        {
            return;
        }

        // If the isGhostedTimer is greater than 0, the snake is ghosted
        if (isGhostedTimer.Value > 0)
        {
            isGhosted = true;
            isGhostedTimer.Value -= Time.unscaledDeltaTime;

            // Change opacity if snake is ghosting
            if (snakeMat.color.a != ghostingOpacity)
            {
                Color tmp = snakeMat.color;
                tmp.a = ghostingOpacity;
                snakeMat.color = tmp;
            }
        }
        else
        {
            isGhosted = false;

            // Change opacity if snake is ghosting
            if (snakeMat.color.a == ghostingOpacity)
            {
                Color tmp = snakeMat.color;
                tmp.a = 1;
                snakeMat.color = tmp;
            }
        }

        // Track movement / control when it happens
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

        if (!moving)
        {
            return;
        }

        Move();
    }

    private void ChangeDirection(Vector2Int disallowDirection, Vector2Int setDirection)
    {
        if (lastDirectionMoved != disallowDirection)
        {
            direction = setDirection;
            if (snakeEnabled && lastDirectionMoved != direction)
            {
                moving = true;
                moveTimer = 0;
            }
        }
    }

    private IEnumerator BreakAhead()
    {
        List<GridCell> selectedCells = new List<GridCell>();
        GridCell startCell = currentCell;
        int currentRange = 0;

        while (currentRange < breakAheadRange)
        {
            // increment current range
            currentRange++;
            selectedCells = SetSelectedCellBreakAhead(currentRange, startCell, selectedCells);

            // Wait some time
            yield return new WaitForSeconds(gainBreakAheadRate);

            // redo
            yield return null;
        }

        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;

            cell.BreakDestroyables();
            cell.SetSelected(false);
        }
    }

    private List<GridCell> SetSelectedCellBreakAhead(int range, GridCell origin, List<GridCell> selectedCells)
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
        selectedCells = origin.GetPath(pathDirections, new List<GridCell>());

        // Select new cells
        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;
            cell.SetSelected(true);
        }
        return selectedCells;
    }

    private IEnumerator BreakAround()
    {
        List<GridCell> selectedCells = new List<GridCell>();
        GridCell startCell = currentCell;
        int currentRange = 0;

        while (currentRange < breakAroundRange)
        {
            // increment current range
            currentRange++;
            selectedCells = SetSelectedCellBreakAround(currentRange, startCell, selectedCells);

            // Wait some time
            yield return new WaitForSeconds(gainBreakAroundRate);

            // redo
            yield return null;
        }

        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;

            cell.BreakDestroyables();
            cell.SetSelected(false);
        }
    }

    private List<GridCell> SetSelectedCellBreakAround(int range, GridCell origin, List<GridCell> selectedCells)
    {
        // Deselect previous cells
        foreach (GridCell cell in selectedCells)
        {
            if (!cell) continue;
            cell.SetSelected(false);
        }

        // Determine new cells to select
        selectedCells = origin.GetNeighbours(false, true, Mathf.RoundToInt(range));

        // Select new cells
        foreach (GridCell cell in selectedCells)
        {
            // Recieved a null cell
            if (!cell) continue;
            cell.SetSelected(true);
        }
        return selectedCells;
    }

    private void Move()
    {
        GridCell nextCell;
        if (teleportToCell)
        {
            nextCell = teleportToCell;
            teleportToCell = null;
        }
        else
        {
            nextCell = currentCell.GetNeighbour(direction);
        }

        // Lose conditions
        // hitting a nothing 
        // hitting a border wall
        // hitting an obstruction while not ghosted
        if (!nextCell || nextCell.IsBorderWall() || (nextCell.IsOccupiedByObstruction() &&
            !isGhosted))
        {
            hasLost = true;
            UIManager._Instance.OpenLoseScreen();
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

    public void Teleport(GridCell teleportToCell)
    {
        this.teleportToCell = teleportToCell;
    }

    public void SetGhost(float duration)
    {
        isGhostedTimer.Value += duration;
    }

    public void TryPlaceBomb()
    {
        if (bombStore.Value <= 0) return;
        StartCoroutine(BreakAround());
        bombStore.Value--;
    }

    private void GameControls()
    {
        // Up
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeDirection(Vector2Int.down, Vector2Int.up);
        }
        // Left
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeDirection(Vector2Int.right, Vector2Int.left);
        }
        // Down
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeDirection(Vector2Int.up, Vector2Int.down);
        }
        // Right
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeDirection(Vector2Int.left, Vector2Int.right);
        }

        // Place Bomb
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryPlaceBomb();
        }
    }

    private void MobileControls()
    {
        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                //save began touch 2d point
                firstPressPos = new Vector2(t.position.x, t.position.y);
            }
            if (t.phase == TouchPhase.Ended)
            {
                //save ended touch 2d point
                secondPressPos = new Vector2(t.position.x, t.position.y);

                //create vector from the two points
                currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

                //normalize the 2d vector
                currentSwipe.Normalize();

                //swipe upwards
                if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                {
                    ChangeDirection(Vector2Int.down, Vector2Int.up);
                }
                //swipe down
                if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                {
                    ChangeDirection(Vector2Int.up, Vector2Int.down);
                }
                //swipe left
                if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                {
                    ChangeDirection(Vector2Int.right, Vector2Int.left);
                }
                //swipe right
                if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                {
                    ChangeDirection(Vector2Int.left, Vector2Int.right);
                }
            }
        }
    }
}
