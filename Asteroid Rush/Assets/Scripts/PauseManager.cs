using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject confirmationScreen;
    public GameObject pausedMenu;
    public static bool isPaused = false;

    private void Update()
    {
        TogglePausedMenu();

        if(isPaused)
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }

    /// <summary>
    /// Returns the user to the start screen whenever called
    /// </summary>
    public void ReturnToMenu()
    {
        Time.timeScale = 1.0f;
        isPaused = false;
        
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Exits the game whenever called
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Opens the confirmation screen
    /// </summary>
    public void ConfirmRetreat()
    {
        confirmationScreen.SetActive(true);
    }

    /// <summary>
    /// Cancels the retreat order
    /// </summary>
    public void CancelRetreat()
    {
        confirmationScreen.SetActive(false);
    }

    /// <summary>
    /// Takes the player back to the shop scene
    /// </summary>
    public void ReturnToShop()
    {
        isPaused = false;
        SceneManager.LoadScene("WinsonScene");
    }

    /// <summary>
    /// Toggles the paused menu screen
    /// </summary>
    public void TogglePausedMenu()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pausedMenu.SetActive(isPaused);
        }
    }
}
