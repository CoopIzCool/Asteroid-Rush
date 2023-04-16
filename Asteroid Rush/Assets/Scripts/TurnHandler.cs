using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnHandler : MonoBehaviour
{
    #region Fields
    private RaycastManager raycastManager;
    private GameObject selectedCharacter;
   

    [Header("Movement Components:")]
    private int currentMovement;
    private Stack<Tile> tiles = new Stack<Tile>();
    private LineRenderer lineRenderer;
    #endregion

    #region Properties
    public GameObject SelectedCharacter 
    { 
        get { return selectedCharacter; }
        set { selectedCharacter = value; }
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        raycastManager = gameObject.GetComponent<RaycastManager>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (selectedCharacter != null)
            {
                if(tiles.Count < currentMovement)
                {
                    AddTile();
                }

            }
            //If character is not selected, look for one to select
            else
            {
                raycastManager.TestRaycast();
                if(selectedCharacter != null)
                {
                    Debug.Log(selectedCharacter.name);
                    currentMovement = selectedCharacter.GetComponent<Character>().Movement;
                    lineRenderer.positionCount = 1;
                    lineRenderer.SetPosition(0,selectedCharacter.transform.position);
                }
            }
        }
    }

    public void AddTile()
    {
        Tile selectedTile = raycastManager.TileRaycast();
        
        if(selectedTile.occupant == null)
        {

            if(selectedTile != tiles.Peek())
            {
                tiles.Push(selectedTile);
                lineRenderer.SetPosition(tiles.Count, selectedTile.transform.position);
            }
            else
            {
                tiles.Pop();
                lineRenderer.positionCount = lineRenderer.positionCount - 1;
            }
            
        }
    }
}
