using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningDrill : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, -200 * Time.deltaTime, 0));
    }
}
