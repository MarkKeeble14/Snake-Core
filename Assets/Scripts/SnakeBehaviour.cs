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
    [SerializeField] private float timeBetweenTicks = .25f;

    private LinkedList<GridCellOccupant> snakeSegments = new LinkedList<GridCellOccupant>();
    [SerializeField] private GridCellOccupant snakeTailPrefab;

    [SerializeField] private IntStore segmentsCounter;
    [SerializeField] private IntStore allowedCollisions;

    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private float teleportDelay = 1f;

    private bool teleportFlag;

    private void Start()
    {
        // Reset value
        segmentsCounter.Reset();
        allowedCollisions.Reset();

        // Add head to linked list
        snakeSegments.AddFirst(this);

        StartCoroutine(Movement(true));
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

    private IEnumerator Movement(bool useDelay)
    {
        if (useDelay)
            yield return new WaitForSeconds(spawnDelay);

        GridCell nextCell;
        bool teleported = false;
        if (teleportFlag)
        {
            nextCell = GridGenerator._Instance.FindUnoccupiedCell();
            teleportFlag = false;
            teleported = true;
        }
        else
        {
            nextCell = currentCell.GetNeighbour(direction);
        }

        // Restart scene if run into an obstruction

        if (nextCell.HasInstantLossOccupant())
        {
            UIManager._Instance.OpenLoseScreen();
            yield break;
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
                yield break;
            }
        }

        lastDirectionMoved = direction;
        ChangeCell(nextCell);

        if (teleported)
            yield return new WaitForSeconds(teleportDelay);

        yield return new WaitForSeconds(timeBetweenTicks);

        StartCoroutine(Movement(false));
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (lastDirectionMoved != Vector2Int.down)
                direction = Vector2Int.up;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (lastDirectionMoved != Vector2Int.right)
                direction = Vector2Int.left;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (lastDirectionMoved != Vector2Int.up)
                direction = Vector2Int.down;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (lastDirectionMoved != Vector2Int.left)
                direction = Vector2Int.right;
        }
    }

    public void Grow()
    {
        // Track number of segments
        segmentsCounter.Value++;

        // Add new segments
        GridCell spawnCell = snakeSegments.Last.Value.PreviousCell;
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
