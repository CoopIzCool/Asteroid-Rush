using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    public Tile Tile { get; set; }
    public int TurnsLeft { get; set; }

    void Start()
    {
        TurnsLeft = 5;

        //Boosts slow zone duration if supporter equipment #1 is equipped
        if (ShopManager.charEquipments[4].isSelected == true)
        {
            TurnsLeft = 8;
        }
    }

    public bool IsInRange(Tile test) {
        return Mathf.Abs(test.xPos - Tile.xPos) <= 1 && Mathf.Abs(test.zPos - Tile.zPos) <= 1;
    }
}
