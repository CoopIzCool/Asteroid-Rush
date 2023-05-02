using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : Character
{
    public int Damage { get { return damage; } }

    protected override void Death()
    {
        AlienManager.Instance.RemoveAlien(this);

        int numKills = int.Parse(DataTracking.GetData(3)) + 1;
        DataTracking.SetData(3, numKills.ToString());
    }
}
