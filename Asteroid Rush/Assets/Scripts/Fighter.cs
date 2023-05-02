using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : Character
{
    public int TrapCooldown { get; set; }
    public bool CanTrap { get { return TrapCooldown <= 0; } }

    protected override void Start()
    {
        base.Start();
        
    }


    public override void SpecialAction()
    {
        Debug.Log("in Fighter.cs, trying to set a trap");
        AlienManager.Instance.AddTrap(CurrentTile);
        TrapCooldown = 4;
    }
}
