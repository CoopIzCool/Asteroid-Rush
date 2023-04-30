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
        PlayerSelect,
        MovementSelect, 
        AttackSelect,
        Animating,
    }

    private RaycastManager raycastManager;
    [SerializeField]private GameObject selectedCharacter;
    private TurnOrder currentTurn;
    [SerializeField]
    private PlayerState currentPlayerState;
    public GameObject[] characters;
    private TileFinder tileFinder = new TileFinder();
    [SerializeField]
    private Rocket rocket;

    [Header("Movement Components")]
    [SerializeField] private GenerateLevel levelGenerator;
    [SerializeField]private List<Tile> availableTiles = new List<Tile>();
    private Tile startingTile;
    private List<UnrefinedOre> oresWithDrillBots;
    private LineRenderer lineRenderer;

    [Header("Attacking Components")]
    [SerializeField]
    private List<Tile> attackableTiles = new List<Tile>();

    [Header("Mining Components")]
    [SerializeField]
    private List<Tile> mineableTiles = new List<Tile>();
    private bool canDeposit = false;
    #endregion

    #region Properties
    public GameObject SelectedCharacter 
    { 
        get { return selectedCharacter; }
        set { selectedCharacter = value; }
    }
    public TileFinder TileLocater
    {
        get { return tileFinder; }
    }
    public TurnOrder CurrentTurn 
    { 
        get { return currentTurn; } 
    }
    public Rocket RocketObject
    {
        set { rocket = value; }
    }
    #endregion

    private static TurnHandler instance;
    public static TurnHandler Instance {  get { return instance; } }

    // use awake because this is referenced by GenerateLevel.cs Start()
    void Awake()
    {
        raycastManager = gameObject.GetComponent<RaycastManager>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        currentTurn = TurnOrder.Player;
        currentPlayerState = PlayerState.PlayerSelect;
        instance = this;
        oresWithDrillBots = new List<UnrefinedOre>();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentTurn == TurnOrder.Alien) {
            AlienManager.Instance.UpdateTurn();
            return;
        }

        
        
        // prevent actions while characters are animating
        //foreach(GameObject character in characters) {
        //    if(character.GetComponent<Character>().Animating) {
        //        return;
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //{
        //    if (selectedCharacter != null)
        //    {
        //        //AddTile();
        //        if (currentPlayerState == PlayerState.MovementSelect)
        //        {
        //            MoveToTile();
        //        }
        //        else if (currentPlayerState == PlayerState.AttackSelect)
        //        {
        //            Debug.Log("Attackin");
        //            AttackAtTile();
        //        }

        //    }
        //    //If character is not selected, look for one to select
        //    else
        //    {
        //        raycastManager.TestRaycast();
        //        if (selectedCharacter != null )
        //        {
        //            if (!selectedCharacter.GetComponent<Character>().Moved)
        //            {
        //                //select character for moving and set up line renderer
        //                #region Defunct LineRendererCode
        //                /*
        //                lineRenderer.positionCount = 1;
        //                Vector3 startLocation = selectedCharacter.GetComponent<Character>().CurrentTile.gameObject.transform.position;
        //                lineRenderer.SetPosition(0, new Vector3(startLocation.x, startLocation.y + 0.2f, startLocation.z));*/
        //                #endregion
        //                availableTiles = tileFinder.FindAvailableTiles(selectedCharacter.GetComponent<Character>());
        //                // highlight available tiles
        //                SetAvailableTiles();
        //                currentPlayerState = PlayerState.MovementSelect;
        //            }
        //            else
        //            {
        //                selectedCharacter = null;
        //                currentPlayerState = PlayerState.None;
        //            }
        //        }
        //    }
        //}

        ////If there is a player selected
        //if (selectedCharacter != null)
        //{
        //    if(currentPlayerState == PlayerState.AttackSelect && (attackableTiles.Count == 0 && mineableTiles.Count == 0 && !canDeposit))
        //    {
        //        selectedCharacter = null;
        //        currentPlayerState = PlayerState.None;
        //    }


        //    if(Input.GetKeyDown(KeyCode.C))
        //    {
        //        CancelCharacterTurn();
        //    }

        //}


        //Enemy turn
        if (Input.GetKeyDown(KeyCode.P))
        {
            EndPlayerTurn();
        }

        // player turn
        switch (currentPlayerState) {
            case PlayerState.PlayerSelect:
                // look for a character to select
                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    raycastManager.TestRaycast();
                    if (selectedCharacter != null && !selectedCharacter.GetComponent<Character>().Moved)
                    {
                        //select character for moving and set up line renderer
                        #region Defunct LineRendererCode
                        /*
                        lineRenderer.positionCount = 1;
                        Vector3 startLocation = selectedCharacter.GetComponent<Character>().CurrentTile.gameObject.transform.position;
                        lineRenderer.SetPosition(0, new Vector3(startLocation.x, startLocation.y + 0.2f, startLocation.z));*/
                        #endregion
                        availableTiles = tileFinder.FindAvailableTiles(selectedCharacter.GetComponent<Character>());
                        // highlight available tiles
                        SetAvailableTiles();
                        currentPlayerState = PlayerState.MovementSelect;
                    }
                }

                // end turn button can end the turn
                break;

            case PlayerState.MovementSelect:
                // choose a tile to move to
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    MoveToTile();
                }
                break;

            case PlayerState.AttackSelect:
                // choose a tile to attack
                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    AttackAtTile();
                    currentPlayerState = PlayerState.PlayerSelect;
                }
                break;

            case PlayerState.Animating:
                // wait for the character to stop moving
                if(!selectedCharacter.GetComponent<Character>().Animating) {
                    currentPlayerState = PlayerState.AttackSelect;

                    FindAttackableTiles(selectedCharacter.GetComponent<Character>().CurrentTile);

                    // go back to player select if this character has nothing to attack
                    if (attackableTiles.Count == 0 && mineableTiles.Count == 0 && !canDeposit)
                    {
                        selectedCharacter = null;
                        currentPlayerState = PlayerState.PlayerSelect;
                    }
                }
                break;
        }
    }

    #region Tile Clears
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

    private void ClearMiningPath()
    {
        selectedCharacter = null;
        mineableTiles.Clear();
    }

    private void ClearAvailableTiles()
    {
        foreach (Tile tile in availableTiles)
        {
            tile.SetAvailabillitySelector(false);
        }
        availableTiles.Clear();

        foreach (Tile tile in attackableTiles)
        {
            tile.SetAvailabillitySelector(false);
        }
        attackableTiles.Clear();

        foreach (Tile tile in mineableTiles)
        {
            tile.SetAvailabillitySelector(false);
        }
        mineableTiles.Clear();

        canDeposit = false;
    }
    #endregion


    private void SetUpPlayerTurn()
    {
        Debug.Log("Setting up next turn");
        foreach (GameObject character in characters)
        {
            character.GetComponent<Character>().Moved = false;
        }
    }

    #region Set Tile
    private void SetAvailableTiles()
    {
        //Debug.Log("Turning moving on");
        foreach (Tile tile in availableTiles)
        {
            //Debug.Log("Turning on " + tile.zPos + " , " + tile.xPos);
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

    private void SetMineableTiles()
    {
        foreach (Tile tile in mineableTiles)
        {
            tile.SetAvailabillitySelector(true);
        }
    }
    #endregion


    #region Turn End
    public void EndPlayerTurn()
    {
        // drill bots deal damage at the end of each turn
        for(int i = 0; i < oresWithDrillBots.Count; i++) {
            bool destroyed = oresWithDrillBots[i].DealDrillBotDamage();
            if(destroyed) {
                oresWithDrillBots.RemoveAt(i);
                i--;
                characters[2].GetComponent<Supporter>().CollectOre();
            }
        }

        ClearAvailableTiles();
        currentTurn = TurnOrder.Alien;
        AlienManager.Instance.TakeTurn();
    }

    public void EndAlienTurn() {
        currentTurn = TurnOrder.Player;
        SetUpPlayerTurn();
    }
    #endregion

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
        if(selectedTile) {
            Debug.Log(selectedTile.occupant);
        }
        if (selectedTile != null)
        {
            if (selectedTile.occupant.GetComponent<Character>())
                AttackEnemy(selectedTile);
            else if (selectedTile.occupant.GetComponent<UnrefinedOre>())
                MineAtTile(selectedTile);
            else if (selectedTile.occupant.GetComponent<Rocket>())
                DepositOre();
        }
    }

    #region Player Options
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
        currentPlayerState = PlayerState.Animating;
    }

    private void FindAttackableTiles(Tile currentTile) {
        if (currentTile.rocketAccessible && (selectedCharacter.GetComponent<Character>().OreCount >= 1))
        {
            canDeposit = true;
            rocket.CanDeposit();
        }
        attackableTiles = tileFinder.FindAvailableAttackingTiles(selectedCharacter.GetComponent<Character>(), currentTile);
        SetAttackableTiles();
        mineableTiles = tileFinder.FindAvailableMinableTiles(selectedCharacter.GetComponent<Character>(), currentTile);
        SetMineableTiles();
        
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
        selectedCharacter.GetComponent<Character>().RotateToward(selectedTile.occupant.transform.position - selectedCharacter.transform.position);

        ClearAvailableTiles();
    }

    private void MineAtTile(Tile selectedTile)
    {
        foreach (Tile tile in mineableTiles)
        {
            tile.SetAvailabillitySelector(false);
        }
        Debug.Log("Mining");
        selectedCharacter.gameObject.GetComponent<Character>().MineOre(selectedTile.occupant.GetComponent<UnrefinedOre>());
        ClearAvailableTiles();
    }

    private void DepositOre()
    {
        Debug.Log("Depositing Ore");
        rocket.DepositOre(selectedCharacter.GetComponent<Character>().OreCount);
        selectedCharacter.GetComponent<Character>().OreCount = 0;
        canDeposit = false;
        ClearAvailableTiles();
    }
    #endregion


    public void CancelCharacterTurn()
    {
        ClearAvailableTiles();
        selectedCharacter = null;
        currentPlayerState = PlayerState.PlayerSelect;
    }

    public void AddDrillingOre(UnrefinedOre ore) {
        oresWithDrillBots.Add(ore);
    }



}
