using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : Character
{
    public int Damage { get { return damage; } }

    protected override void Death()
    {
        AlienManager.Instance.RemoveAlien(this);
    }
}
