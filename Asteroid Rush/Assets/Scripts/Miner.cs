using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : Character
{
    #region Fields

    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //Boosts mining damage if miner equipment #1 is equipped
        if (ShopManager.charEquipments[0].isSelected == true)
        {
            MiningPower = 6;
        }

        //Boosts miner movement range if miner equipment #2 is equipped
        if (ShopManager.charEquipments[1].isSelected == true)
        {
            Movement = 6;
        }
    }


    public override void SpecialAction()
    {
        base.SpecialAction();
    }
}
