using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class DataTracking : MonoBehaviour
{
	// ORDERING OF DATA IN THE DATA STRING:
	// 1. TOTAL NUMBER OF RUNS
	// 2. AVERAGE NUMBER OF RUNS SUCCEEDED
	// 3. AVERAGE DAMAGE TAKEN PER RUN
	// 4. AVERAGE NUMBER OF ENEMIES DEFEATED PER RUN
	// 5. AVERAGE NUMBER OF TURNS TAKEN PER RUN
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

	public static IEnumerator LoadData()
	{
		yield return new WaitForSeconds(3f);
		UnityWebRequest request = UnityWebRequest.Get("https://firebasestorage.googleapis.com/v0/b/asteroid-rush.appspot.com/o/SaveData.txt?alt=media&token=c237e539-9b33-4209-9e07-f3230e3e2f52");
		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.Log(request.error);
			data = new string[] { "0", "0", "0", "0", "0" };
		}
		else
		{
			string dataString = request.downloadHandler.text;
			if (dataString.Contains(',')) data = dataString.Split(',');
			else data = dataString.Split("%2c");
		}

		Debug.Log(data[0]);
	}

	public static IEnumerator SaveData(string postData)
	{
		UnityWebRequest request = UnityWebRequest.Post("https://firebasestorage.googleapis.com/v0/b/asteroid-rush.appspot.com/o/SaveData.txt?alt=media&token=c237e539-9b33-4209-9e07-f3230e3e2f52", postData);
		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.Log(request.error);
		}
		else
		{
			Debug.Log("Success!");
		}
	}
}
