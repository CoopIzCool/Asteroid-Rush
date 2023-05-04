using System.Collections;
using System.Collections.Generic;
using TMPro;
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
	private TMP_Text currentOreText = null;
	private TMP_Text requiredOreText = null;
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

		currentOreText = GameObject.Find("ShipOre").GetComponent<TMP_Text>();
		GameObject.Find("RequiredOre").GetComponent<TMP_Text>().text = "Ore Required: " + oreNeeded;
	}
    public void ActivatingShipTiles()
    {
        Debug.Log("Depositing");
        rocketTile.SetAvailabillitySelector(true);
    }
    public void DepositOre(int oreDeposit)
    {
        oreTotal += oreDeposit;
		currentOreText.text = "Ore Secured: " + oreTotal.ToString();
        rocketTile.SetAvailabillitySelector(false);
    }

    public void AboardShip(GameObject character)
    {
        character.GetComponent<Character>().CurrentTile.occupant = null;
        character.SetActive(false);
        rocketTile.SetAvailabillitySelector(false);
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
        Debug.Log("Victory");
        rocketTile.SetAvailabillitySelector(false);

        int amtOre = int.Parse(DataTracking.GetData(5)) + oreTotal;
        DataTracking.SetData(5, amtOre.ToString());
		int numWins = int.Parse(DataTracking.GetData(1)) + 1;
		DataTracking.SetData(1, numWins.ToString());
        DataTracking.SaveData();

		SceneManager.LoadScene("Victory");
    }
}
