using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a script to make a ui element appear above the head of an in-game character
public class UITrackCharacter : MonoBehaviour
{
    public GameObject TargetObject;
    private const float UPSHIFT = 50.0f; // distance above the head of the character

    void Update()
    {
        Vector3 worldPosition = TargetObject.transform.position;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        transform.position = screenPosition + new Vector3(0, UPSHIFT, 0);
        //Deactivate UI if the player is deactivated
        if(!TargetObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }
}
