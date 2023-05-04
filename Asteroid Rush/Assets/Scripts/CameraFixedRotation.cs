using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Code developed by Ryan Cooper in 2020/ refurbished in 2023
/// </summary>
public class CameraFixedRotation : MonoBehaviour
{
    #region Fields
    public enum RotationState
    {
        Left,
        Right,
        None,
        Start
    }

    private RotationState state = RotationState.None;
    private float rotationCounter;
    private float goalRotation;

    [SerializeField]
    private float radius;
    private float counter;
    //private float yPos;
    private float xRotate;
    //private float xRotateSensitivity;
    private bool itemHeld;
    [SerializeField]
    private Transform centerPoint;

    [SerializeField]
    private float movementScalar = 20f;


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
        xRotate = Mathf.PI / 6;
        //xRotateSensitivity = 1.0f;
        //CenterCameraForRotation();
        state = RotationState.Start;
    }

    // Update is called once per frame
    void Update()
    {
        bool isActive = false;
        //increments counter;
        if ((Input.GetKey(KeyCode.J)) && state == RotationState.None )
        {
            //counter += 36.0f * Time.deltaTime;
            state = RotationState.Left;
            goalRotation = counter + 90.0f;
        }
        else if ((Input.GetKey(KeyCode.L)) && state == RotationState.None)
        {
            //counter -= 36.0f * Time.deltaTime;
            state = RotationState.Right;
            goalRotation = counter - 90.0f;
        }

        if(state == RotationState.Left)
        {
            counter += 144.0f * Time.deltaTime;
            if(counter >= goalRotation)
            {
                counter = goalRotation;
                state = RotationState.None;
            }
        }
        else if(state == RotationState.Right)
        {
            counter -= 144.0f * Time.deltaTime;
            if (counter <= goalRotation)
            {
                counter = goalRotation;
                state = RotationState.None;
            }
        }

        
        //camera Zoom
        if (Input.mouseScrollDelta.y > 0.0f && GetComponent<Camera>().fieldOfView > 20.0f)
        {
            GetComponent<Camera>().fieldOfView -= 720.0f * Time.deltaTime;
            //xRotateSensitivity -= 8.00f * Time.deltaTime;
        }
        else if (Input.mouseScrollDelta.y < 0.0f && GetComponent<Camera>().fieldOfView < 90.0f)
        {
            GetComponent<Camera>().fieldOfView += 720.0f * Time.deltaTime;
            //xRotateSensitivity += 8.00f * Time.deltaTime;
        }



        #region Defunct Code
        //changes cameras horizantal view
        /*
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            xRotate -= 0.6f * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            xRotate += 0.6f * Time.deltaTime;
        }
        */

        /* Shift CenterPoint Code, defunct for now.
        if (Input.GetKey(KeyCode.I))
        {
            float centerShiftAxis = Mathf.Clamp(((centerPointShift * Time.deltaTime) + centerPoint.position.z), 0, zShiftIndex * 2);

            centerPoint.position = new Vector3(centerPoint.position.x, centerPoint.position.y, centerShiftAxis);
        }
        else if (Input.GetKey(KeyCode.K))
        {
            float centerShiftAxis = Mathf.Clamp((centerPoint.position.z - (centerPointShift * Time.deltaTime)), 0, zShiftIndex * 2);
            centerPoint.position = new Vector3(centerPoint.position.x, centerPoint.position.y, centerShiftAxis);
        }

        //Shift the center Point
        if (Input.GetKey(KeyCode.L))
        {
            float centerShiftAxis = Mathf.Clamp(((centerPointShift * Time.deltaTime) + centerPoint.position.x), 0, xShiftIndex * 2);
            centerPoint.position = new Vector3(centerShiftAxis, centerPoint.position.y, centerPoint.position.z);
        }
        else if (Input.GetKey(KeyCode.J))
        {
            float centerShiftAxis = Mathf.Clamp((centerPoint.position.x - (centerPointShift * Time.deltaTime)), 0, xShiftIndex * 2);
            centerPoint.position = new Vector3(centerShiftAxis, centerPoint.position.y, centerPoint.position.z);
        }
        */
        #endregion

        if(state == RotationState.None)
        {
            float x = 0;
            float z = 0;
            if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)))
            {
                x -= Time.deltaTime * movementScalar;
            }
            else if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)))
            {
                x += Time.deltaTime * movementScalar;
            }
            if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)))
            {
                z += Time.deltaTime * movementScalar;
            }
            else if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)))
            {
                z -= Time.deltaTime * movementScalar;
            }
            //Clamp values to border

            Debug.Log(counter);
            transform.position += (Quaternion.Euler(0, counter - 180, 0) * new Vector3(x, 0, z));
            float posX = transform.position.x;
            float posZ = transform.position.z;
            posX = Mathf.Clamp(posX, 0, xShiftIndex * 2);
            posZ = Mathf.Clamp(posZ, 0, zShiftIndex * 2);
            transform.position = new Vector3(posX, transform.position.y, posZ);
        }
        else if (state != RotationState.None)
        {
            CenterCameraForRotation();
        }

        //Change camera back after scene is loaded in
        if(state == RotationState.Start)
            state = RotationState.None;
    }



    public void SetRadiusAndCenter()
    {
        //Set radius to be the grid dimentions plus an offset
        radius = ((xShiftIndex + zShiftIndex) / 2.0f) + radiusOffset;
        centerPoint.position = new Vector3(xShiftIndex, 2.0f, zShiftIndex);
    }


    private void CenterCameraForRotation()
    {
        //Clamp XRotation to prevent Axis Flipping
        xRotate = Mathf.Clamp(xRotate, 0.1f, Mathf.PI / 2);
        //Debug.Log(xRotate);
        //convert x and y to radians
        float radians = counter * (Mathf.PI / 180.0f);
        float x = Mathf.Sin(radians) * Mathf.Sin(xRotate) * radius;
        float y = Mathf.Cos(xRotate) * radius;
        float z = Mathf.Cos(radians) * Mathf.Sin(xRotate) * radius;
        //set position and rotation
        transform.position = new Vector3(x + xShiftIndex, y, z + zShiftIndex);
        transform.LookAt(centerPoint);
    }
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
