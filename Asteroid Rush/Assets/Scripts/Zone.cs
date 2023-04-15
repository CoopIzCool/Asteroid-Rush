using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum ZoneTypes
{
    Field,
    Island,
    Pit,
    Tunnel,
    Maze
}

public class Zone : MonoBehaviour
{
    public List<GameObject> tiles = new List<GameObject>();
    public ZoneTypes zoneType = ZoneTypes.Field;
	public bool isOreZone = false;
    public int width = 0;
    public int height = 0;
	public int xPos = 0;
	public int zPos = 0;

    public void BuildZone(GameObject[] tilePrefabs, GameObject[] objectPrefabs)
    {
		switch (zoneType)
		{
			case ZoneTypes.Field:
				BuildField(tilePrefabs, objectPrefabs);
				break;
			case ZoneTypes.Island:
				BuildIsland(tilePrefabs, objectPrefabs);
				break;
			case ZoneTypes.Pit:
				BuildPit(tilePrefabs, objectPrefabs);
				break;
			case ZoneTypes.Tunnel:
				BuildTunnel(tilePrefabs, objectPrefabs);
				break;
			case ZoneTypes.Maze:
				BuildMaze(tilePrefabs, objectPrefabs);
				break;
		}
	}

	/// <summary>
	/// Build a "Field" type zone
	/// </summary>
    private void BuildField(GameObject[] tilePrefabs, GameObject[] objectPrefabs)
    {
		for(int row = zPos; row < zPos + height; row++)
		{
			for(int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(row, col));
				}
			}
		}

		int numWalls = Random.Range(2, 4);

		// Bail out early if the zone can't support the maximum number of walls
		if (tiles.Count < numWalls * 3 + 1) return;

		GameObject pitOrWall = Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0];

		for (int i = 0; i < numWalls; i++)
		{
			int randomX;
			int randomZ;
			do
			{
				randomX = Random.Range(xPos, xPos + width);
				randomZ = Random.Range(zPos, zPos + height);
			} while (GenerateLevel.GetGridItem(randomZ, randomX).transform.parent != transform || GenerateLevel.GetGridItem(randomZ, randomX).GetComponent<Tile>().tileType == TileType.Pit || GenerateLevel.GetGridItem(randomZ, randomX).GetComponent<Tile>().occupant != null);

			int wallSize = 0;

			do
			{
				List<Vector2Int> validPositions = new List<Vector2Int>();
				if (pitOrWall == objectPrefabs[0])
				{
					GenerateLevel.GetGridItem(randomZ, randomX).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(randomX, pitOrWall.transform.position.y, randomZ), Quaternion.identity, GenerateLevel.GetGridItem(randomZ, randomX).transform);
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(randomZ, randomX));
					GenerateLevel.SetGridItem(randomZ, randomX, Instantiate(pitOrWall, new Vector3(randomX, pitOrWall.transform.position.y, randomZ), Quaternion.identity, transform));
					tiles.Add(GenerateLevel.GetGridItem(randomZ, randomX));
				}

				if (randomX > xPos) validPositions.Add(new Vector2Int(randomX - 1, randomZ));
				if (randomX < xPos + width - 1) validPositions.Add(new Vector2Int(randomX + 1, randomZ));
				if (randomZ > zPos) validPositions.Add(new Vector2Int(randomX, randomZ - 1));
				if (randomZ < zPos + height - 1) validPositions.Add(new Vector2Int(randomX, randomZ + 1));

				if (validPositions.Count > 0)
				{
					Vector2Int chosenPosition = validPositions[Random.Range(0, validPositions.Count)];
					randomX = chosenPosition.x;
					randomZ = chosenPosition.y;
				}
				else
				{
					break;
				}

				wallSize++;
			} while (Random.Range(0f, 1f) < 0.5f && wallSize < 3);
		}
	}

	private void BuildIsland(GameObject[] tilePrefabs, GameObject[] objectPrefabs)
	{

	}

	private void BuildPit(GameObject[] tilePrefabs, GameObject[] objectPrefabs)
	{

	}

	private void BuildTunnel(GameObject[] tilePrefabs, GameObject[] objectPrefabs)
	{

	}

	private void BuildMaze(GameObject[] tilePrefabs, GameObject[] objectPrefabs)
	{

	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
