using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnrefinedOre : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private int breakabillity;

    private GameObject drillBot;
    public bool HasDrillBot { get { return drillBot != null; } }
    private const int BOT_DAMAGE_PER_TURN = 2;
    #endregion

    public bool MineOre(int damage)
    {
        breakabillity -= damage;
        if(breakabillity <= 0)
        {
            if(HasDrillBot) {
                Destroy(drillBot);
            }
            gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public void AddDrillBot(GameObject drillBot) {
        this.drillBot = drillBot;
        drillBot.transform.position = transform.position;
        TurnHandler.Instance.AddDrillingOre(this); // this makes the ore take damage every turn
    }

    public bool DealDrillBotDamage()
    {
        if(!HasDrillBot) {
            return false;
        }

        bool destroyed = MineOre(BOT_DAMAGE_PER_TURN);
        return destroyed;
    }
}
