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
       SceneManager.LoadScene("NewRyanScene");  
    }
}
