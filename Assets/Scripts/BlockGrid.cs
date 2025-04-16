using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
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
    public SteamVR_Action_Boolean play = SteamVR_Input.GetBooleanAction("play");

    private float scale = 0.2f;

    private Vector3 startPos;
    private Vector2 lastPos = Vector2.zero;
    private Vector3 origin;

    private Rigidbody rb;
    private Element[,,] grid;

    private void Awake()
    {
        leftPointer.PointerClick += OnPointerClick;
        rightPointer.PointerClick += OnPointerDelete;
    }

    private void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();

        scale = blockPrefabs[0].transform.localScale.x;

        origin = new Vector3(size / 2 + .5f, size / 2 + .5f, size / 2 + .5f) * scale;
        grid = new Element[size, size, size];

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        for (int z = 0; z < size; z++)
        {
            grid[z, y, x] = new Element()
            {
                type = 0,
                orientation = 0,
                rotation = 0,
            };
        }

        grid[size / 2, size / 2, size / 2] = new Element()
        {
            type = 2,
            orientation = 0,
            rotation = 0,
        };
        Instantiate(blockPrefabs[grid[size / 2, size / 2, size / 2].type], transform.TransformPoint(Vector3.zero), Quaternion.identity, transform);
    }

    private void Update()
    {
        RotateGrid();

        rb.isKinematic = !play.GetState(SteamVR_Input_Sources.RightHand);

        if (play.GetStateUp(SteamVR_Input_Sources.RightHand))
        {
            transform.position = startPos;
            transform.rotation = Quaternion.identity;
        }
    }

    private void RotateGrid()
    {
        Vector2 inputRight = rotateGrid.GetAxis(SteamVR_Input_Sources.RightHand);
        Vector2 inputLeft = rotateGrid.GetAxis(SteamVR_Input_Sources.LeftHand);

        Vector2 pos = inputRight != Vector2.zero ? inputRight : inputLeft;

        if (pos != Vector2.zero && lastPos != Vector2.zero)
        {
            float angleInitial = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;
            float angleFinal = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;

            float angle = Mathf.DeltaAngle(angleFinal, angleInitial);

            transform.Rotate(0f, angle, 0f, Space.World);
        }

        lastPos = pos;
    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        if (play.GetState(SteamVR_Input_Sources.RightHand)) return;

        Vector3 gridPos = (transform.InverseTransformPoint(leftPointer.transform.TransformPoint(Vector3.forward * (e.distance - 0.01f))) + origin) / scale;
        Vector3Int gridIdx = Vector3Int.FloorToInt(gridPos);

        if (gridIdx.x >= size || gridIdx.y >= size || gridIdx.z >= size)
            return;

        grid[gridIdx.z, gridIdx.y, gridIdx.x] = new Element()
        {
            type = 1,
            orientation = 0,
            rotation = 0,
        };
        Instantiate(blockPrefabs[grid[gridIdx.z, gridIdx.y, gridIdx.x].type], transform.TransformPoint(new Vector3(gridIdx.x + .5f, gridIdx.y + .5f, gridIdx.z + .5f) * scale - origin), e.target.transform.rotation, transform);
    }

    private void OnPointerDelete(object sender, PointerEventArgs e)
    {
        if (play.GetState(SteamVR_Input_Sources.RightHand)) return;

        Vector3 gridPos = (transform.InverseTransformPoint(rightPointer.transform.TransformPoint(Vector3.forward * (e.distance + 0.01f))) + origin ) / scale;
        Vector3Int gridIdx = Vector3Int.FloorToInt(gridPos);

        Debug.Log(gridIdx);
        if (gridIdx == new Vector3Int(size/2, size/2, size/2))
            return;
        foreach (Transform child in transform) 
        {
            Vector3Int index = Vector3Int.FloorToInt((child.localPosition + origin) / scale);
            Debug.Log(index);
            if (index == gridIdx)
            {

                grid[gridIdx.z, gridIdx.y, gridIdx.x] = new Element()
                {
                    type = 0,
                    orientation = 0,
                    rotation = 0,
                };
                Destroy(child.gameObject);
            }
        }
    }
}