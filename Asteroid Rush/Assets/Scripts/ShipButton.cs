using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShipButton : MonoBehaviour
{
    public GameObject shopManager;
    private ShopManager spManager;
    public GameObject levelScreens;

    public TextMeshPro buttonText;
    public int buttonID;

    private bool isLevelScreenOpen;

    private void Start()
    {
        spManager = shopManager.GetComponent<ShopManager>();

        isLevelScreenOpen = false;
    }

    private void OnMouseUpAsButton()
    {
        //If levels button
        if(buttonID == 1)
        {
           ToggleLevelScreen();
        }

        //If shop button
        if(buttonID == 2)
        {
            if(spManager.isShopOpen == false)
            {
                spManager.ToggleShop();
            }
        }
    }

    private void OnMouseOver()
    {
        buttonText.color = Color.red;
        buttonText.fontStyle = FontStyles.Underline;
    }

    private void OnMouseExit()
    {
        buttonText.color = Color.green;
        buttonText.fontStyle = FontStyles.Normal;
    }

    private void ToggleLevelScreen()
    {
        isLevelScreenOpen = !isLevelScreenOpen;
        levelScreens.SetActive(isLevelScreenOpen);
    }
}
