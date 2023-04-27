using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    #region Fields
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

    public void CanDeposit()
    {
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
