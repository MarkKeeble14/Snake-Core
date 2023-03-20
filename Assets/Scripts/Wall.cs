using System;
using UnityEngine;

public class Wall : GridCellOccupant
{
    [Header("Wall Settings")]
    [SerializeField] private SerializableDictionary<WallType, Color[]> colorOptions = new SerializableDictionary<WallType, Color[]>();

    private Material material;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    public void SetWallType(WallType type)
    {
        Color[] options = colorOptions[type];
        material.SetColor("_BaseColor", options[UnityEngine.Random.Range(0, options.Length)]);
    }
}
