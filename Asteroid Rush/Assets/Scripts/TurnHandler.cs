using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnHandler : MonoBehaviour
{

    #region Fields
    public enum TurnOrder
    {
        Player,
        Alien
    }
    private RaycastManager raycastManager;
    [SerializeField]private GameObject selectedCharacter;
    private TurnOrder currentTurn;
    [SerializeField]private GameObject[] characters;

    [Header("Movement Components:")]
    private int currentMovement;
    private Stack<Tile> tiles = new Stack<Tile>();
    private LineRenderer lineRenderer;
    private float lineYOffset = 0.2f;

    [Header("New Movement Components")]
    private List<Tile> availableTiles = new List<Tile>();
    private Tile startingTile;
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
        currentTurn = TurnOrder.Player;

    }

    // Update is called once per frame
    void Update()
    {
        if (currentTurn == TurnOrder.Player)
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
                    if (selectedCharacter != null )
                    {
                        if (!selectedCharacter.GetComponent<Character>().Moved)
                        {
                            //select character for moving and set up line renderer
                            currentMovement = selectedCharacter.GetComponent<Character>().Movement;
                            lineRenderer.positionCount = 1;
                            tiles.Push(selectedCharacter.GetComponent<Character>().CurrentTile);
                            Vector3 startLocation = selectedCharacter.GetComponent<Character>().CurrentTile.gameObject.transform.position;
                            lineRenderer.SetPosition(0, new Vector3(startLocation.x, startLocation.y + 0.2f, startLocation.z));
                        }
                        else
                        {
                            selectedCharacter = null;
                        }
                    }
                }
            }

            //If there is a player selected
            if (selectedCharacter != null)
            {
                //Move Player to selected tile
                if (Input.GetKeyDown(KeyCode.M))
                {
                    selectedCharacter.gameObject.GetComponent<Character>().MoveToTile(tiles.Peek());
                    selectedCharacter.gameObject.GetComponent<Character>().Moved = true;
                    ClearCurrentPath();
                }

                //Clear current path but do not move
                if (Input.GetKeyDown(KeyCode.N))
                {
                    ClearCurrentPath();
                }


            }


            //Enemy turn
            if (Input.GetKeyDown(KeyCode.P))
            {
                ClearCurrentPath();
                currentTurn = TurnOrder.Alien;
            }
        }
        else
        {
            //Fill in enemy behavior here
            currentTurn = TurnOrder.Player;
            SetUpPlayerTurn();
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
                lineRenderer.SetPosition(tiles.Count - 1, new Vector3(tilePos.x, tilePos.y + lineYOffset, tilePos.z));
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
        if(tileInQuestion.IsAvailableTile())
        {
            //Distance so tiles are orthoganally adjacent
            if(Vector3.Distance(tileInQuestion.gameObject.transform.position,tiles.Peek().transform.position) < 1.2)
            return true;
        }
        return false;
    }

    private void SetUpPlayerTurn()
    {
        characters = GameObject.FindGameObjectsWithTag("Character");
        Debug.Log("Setting up next turn");
        foreach (GameObject character in characters)
        {
            character.GetComponent<Character>().Moved = false;
        }
    }

    public void FindAvailableTiles()
    {
        List<Tile> currentLevelTiles = new List<Tile>();
        currentLevelTiles.Add(startingTile);
        for(int i = 0; i <= currentMovement; i++)
        {
            List<Tile> nextLevelTiles = new List<Tile>();
            foreach (Tile tile in currentLevelTiles)
            {
                //left tile to current tile
                Tile adjacentLeftTile = GenerateLevel.grid[tile.xPos - 1, tile.zPos].GetComponent<Tile>();
                if(!availableTiles.Contains(adjacentLeftTile))
                {
                    availableTiles.Add(adjacentLeftTile);
                    nextLevelTiles.Add(adjacentLeftTile);
                }

                //right Tile
                Tile adjacentRightTile = GenerateLevel.grid[tile.xPos + 1, tile.zPos].GetComponent<Tile>();
                if (!availableTiles.Contains(adjacentRightTile))
                {
                    availableTiles.Add(adjacentRightTile);
                    nextLevelTiles.Add(adjacentRightTile);
                }

                //bottom tile
                Tile adjacentBottomTile = GenerateLevel.grid[tile.xPos, tile.zPos - 1].GetComponent<Tile>();
                if (!availableTiles.Contains(adjacentBottomTile))
                {
                    availableTiles.Add(adjacentBottomTile);
                    nextLevelTiles.Add(adjacentBottomTile);
                }

                //top tile
                Tile adjacentTopTile = GenerateLevel.grid[tile.xPos, tile.zPos + 1].GetComponent<Tile>();
                if (!availableTiles.Contains(adjacentTopTile))
                {
                    availableTiles.Add(adjacentTopTile);
                    nextLevelTiles.Add(adjacentTopTile);
                }
            }

            currentLevelTiles.Clear();
            currentLevelTiles = nextLevelTiles;
            nextLevelTiles.Clear();
        }
    }
}
