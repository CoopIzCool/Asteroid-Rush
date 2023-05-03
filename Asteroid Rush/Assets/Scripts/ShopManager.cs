using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public int[,] shopItems = new int[4, 12];

    /// <summary>
    /// Number of coins the player currently has
    /// </summary>
    private float coins = 300;

    public Text CoinsText;
    public Text UICoinsText;
    public GameObject canvas;

    /// <summary>
    /// Checks to see if shop is open
    /// </summary>
    public bool isShopOpen = false;
    public bool isEquipMenuOpen = false;

    private bool gameIsPaused;

    //shop
    public GameObject itemScrollView;
    public GameObject equipmentScrollView;

    //equip
    public GameObject equipMenu;
    public GameObject characterScrollView;

    //miner
    public GameObject minerScrollView;
    public GameObject minerEquipScrollView;
    public GameObject minerItemViewOne;
    public GameObject minerItemViewTwo;

    //attacker
    public GameObject attackerScrollView;
    public GameObject attackerEquipScrollView;
    public GameObject attackerItemViewOne;
    public GameObject attackerItemViewTwo;

    //supporter
    public GameObject supportScrollView;
    public GameObject supportEquipScrollView;
    public GameObject supportItemViewOne;
    public GameObject supportItemViewTwo;

    /// <summary>
    /// All six equipment
    /// </summary>
    public EquipmentButton[] equipments;
    /// <summary>
    /// Miner's equipped items
    /// </summary>
    public EquipmentButton[] minerItems;
    /// <summary>
    /// Attacker's equipped items
    /// </summary>
    public EquipmentButton[] attackerItems;
    /// <summary>
    /// Attacker's equipped items
    /// </summary>
    public EquipmentButton[] supporterItems;

    /// <summary>
    /// COPIED ARRAYS FROM ABOVE, USABLE IN ANY SCRIPT
    /// </summary>
    /// 
    //checks which equipments are equipped
    public static EquipmentButton[] charEquipments;
    //what items the miner has equipped
    public static EquipmentButton[] minerItemsEquipped;
    //what items the attacker has equipped
    public static EquipmentButton[] attackerItemsEquipped;
    //what items the supporter has equipped
    public static EquipmentButton[] supporterItemsEquipped;
    //quantity of each item in the shop
    public static int[,] updatedShopItems;

	private void Awake()
	{
        if(!DataTracking.DataExists()) StartCoroutine(DataTracking.LoadData());
	}

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
        shopItems[2, 1] = 50;
        shopItems[2, 2] = 100;
        shopItems[2, 3] = 10;
        shopItems[2, 4] = 10;
        shopItems[2, 5] = 10;
        shopItems[2, 6] = 150;
        shopItems[2, 7] = 150;
        shopItems[2, 8] = 150;
        shopItems[2, 9] = 150;
        shopItems[2, 10] = 150;
        shopItems[2, 11] = 150;

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
        shopItems[3, 11] = 0;
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
            //Prevents the player from buying each equipment more than once
            if(shopItems[1, ButtonRef.GetComponent<ButtonInfo>().ItemID] >= 6)
            {
                if (shopItems[3, ButtonRef.GetComponent<ButtonInfo>().ItemID] >= 1)
                {
                    return;
                }
            }
                
            //Decreases the amount of money
            ChangeCoins(-shopItems[2, ButtonRef.GetComponent<ButtonInfo>().ItemID]);

            //Increases the quantity
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

        //for(int i = 0; i < equipments.Length; i++)
        //{
        //    Debug.Log(i + " " + equipments[i].isSelected);
        //}

        charEquipments = equipments;
        minerItemsEquipped = minerItems;
        attackerItemsEquipped = attackerItems;
        supporterItemsEquipped = supporterItems;
        updatedShopItems = shopItems;
    }

    public void GoToView(GameObject scrollView)
    {
        characterScrollView.SetActive(false);

        SetAllInactive();

        scrollView.SetActive(true);
    }

    /// <summary>
    /// Switches to the equipment products
    /// </summary>
    public void OpenEquipmentTab()
    {
        itemScrollView.SetActive(false);
        equipmentScrollView.SetActive(true);
    }

    /// <summary>
    /// Switches to the consumable products
    /// </summary>
    public void OpenItemTab()
    {
        itemScrollView.SetActive(true);
        equipmentScrollView.SetActive(false);
    }

    /// <summary>
    /// Opens the equip menu interface
    /// </summary>
    public void OpenEquipMenu()
    {
        equipMenu.SetActive(true);

        characterScrollView.SetActive(true);

        SetAllInactive();

        isEquipMenuOpen = true;
    }

    public void SetAllInactive()
    {
        minerScrollView.SetActive(false);
        minerEquipScrollView.SetActive(false);
        attackerScrollView.SetActive(false);
        attackerEquipScrollView.SetActive(false);
        supportScrollView.SetActive(false);
        supportEquipScrollView.SetActive(false);
        minerItemViewOne.SetActive(false);
        minerItemViewTwo.SetActive(false);
        attackerItemViewOne.SetActive(false);
        attackerItemViewTwo.SetActive(false);
        supportItemViewOne.SetActive(false);
        supportItemViewTwo.SetActive(false);
    }

    /// <summary>
    /// Closes the equip menu
    /// </summary>
    public void CloseEquipMenu()
    {
        equipMenu.SetActive(false);

        isEquipMenuOpen = false;
    }

    /// <summary>
    /// Closes the character's menu
    /// </summary>
    public void OpenCharacterMenu(GameObject roleScrollView)
    {
        characterScrollView.SetActive(false);
        roleScrollView.SetActive(true);
    }

    /// <summary>
    /// Opens the equipment UI for each character
    /// </summary>
    public void OpenCharacterEquipment(GameObject roleEquipScrollView)
    {
        characterScrollView.SetActive(false);
        minerScrollView.SetActive(false);
        attackerScrollView.SetActive(false);
        supportScrollView.SetActive(false);
        roleEquipScrollView.SetActive(true);
    }
}
