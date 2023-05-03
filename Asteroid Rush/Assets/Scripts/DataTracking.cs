using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class DataTracking : MonoBehaviour
{
	// ORDERING OF DATA IN THE DATA STRING:
	// 1. TOTAL NUMBER OF RUNS
	// 2. PERCENT OF RUNS SUCCEEDED
	// 3. AVERAGE DAMAGE TAKEN PER RUN
	// 4. AVERAGE NUMBER OF ENEMIES DEFEATED PER RUN
	// 5. AVERAGE NUMBER OF TURNS TAKEN PER RUN
	// 6. AVERAGE NUMBER OF ORE COLLECTED PER RUN
	private static string[] data;

	public static string GetData(int index)
	{
		return data[index];
	}

	public static void SetData(int index, string value)
	{
		data[index] = value;
	}

	public static bool DataExists()
	{
		return data != null;
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public static void LoadData()
	{
		if (PlayerPrefs.HasKey("SaveData")) data = PlayerPrefs.GetString("SaveData").Split(",");
		else data = new string[] { "0", "0", "0", "0", "0", "0" };
	}

	public static void SaveData()
	{
		string saveData = data[0] + ",";
		for (int i = 1; i < data.Length; i++)
		{
			saveData += data[i] + ",";
		}
		saveData = saveData.Substring(0, saveData.Length - 1);

		PlayerPrefs.SetString("SaveData", saveData);

		Debug.Log(data[0]);
		for (int i = 1; i < data.Length; i++)
		{
			Debug.Log((float.Parse(data[i]) / float.Parse(data[0])).ToString());
		}
	}
}
