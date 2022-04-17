using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using ClemCAddons;
using System.Linq;

public class Saver : MonoBehaviour
{
    public class SaveData
    {
        public SystemSave[] Systems;
        public Scoring Scoring;
        public Vector3 CloudPosition;
        public OrderHandler Queue;
    }

    public class SystemSave
    {
        public StellarSystem System;
        public PlanetSave[] Planets;
    }

    public class PlanetSave
    {
        public GameObject Planet;
        public GameObject[] Children;
    }

    public void Save()
    {
        var save = new SaveData();

        save = SetCloud(save);
        save = SetQueue(save);
        save = SetScoring(save);
        save = SetSystems(save);

        SaveSave(save);
    }

    public void Load()
    {
        //if (File.Exists(Application.persistentDataPath
        //           + "/MySaveData.dat"))
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    FileStream file =
        //               File.Open(Application.persistentDataPath
        //               + "/MySaveData.dat", FileMode.Open);
        //    SaveData data = (SaveData)bf.Deserialize(file);
        //    file.Close();
        //}
    }

    private SaveData SetSystems(SaveData data)
    {
        var systems = SystemSpawning.Instance.Spawned;
        for(int i = 0; i < systems.Count; i++)
        {
            var system = systems[i];
            var save = new SystemSave();
            save.System = system.GetComponent<StellarSystem>();
            var planets = system.transform.GetChildrenWithComponent(typeof(Planet)).Select(t => t.gameObject).ToArray();
            var planet = new PlanetSave[planets.Length];
            for(int r = 0; r < planet.Length; r++)
            {
                planet[r].Planet = planets[r];
                planet[r].Children = planets[r].transform.GetChildrenWithComponent(typeof(Transform)).Select(t => t.gameObject).ToArray();
            }
            save.Planets = planet;
            data.Systems.SetOrCreateAt(save, i);
        }
        return data;
    }

    private SaveData SetQueue(SaveData data)
    {
        data.Queue = OrderHandler.Instance;
        return data;
    }

    private SaveData SetCloud(SaveData data)
    {
        data.CloudPosition = CloudMoveScript.Instance.transform.position;
        return data;
    }

    private SaveData SetScoring(SaveData data)
    {
        data.Scoring = Scoring.Instance;
        return data;
    }

    private void SaveSave(SaveData save)
    {
        // most of the structure is from https://videlais.com/2021/02/28/encrypting-game-data-with-unity/
        Aes iAes = Aes.Create();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath
                     + "/MarchSave.dat");


        byte[] savedKey = iAes.Key;

        PlayerPrefs.SetString("saveKey", System.Convert.ToBase64String(savedKey));

        byte[] inputIV = iAes.IV;

        file.Write(inputIV, 0, inputIV.Length);

        CryptoStream iStream = new CryptoStream(
              file,
              iAes.CreateEncryptor(iAes.Key, iAes.IV),
              CryptoStreamMode.Write);

        StreamWriter sWriter = new StreamWriter(iStream);

        var data = save.ToBytes();

        sWriter.Write(data);
        sWriter.Close();
        iStream.Close();
        file.Close();
    }

    private SaveData LoadSave()
    {
        // most of the structure is from https://videlais.com/2021/02/28/encrypting-game-data-with-unity/

        if (File.Exists(Application.persistentDataPath
                     + "/MarchSave.dat") && PlayerPrefs.HasKey("saveKey"))
        {
            // Update key based on PlayerPrefs
            // (Convert the String into a Base64 byte[] array.)
            byte[] savedKey = System.Convert.FromBase64String(PlayerPrefs.GetString("key"));

            // Create FileStream for opening files.
            var file = new FileStream(Application.persistentDataPath
                     + "/MarchSave.dat", FileMode.Open);

            // Create new AES instance.
            Aes oAes = Aes.Create();

            // Create an array of correct size based on AES IV.
            byte[] outputIV = new byte[oAes.IV.Length];

            // Read the IV from the file.
            file.Read(outputIV, 0, outputIV.Length);

            // Create CryptoStream, wrapping FileStream
            CryptoStream oStream = new CryptoStream(
                   file,
                   oAes.CreateDecryptor(savedKey, outputIV),
                   CryptoStreamMode.Read);

            // Create a StreamReader, wrapping CryptoStream
            using (var memoryStream = new MemoryStream())
            {
                oStream.CopyTo(memoryStream);
                return memoryStream.ToArray().ToType(typeof(SaveData));
            }
        }
        return null;
    }
}
