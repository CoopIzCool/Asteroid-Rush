using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// player character with support abilities
public class Supporter : Character
{
    public override void SpecialAction()
    {
        AlienManager.Instance.AddSlowZone(CurrentTile);
    }
}
