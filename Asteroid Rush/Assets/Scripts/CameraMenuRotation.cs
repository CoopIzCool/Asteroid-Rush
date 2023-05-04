using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMenuRotation : MonoBehaviour
{
    private float rotX;
    private float rotY;
    private float rotZ;
    // Start is called before the first frame update
    void Start()
    {
        rotX = Random.Range(-0.07f, 0.07f);
        rotY = Random.Range(-0.07f, 0.07f);
        rotZ = Random.Range(-0.07f, 0.07f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion rot;
        rot = Quaternion.Euler(rotX, rotY, rotZ);
        gameObject.transform.Rotate(new Vector3(rotX, rotY, rotZ));
    }
}
