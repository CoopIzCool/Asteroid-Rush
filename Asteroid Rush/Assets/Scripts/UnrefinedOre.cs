using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class UnrefinedOre : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private int breakabillity;
    [SerializeField] private GameObject drillBotPrefab;

    public int Breakability
    {
        get { return breakabillity; }
    }

    private GameObject drillBot;
    public bool HasDrillBot { get { return drillBot != null; } }
    private const int BOT_DAMAGE_PER_TURN = 1;

    public GameObject healthBar = null;
    #endregion

    public bool MineOre(int damage)
    {
        breakabillity -= damage;
        for (int i = 0; i < damage; i++)
        {
            // Interesting Fact: Destroy activates at the end of the frame.
            // If you do not include the "- i" here, Destroy() will just activate on the last child multiple times.
            // You could use DestroyImmediate() to achieve the same effect more cleanly, but Unity highly recommends against using it.
			if (healthBar.transform.childCount > 0) Destroy(healthBar.transform.GetChild(healthBar.transform.childCount - 1 - i).gameObject);
			else break;
		}

		if (breakabillity <= 0)
        {
            if(HasDrillBot) {
                Destroy(drillBot);
            }
            Destroy(healthBar);
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
