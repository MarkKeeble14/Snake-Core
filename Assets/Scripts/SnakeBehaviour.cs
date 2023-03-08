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

    [SerializeField] private Vector2Int direction = Vector2Int.right;
    private Vector2Int lastDirectionMoved;
    [SerializeField] private float timeBetweenMoves = .25f;

    private LinkedList<GridCellOccupant> snakeSegments = new LinkedList<GridCellOccupant>();
    [SerializeField] private GridCellOccupant snakeTailPrefab;

    [SerializeField] private IntStore segmentsCounter;
    [SerializeField] private IntStore allowedCollisions;

    [SerializeField] private float spawnDelay = 1f;

    private bool snakeEnabled;
    private bool moving;
    private float moveTimer;

    private bool teleportFlag;

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

        // Update forward
        transform.forward = new Vector3(direction.x, 0, direction.y);

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

        if (!moving)
        {
            return;
        }

        Move();
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
        if (nextCell.HasInstantLossOccupant())
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
        {
            spawnCell = snakeSegments.Last.Value.CurrentCell.GetUnobstructedNeighbour();
        }
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
