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
        if (DataTracking.DataExists())
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

            Debug.Log(DataTracking.GetData(0));
            int numRuns = int.Parse(DataTracking.GetData(0));
            numRuns++;
            DataTracking.SetData(0, numRuns.ToString());

            SceneManager.LoadScene("JohnScene");
        }
        else
        {
            Debug.Log("Data not yet loaded!");
        }
    }
}
