using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    public Text nameText;
    public string characterName;
    public int characterID;

    public GameObject characterScrollView;
    public GameObject roleScrollView;

    public GameObject minerEquipScrollView;


    // Update is called once per frame
    void Update()
    {
        nameText.text = characterName;
    }

    public void OpenCharacterMenu()
    {
        characterScrollView.SetActive(false);
        roleScrollView.SetActive(true);
    }

    public void OpenMinerEquipment()
    {
        characterScrollView.SetActive(false);
        roleScrollView.SetActive(false);
        minerEquipScrollView.SetActive(true);
    }
}
