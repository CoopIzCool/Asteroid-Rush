using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Direction
{
	Up,
	Down,
	Left,
	Right
}

public class GenerateLevel : MonoBehaviour
{
	// The grid
	public static GameObject[,] grid = null;
	private static int gridWidth = 0;
	private static int gridHeight = 0;

	public static int GridWidth
	{
		get { return gridWidth; }
	}

	public static int GridHeight
	{
		get { return gridHeight; }
	}

	// Validation fields
	private List<Vector2Int> corePositions = new List<Vector2Int>();
	private Vector2Int shipPosition;

	[Header("Prefabs")]
	[SerializeField] private GameObject[] tilePrefabs;
	[SerializeField] private GameObject[] objectPrefabs;
	[SerializeField] private GameObject[] playerPrefabs;
	[SerializeField] private GameObject[] enemyPrefabs;
	[SerializeField] private GameObject[] orePrefabs;
	[SerializeField] private GameObject spaceshipPrefab;

	[Space]
	[Header("Zone Parameters")]
	[SerializeField] private GameObject[] parentZones;
	[SerializeField] private int enemyZoneWidth;
	[SerializeField] private int enemyZoneHeight;
	[SerializeField] private int minZoneWidth;
	[SerializeField] private int maxZoneWidth;
	[SerializeField] private int minZoneHeight;
	[SerializeField] private int maxZoneHeight;

	[Space]
	[Header("Grid Size Parameters")]
	[SerializeField] private int minGridWidth;
	[SerializeField] private int maxGridWidth;
	[SerializeField] private int minGridHeight;
	[SerializeField] private int maxGridHeight;

	[Space]
	[Header("Grid Item Parameters")]
	[SerializeField, Min(4)] private int totalOres;
	[SerializeField, Min(1)] private int numOresPerZone;

	[Space]
	[Header("Camera Logic & Misc")]
	[SerializeField] CameraFixedRotation cameraLogic;

	// Start is called before the first frame update
	void Start()
	{

		AlienManager.Instance.Grid = this;
		//do ResetGrid(); while (!IsGridValid());
		ResetGrid();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			ResetGrid();
		}
	}

	/// <summary>
	/// Get the item from the specified position in the grid.
	/// </summary>
	/// <param name="row">The row to access</param>
	/// <param name="col">The column to access</param>
	/// <returns>The item at grid[row, col]. Returns null if outside the grid. </returns>
	public static GameObject GetGridItem(int row, int col)
	{
		if (row < 0 || col < 0 || row >= grid.GetLength(0) || col >= grid.GetLength(1))
		{
			return null;
		}
		return grid[row, col];
	}

	/// <summary>
	/// Set the item at the specified position in the grid to the given value.
	/// </summary>
	/// <param name="row">The row to access</param>
	/// <param name="col">The column to access</param>
	/// <param name="value">The value to assign</param>
	public static void SetGridItem(int row, int col, GameObject value)
	{
		if (grid[row, col] != null)
		{
			if (grid[row, col].TryGetComponent(out Tile tileScript)) Destroy(tileScript.occupant);
			Destroy(grid[row, col]);
		}
		grid[row, col] = value;
		grid[row, col].GetComponent<Tile>().xPos = col;
		grid[row, col].GetComponent<Tile>().zPos = row;
	}

	// finds a path of tiles from the start tile to the end tile avoiding obstacles and occupied tiles.
	// Returns the tiles in the path in order including the start and end tiles. Returns null if no valid path
	public static List<Tile> FindPath(Tile start, Tile end)
	{
		List<Tile> path = new List<Tile>() { start };
		List<Tile> visited = new List<Tile>() { start };
		Stack<Tile> checkOrder = new Stack<Tile>();
		checkOrder.Push(start);

		// simulate stepping one tile at a time
		while (checkOrder.Count > 0)
		{
			Tile current = checkOrder.Pop();
			visited.Add(current);

			// go back to the shortest way to get to this tile
			for (int i = 0; i < path.Count - 1; i++)
			{
				if (path[i].IsAdjacent(current))
				{
					path.RemoveRange(i + 1, path.Count - i - 1);
					break;
				}
			}
			path.Add(current);

			// determine the best order to attempt a move
			Direction[] directionPriority = new Direction[4];
			bool leftBetterThanRight = end.xPos < current.xPos;
			bool upBetterThanDown = end.zPos > current.zPos;

			if (Mathf.Abs(current.xPos - end.xPos) > Mathf.Abs(current.zPos - end.zPos))
			{
				// horizontal first
				directionPriority[0] = (leftBetterThanRight ? Direction.Left : Direction.Right);
				directionPriority[1] = (upBetterThanDown ? Direction.Up : Direction.Down);
				directionPriority[2] = (upBetterThanDown ? Direction.Down : Direction.Up);
				directionPriority[3] = (leftBetterThanRight ? Direction.Right : Direction.Left);
			}
			else
			{
				// vertical first
				directionPriority[0] = (upBetterThanDown ? Direction.Up : Direction.Down);
				directionPriority[1] = (leftBetterThanRight ? Direction.Left : Direction.Right);
				directionPriority[2] = (leftBetterThanRight ? Direction.Right : Direction.Left);
				directionPriority[3] = (upBetterThanDown ? Direction.Down : Direction.Up);
			}

			// find the next tile to check
			Tile nextTile = null;
			for (int i = 3; i >= 0; i--)
			{
				Direction direction = directionPriority[i];
				// check for a valid tile in each direction
				int nextX = current.xPos + (direction == Direction.Left ? -1 : 0) + (direction == Direction.Right ? 1 : 0);
				int nextZ = current.zPos + (direction == Direction.Down ? -1 : 0) + (direction == Direction.Up ? 1 : 0);

				GameObject testTileObject = GetGridItem(nextZ, nextX);
				if (testTileObject == null)
				{
					continue; // tile is outside the grid
				}

				Tile testTile = testTileObject.GetComponent<Tile>();
				if ((testTile != end && !testTile.IsAvailableTile()) || visited.Contains(testTile))
				{
					continue; // tile is not walkable or already checked
				}

				checkOrder.Push(testTile);
				nextTile = testTile;
			}

			if (nextTile == null && checkOrder.Count == 0)
			{
				return null; // no valid path
			}
			else if (nextTile == end)
			{
				path.Add(nextTile);
				break;
			}
		}

		return path;
	}

	/// <summary>
	/// Spawns the given prefab at a random unoccupied location in the grid
	/// </summary>
	/// <param name="prefab">The prefab to spawn</param>
	private void SpawnEntityAtRandom(GameObject prefab)
	{
		// Select random positions on the grid until one that is unoccupied is found
		Vector2Int randomPos;
		do
		{
			randomPos = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
		}
		while (grid[randomPos.y, randomPos.x].GetComponent<Tile>().tileType != TileType.Basic && grid[randomPos.y, randomPos.x].GetComponent<Tile>().occupant != null);

		grid[randomPos.y, randomPos.x].GetComponent<Tile>().occupant = Instantiate(prefab, new Vector3(randomPos.x, prefab.transform.position.y, randomPos.y), prefab.transform.rotation);
	}

	/// <summary>
	/// Destroy the old grid if one exists and generate a new one
	/// </summary>
	public void ResetGrid()
	{
		bool hasGoodZones;
		do
		{
			corePositions.Clear();
			gridWidth = Random.Range(minGridWidth, maxGridWidth);
			gridHeight = Random.Range(minGridHeight, maxGridHeight);
			if (grid != null) DestroyGrid();
			hasGoodZones = BuildGrid();
		}
		while (!hasGoodZones || !IsGridValid());
	}

	/// <summary>
	/// Destroy all tiles and their occupants in the grid
	/// </summary>
	private void DestroyGrid()
	{
		for (int row = 0; row < grid.GetLength(0); row++)
		{
			for (int col = 0; col < grid.GetLength(1); col++)
			{
				if (grid[row, col] != null)
				{
					if (grid[row, col].TryGetComponent(out Tile tileScript)) Destroy(tileScript.occupant);
					Destroy(grid[row, col]);
				}
			}
		}

		foreach (GameObject parentZone in parentZones)
		{
			for (int i = 0; i < parentZone.transform.childCount; i++)
			{
				Destroy(parentZone.transform.GetChild(i).gameObject);
			}
		}

		grid = null;
	}

	/// <summary>
	/// Build a new grid, then spawn all necessary entities
	/// </summary>
	/// <returns>True if a valid grid is built, false otherwise</returns>
	private bool BuildGrid()
	{
		// Make new grid
		grid = new GameObject[gridHeight, gridWidth];

		//Set offset of the camera based on grid height
		cameraLogic.XShift = gridHeight / 2.0f;
		cameraLogic.ZShift = gridWidth / 2.0f;
		cameraLogic.SetRadiusAndCenter();

		// Create spaceship / player spawn zone in center
		#region Spaceship Zone
		GameObject shipZoneObj = new GameObject();
		int shipZoneWidth = Random.Range(minZoneWidth, maxZoneWidth);
		int shipZoneHeight = Random.Range(minZoneHeight, maxZoneHeight);
		int shipZoneXPos = gridWidth / 2 - shipZoneWidth / 2;
		int shipZoneZPos = gridHeight / 2 - shipZoneHeight / 2;
		Zone shipZone = InitializeZone(shipZoneObj, "ShipZone", parentZones[0].transform, shipZoneXPos, shipZoneZPos, shipZoneWidth, shipZoneHeight);

		for (int row = shipZone.zPos; row < shipZone.zPos + shipZone.height; row++)
		{
			for (int col = shipZone.xPos; col < shipZone.xPos + shipZone.width; col++)
			{
				grid[row, col] = Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, shipZoneObj.transform);
				shipZone.tiles.Add(grid[row, col]);
				grid[row, col].GetComponent<Tile>().xPos = col;
				grid[row, col].GetComponent<Tile>().zPos = row;
			}
		}

		shipPosition = new Vector2Int(gridHeight / 2, gridWidth / 2);
		grid[gridHeight / 2, gridWidth / 2].GetComponent<Tile>().occupant = Instantiate(spaceshipPrefab, new Vector3(gridWidth / 2, spaceshipPrefab.transform.position.y, gridHeight / 2), spaceshipPrefab.transform.rotation, shipZoneObj.transform);
		GameObject player1 = Instantiate(playerPrefabs[0], new Vector3(gridWidth / 2 - 1, playerPrefabs[0].transform.position.y, gridHeight / 2 + 1), playerPrefabs[0].transform.rotation, shipZoneObj.transform);
		grid[gridHeight / 2 + 1, gridWidth / 2 - 1].GetComponent<Tile>().occupant = player1;
		GameObject player2 = Instantiate(playerPrefabs[1], new Vector3(gridWidth / 2 + 1, playerPrefabs[1].transform.position.y, gridHeight / 2 + 1), playerPrefabs[1].transform.rotation, shipZoneObj.transform);
		grid[gridHeight / 2 + 1, gridWidth / 2 + 1].GetComponent<Tile>().occupant = player2;
		AlienManager.Instance.PlayerCharacters = new GameObject[2] { player1, player2 };

		//Set initial character tile to players
		grid[gridHeight / 2 + 1, gridWidth / 2 - 1].GetComponent<Tile>().occupant.GetComponent<Character>().CurrentTile = grid[gridHeight / 2 + 1, gridWidth / 2 - 1].GetComponent<Tile>();
		grid[gridHeight / 2 + 1, gridWidth / 2 + 1].GetComponent<Tile>().occupant.GetComponent<Character>().CurrentTile = grid[gridHeight / 2 + 1, gridWidth / 2 + 1].GetComponent<Tile>();

		#endregion

		// Create travel zones orthogonally adjacent to the spaceship zone
		#region Travel Zones
		for (int i = 0; i < 4; i++)
		{
			GameObject travelZoneObj = new GameObject();
			int travelZoneWidth = Random.Range(minZoneWidth, maxZoneWidth);
			int travelZoneHeight = Random.Range(minZoneHeight, maxZoneHeight);
			int travelZoneXPos;
			int travelZoneZPos;
			ZoneTypes zoneType = (ZoneTypes)Random.Range(2, 5);
			//ZoneTypes zoneType = ZoneTypes.Tunnel;

			switch (i)
			{
				case 0:
					travelZoneXPos = travelZoneWidth + 1;
					travelZoneZPos = 1;
					break;
				case 1:
					travelZoneXPos = travelZoneWidth + 1;
					travelZoneZPos = gridHeight - 1 - travelZoneHeight;
					break;
				case 2:
					travelZoneXPos = 1;
					travelZoneZPos = travelZoneHeight + 1;
					break;
				default:
					travelZoneXPos = gridWidth - 1 - travelZoneWidth;
					travelZoneZPos = travelZoneWidth + 1;
					break;
			}

			Zone travelZone = InitializeZone(travelZoneObj, zoneType.ToString() + "Zone", parentZones[3].transform, travelZoneXPos, travelZoneZPos, travelZoneWidth, travelZoneHeight);
			travelZone.zoneType = zoneType;

			travelZone.BuildZone(tilePrefabs, objectPrefabs);
		}
		#endregion

		// Create alien spawn zones on each edge
		#region Enemy Zones
		GameObject[] enemyZoneObjs = new GameObject[4];
		for (int i = 0; i < enemyZoneObjs.Length; i++)
		{
			enemyZoneObjs[i] = new GameObject();
			InitializeZone(enemyZoneObjs[i], "EnemyZone", parentZones[1].transform, 0, 0, enemyZoneWidth, enemyZoneHeight);
		}

		int travelZonePos = parentZones[3].transform.GetChild(0).GetComponent<Zone>().xPos;
		int travelZoneDim = parentZones[3].transform.GetChild(0).GetComponent<Zone>().width;
		Tile tile;
		for (int col = travelZonePos; col < travelZonePos + travelZoneDim; col++)
		{
			if (enemyZoneObjs[0].transform.childCount == enemyZoneWidth)
			{
				corePositions.Add(new Vector2Int(0, col - 1));
				break;
			}

			if (IsTileWalkable(col, 1))
			{
				grid[0, col] = Instantiate(tilePrefabs[1], new Vector3(col, 0, 0), tilePrefabs[1].transform.rotation, enemyZoneObjs[0].transform);
				enemyZoneObjs[0].GetComponent<Zone>().tiles.Add(grid[0, col]);
				grid[0, col].GetComponent<Tile>().xPos = col;
				grid[0, col].GetComponent<Tile>().zPos = 0;
			}
		}

		travelZonePos = parentZones[3].transform.GetChild(1).GetComponent<Zone>().xPos;
		travelZoneDim = parentZones[3].transform.GetChild(1).GetComponent<Zone>().width;
		for (int col = travelZonePos; col < travelZonePos + travelZoneDim; col++)
		{
			if (enemyZoneObjs[2].transform.childCount == enemyZoneWidth)
			{
				corePositions.Add(new Vector2Int(gridHeight - 1, col - 1));
				break;
			}

			if (IsTileWalkable(col, gridHeight - 2))
			{
				grid[gridHeight - 1, col] = Instantiate(tilePrefabs[1], new Vector3(col, 0, gridHeight - 1), tilePrefabs[1].transform.rotation, enemyZoneObjs[2].transform);
				enemyZoneObjs[2].GetComponent<Zone>().tiles.Add(grid[gridHeight - 1, col]);
				grid[gridHeight - 1, col].GetComponent<Tile>().xPos = col;
				grid[gridHeight - 1, col].GetComponent<Tile>().zPos = gridHeight - 1;
			}
		}

		travelZonePos = parentZones[3].transform.GetChild(2).GetComponent<Zone>().zPos;
		travelZoneDim = parentZones[3].transform.GetChild(2).GetComponent<Zone>().height;
		for (int row = travelZonePos; row < travelZonePos + travelZoneDim; row++)
		{
			if (enemyZoneObjs[1].transform.childCount == enemyZoneWidth)
			{
				corePositions.Add(new Vector2Int(row - 1, 0));
				break;
			}

			tile = grid[row, 1].GetComponent<Tile>();
			if (IsTileWalkable(1, row))
			{
				grid[row, 0] = Instantiate(tilePrefabs[1], new Vector3(0, 0, row), tilePrefabs[1].transform.rotation, enemyZoneObjs[1].transform);
				enemyZoneObjs[1].GetComponent<Zone>().tiles.Add(grid[row, 0]);
				grid[row, 0].GetComponent<Tile>().xPos = 0;
				grid[row, 0].GetComponent<Tile>().zPos = row;
			}
		}

		travelZonePos = parentZones[3].transform.GetChild(3).GetComponent<Zone>().zPos;
		travelZoneDim = parentZones[3].transform.GetChild(3).GetComponent<Zone>().height;
		for (int row = travelZonePos; row < travelZonePos + travelZoneDim; row++)
		{
			if (enemyZoneObjs[3].transform.childCount == enemyZoneWidth)
			{
				corePositions.Add(new Vector2Int(row - 1, gridWidth - 1));
				break;
			}

			tile = grid[row, gridWidth - 2].GetComponent<Tile>();
			if (IsTileWalkable(gridWidth - 2, row))
			{
				grid[row, gridWidth - 1] = Instantiate(tilePrefabs[1], new Vector3(gridWidth - 1, 0, row), tilePrefabs[1].transform.rotation, enemyZoneObjs[3].transform);
				enemyZoneObjs[3].GetComponent<Zone>().tiles.Add(grid[row, gridWidth - 1]);
				grid[row, gridWidth - 1].GetComponent<Tile>().xPos = gridWidth - 1;
				grid[row, gridWidth - 1].GetComponent<Tile>().zPos = row;
			}
		}
		#endregion

		// Create borders along edges
		#region Borders
		GameObject borderZoneObj = new GameObject();
		Zone borderZone = InitializeZone(borderZoneObj, "BorderZone", parentZones[2].transform, 0, 0, gridWidth, gridHeight);

		for (int col = 0; col < gridWidth; col++)
		{
			if (col == 0 || col == gridWidth - 1)
			{
				if (grid[0, col] == null)
				{
					grid[0, col] = Instantiate(tilePrefabs[4], new Vector3(col, tilePrefabs[4].transform.position.y, 0), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[0, col]);
					grid[0, col].GetComponent<Tile>().xPos = col;
					grid[0, col].GetComponent<Tile>().zPos = 0;
				}

				if (grid[gridHeight - 1, col] == null)
				{
					grid[gridHeight - 1, col] = Instantiate(tilePrefabs[4], new Vector3(col, tilePrefabs[4].transform.position.y, gridHeight - 1), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[gridHeight - 1, col]);
					grid[gridHeight - 1, col].GetComponent<Tile>().xPos = col;
					grid[gridHeight - 1, col].GetComponent<Tile>().zPos = gridHeight - 1;
				}
			}
			else
			{
				if (grid[0, col] == null)
				{
					grid[0, col] = Instantiate(tilePrefabs[3], new Vector3(col, tilePrefabs[3].transform.position.y, 0), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[0, col]);
					grid[0, col].GetComponent<Tile>().xPos = col;
					grid[0, col].GetComponent<Tile>().zPos = 0;
				}

				if (grid[gridHeight - 1, col] == null)
				{
					grid[gridHeight - 1, col] = Instantiate(tilePrefabs[3], new Vector3(col, tilePrefabs[3].transform.position.y, gridHeight - 1), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[gridHeight - 1, col]);
					grid[gridHeight - 1, col].GetComponent<Tile>().xPos = col;
					grid[gridHeight - 1, col].GetComponent<Tile>().zPos = gridHeight - 1;
				}
			}
		}

		for (int row = 0; row < gridHeight; row++)
		{
			if (row == 0 || row == gridHeight - 1)
			{
				if (grid[row, 0] == null)
				{
					grid[row, 0] = Instantiate(tilePrefabs[4], new Vector3(0, tilePrefabs[4].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, 0]);
					grid[row, 0].GetComponent<Tile>().xPos = 0;
					grid[row, 0].GetComponent<Tile>().zPos = row;
				}

				if (grid[row, gridWidth - 1] == null)
				{
					grid[row, gridWidth - 1] = Instantiate(tilePrefabs[4], new Vector3(gridWidth - 1, tilePrefabs[4].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, gridWidth - 1]);
					grid[row, gridWidth - 1].GetComponent<Tile>().xPos = gridWidth - 1;
					grid[row, gridWidth - 1].GetComponent<Tile>().zPos = row;
				}
			}
			else
			{
				if (grid[row, 0] == null)
				{
					grid[row, 0] = Instantiate(tilePrefabs[3], new Vector3(0, tilePrefabs[3].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, 0]);
					grid[row, 0].GetComponent<Tile>().xPos = 0;
					grid[row, 0].GetComponent<Tile>().zPos = row;
				}

				if (grid[row, gridWidth - 1] == null)
				{
					grid[row, gridWidth - 1] = Instantiate(tilePrefabs[3], new Vector3(gridWidth - 1, tilePrefabs[3].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, gridWidth - 1]);
					grid[row, gridWidth - 1].GetComponent<Tile>().xPos = gridWidth - 1;
					grid[row, gridWidth - 1].GetComponent<Tile>().zPos = row;
				}
			}
		}
		#endregion

		// Create ore spawn zones in the corners of the level
		#region Ore Zones
		GameObject[] oreZoneObjs = new GameObject[4];
		for (int i = 0; i < oreZoneObjs.Length; i++)
		{
			oreZoneObjs[i] = new GameObject();
			Zone oreZone = InitializeZone(oreZoneObjs[i], "OreZone", parentZones[4].transform, 0, 0, Random.Range(minZoneWidth, maxZoneWidth), Random.Range(minZoneHeight, maxZoneHeight));
			oreZone.zoneType = (ZoneTypes)Random.Range(0, 2);
			bool isOreClose;
			int oreX;
			int oreZ;

			switch (i)
			{
				case 0:
					oreX = 1;
					oreZ = 1;
					isOreClose = IsOreClose(oreZone, oreX, oreZ, grid[oreZone.height + 1, oreX].transform.parent.GetComponent<Zone>(), grid[oreZ, oreZone.width + 1].transform.parent.GetComponent<Zone>(), i);
					break;
				case 1:
					oreX = 1;
					oreZ = gridHeight - 1 - oreZone.height;
					isOreClose = IsOreClose(oreZone, oreX, oreZ, grid[oreZ - 1, oreX].transform.parent.GetComponent<Zone>(), grid[oreZ, oreZone.width + 1].transform.parent.GetComponent<Zone>(), i);
					break;
				case 2:
					oreX = gridWidth - 1 - oreZone.width;
					oreZ = gridHeight - 1 - oreZone.height;
					isOreClose = IsOreClose(oreZone, oreX, oreZ, grid[oreZ - 1, oreX].transform.parent.GetComponent<Zone>(), grid[oreZ, oreX - 1].transform.parent.GetComponent<Zone>(), i);
					break;
				default:
					oreX = gridWidth - 1 - oreZone.width;
					oreZ = 1;
					isOreClose = IsOreClose(oreZone, oreX, oreZ, grid[oreZone.height + 1, oreX].transform.parent.GetComponent<Zone>(), grid[oreZ, oreX - 1].transform.parent.GetComponent<Zone>(), i);
					break;
			}

			if (!isOreClose) return false;

			oreZone.BuildZone(tilePrefabs, objectPrefabs);

			int randomX;
			int randomZ;
			int numOres = 0;

			do
			{
				do
				{
					randomX = Random.Range(oreZone.xPos, oreZone.xPos + oreZone.width);
					randomZ = Random.Range(oreZone.zPos, oreZone.zPos + oreZone.height);
				} while (!IsTileWalkable(randomX, randomZ));

				SetGridItem(randomZ, randomX, Instantiate(tilePrefabs[0], new Vector3(randomX, 0, randomZ), tilePrefabs[0].transform.rotation, oreZoneObjs[i].transform));
				grid[randomZ, randomX].GetComponent<Tile>().occupant = Instantiate(orePrefabs[0], new Vector3(randomX, orePrefabs[0].transform.position.y, randomZ), Quaternion.identity, oreZoneObjs[i].transform);
				grid[randomZ, randomX].GetComponent<Tile>().xPos = randomX;
				grid[randomZ, randomX].GetComponent<Tile>().zPos = randomZ;

				numOres++;
			} while (numOres < numOresPerZone);

			corePositions.Add(new Vector2Int(randomZ, randomX));
		}

		int remainingOres = totalOres / (numOresPerZone * oreZoneObjs.Length);
		for (int i = 0; i < remainingOres; i++)
		{
			Zone randomZone = oreZoneObjs[Random.Range(0, oreZoneObjs.Length)].GetComponent<Zone>();
			int randomX;
			int randomZ;
			int timesRun = 0;
			bool badZone = false;

			do
			{
				randomX = Random.Range(randomZone.xPos, randomZone.xPos + randomZone.width);
				randomZ = Random.Range(randomZone.zPos, randomZone.zPos + randomZone.height);

				if (timesRun > 100)
				{
					badZone = true;
					break;
				}

				timesRun++;
			} while (!IsTileWalkable(randomX, randomZ));

			if (badZone)
			{
				i--;
				continue;
			}

			SetGridItem(randomZ, randomX, Instantiate(tilePrefabs[0], new Vector3(randomX, 0, randomZ), tilePrefabs[0].transform.rotation, oreZoneObjs[i].transform));
			grid[randomZ, randomX].GetComponent<Tile>().occupant = Instantiate(orePrefabs[0], new Vector3(randomX, orePrefabs[0].transform.position.y, randomZ), Quaternion.identity, randomZone.transform);
			grid[randomZ, randomX].GetComponent<Tile>().xPos = randomX;
			grid[randomZ, randomX].GetComponent<Tile>().zPos = randomZ;
		}
		#endregion

		return true;
	}

	private Zone InitializeZone(GameObject zoneObj, string name, Transform parent, int xPos, int zPos, int width, int height)
	{
		zoneObj.name = name;
		zoneObj.transform.parent = parent;
		Zone zone = zoneObj.AddComponent<Zone>();
		zone.xPos = xPos;
		zone.zPos = zPos;
		zone.width = width;
		zone.height = height;

		return zone;
	}

	private bool IsOreClose(Zone oreZone, int oreZoneXPos, int oreZoneZPos, Zone adjacentZone1, Zone adjacentZone2, int location)
	{
		Tuple<Zone, Zone> adjacentZones = new Tuple<Zone, Zone>(adjacentZone1, adjacentZone2);
		bool oreToTravel = false;
		bool travelToShip = false;

		int[] modifiers;

		switch (location)
		{
			case 0:
				modifiers = new int[] { 0, adjacentZones.Item1.width - 1, 0, adjacentZones.Item2.height - 1 };
				break;
			case 1:
				modifiers = new int[] { adjacentZones.Item1.height - 1, adjacentZones.Item1.width - 1, 0, 0 };
				break;
			case 2:
				modifiers = new int[] { adjacentZones.Item1.height - 1, 0, adjacentZones.Item2.width - 1, 0 };
				break;
			default:
				modifiers = new int[] { 0, 0, adjacentZones.Item2.width - 1, adjacentZones.Item2.height - 1 };
				break;
		}

		oreZone.xPos = oreZoneXPos;
		oreZone.zPos = oreZoneZPos;

		for (int col = adjacentZones.Item1.xPos; col < adjacentZones.Item1.xPos + adjacentZones.Item1.width; col++)
		{
			if (IsTileWalkable(col, adjacentZones.Item1.zPos + modifiers[0]))
			{
				oreToTravel = true;
				break;
			}
		}

		for (int row = adjacentZones.Item1.zPos; row < adjacentZones.Item1.zPos + adjacentZones.Item1.height; row++)
		{
			if (IsTileWalkable(adjacentZones.Item1.xPos + modifiers[1], row))
			{
				travelToShip = true;
				break;
			}
		}

		if (!oreToTravel || !travelToShip)
		{
			oreToTravel = false;
			travelToShip = false;
			for (int row = adjacentZones.Item2.zPos; row < adjacentZones.Item2.zPos + adjacentZones.Item2.height; row++)
			{
				if (IsTileWalkable(adjacentZones.Item2.xPos + modifiers[2], row))
				{
					oreToTravel = true;
					break;
				}
			}

			for (int col = adjacentZones.Item2.xPos; col < adjacentZones.Item2.xPos + adjacentZones.Item2.width; col++)
			{
				if (IsTileWalkable(col, adjacentZones.Item2.zPos + modifiers[3]))
				{
					travelToShip = true;
					break;
				}
			}
		}

		if (!oreToTravel || !travelToShip) return false;
		return true;
	}

	/// <summary>
	/// Returns whether a tile can be occupied
	/// </summary>
	/// <param name="xPos">The x-position of the tile</param>
	/// <param name="zPos">The z-position of the tile</param>
	/// <returns>True if a tile can be occupied, false otherwise</returns>
	public static bool IsTileWalkable(int xPos, int zPos)
	{
		Tile tile = grid[zPos, xPos].GetComponent<Tile>();
		return tile.tileType == TileType.Basic && tile.occupant == null;
	}

	/// <summary>
	/// Runs a modified version of Dijkstra's Algorithm to check if there is a path from the player spawn to all important landmarks in the current level
	/// </summary>
	/// <returns>Whether the current level is valid</returns>
	private bool IsGridValid()
	{
		#region Dijkstra's Algorithm
		List<Vector2Int> openList = new List<Vector2Int> { new Vector2Int(shipPosition.x, shipPosition.y) };
		List<Vector2Int> closedList = new List<Vector2Int>();

		// Current tile
		Vector2Int current = openList[0];
		int currentZ;
		int currentX;

		// Run until there are no tiles left to search
		while (openList.Count > 0)
		{
			current = openList[0];
			currentZ = current.x;
			currentX = current.y;

			// If the item is reached, we are successful
			if (corePositions.Contains(current))
			{
				corePositions.Remove(current);
			}
			else if (corePositions.Count == 0)
			{
				break;
			}

			// Check the four cardinal directions
			for (int i = 0; i < 4; i++)
			{
				Vector2Int endTile;

				switch (i)
				{
					case 0:
						if (currentX == gridWidth - 1) continue;
						endTile = new Vector2Int(currentZ, currentX + 1);
						break;
					case 1:
						if (currentZ == 0) continue;
						endTile = new Vector2Int(currentZ - 1, currentX);
						break;
					case 2:
						if (currentX == 0) continue;
						endTile = new Vector2Int(currentZ, currentX - 1);
						break;
					default:
						if (currentZ == gridHeight - 1) continue;
						endTile = new Vector2Int(currentZ + 1, currentX);
						break;
				}

				// Add any unsearched tiles
				if (!openList.Contains(endTile) && !closedList.Contains(endTile) && (grid[endTile.x, endTile.y].GetComponent<Tile>().tileType == TileType.Basic || grid[endTile.x, endTile.y].GetComponent<Tile>().tileType == TileType.EnemySpawn))
				{
					if (grid[endTile.x, endTile.y].GetComponent<Tile>().occupant == null || grid[endTile.x, endTile.y].GetComponent<Tile>().occupant.tag == "Ore")
					{
						openList.Add(endTile);
					}
				}
			}
			openList.Remove(current);
			closedList.Add(current);
		}

		// If the item was never found, we failed
		if (corePositions.Count > 0)
		{
			Debug.Log("-----------------");
			Debug.Log("MISSED POSITIONS:");
			Debug.Log("-----------------");
			corePositions.ForEach(position =>
			{
				Debug.Log(grid[position.x, position.y].transform.parent);
				Debug.Log(position);
			});
			return false;
		}
		#endregion

		return true;
	}
}
