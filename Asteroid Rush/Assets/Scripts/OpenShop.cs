using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OpenShop : MonoBehaviour
{
    public GameObject shopManager;
    private ShopManager spManager;

    public TextMeshPro shopText;

    private void Start()
    {
        spManager = shopManager.GetComponent<ShopManager>();
    }

    private void OnMouseUpAsButton()
    {
        if(spManager.isShopOpen == false)
        {
            spManager.ToggleShop();
        }

    }

    private void OnMouseOver()
    {
        shopText.color = Color.red;
        shopText.fontStyle = FontStyles.Underline;
    }

    private void OnMouseExit()
    {
        shopText.color = Color.green;
        shopText.fontStyle = FontStyles.Normal;
    }
}
