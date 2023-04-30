using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipIcon : MonoBehaviour
{
    public ShopManager shopManager;
    public int shopID;
    public GameObject icon;

    // Update is called once per frame
    void Update()
    {
        //Changes icon to the selected equipment's icon
        if(shopManager.equipments[shopID].isSelected)
        {
            icon.GetComponent<SpriteRenderer>().sprite = shopManager.equipments[shopID].icon.GetComponent<SpriteRenderer>().sprite;
            icon.transform.localScale = shopManager.equipments[shopID].icon.transform.localScale;
        }

        else if (shopManager.equipments[shopID + 1].isSelected)
        {
            icon.GetComponent<SpriteRenderer>().sprite = shopManager.equipments[shopID + 1].icon.GetComponent<SpriteRenderer>().sprite;
            icon.transform.localScale = shopManager.equipments[shopID].icon.transform.localScale;
        }
    }
}
