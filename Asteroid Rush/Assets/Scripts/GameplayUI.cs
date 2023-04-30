using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // sets the character selected buttons back to active
    public void AllowCharacterSelection() {
        minerSelect.SetActive(true);
        fighterSelect.SetActive(true);
        supporterSelect.SetActive(true);
    }

    public void OpenMenu(GameplayMenu menu) {
        PlayerSelectMenu.SetActive(false);
        MinerActionSelect.SetActive(false);
        FighterActionSelect.SetActive(false);
        SupporterActionSelect.SetActive(false);

        switch(menu) {
            case GameplayMenu.PlayerSelect:
                PlayerSelectMenu.SetActive(true);
                break;

            case GameplayMenu.MinerAction:
                MinerActionSelect.SetActive(true);
                break;

            case GameplayMenu.FighterAction:
                FighterActionSelect.SetActive(true);
                break;

            case GameplayMenu.SupporterAction:
                SupporterActionSelect.SetActive(true);
                break;
        }
    }

    // button events
    public void EndTurn() {
        TurnHandler.Instance.EndPlayerTurn();
    }

    public void SelectMiner() {
        minerSelect.SetActive(false);
        TurnHandler.Instance.SelectCharacter(0);
        //OpenMenu(GameplayMenu.MinerAction);
    }

    public void SelectFighter() {
        fighterSelect.SetActive(false);
        TurnHandler.Instance.SelectCharacter(1);
        //OpenMenu(GameplayMenu.MinerAction);
    }

    public void SelectSupporter() {
        supporterSelect.SetActive(false);
        TurnHandler.Instance.SelectCharacter(2);
        //OpenMenu(GameplayMenu.MinerAction);
    }
}
