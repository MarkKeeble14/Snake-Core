using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private Color selectedColor;
    private Color defaultColor;

    private Material material;

    private int x, y;

    public bool isOccupied => occupants.Count > 0;

    public bool Selected { get; private set; }

    private List<GridCellOccupant> occupants = new List<GridCellOccupant>();

    [SerializeField] private Renderer topRenderer;
    [SerializeField] private int materialIndex = 1;

    [Header("Polish")]
    [SerializeField] private GameObject[] onBreakParticles;

    private void Awake()
    {
        material = topRenderer.materials[materialIndex];
        defaultColor = material.color;
    }

    public void Set(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void SetSelected(bool v)
    {
        Selected = v;
        SetSelectedColor();
    }

    public void SetSelectedColor()
    {
        if (Selected)
            material.color = selectedColor;
        else
            material.color = defaultColor;
    }

    public GridCell GetNeighbour(Vector2Int neighbouringDirection)
    {
        try
        {
            return GridGenerator._Instance.GridCells[x + neighbouringDirection.x, y + neighbouringDirection.y];
        }
        catch // Tried to access outside of array bounds
        {
            return null;
        }
    }

    public bool HasObstructionOccupant()
    {
        foreach (GridCellOccupant occupant in occupants)
        {
            if (occupant.IsObstruction)
                return true;
        }
        return false;
    }

    public bool IsBorderWall()
    {
        foreach (GridCellOccupant occupant in occupants)
        {
            if (occupant.IsBorderWall)
                return true;
        }
        return false;
    }

    public bool IsOccupiedByObstruction()
    {
        return isOccupied && HasObstructionOccupant();
    }

    public void AddOccupant(GridCellOccupant occupant)
    {
        occupants.Add(occupant);
    }

    public void TriggerEvents()
    {
        for (int i = 0; i < occupants.Count;)
        {
            GridCellOccupant occupant = occupants[i];
            if (occupant.HasEvents)
                occupant.TriggerEvents();
            if (occupant) // Occupant has not destroyed itself
                i++;
        }
    }

    public bool RemoveOccupant(GridCellOccupant occupant)
    {
        bool containedOccupant = occupants.Contains(occupant);
        occupants.Remove(occupant);
        return containedOccupant;
    }

    public GridCellOccupant SpawnOccupant(GridCellOccupant obj)
    {
        GridCellOccupant occupant = Instantiate(obj, transform.position, Quaternion.identity);
        occupants.Add(occupant);
        occupant.SetToCell(this);
        return occupant;
    }

    public List<GridCell> GetPath(List<Vector2Int> directions, List<GridCell> path)
    {
        if (directions.Count == 0) return path;
        GridCell nextNode = GetNeighbour(directions[0]);
        if (nextNode == null) return path;
        directions.Remove(directions[0]);
        path.Add(nextNode);
        return nextNode.GetPath(directions, path);
    }

    private List<Vector2Int> GetNeighbouringDirections(bool includeDiagonals)
    {
        List<Vector2Int> neighbouringDirections = new List<Vector2Int> { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };
        if (includeDiagonals)
            neighbouringDirections.AddRange(new List<Vector2Int> { new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(1, 1) });
        return neighbouringDirections;
    }

    public GridCell GetNeighbour(bool includeDiagonals, bool allowObstructed, bool allowSelected)
    {
        List<GridCell> options = GetNeighbours(includeDiagonals, allowObstructed, allowSelected);
        if (options.Count == 0) return null;
        return options[UnityEngine.Random.Range(0, options.Count)];
    }

    public List<GridCell> GetNeighbours(bool includeDiagonals, bool allowObstructed, bool allowSelected, int steps = 1)
    {
        List<Vector2Int> neighbouringDirections = GetNeighbouringDirections(includeDiagonals);
        List<GridCell> neighbours = new List<GridCell>();

        foreach (Vector2Int direction in neighbouringDirections)
            neighbours.Add(GetNeighbour(direction));

        // One less step
        steps--;

        // if out of steps, return list
        if (steps == 0)
        {
            return neighbours;
        }
        else // otherwise
        {
            List<GridCell> nextNeighbours = new List<GridCell>();
            // add the neighbours of all previous neighbours to the list as well
            foreach (GridCell cell in neighbours)
            {
                // if recieved a null cell, we can skip it
                if (!cell) continue;

                if (!allowSelected && cell.Selected) continue;

                // Get all neighbours of the other cell
                List<GridCell> cellNeighbours = cell.GetNeighbours(includeDiagonals, allowObstructed, allowSelected, steps);
                foreach (GridCell cellNeighbour in cellNeighbours)
                {
                    // Make sure we don't add duplicates
                    if (neighbours.Contains(cellNeighbour))
                    {
                        continue;
                    }
                    // Make sure we don't add obstructed cells if specified
                    if (!allowObstructed && cellNeighbour.IsOccupiedByObstruction())
                    {
                        continue;
                    }

                    // Passed checks, valid neighbour
                    nextNeighbours.Add(cellNeighbour);
                }
            }
            neighbours.AddRange(nextNeighbours);
            return neighbours;
        }
    }

    public void BreakObstructions()
    {
        // Spawn Particles
        SpawnOnBreakParticles();

        for (int i = 0; i < occupants.Count;)
        {
            GridCellOccupant occupant = occupants[i];
            if (occupant.IsObstruction && !occupant.IsBorderWall)
            {
                occupant.Break();
            }
            else
            {
                i++;
            }
        }
    }

    public void BreakDestroyables()
    {
        // Spawn Particles
        SpawnOnBreakParticles();

        for (int i = 0; i < occupants.Count;)
        {
            GridCellOccupant occupant = occupants[i];
            if (occupant.IsDestroyable && !occupant.IsBorderWall)
            {
                if (!occupant.Break())
                {
                    i++;
                }
            }
            else
            {
                i++;
            }
        }
    }

    private void SpawnOnBreakParticles()
    {
        foreach (GameObject particles in onBreakParticles)
        {
            Instantiate(particles, transform.position, Quaternion.identity);
        }
    }

    public void Delete()
    {
        for (int i = 0; i < occupants.Count;)
        {
            GridCellOccupant occupant = occupants[i];
            if (occupant.IsSnake)
            {
                i++;
                continue;
            };
            occupant.Break();
        }
        Destroy(gameObject);
    }
}
