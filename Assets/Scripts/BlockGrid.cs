using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using Valve.VR.Extras;
struct Element{
    public short type;
    public byte orientation;
    public byte rotation;
}


public class BlockGrid : MonoBehaviour
{
    [SerializeField] private List<GameObject> blockPrefabs;
    [SerializeField] private int size = 16;
    [SerializeField] private SteamVR_LaserPointer leftPointer;
    [SerializeField] private SteamVR_LaserPointer rightPointer;

    public SteamVR_Action_Vector2 rotateGrid = SteamVR_Input.GetVector2Action("rotate");
    public float rotationSpeed = 100f;

    private float scale = 0.2f;


    private Element[,,] grid;

    private void Awake()
    {
        leftPointer.PointerClick += OnPointerClick;
        rightPointer.PointerClick += OnPointerDelete;
    }

    void Start()
    {
        grid = new Element[size, size, size];
        scale = blockPrefabs[0].transform.localScale.x;

        Instantiate(blockPrefabs[1], transform.TransformPoint(new Vector3(size/2 + .5f, size / 2 + .5f, size / 2 + .5f) * scale), Quaternion.identity, transform);

        /*for (int x = 0; x < size; x++)
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
                        Instantiate(blockPrefabs[grid[z, y, x].type], transform.TransformPoint(new Vector3(x + .5f, y + .5f, z + .5f) * scale), Quaternion.identity, transform);
                }*/
    }

    void Update()
    {
        /*Vector2 inputRight = rotateGrid.GetAxis(SteamVR_Input_Sources.RightHand);
        Vector2 inputLeft = rotateGrid.GetAxis(SteamVR_Input_Sources.LeftHand);

        Vector2 input = inputRight != Vector2.zero ? inputRight : inputLeft;*/

        Vector2 deltaRight = rotateGrid[SteamVR_Input_Sources.RightHand].delta;
        Vector2 deltaLeft = rotateGrid[SteamVR_Input_Sources.LeftHand].delta;

        Vector2 delta = deltaRight != Vector2.zero ? deltaRight : deltaLeft;
        

        if (delta != Vector2.zero)
        {
            float yRotation = delta.x * rotationSpeed;

            transform.Rotate(0f, yRotation, 0f, Space.World);
        }
    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        Vector3 gridPos = transform.InverseTransformPoint(leftPointer.transform.TransformPoint(Vector3.forward * (e.distance - 0.001f))) / (scale);
        Vector3Int gridIdx = Vector3Int.FloorToInt(gridPos);

        if (gridIdx.x >= size || gridIdx.y >= size || gridIdx.z >= size)
            return;

        grid[gridIdx.z, gridIdx.y, gridIdx.x] = new Element()
        {
            type = 2,
            orientation = 0,
            rotation = 0,
        };
        Instantiate(blockPrefabs[grid[gridIdx.z, gridIdx.y, gridIdx.x].type], transform.TransformPoint(new Vector3(gridIdx.x + .5f, gridIdx.y + .5f, gridIdx.z + .5f) * scale), Quaternion.identity, transform);
    }

    private void OnPointerDelete(object sender, PointerEventArgs e)
    {
        Vector3 gridPos = transform.InverseTransformPoint(rightPointer.transform.TransformPoint(Vector3.forward * (e.distance + 0.001f))) / (scale);
        Vector3Int gridIdx = Vector3Int.FloorToInt(gridPos);

        if (gridIdx == new Vector3Int(size/2, size/2, size/2))
            return;

        foreach (Transform child in transform) 
        {
            Vector3Int index = Vector3Int.FloorToInt(child.localPosition / scale);
            if (index == gridIdx)
            {
                Destroy(child.gameObject);
            }
        }
    }
}