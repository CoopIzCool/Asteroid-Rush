using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnrefinedOre : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private int breakabillity;
    [SerializeField] private GameObject drillBotPrefab;

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
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public void AddDrillBot() {
        drillBot = Instantiate(drillBotPrefab);
        drillBot.transform.position = transform.position + new Vector3(0, 0.6f, 0);
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
