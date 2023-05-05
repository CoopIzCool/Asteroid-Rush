using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsPage;

    public void StartGame()
    {
        SceneManager.LoadScene("WinsonScene");
		Debug.Log("Clicked");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ShowOptions()
    {
        optionsPage.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void GoToMainMenu()
    {
        optionsPage.SetActive(false);
        mainMenu.SetActive(true);
    }
}
