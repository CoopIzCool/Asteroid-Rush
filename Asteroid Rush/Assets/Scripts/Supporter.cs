using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// player character with support abilities
public class Supporter : Character
{
    public override void SpecialAction()
    {
        Debug.Log("in Supporter.cs, trying to add a slow zone");
        //AlienManager.Instance.AddSlowZone(CurrentTile);
    }
}
