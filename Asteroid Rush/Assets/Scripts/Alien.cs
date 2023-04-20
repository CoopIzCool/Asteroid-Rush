using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : Character
{
    [SerializeField] private int damage;
    public int Damage { get { return damage; } }

    protected override void Death()
    {
        AlienManager.Instance.RemoveAlien(this);
    }
}
