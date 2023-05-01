using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipIcon : MonoBehaviour
{
    public ShopManager shopManager;
    public int shopID;
    public GameObject icon;
    public string equipType;

    // Update is called once per frame
    void Update()
    {
        if(equipType == "Equipment")
        {
            ChangeIcon(shopManager.equipments);
        }

        if(equipType == "ItemM")
        {
            ChangeIcon(shopManager.minerItems);
        }

        if (equipType == "ItemA")
        {
            ChangeIcon(shopManager.attackerItems);
        }
    }

    public void ChangeIcon(EquipmentButton[] equipments)
    {
        if(equipments[shopID].isSelected)
        {
            icon.GetComponent<SpriteRenderer>().sprite = equipments[shopID].icon.GetComponent<SpriteRenderer>().sprite;
            icon.transform.localScale = equipments[shopID].icon.transform.localScale;
        }

        else if (equipments[shopID + 1].isSelected)
        {
            icon.GetComponent<SpriteRenderer>().sprite = equipments[shopID + 1].icon.GetComponent<SpriteRenderer>().sprite;
            icon.transform.localScale = equipments[shopID + 1].icon.transform.localScale;
        }
    }
}
