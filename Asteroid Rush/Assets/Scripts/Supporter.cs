using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// player character with support abilities
public class Supporter : Character
{
    [SerializeField] private GameObject drillBotPrefab;

    public void CreateDrillBot(UnrefinedOre targetOre) {
        if(targetOre.HasDrillBot) {
            return;
        }

        targetOre.AddDrillBot(Instantiate(drillBotPrefab));
    }

    public override void SpecialAction()
    {
        AlienManager.Instance.AddSlowZone(CurrentTile);
    }
}
