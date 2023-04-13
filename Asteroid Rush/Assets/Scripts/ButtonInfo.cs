using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInfo : MonoBehaviour
{
    public int ItemID;
    public Text PriceText;
    public Text QuantityText;
    public string upgradeName;
    public GameObject ShopManager;

    void Update()
    {
        PriceText.text = "Price: $" + ShopManager.GetComponent<ShopManager>().shopItems[2, ItemID].ToString();
        QuantityText.text = upgradeName + " Lvl: " + ShopManager.GetComponent<ShopManager>().shopItems[3, ItemID].ToString();
    }
}
