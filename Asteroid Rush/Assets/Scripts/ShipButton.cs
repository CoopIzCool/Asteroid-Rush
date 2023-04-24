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

    /// <summary>
    /// Opens a different UI depending on which button was pressed
    /// </summary>
    private void OnMouseUpAsButton()
    {
        //If levels button
        if(buttonID == 1)
        {
           if (spManager.isShopOpen == false && spManager.isEquipMenuOpen == false)
           {
                OpenLevelScreen();
           }
         
        }

        //If shop button
        if(buttonID == 2)
        {
            if (spManager.isShopOpen == false && spManager.isEquipMenuOpen == false)
            {
                spManager.ToggleShop();
                CloseLevelScreen();
            }
        }

        //If equip button
        if (buttonID == 3)
        {
            if (spManager.isShopOpen == false && spManager.isEquipMenuOpen == false)
            {
                spManager.OpenEquipMenu();
                CloseLevelScreen();
            }
        }
    }

    /// <summary>
    /// Highlights text when mouse is over button
    /// </summary>
    private void OnMouseOver()
    {
        if (spManager.isShopOpen == false && spManager.isEquipMenuOpen == false)
        {
            buttonText.color = Color.red;
            buttonText.fontStyle = FontStyles.Underline;
        }
      
    }

    private void OnMouseExit()
    {
        buttonText.color = Color.green;
        buttonText.fontStyle = FontStyles.Normal;
    }

    /// <summary>
    /// Shows the three level buttons
    /// </summary>
    private void OpenLevelScreen()
    {
        isLevelScreenOpen = true;
        levelScreens.SetActive(isLevelScreenOpen);
    }

    private void CloseLevelScreen()
    {
        isLevelScreenOpen = false;
        levelScreens.SetActive(isLevelScreenOpen);
    }
}
