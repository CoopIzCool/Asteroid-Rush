using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Code developed by Ryan Cooper in 2020/ refurbished in 2023
/// </summary>
public class CameraFixedRotation : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private float radius;
    private float counter;
    //private float yPos;
    private float xRotate;
    //private float xRotateSensitivity;
    private bool itemHeld;
    [SerializeField]
    private Transform centerPoint;

    [SerializeField] private float inactiveTime;
    private float activeTimer;
    private bool camActive = true;

    [SerializeField] private float xShiftIndex;
    [SerializeField] private float zShiftIndex;
    [SerializeField] private float radiusOffset;

    private float centerPointShift = 10.0f;
    #endregion Fields

    #region Properties
    public bool ItemIsHeld
    {
        set { itemHeld = value; }
    }

    public bool CamActive
    {
        get { return camActive; }
    }

    public float XShift
    {
        get { return xShiftIndex; }
        set { xShiftIndex = value; }
    }
    public float ZShift
    {
        get { return zShiftIndex; }
        set { zShiftIndex = value; }
    }
    #endregion Properties
    // Start is called before the first frame update
    void Start()
    {
        //radius = 12.0f;
        //yPos = transform.position.y;
        counter = 180;
        xRotate = Mathf.PI / 4;
        //xRotateSensitivity = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        bool isActive = false;
        //increments counter;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            counter += 36.0f * Time.deltaTime;
            SetActive();
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            counter -= 36.0f * Time.deltaTime;
            SetActive();
        }

        //If the player is not holding an item
        if(!itemHeld)
        {
            //camera Zoom
            if (Input.mouseScrollDelta.y > 0.0f && GetComponent<Camera>().fieldOfView > 20.0f)
            {
                GetComponent<Camera>().fieldOfView -= 720.0f * Time.deltaTime;
                //xRotateSensitivity -= 8.00f * Time.deltaTime;
                SetActive();
            }
            else if (Input.mouseScrollDelta.y < 0.0f && GetComponent<Camera>().fieldOfView < 90.0f)
            {
                GetComponent<Camera>().fieldOfView += 720.0f * Time.deltaTime;
                //xRotateSensitivity += 8.00f * Time.deltaTime;
                SetActive();
            }

        }
        //changes cameras horizantal view
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            xRotate -= 0.6f * Time.deltaTime;
            SetActive();
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            xRotate += 0.6f * Time.deltaTime;
            SetActive();
        }

        //reset counter for easy calculations
        
        if (counter >= 360.0f || counter <= -360.0f)
        {
            counter = 0.0f;
        }

        //Code for centerpoint shifting
        /*
        //Shift the center Point
        if(Input.GetKey(KeyCode.I))
        {
            float centerShiftAxis = Mathf.Clamp(((centerPointShift * Time.deltaTime) + centerPoint.position.z),0,zShiftIndex * 2);

            centerPoint.position = new Vector3 (centerPoint.position.x, centerPoint.position.z,centerShiftAxis);
        }
        else if (Input.GetKey(KeyCode.K))
        {
            float centerShiftAxis = Mathf.Clamp((centerPoint.position.z - (centerPointShift * Time.deltaTime)), 0, zShiftIndex * 2);
            centerPoint.position = new Vector3(centerPoint.position.x, centerPoint.position.z, centerShiftAxis);
        }

        //Shift the center Point
        if (Input.GetKey(KeyCode.L))
        {
            centerPoint.position += new Vector3(centerPointShift * Time.deltaTime, 0,0);
        }
        else if (Input.GetKey(KeyCode.J))
        {
            centerPoint.position += new Vector3(centerPointShift * Time.deltaTime * -1.0f, 0, 0);
        }
        */ 

        //Clamp XRotation to prevent Axis Flipping
        xRotate = Mathf.Clamp(xRotate, 0.1f, Mathf.PI/2);
        //Debug.Log(xRotate);
        //convert x and y to radians
        float radians = counter * (Mathf.PI / 180.0f);
        float x = Mathf.Sin(radians) * Mathf.Sin(xRotate) * radius;
        float y = Mathf.Cos(xRotate) * radius;
        float z = Mathf.Cos(radians) * Mathf.Sin(xRotate) * radius;
        //set position and rotation
        transform.position = new Vector3(x + xShiftIndex, y, z+ zShiftIndex);
        transform.LookAt(centerPoint);
        if(!isActive && activeTimer < inactiveTime)
        {
            activeTimer += Time.deltaTime;

            if(activeTimer >= inactiveTime)
            {
                camActive = false;
            }
        }
    }

    public void SetActive()
    {
        camActive = true;
        activeTimer = 0f;
    }

    public void SetRadiusAndCenter()
    {
        //Set radius to be the grid dimentions plus an offset
        radius = ((xShiftIndex + zShiftIndex) / 2.0f) + radiusOffset;
        centerPoint.position = new Vector3(xShiftIndex, 2.0f, zShiftIndex);
    }
}
