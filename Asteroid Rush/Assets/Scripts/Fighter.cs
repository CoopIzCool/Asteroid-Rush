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

        //Boosts attack damage if attacker equipment #1 is equipped
        if (ShopManager.charEquipments[2].isSelected == true)
        {
            damage = 8;
        }

        //Boosts attack range if attacker equipment #2 is equipped
        if (ShopManager.charEquipments[3].isSelected == true)
        {
            attackRange = 8;
        }
    }


    public override void SpecialAction()
    {
        Debug.Log("in Fighter.cs, trying to set a trap");
        AlienManager.Instance.AddTrap(CurrentTile);
        TrapCooldown = 4;
    }
}
