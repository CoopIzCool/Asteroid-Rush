using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    /// <summary>
    /// Returns the user to the start screen whenever called
    /// </summary>
    public void ReturnToMenu()
    {
        Time.timeScale = 1.0f;
        
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Exits the game whenever called
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
