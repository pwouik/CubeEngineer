using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
struct Element{
    public short type;
    public byte orientation;
    public byte rotation;
}


public class BlockGrid : MonoBehaviour
{
    [SerializeField] private List<GameObject> blockPrefabs;
    [SerializeField] private int size = 16;
    private Element[,,] grid;
    // Start is called before the first frame update
    void Start()
    {
        grid = new Element[size, size, size];
        float scale = blockPrefabs[0].transform.localScale.x;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    grid[z, y, x] = new Element()
                    {
                        type = (short)Math.Max(0, UnityEngine.Random.Range(0, blockPrefabs.Count + 16) - 16),
                        orientation = 0,
                        rotation = 0,
                    };
                    if (grid[z, y, x].type != 0)
                        Instantiate(blockPrefabs[grid[z, y, x].type], new Vector3(x + .5f, y + .5f, z + .5f) * scale + transform.position, Quaternion.identity, transform);
                }
    }

    // Update is called once per frame
    void Update()
    {

    }
}