using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameplayMenu {
    None,
    PlayerSelect,
    MinerAction,
    FighterAction,
    SupporterAction
}

public class GameplayUI : MonoBehaviour
{
    // set in inspector
    public GameObject PlayerSelectMenu;
    public GameObject MinerActionSelect;
    public GameObject FighterActionSelect;
    public GameObject SupporterActionSelect;
    public GameObject SharedActionSelect;

    [SerializeField] private GameObject minerSelect;
    [SerializeField] private GameObject fighterSelect;
    [SerializeField] private GameObject supporterSelect;

    private static GameplayUI instance;
    public static GameplayUI Instance { get { return instance; } }

    void Start()
    {
        instance = this;
    }

    // called by GenerateLevel.cs when the player characters are created
    public void SetupUITrackers() {
        minerSelect.GetComponent<UITrackCharacter>().TargetObject = GenerateLevel.PlayerCharacters[0];
        fighterSelect.GetComponent<UITrackCharacter>().TargetObject = GenerateLevel.PlayerCharacters[1];
        supporterSelect.GetComponent<UITrackCharacter>().TargetObject = GenerateLevel.PlayerCharacters[2];
    }

    public void CloseMenus() {
        PlayerSelectMenu.SetActive(false);
        MinerActionSelect.SetActive(false);
        FighterActionSelect.SetActive(false);
        SupporterActionSelect.SetActive(false);
        SharedActionSelect.SetActive(false);
    }

    // opens the player select menu. If the parameter is true, it allows all of the players to use a move again
    public void OpenPlayerSelect(bool resetSelectable) {
        CloseMenus();
        PlayerSelectMenu.SetActive(true);

        if(resetSelectable) {
            minerSelect.SetActive(true);
            fighterSelect.SetActive(true);
            supporterSelect.SetActive(true);
        }
    }

    public void OpenMinerActions(bool canAttack, bool canMine, bool canDeposit,bool canBoard) {
        CloseMenus();
        MinerActionSelect.SetActive(true);
        SharedActionSelect.SetActive(true);

        MinerActionSelect.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = canAttack;
        MinerActionSelect.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = canMine;
        SharedActionSelect.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = canDeposit;
        SharedActionSelect.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = canBoard;
    }

    public void OpenFighterActions(bool canAttack, bool canTrap, bool canDeposit, bool canBoard) {
        CloseMenus();
        FighterActionSelect.SetActive(true);
        SharedActionSelect.SetActive(true);

        FighterActionSelect.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = canAttack;
        FighterActionSelect.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = canTrap;
        SharedActionSelect.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = canDeposit;
        SharedActionSelect.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = canBoard;
    }

    public void OpenSupporterActions(bool canMine, bool canSlowZone, bool canDeposit, bool canBoard) {
        CloseMenus();
        SupporterActionSelect.SetActive(true);
        SharedActionSelect.SetActive(true);

        SupporterActionSelect.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = canMine;
        SupporterActionSelect.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = canSlowZone;
        SharedActionSelect.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = canDeposit;
        SharedActionSelect.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = canBoard;
    }

    public void DisableCharacterMoves(Character playerScript) {
        if(playerScript is Miner) {
            minerSelect.SetActive(false);
        }
        else if(playerScript is Fighter) {
            fighterSelect.SetActive(false);
        }
        else if(playerScript is Supporter) {
            supporterSelect.SetActive(false);
        }
    }

    // button events
    public void EndTurn() {
        TurnHandler.Instance.EndPlayerTurn();
    }

    public void SelectMiner() {
        TurnHandler.Instance.SelectCharacter(0);
    }

    public void SelectFighter() {
        TurnHandler.Instance.SelectCharacter(1);
    }

    public void SelectSupporter() {
        TurnHandler.Instance.SelectCharacter(2);
    }

    public void SelectMine() {
        CloseMenus();
        TurnHandler.Instance.ChooseMine();
    }

    public void SelectAttack() {
        CloseMenus();
        TurnHandler.Instance.ChooseAttack();
    }

    public void SelectDeposit() {
        CloseMenus();
        TurnHandler.Instance.ChooseDeposit();
    }

    public void SelectTrap() {
        CloseMenus();
        GenerateLevel.PlayerCharacters[1].GetComponent<Fighter>().SpecialAction();
        TurnHandler.Instance.SetStatePlayerSelect();
    }

    public void SelectSlowZone() {
        CloseMenus();
        GenerateLevel.PlayerCharacters[2].GetComponent<Supporter>().SpecialAction();
        TurnHandler.Instance.SetStatePlayerSelect();
    }

    public void SelectWait() {
        TurnHandler.Instance.SetStatePlayerSelect();
    }
}
