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
 
                 AddTile();
                

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
                    tiles.Push(selectedCharacter.GetComponent<Character>().CurrentTile);
                    Vector3 startLocation = selectedCharacter.GetComponent<Character>().CurrentTile.gameObject.transform.position;
                    lineRenderer.SetPosition(0, new Vector3(startLocation.x,startLocation.y + 0.2f,startLocation.z));
                }
            }
        }

        if(selectedCharacter != null)
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                selectedCharacter.gameObject.GetComponent<Character>().MoveToTile(tiles.Peek());
                ClearCurrentPath();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                ClearCurrentPath();
            }
        }

    }

    public void AddTile()
    {
        Tile selectedTile = raycastManager.TileRaycast();
        if (selectedTile != null)
        {

            if (selectedTile != tiles.Peek() && tiles.Count <= currentMovement && TileCheck(selectedTile))
            {
                tiles.Push(selectedTile);
                lineRenderer.positionCount++;
                Vector3 tilePos = selectedTile.transform.position;
                lineRenderer.SetPosition(tiles.Count - 1, new Vector3(tilePos.x, tilePos.y + 0.2f, tilePos.z));
            }
            else if(selectedTile.occupant == SelectedCharacter)
            {
                ClearCurrentPath();
            }
            else if(selectedTile == tiles.Peek())
            {
                tiles.Pop();
                lineRenderer.positionCount = lineRenderer.positionCount - 1;
                if (tiles.Count <= 0)
                {
                    ClearCurrentPath();
                }
            }
            
        }
    }

    private void ClearCurrentPath()
    {
        selectedCharacter = null;
        tiles.Clear();
        lineRenderer.positionCount = 0;
        currentMovement = 0;
    }

    private bool TileCheck(Tile tileInQuestion)
    {
        if(tileInQuestion.tileType == TileType.Basic && tileInQuestion.occupant == null)
        {
            if(Vector3.Distance(tileInQuestion.gameObject.transform.position,tiles.Peek().transform.position) < 1.2)
            return true;
        }
        return false;
    }
}
