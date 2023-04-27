using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFinder : MonoBehaviour
{

    public TileFinder()
    {

    }

    public List<Tile> FindAvailableTiles(Character chosenCharacter)
    {
        Tile start = chosenCharacter.CurrentTile;
        List<Tile> currentLevelTiles = new List<Tile>();
        List<Tile> moveableTiles = new List<Tile>();
        currentLevelTiles.Add(start);
        for (int i = 0; i < chosenCharacter.Movement; i++)
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
                if (tile.zPos > 0)
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
                if (tile.zPos < GenerateLevel.GridHeight - 1)
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
                //Debug.Log("Starting at " + tile.zPos + " , " + tile.xPos);
                //left tile to current tile
                if (tile.xPos > 0)
                {
                    Tile adjacentLeftTile = GenerateLevel.grid[tile.zPos, tile.xPos - 1].GetComponent<Tile>();

                    if (!lookedAtTiles.Contains(adjacentLeftTile))
                    {
                        //Debug.Log("Checking at " + adjacentLeftTile.zPos + " , " + adjacentLeftTile.xPos);
                        lookedAtTiles.Add(adjacentLeftTile);
                        if (adjacentLeftTile.IsAvailableTile())
                        {
                            nextLevelTiles.Add(adjacentLeftTile);
                            //adjacentLeftTile.SetAvailabillitySelector(true);
                        }
                        else if (adjacentLeftTile.IsAttackable(chosenCharacter))
                        {
                            attackableTiles.Add(adjacentLeftTile);
                        }
                    }

                }

                //right Tile
                if (tile.xPos < GenerateLevel.GridWidth - 1)
                {
                    Tile adjacentRightTile = GenerateLevel.grid[tile.zPos, tile.xPos + 1].GetComponent<Tile>();
                    if (!lookedAtTiles.Contains(adjacentRightTile))
                    {
                        //Debug.Log("Checking at " + adjacentRightTile.zPos + " , " + adjacentRightTile.xPos);
                        lookedAtTiles.Add(adjacentRightTile);
                        if (adjacentRightTile.IsAvailableTile())
                        {
                            nextLevelTiles.Add(adjacentRightTile);
                            //adjacentRightTile.SetAvailabillitySelector(true);
                        }
                        else if (adjacentRightTile.IsAttackable(chosenCharacter))
                        {
                            attackableTiles.Add(adjacentRightTile);
                        }
                    }

                }

                //bottom tile
                if (tile.zPos > 0)
                {
                    Tile adjacentBottomTile = GenerateLevel.grid[tile.zPos - 1, tile.xPos].GetComponent<Tile>();
                    if (!lookedAtTiles.Contains(adjacentBottomTile))
                    {
                        //Debug.Log("Checking at " + adjacentBottomTile.zPos + " , " + adjacentBottomTile.xPos);
                        lookedAtTiles.Add(adjacentBottomTile);
                        if (adjacentBottomTile.IsAvailableTile())
                        {
                            nextLevelTiles.Add(adjacentBottomTile);
                            //adjacentBottomTile.SetAvailabillitySelector(true);
                        }
                        else if (adjacentBottomTile.IsAttackable(chosenCharacter))
                        {
                            attackableTiles.Add(adjacentBottomTile);
                        }
                    }

                }


                //top tile
                if (tile.zPos < GenerateLevel.GridHeight - 1)
                {
                    Tile adjacentTopTile = GenerateLevel.grid[tile.zPos + 1, tile.xPos].GetComponent<Tile>();
                    if (!lookedAtTiles.Contains(adjacentTopTile))
                    {
                        //Debug.Log("Checking at " + adjacentTopTile.zPos + " , " + adjacentTopTile.xPos);
                        lookedAtTiles.Add(adjacentTopTile);
                        if (adjacentTopTile.IsAvailableTile())
                        {
                            nextLevelTiles.Add(adjacentTopTile);
                            //adjacentTopTile.SetAvailabillitySelector(true);
                        }
                        else if (adjacentTopTile.IsAttackable(chosenCharacter))
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

    public List<Tile> FindAvailableMinableTiles(Character chosenCharacter, Tile startTile)
    {
        List<Tile> mineTiles = new List<Tile>();

        if (startTile.xPos > 0)
        {
            Tile adjacentLeftTile = GenerateLevel.grid[startTile.zPos, startTile.xPos - 1].GetComponent<Tile>();


            Debug.Log("Checking at " + adjacentLeftTile.zPos + " , " + adjacentLeftTile.xPos);
            if (adjacentLeftTile.IsMineable(chosenCharacter))
            {
                mineTiles.Add(adjacentLeftTile);
            }


        }

        //right Tile
        if (startTile.xPos < GenerateLevel.GridWidth - 1)
        {
            Tile adjacentRightTile = GenerateLevel.grid[startTile.zPos, startTile.xPos + 1].GetComponent<Tile>();

            Debug.Log("Checking at " + adjacentRightTile.zPos + " , " + adjacentRightTile.xPos);
            if (adjacentRightTile.IsMineable(chosenCharacter))
            {
                mineTiles.Add(adjacentRightTile);
            }
        }

        //bottom tile
        if (startTile.zPos > 0)
        {
            Tile adjacentBottomTile = GenerateLevel.grid[startTile.zPos - 1, startTile.xPos].GetComponent<Tile>();

            Debug.Log("Checking at " + adjacentBottomTile.zPos + " , " + adjacentBottomTile.xPos);
            if (adjacentBottomTile.IsMineable(chosenCharacter))
            {
                mineTiles.Add(adjacentBottomTile);
            }


        }

        //top tile
        if (startTile.zPos < GenerateLevel.GridHeight - 1)
        {
            Tile adjacentTopTile = GenerateLevel.grid[startTile.zPos + 1, startTile.xPos].GetComponent<Tile>();

            Debug.Log("Checking at " + adjacentTopTile.zPos + " , " + adjacentTopTile.xPos);
            if (adjacentTopTile.IsMineable(chosenCharacter))
            {
                mineTiles.Add(adjacentTopTile);
            }
        }

        return mineTiles;
    }

}
