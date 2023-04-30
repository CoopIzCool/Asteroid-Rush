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

    void Start()
    {
        raycastManager = gameObject.GetComponent<RaycastManager>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        currentTurn = TurnOrder.Player;
        currentPlayerState = PlayerState.PlayerSelect;
        instance = this;
        oresWithDrillBots = new List<UnrefinedOre>();
    }

    void Update()
    {
        if(currentTurn == TurnOrder.Alien) {
            AlienManager.Instance.UpdateTurn();
            return;
        }

        // player turn
        switch (currentPlayerState) {
            case PlayerState.PlayerSelect:
                // look for a character to select
                //if (Input.GetKeyDown(KeyCode.Mouse0)) {
                //    raycastManager.TestRaycast();
                //    if (selectedCharacter != null && !selectedCharacter.GetComponent<Character>().Moved)
                //    {
                //        availableTiles = tileFinder.FindAvailableTiles(selectedCharacter.GetComponent<Character>());
                //        // highlight available tiles
                //        SetAvailableTiles();
                //        currentPlayerState = PlayerState.MovementSelect;
                //        GameplayUI.Instance.OpenMenu(GameplayMenu.None);
                //    }
                //}

                // wait for the player to click one of the buttons
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
                    bool success = AttackAtTile();
                    if(success) {
                        SetStatePlayerSelect();
                    }
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
                        SetStatePlayerSelect();
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
        currentPlayerState = PlayerState.PlayerSelect;
        GameplayUI.Instance.OpenMenu(GameplayMenu.PlayerSelect);
        GameplayUI.Instance.AllowCharacterSelection();
        foreach (GameObject character in GenerateLevel.PlayerCharacters)
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
                GenerateLevel.PlayerCharacters[2].GetComponent<Supporter>().CollectOre();
            }
        }

        ClearAvailableTiles();
        currentTurn = TurnOrder.Alien;
        GameplayUI.Instance.OpenMenu(GameplayMenu.None);
        AlienManager.Instance.TakeTurn();
    }

    // 0: miner, 1: fighter, 2: supporter
    public void SelectCharacter(int index) {
        // this does not need to check if the player has already moved because
        // the button to select the character would have disappeared
        selectedCharacter = GenerateLevel.PlayerCharacters[index];

        availableTiles = tileFinder.FindAvailableTiles(selectedCharacter.GetComponent<Character>());
        // highlight available tiles
        SetAvailableTiles();
        currentPlayerState = PlayerState.MovementSelect;
        GameplayUI.Instance.OpenMenu(GameplayMenu.None);
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

    // returns whether or not this was successfull
    private bool AttackAtTile()
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

            return true;
        }

        return false;
    }

    // used to auto end the turn whenever a player is done with their move
    private void SetStatePlayerSelect() {
        currentPlayerState = PlayerState.PlayerSelect;
        GameplayUI.Instance.OpenMenu(GameplayMenu.PlayerSelect);

        // auto end turn if no moves left
        bool hasMove = false;
        foreach(GameObject player in GenerateLevel.PlayerCharacters) {
            if(!player.GetComponent<Character>().Moved) {
                hasMove = true;
                break;
            }
        }

        if(!hasMove) {
            EndPlayerTurn();
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
        SetStatePlayerSelect();
    }

    public void AddDrillingOre(UnrefinedOre ore) {
        oresWithDrillBots.Add(ore);
    }



}
