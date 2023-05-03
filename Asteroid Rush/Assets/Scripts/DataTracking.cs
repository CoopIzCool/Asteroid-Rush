using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
		BinaryFormatter formatter= new BinaryFormatter();
		FileStream readStream = null;

		try
		{
			readStream = File.OpenRead(Application.persistentDataPath + @"\SaveData.dat");
			data = (string[])formatter.Deserialize(readStream);
		}
		catch
		{
			data = new string[] { "0", "0", "0", "0", "0", "0" };
		}
		finally
		{
			if(readStream != null ) readStream.Close();
		}
	}

	public static void SaveData()
	{
		FileStream writeStream = null;
		BinaryFormatter formatter = new BinaryFormatter();

		try
		{
			writeStream = File.OpenWrite(Application.persistentDataPath + @"\SaveData.dat");
			formatter.Serialize(writeStream, data);
		}
		catch(Exception e)
		{
			Debug.Log(e);
		}
		finally
		{ 
			if(writeStream != null) writeStream.Close(); 
		}
	}
}
