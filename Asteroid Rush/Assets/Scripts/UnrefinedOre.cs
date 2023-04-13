using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnrefinedOre : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private int breakabillity;
    #endregion

    public bool MineOre(int damage)
    {
        breakabillity -= damage;
        if(breakabillity <= 0)
        {
            return true;
        }
        return false;
    }

}
