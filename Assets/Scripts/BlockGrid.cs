using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.WSA;
using Valve.VR;
using Valve.VR.Extras;

public class BlockGrid : MonoBehaviour
{
    [SerializeField] private List<GameObject> blockPrefabs;
    [SerializeField] private Transform currentObject;
    [SerializeField] private SteamVR_LaserPointer leftPointer;
    [SerializeField] private SteamVR_LaserPointer rightPointer;

    public SteamVR_Action_Vector2 rotateGrid = SteamVR_Input.GetVector2Action("rotate");
    public SteamVR_Action_Boolean liftUp = SteamVR_Input.GetBooleanAction("liftUp");
    public SteamVR_Action_Boolean liftDown = SteamVR_Input.GetBooleanAction("liftDown");
    public SteamVR_Action_Boolean selectNext = SteamVR_Input.GetBooleanAction("selectNext");
    public SteamVR_Action_Boolean selectPrevious = SteamVR_Input.GetBooleanAction("selectPrevious");
    public SteamVR_Action_Boolean play = SteamVR_Input.GetBooleanAction("play");

    private float scale = 0.2f;

    private Vector3 startPos;
    private Vector2 lastPos = Vector2.zero;
    private Vector3 offset;

    private Rigidbody rb;
    private int currentType = 0;

    private void Awake()
    {
        leftPointer.PointerClick += OnPointerClick;
        rightPointer.PointerClick += OnPointerDelete;
    }

    private void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();

        scale = blockPrefabs[currentType].transform.localScale.x;

        GameObject holder = Instantiate(blockPrefabs[currentType], currentObject);
        holder.GetComponent<BoxCollider>().enabled = false;
        holder.transform.localScale = new Vector3(.05f, .05f, .05f);

        offset = new Vector3(.5f, .5f, .5f) * scale;
        Instantiate(blockPrefabs[1], transform.TransformPoint(Vector3.zero), Quaternion.identity, transform);
    }

    private void FixedUpdate()
    {
        LiftGrid();
    }
    
    private void Update()
    {
        RotateGrid();
        SelectObject();

        rb.isKinematic = !play.GetState(SteamVR_Input_Sources.RightHand);
        leftPointer.thickness = (rb.isKinematic ? 0.001f : 0f);
        rightPointer.thickness = (rb.isKinematic ? 0.001f : 0f);

        if (play.GetStateUp(SteamVR_Input_Sources.RightHand))
        {
            transform.position = startPos;
            transform.rotation = Quaternion.identity;
        }
    }

    private void RotateGrid()
    {
        Vector2 pos = rotateGrid.GetAxis(SteamVR_Input_Sources.LeftHand);

        if (pos != Vector2.zero && lastPos != Vector2.zero)
        {
            float angleInitial = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;
            float angleFinal = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;

            float angle = Mathf.DeltaAngle(angleFinal, angleInitial);

            transform.Rotate(0f, angle, 0f, Space.World);
        }

        lastPos = pos;
    }

    private void LiftGrid()
    {
        if (liftUp.GetState(SteamVR_Input_Sources.RightHand))
        {
            rb.MovePosition(transform.position + Vector3.up * Time.deltaTime);
        }
        else if (liftDown.GetState(SteamVR_Input_Sources.RightHand))
        {
            rb.MovePosition(transform.position + Vector3.down * Time.deltaTime);
        }
    }

    private void SelectObject()
    {
        if (selectNext.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            currentType = mod(currentType + 1, blockPrefabs.Count);

            Destroy(currentObject.GetChild(0).gameObject);
            GameObject holder = Instantiate(blockPrefabs[currentType], currentObject);
            holder.GetComponent<BoxCollider>().enabled = false;
            holder.transform.localScale = new Vector3(.05f, .05f, .05f);

        }
        else if (selectPrevious.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            currentType = mod(currentType - 1, blockPrefabs.Count);

            Destroy(currentObject.GetChild(0).gameObject);
            GameObject holder = Instantiate(blockPrefabs[currentType], currentObject);
            holder.GetComponent<BoxCollider>().enabled = false;
            holder.transform.localScale = new Vector3(.05f, .05f, .05f);
        }
    }
    private int mod(int a, int n)
    {
        return (a % n + n) % n;
    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        if (play.GetState(SteamVR_Input_Sources.RightHand)) return;
        if (e.target != transform) return;

        Vector3 gridPos = (transform.InverseTransformPoint(leftPointer.transform.TransformPoint(Vector3.forward * (e.distance - 0.01f))) + offset) / scale;
        Vector3Int gridIdx = Vector3Int.FloorToInt(gridPos);

        Instantiate(blockPrefabs[currentType], transform.TransformPoint(new Vector3(gridIdx.x + .5f, gridIdx.y + .5f, gridIdx.z + .5f) * scale - offset), transform.rotation, transform);
    }

    private void OnPointerDelete(object sender, PointerEventArgs e)
    {
        if (play.GetState(SteamVR_Input_Sources.RightHand)) return;

        Vector3 gridPos = (transform.InverseTransformPoint(rightPointer.transform.TransformPoint(Vector3.forward * (e.distance + 0.01f))) + offset ) / scale;
        Vector3Int gridIdx = Vector3Int.FloorToInt(gridPos);

        if (gridIdx == new Vector3Int(0, 0, 0))
            return;
        foreach (Transform child in transform) 
        {
            Vector3Int index = Vector3Int.FloorToInt((child.localPosition + offset) / scale);
            if (index == gridIdx)
            {
                Destroy(child.gameObject);
            }
        }
    }
}