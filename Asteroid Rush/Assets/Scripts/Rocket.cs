using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private int oreTotal = 0;
    [SerializeField]
    private Tile rocketTile;
    private int crewmatesAboard = 0;
    [SerializeField]
    private int oreNeeded;
    private int crewmateTotal = 3;
    #endregion

    #region Properties
    public int OreNeeded
    {
        set { oreNeeded = value; }
    }

    public Tile RocketTile
    {
        set { rocketTile = value; }
    }
    #endregion

    private void Start()
    {
        GameObject turnHandlerObject = GameObject.FindGameObjectWithTag("TurnHandler");
        turnHandlerObject.GetComponent<TurnHandler>().RocketObject = this;
    }
    public void CanDeposit()
    {
        Debug.Log("Depositing");
        rocketTile.SetAvailabillitySelector(true);
    }
    public void DepositOre(int oreDeposit)
    {
        oreTotal += oreDeposit;
        rocketTile.SetAvailabillitySelector(false);
    }

    public void AboardShip(GameObject character)
    {
        character.SetActive(false);
        crewmatesAboard++;
        if(crewmatesAboard == crewmateTotal)
        {
            WinState();
        }
    }

    public bool CanEscape()
    {
        return oreTotal >= oreNeeded;
    }

    public void WinState()
    {

    }
}
