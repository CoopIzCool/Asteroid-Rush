using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockButton : MonoBehaviour
{
    public GameObject ShopManager;

    //public GameObject button;

    public int shopID;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(ShopManager.GetComponent<ShopManager>().shopItems[3, shopID]);
        Unlock();
    }

    private void Unlock()
    {

        if(ShopManager.GetComponent<ShopManager>().shopItems[3,shopID] >= 1)
        {
            //button.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            gameObject.GetComponent<Image>().material.color = new Color(255, 255, 255);
        }
    }
    
}
