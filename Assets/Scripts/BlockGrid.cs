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
    [SerializeField]private List<GameObject> blockPrefabs;
    private Element[,,] grid = new Element[32, 32, 32];
    // Start is called before the first frame update
    void Start()
    {
        for(int x = 0;x<32;x++)
        for(int y = 0;y<32;y++)
        for(int z = 0;z<32;z++){
            grid[z,y,x] = new Element()
            {
                type = (short)Random.Range(0,blockPrefabs.Count),
                orientation = 0,
                rotation = 0,
            };
            Instantiate(blockPrefabs[grid[z,y,x].type], new Vector3(x, y, z), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
