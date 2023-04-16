using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public int[,] shopItems = new int[4, 11];

    /// <summary>
    /// Number of coins the player currently has
    /// </summary>
    private float coins = 100;

    public Text CoinsText;
    public Text UICoinsText;
    public GameObject canvas;

    /// <summary>
    /// Checks to see if shop is open
    /// </summary>
    private bool isShopOpen = false;

    private bool gameIsPaused;

    //public GameObject itemViewport;
    //public GameObject equipmentViewport;

    public GameObject itemScrollView;
    public GameObject equipmentScrollView;

    // Start is called before the first frame update
    void Start()
    {
        CoinsText.text = "Currency: " + coins.ToString();
        UICoinsText.text = "Currency: " + coins.ToString();

        //ID
        //Potion
        //shopItems[1, 1] = 1;
        ////Item 2
        //shopItems[1, 2] = 2;
        ////Item 3
        //shopItems[1, 3] = 3;
        ////Item 4
        //shopItems[1, 4] = 4;
        ////Item 5
        //shopItems[1, 5] = 5;

        ////Item 6
        //shopItems[1, 6] = 6;
        ////Item 7
        //shopItems[1, 7] = 7;
        ////Item 8
        //shopItems[1, 8] = 8;
        ////Item 9
        //shopItems[1, 9] = 9;
        ////Item 10
        //shopItems[1, 10] = 10;

        //Gives each item an ID
        for(int i = 1; i < shopItems.Length / 4; i ++)
        {
            shopItems[1, i] = i;
        }

        //Prices of each item
        shopItems[2, 1] = 10;
        shopItems[2, 2] = 10;
        shopItems[2, 3] = 10;
        shopItems[2, 4] = 10;
        shopItems[2, 5] = 10;
        shopItems[2, 6] = 100;
        shopItems[2, 7] = 100;
        shopItems[2, 8] = 100;
        shopItems[2, 9] = 100;
        shopItems[2, 10] = 100;

        //Quantity of each item
        shopItems[3, 1] = 0;
        shopItems[3, 2] = 0;
        shopItems[3, 3] = 0;
        shopItems[3, 4] = 0;
        shopItems[3, 5] = 0;
        shopItems[3, 6] = 0;
        shopItems[3, 7] = 0;
        shopItems[3, 8] = 0;
        shopItems[3, 9] = 0;
        shopItems[3, 10] = 0;
    }

    /// <summary>
    /// Function for purchasing an item
    /// </summary>
    public void Buy()
    {
        GameObject ButtonRef = GameObject.FindGameObjectWithTag("Event").GetComponent<EventSystem>().currentSelectedGameObject;

        //If the player has enough money
        if (coins >= shopItems[2, ButtonRef.GetComponent<ButtonInfo>().ItemID])
        {
            //Decreases the amount of money
            ChangeCoins(-shopItems[2, ButtonRef.GetComponent<ButtonInfo>().ItemID]);

            //Increases the quantity/level of upgrade
            shopItems[3, ButtonRef.GetComponent<ButtonInfo>().ItemID]++;

            CoinsText.text = "Currency: " + coins.ToString(); 

            //Updates text
            ButtonRef.GetComponent<ButtonInfo>().QuantityText.text = shopItems[3, ButtonRef.GetComponent<ButtonInfo>().ItemID].ToString();
        }
    }

    /// <summary>
    /// Function to change the amount of coins currently possessed
    /// </summary>
    /// <param name="valueChange">Amount to change coins by</param>
    public void ChangeCoins(float valueChange)
    {
        CoinsText.text = "Currency: " + (coins + valueChange).ToString();
        UICoinsText.text = "Currency: " + (coins + valueChange).ToString();
        coins += valueChange;
    }

    /// <summary>
    /// Opens and closes the shop
    /// </summary>
    public void ToggleShop()
    {
        isShopOpen = !isShopOpen;
        canvas.SetActive(isShopOpen);

        if (isShopOpen)
        {
            gameIsPaused = true;
        }
        else
        {
            gameIsPaused = false;
        }
    }

    public void PauseGame()
    {
        if(gameIsPaused)
        {
            Time.timeScale = 0f;
        }
        if(gameIsPaused == false)
        {
            Time.timeScale = 1;
        }
    }

    public void Update()
    {   
        //Opens and closes shop
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleShop();
        }

        PauseGame();
    }

    /// <summary>
    /// Switches to the equipment products
    /// </summary>
    public void OpenEquipmentTab()
    {
        //itemViewport.SetActive(false);
        //equipmentViewport.SetActive(true);

        itemScrollView.SetActive(false);
        equipmentScrollView.SetActive(true);
    }

    /// <summary>
    /// Switches to the consumable products
    /// </summary>
    public void OpenItemTab()
    {
        //itemViewport.SetActive(true);
        //equipmentViewport.SetActive(false);

        itemScrollView.SetActive(true);
        equipmentScrollView.SetActive(false);
    }
}
