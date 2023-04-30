using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour
{
    public GameObject icon;
    public ShopManager shopManager;
    public int shopID;
    public string equipName;

    public EquipmentButton altEquipment;

    public bool isSelected = false;

    // Update is called once per frame
    void Update()
    {
        //Unlocks the equipment if the player has bought it
        if (shopManager.shopItems[3, shopID] >= 1 && isSelected == false)
        {
            gameObject.GetComponent<Image>().color = new Color(255, 255, 255);
            icon.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
        }
        
    }

    /// <summary>
    /// Changes the active equipment
    /// </summary>
    public void SelectEquipment()
    {
        if (shopManager.shopItems[3, shopID] >= 1)
        {
            gameObject.GetComponent<Image>().color = new Color(0, 255, 0);

            isSelected = true;
            altEquipment.isSelected = false;
        }

    }
}
