using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    // A list of the characters in the scene
    private static List<GameObject> characters = new List<GameObject>();

    // A list of the health bars in the scene
    [SerializeField] private GameObject[] healthBars;

    // Health blocks
    [SerializeField] private GameObject healthBlockPrefab;
    [SerializeField] private float padding;

    // The canvas
    [SerializeField] private static GameObject canvas;

    private void InitHealthBars()
    {
        for(int i = 0; i < healthBars.Length; i++)
        {
            int numBlocks = characters[i].GetComponent<Character>().MaxHealth;
            float healthBlockWidth = healthBars[i].GetComponent<RectTransform>().rect.width / numBlocks;
			float healthBarLeft = -healthBars[i].GetComponent<RectTransform>().rect.width / 2f;

            for(int j = 0; j < numBlocks; j++)
            {
				GameObject healthBlock = Instantiate(healthBlockPrefab, new Vector3(healthBarLeft + padding + healthBlockWidth * j, healthBars[i].transform.position.y, healthBars[i].transform.position.z), Quaternion.identity, healthBars[i].transform);
                healthBlock.transform.localPosition = new Vector3(healthBarLeft + padding + healthBlockWidth / 2f + healthBlockWidth * j, healthBlock.transform.localPosition.y, healthBlock.transform.localPosition.z);
				if (i < healthBars.Length - 1) healthBlock.transform.localScale = new Vector3(healthBlockPrefab.transform.localScale.x, healthBlockPrefab.transform.localScale.y / healthBars[i].transform.localScale.y, healthBlockPrefab.transform.localScale.z);
                else healthBlock.transform.localScale = new Vector3(numBlocks / healthBars[i].GetComponent<RectTransform>().rect.width, healthBlockPrefab.transform.localScale.y / healthBars[i].transform.localScale.y, healthBlockPrefab.transform.localScale.z);
			}
		}
    }

    /// <summary>
    /// Remove "damage" amount of health blocks from "character's" health bar.
    /// </summary>
    /// <param name="character">The character who was attacked</param>
    /// <param name="damage">The number of health blocks to remove</param>
    public static void UpdateHealthBar(GameObject character, int damage)
    {
        Transform characterBar = canvas.transform.GetChild(characters.IndexOf(character) * 2);

        for (int i = 0; i < damage; i++)
        {
			// Interesting Fact: Destroy activates at the end of the frame.
			// If you do not include the "- i" here, Destroy() will just activate on the last child multiple times.
            // You could use DestroyImmediate() to achieve the same effect more cleanly, but Unity highly recommends against using it.
			if (characterBar.childCount > 0) Destroy(characterBar.GetChild(characterBar.childCount - 1 - i).gameObject);
            else break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.Find("Canvas");
    }

    // Update is called once per frame
    void Update()
    {
        if(characters == null || characters.Count == 0)
        {
			characters.AddRange(GameObject.FindGameObjectsWithTag("Character"));

			InitHealthBars();
		}
    }
}
