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
    [SerializeField] private GenerateLevel levelGenerator;
    [SerializeField]private List<Tile> availableTiles = new List<Tile>();
    private Tile startingTile;
    private List<UnrefinedOre> oresWithDrillBots;
    #endregion

    #region Properties
    public GameObject SelectedCharacter 
    { 
        get { return selectedCharacter; }
        set { selectedCharacter = value; }
    }
    #endregion

    private static TurnHandler instance;
    public static TurnHandler Instance {  get { return instance; } }

    // Start is called before the first frame update
    void Start()
    {
        raycastManager = gameObject.GetComponent<RaycastManager>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        currentTurn = TurnOrder.Player;
        instance = this;
        oresWithDrillBots = new List<UnrefinedOre>();
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

                    //AddTile();
                    MoveToTile();

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
                            availableTiles = FindAvailableTiles(selectedCharacter.GetComponent<Character>());
                            SetAvailableTiles();
                            // highlight available tiles
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

                }

                //Clear current path but do not move
                if (Input.GetKeyDown(KeyCode.N))
                {
                    ClearCurrentPath();
                }

                if(Input.GetKeyDown(KeyCode.C))
                {
                    ClearAvailableTiles();
                }

            }


            //Enemy turn
            if (Input.GetKeyDown(KeyCode.P))
            {
                EndPlayerTurn();
            }
        }
        else
        {
            //Fill in enemy behavior here
            

            // TEMP: immediately go back to player turn
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

    public List<Tile> FindAvailableTiles(Character chosenCharacter)
    {
        Tile start = chosenCharacter.CurrentTile;
        List<Tile> currentLevelTiles = new List<Tile>();
        List<Tile> moveableTiles = new List<Tile>();
        currentLevelTiles.Add(start);
        for(int i = 0; i < chosenCharacter.Movement; i++)
        {
            List<Tile> nextLevelTiles = new List<Tile>();
            foreach (Tile tile in currentLevelTiles)
            {
                //Debug.Log(tile.gameObject);
                //left tile to current tile
                if (tile.xPos > 0)
                {
                    
                    Tile adjacentLeftTile = GenerateLevel.grid[tile.zPos, tile.xPos - 1].GetComponent<Tile>();
                    if (!moveableTiles.Contains(adjacentLeftTile) && adjacentLeftTile.IsAvailableTile())
                    {
                        moveableTiles.Add(adjacentLeftTile);
                        nextLevelTiles.Add(adjacentLeftTile);
                        //adjacentLeftTile.SetAvailabillitySelector(true);
                    }
                }

                //right Tile
                if (tile.xPos < GenerateLevel.GridWidth - 1)
                {
                    Tile adjacentRightTile = GenerateLevel.grid[tile.zPos, tile.xPos + 1].GetComponent<Tile>();
                    if (!moveableTiles.Contains(adjacentRightTile) && adjacentRightTile.IsAvailableTile())
                    {
                        moveableTiles.Add(adjacentRightTile);
                        nextLevelTiles.Add(adjacentRightTile);
                        //adjacentRightTile.SetAvailabillitySelector(true);
                    }
                }

                //bottom tile
                if(tile.zPos > 0)
                {
                    Tile adjacentBottomTile = GenerateLevel.grid[tile.zPos - 1, tile.xPos].GetComponent<Tile>();
                    if (!moveableTiles.Contains(adjacentBottomTile) && adjacentBottomTile.IsAvailableTile())
                    {
                        moveableTiles.Add(adjacentBottomTile);
                        nextLevelTiles.Add(adjacentBottomTile);
                        //adjacentBottomTile.SetAvailabillitySelector(true);
                    }
                }


                //top tile
                if(tile.zPos < GenerateLevel.GridHeight - 1)
                {
                    Tile adjacentTopTile = GenerateLevel.grid[tile.zPos + 1, tile.xPos].GetComponent<Tile>();
                    if (!moveableTiles.Contains(adjacentTopTile) && adjacentTopTile.IsAvailableTile())
                    {
                        moveableTiles.Add(adjacentTopTile);
                        nextLevelTiles.Add(adjacentTopTile);
                        //adjacentTopTile.SetAvailabillitySelector(true);
                    }
                }

            }

            currentLevelTiles.Clear();
            foreach (Tile tile in nextLevelTiles)
            {
                currentLevelTiles.Add(tile);
            }
            nextLevelTiles.Clear();
        }

        return moveableTiles;
    }

    private void ClearAvailableTiles()
    {
        foreach (Tile tile in availableTiles)
        {
            tile.SetAvailabillitySelector(false);
        }
        availableTiles.Clear();
    }

    private void SetAvailableTiles()
    {
        foreach (Tile tile in availableTiles)
        {
            tile.SetAvailabillitySelector(true);
        }
    }

    public void EndPlayerTurn()
    {
        // drill bots deal damage at the end of each turn
        for(int i = 0; i < oresWithDrillBots.Count; i++) {
            bool destroyed = oresWithDrillBots[i].DealDrillBotDamage();
            if(destroyed) {
                oresWithDrillBots.RemoveAt(i);
                i--;
                // TODO: add ore somewhere
            }
        }

        ClearCurrentPath();
        currentTurn = TurnOrder.Alien;
        AlienManager.Instance.TakeTurn();
    }

    private void MoveToTile()
    {
        Tile selectedTile = raycastManager.TileRaycast();
        if (selectedTile != null)
        {
            MoveCharacter(selectedTile);
        }
    }

    private void MoveCharacter(Tile selectedTile)
    {
        foreach(Tile tile in availableTiles)
        {
            tile.SetAvailabillitySelector(false);
        }
        Debug.Log("Moving");
        selectedCharacter.gameObject.GetComponent<Character>().MoveToTile(selectedTile);
        selectedCharacter.gameObject.GetComponent<Character>().Moved = true;
        ClearCurrentPath();
    }

    public void AddDrillingOre(UnrefinedOre ore) {
        oresWithDrillBots.Add(ore);
    }
}
