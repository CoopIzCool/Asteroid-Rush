using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : Character
{
    public override void SpecialAction()
    {
        AlienManager.Instance.AddTrap(CurrentTile);
    }
}
