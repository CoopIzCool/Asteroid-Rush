using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    /// <summary>
    /// Highlights color when mouse is over button
    /// </summary>
    private void OnMouseOver()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(0.78f,0,0,1);
    }

    private void OnMouseExit()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1,1,1,1);
    }

    private void OnMouseUpAsButton()
    {
        switch (gameObject.name)
        {
			case "level_one_screen":
				GenerateLevel.asteroidType = AsteroidType.Gray;
				break;
			case "level_two_screen":
				GenerateLevel.asteroidType = AsteroidType.Blue;
				break;
			case "level_three_screen":
				GenerateLevel.asteroidType = AsteroidType.Brown;
				break;

		}

        SceneManager.LoadScene("JohnScene");  
    }
}
