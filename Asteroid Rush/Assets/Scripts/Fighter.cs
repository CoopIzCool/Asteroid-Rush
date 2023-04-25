using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : Character
{
    protected override void Start()
    {
        base.Start();
        
    }


    public override void SpecialAction()
    {
        AlienManager.Instance.AddTrap(CurrentTile);
    }
}
