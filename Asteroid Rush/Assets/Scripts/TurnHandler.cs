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

    public enum PlayerState
    {
        Movement, 
        Attack,
        None
    }
    private RaycastManager raycastManager;
    [SerializeField]private GameObject selectedCharacter;
    private TurnOrder currentTurn;
    private PlayerState currentPlayerState;
    public GameObject[] characters;

    [Header("Movement Components")]
    [SerializeField] private GenerateLevel levelGenerator;
    [SerializeField]private List<Tile> availableTiles = new List<Tile>();
    private Tile startingTile;
    private List<UnrefinedOre> oresWithDrillBots;
    private LineRenderer lineRenderer;

    [Header("Attacking Components")]
    [SerializeField]
    private List<Tile> attackableTiles = new List<Tile>();
    #endregion

    #region Properties
    public GameObject SelectedCharacter 
    { 
        get { return selectedCharacter; }
        set { selectedCharacter = value; }
    }

    public TurnOrder CurrentTurn { get { return currentTurn; } }
    #endregion

    private static TurnHandler instance;
    public static TurnHandler Instance {  get { return instance; } }

    // use awake because this is referenced by GenerateLevel.cs Start()
    void Awake()
    {
        raycastManager = gameObject.GetComponent<RaycastManager>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        currentTurn = TurnOrder.Player;
        currentPlayerState = PlayerState.None;
        instance = this;
        oresWithDrillBots = new List<UnrefinedOre>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(characters.Length);
        if (currentTurn == TurnOrder.Player)
        {
            // prevent actions while characters are animating
            foreach(GameObject character in characters) {
                if(character.GetComponent<Character>().Animating) {
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (selectedCharacter != null)
                {
                    //AddTile();
                    if (currentPlayerState == PlayerState.Movement)
                    {
                        MoveToTile();
                    }
                    else if (currentPlayerState == PlayerState.Attack)
                    {
                        AttackAtTile();
                    }

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
                            #region Defunct LineRendererCode
                            /*
                            lineRenderer.positionCount = 1;
                            Vector3 startLocation = selectedCharacter.GetComponent<Character>().CurrentTile.gameObject.transform.position;
                            lineRenderer.SetPosition(0, new Vector3(startLocation.x, startLocation.y + 0.2f, startLocation.z));*/
                            #endregion
                            availableTiles = FindAvailableTiles(selectedCharacter.GetComponent<Character>());
                            // highlight available tiles
                            SetAvailableTiles();
                            currentPlayerState = PlayerState.Movement;
                        }
                        else
                        {
                            selectedCharacter = null;
                            currentPlayerState = PlayerState.None;
                        }
                    }
                }
            }

            //If there is a player selected
            if (selectedCharacter != null)
            {
                if(currentPlayerState == PlayerState.Attack && attackableTiles.Count == 0)
                {
                    selectedCharacter = null;
                    currentPlayerState = PlayerState.None;
                }


                if(Input.GetKeyDown(KeyCode.C))
                {
                    CancelCharacterTurn();
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
            AlienManager.Instance.UpdateTurn();
        }

    }

    private void ClearCurrentPath()
    {
        availableTiles.Clear();
        
        lineRenderer.positionCount = 0;
    }

    private void ClearAttackPath()
    {
        selectedCharacter = null;
        attackableTiles.Clear();
    }

    private void SetUpPlayerTurn()
    {
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

    public List<Tile> FindAvailableAttackingTiles(Character chosenCharacter, Tile startTile)
    {
        List<Tile> currentLevelTiles = new List<Tile>();
        List<Tile> lookedAtTiles = new List<Tile>();
        List<Tile> attackableTiles = new List<Tile>();
        currentLevelTiles.Add(startTile);
        for (int i = 0; i < chosenCharacter.Range; i++)
        {
            List<Tile> nextLevelTiles = new List<Tile>();
            foreach (Tile tile in currentLevelTiles)
            {
                Debug.Log("Starting at " + tile.zPos + " , " + tile.xPos);
                //left tile to current tile
                if (tile.xPos > 0)
                {
                    Tile adjacentLeftTile = GenerateLevel.grid[tile.zPos, tile.xPos - 1].GetComponent<Tile>();
                    
                    if(!lookedAtTiles.Contains(adjacentLeftTile))
                    {
                        Debug.Log("Checking at " + adjacentLeftTile.zPos + " , " + adjacentLeftTile.xPos);
                        lookedAtTiles.Add(adjacentLeftTile);
                        if (adjacentLeftTile.IsAvailableTile())
                        {
                            nextLevelTiles.Add(adjacentLeftTile);
                            //adjacentLeftTile.SetAvailabillitySelector(true);
                        }
                        else if (adjacentLeftTile.IsAttackable(chosenCharacter.IsPlayer))
                        {
                            attackableTiles.Add(adjacentLeftTile);
                        }
                    }

                }

                //right Tile
                if (tile.xPos < GenerateLevel.GridWidth - 1)
                {
                    Tile adjacentRightTile = GenerateLevel.grid[tile.zPos, tile.xPos + 1].GetComponent<Tile>();
                    if(!lookedAtTiles.Contains(adjacentRightTile))
                    {
                        Debug.Log("Checking at " + adjacentRightTile.zPos + " , " + adjacentRightTile.xPos);
                        lookedAtTiles.Add(adjacentRightTile);
                        if (adjacentRightTile.IsAvailableTile())
                        {
                            nextLevelTiles.Add(adjacentRightTile);
                            //adjacentRightTile.SetAvailabillitySelector(true);
                        }
                        else if (adjacentRightTile.IsAttackable(chosenCharacter.IsPlayer))
                        {
                            attackableTiles.Add(adjacentRightTile);
                        }
                    }

                }

                //bottom tile
                if (tile.zPos > 0)
                {
                    Tile adjacentBottomTile = GenerateLevel.grid[tile.zPos - 1, tile.xPos].GetComponent<Tile>();
                    if(!lookedAtTiles.Contains(adjacentBottomTile))
                    {
                        Debug.Log("Checking at " + adjacentBottomTile.zPos + " , " + adjacentBottomTile.xPos);
                        lookedAtTiles.Add(adjacentBottomTile);
                        if (adjacentBottomTile.IsAvailableTile())
                        {
                            nextLevelTiles.Add(adjacentBottomTile);
                            //adjacentBottomTile.SetAvailabillitySelector(true);
                        }
                        else if (adjacentBottomTile.IsAttackable(chosenCharacter.IsPlayer))
                        {
                            attackableTiles.Add(adjacentBottomTile);
                        }
                    }

                }


                //top tile
                if (tile.zPos < GenerateLevel.GridHeight - 1)
                {
                    Tile adjacentTopTile = GenerateLevel.grid[tile.zPos + 1, tile.xPos].GetComponent<Tile>();
                    if(!lookedAtTiles.Contains(adjacentTopTile))
                    {
                        Debug.Log("Checking at " + adjacentTopTile.zPos + " , " + adjacentTopTile.xPos);
                        lookedAtTiles.Add(adjacentTopTile);
                        if (adjacentTopTile.IsAvailableTile())
                        {
                            nextLevelTiles.Add(adjacentTopTile);
                            //adjacentTopTile.SetAvailabillitySelector(true);
                        }
                        else if (adjacentTopTile.IsAttackable(chosenCharacter.IsPlayer))
                        {
                            attackableTiles.Add(adjacentTopTile);
                        }
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

        return attackableTiles;
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

    private void SetAttackableTiles()
    {
        foreach(Tile tile in attackableTiles)
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

    public void EndAlienTurn() {
        currentTurn = TurnOrder.Player;
        SetUpPlayerTurn();
    }

    private void MoveToTile()
    {
        Tile selectedTile = raycastManager.TileRaycast(selectedCharacter);
        if (selectedTile != null)
        {
            MoveCharacter(selectedTile);
        }
        else
        {
            CancelCharacterTurn();
        }
    }

    private void AttackAtTile()
    {
        Tile selectedTile = raycastManager.TileRaycast();
        if (selectedTile != null)
        {
            AttackEnemy(selectedTile);
        }
    }


    private void MoveCharacter(Tile selectedTile)
    {
        foreach(Tile tile in availableTiles)
        {
            tile.SetAvailabillitySelector(false);
        }
        if(selectedTile != selectedCharacter.GetComponent<Character>().CurrentTile)
        {
            Debug.Log("Moving to " + selectedTile.zPos + " , " + selectedTile.xPos);

            selectedCharacter.gameObject.GetComponent<Character>().SetPath(GenerateLevel.FindPath(selectedCharacter.GetComponent<Character>().CurrentTile, selectedTile), 5.0f);
        }
        selectedCharacter.gameObject.GetComponent<Character>().Moved = true;
        attackableTiles = FindAvailableAttackingTiles(selectedCharacter.GetComponent<Character>(), selectedTile);
        SetAttackableTiles();
        currentPlayerState = PlayerState.Attack;
        ClearCurrentPath();
    }

    private void AttackEnemy(Tile selectedTile)
    {
        foreach(Tile tile in attackableTiles)
        {
            tile.SetAvailabillitySelector(false);
        }
        Debug.Log("Attacking");
        selectedCharacter.gameObject.GetComponent<Character>().Attack(selectedTile.occupant.GetComponent<Character>());
        ClearAttackPath();
    }

    public void CancelCharacterTurn()
    {
        ClearAvailableTiles();
        ClearAttackPath();
        selectedCharacter = null;
        currentPlayerState = PlayerState.None;
    }

    public void AddDrillingOre(UnrefinedOre ore) {
        oresWithDrillBots.Add(ore);
    }
}

#region Defunct Methods
#endregion