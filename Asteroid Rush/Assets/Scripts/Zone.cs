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

    private void BuildField(GameObject[] tilePrefabs, GameObject[] objectPrefabs)
    {
		for(int row = zPos; row < zPos + height; row++)
		{
			for(int col = xPos; col < xPos + width; col++)
			{
				GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
			}
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
