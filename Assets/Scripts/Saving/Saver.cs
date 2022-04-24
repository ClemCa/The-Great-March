using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using ClemCAddons;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

public class Saver : MonoBehaviour
{
    private static Saver instance;

    public static Saver Instance { get => instance; }
    
    [Serializable]
    public class SaveData
    {
        public SystemSave[] Systems;
        public ScoringData Scoring;
        public string CloudPosition;
        public Dictionary<string, List<OrderHandler.Order>> Queue;
        public string SelectedPlanet;
        public string CargoSave;
        public bool LeaderInTransit;
    }

    [Serializable]
    public class ScoringData
    {
        public int SurvivalTime = 0;
        public int TotalTime = 0;
        public int Systems = 1;
        public long NaturalResourcesUnits = 0;
        public long AdvancedResourcesUnits = 0;
        public long FacilitiesCount = 0;
        public long TransformativeFacilitiesCount = 0;
        public System.DateTime StartTime;
        public System.DateTime LastTime;
        public ScoringData(int survivalTime, int totalTime, int systems, long naturalResourcesUnits, long advancedResourcesUnits, long facilitiesCount, long transformativeFacilitiesCount, DateTime startTime, DateTime lastTime)
        {
            SurvivalTime = survivalTime;
            TotalTime = totalTime;
            Systems = systems;
            NaturalResourcesUnits = naturalResourcesUnits;
            AdvancedResourcesUnits = advancedResourcesUnits;
            FacilitiesCount = facilitiesCount;
            TransformativeFacilitiesCount = transformativeFacilitiesCount;
            StartTime = startTime;
            LastTime = lastTime;
        }
    }

    [Serializable]
    public class SystemSave
    {
        public string Position;
        public string System;
        public string[] Planets;
        public string[] PlanetPositions;
        public Registry.SystemType SystemType;
        public int[] TemperateType;
        public int[] Roll;
    }

    [Serializable]
    public class PlanetSave
    {
        public string Planet;
    }

    public class PlanetStorage
    {
        public string _resources;
        public string _advancedResources;
        public int _people;
        public Registry.Resources[] _availableResources;
        public List<Registry.Facilities> _facilities;
        public List<float> _facilitiesProgression;
        public List<Registry.TransformationFacilities> _transformationFacilities;
        public List<float> _transformationFacilitiesProgression;
        public string _name;
        public int _availableWildcards;
        public int _peopleFed = 0;
        public List<int> _peopleOverTime;
        public List<int> _resourcesOverTime;
        public List<int> _facilitiesOverTime;
        public float _consumption;
        public bool _hasPlayer;
    }

    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    public bool Load(int slot)
    {
        var save = LoadSave(slot);
        if (save == null)
            return false; // failed
        CloudMoveScript.Instance.enabled = false; // disable losing
        LoadScoring(save.Scoring);
        LoadQueue(save.Queue);
        LoadSystems(save.Systems, save.SelectedPlanet);
        LoadCargo(JsonConvert.DeserializeObject<string[]>(save.CargoSave), save.LeaderInTransit);
        CloudMoveScript.Instance.transform.position = JsonUtility.FromJson<SerializableVector3>(save.CloudPosition).Value;
        CloudMoveScript.Instance.enabled = true;
        return true;
    }

    public bool SlotUsed(int slot)
    {
        return
            File.Exists(Application.persistentDataPath
                     + "/MarchSave" + slot + ".dat");
    }

    private void LoadSystems(SystemSave[] systems, string selectedPlanet)
    {
        SystemSpawning.Instance.Respawn(systems, selectedPlanet);
    }

    private void LoadQueue(Dictionary<string, List<OrderHandler.Order>> queue)
    {
        OrderHandler.Instance.QueueData = queue;
    }

    private void LoadScoring(ScoringData data)
    {
        Scoring.survivalTime = data.SurvivalTime;
        Scoring.totalTime = data.TotalTime;
        Scoring.systems = data.Systems;
        Scoring.naturalResourcesUnits = data.NaturalResourcesUnits;
        Scoring.advancedResourcesUnits = data.AdvancedResourcesUnits;
        Scoring.facilitiesCount = data.FacilitiesCount;
        Scoring.transformativeFacilitiesCount = data.TransformativeFacilitiesCount;
        Scoring.Instance.StartTime = data.StartTime;
        Scoring.Instance.LastTime = data.LastTime;
        Scoring.Instance.Loading();
    }


    private SaveData LoadSave(int slot)
    {
        // most of the structure is from https://videlais.com/2021/02/28/encrypting-game-data-with-unity/

        if (File.Exists(Application.persistentDataPath
                     + "/MarchSave" + slot +".dat") && PlayerPrefs.HasKey("saveKey"+slot))
        {
            // Update key based on PlayerPrefs
            // (Convert the String into a Base64 byte[] array.)
            byte[] savedKey = Convert.FromBase64String(PlayerPrefs.GetString("saveKey"+slot));

            // Create FileStream for opening files.
            var file = new FileStream(Application.persistentDataPath
                     + "/MarchSave" + slot + ".dat", FileMode.Open);

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

            var reader = new StreamReader(oStream);
            var str = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<SaveData>(str);
        }
        return null;
    }
    #region Save

    public void Save(int slot)
    {
        var save = new SaveData();

        save = SetCargo(save);
        save = SetCloud(save);
        save = SetQueue(save);
        save = SetScoring(save);
        save = SetSystems(save);
        save.SelectedPlanet = Planet.Selected == null ? "" : Planet.Selected.Name;

        SaveSave(save, slot);
    }

    private SaveData SetCargo(SaveData save)
    {
        var s = new string[] { };
        var cargos = FindObjectsOfType<Cargo>();
        foreach(var cargo in cargos)
        {
            s = s.Add(JsonUtility.ToJson(cargo.GetSave()));
        }
        save.CargoSave = JsonConvert.SerializeObject(s);
        save.LeaderInTransit = Cargo.LeaderInTransit;
        return save;
    }

    private void LoadCargo(string[] cargos, bool leaderInTransit)
    {
        var t = FindObjectsOfType<Cargo>();
        foreach(var toDestroy in t)
        {
            Destroy(toDestroy.gameObject); // clear all current cargo
        }
        foreach(var cargo in cargos)
        {
            var r = CargoGenerator.GenerateCargo();
            r.LoadSave(JsonUtility.FromJson<Cargo.CargoSave>(cargo));
        }
        Cargo.LeaderInTransit = leaderInTransit;
    }

    private SaveData SetSystems(SaveData data)
    {
        var systems = SystemSpawning.Instance.Spawned;
        for(int i = 0; i < systems.Count; i++)
        {
            var system = systems[i];
            var save = new SystemSave();
            save.SystemType = system.GetComponent<StellarSystem>().SystemType;
            save.Position = JsonUtility.ToJson(new SerializableVector3(system.transform.position));
            save.System = JsonUtility.ToJson(system.GetComponent<StellarSystem>());
            var planets = system.transform.GetComponentsInChildren<Planet>();
            var planet = new PlanetSave[planets.Length];
            var temperate = new int[planets.Length];
            var roll = new int[planets.Length];
            for (int r = 0; r < planet.Length; r++)
            {
                planet[r] = new PlanetSave();
                planet[r].Planet = JsonUtility.ToJson(planets[r].GetPlanetStorage());
                temperate[r] = planets[r].TemperateType;
                roll[r] = planets[r].Roll;
            }
            save.Planets = planet.Select(t => JsonUtility.ToJson(t)).ToArray();
            save.PlanetPositions = planets.Select(t => JsonUtility.ToJson(new SerializableVector3(t.transform.localPosition))).ToArray();
            save.TemperateType = temperate;
            save.Roll = roll;
            data.Systems = data.Systems.SetOrCreateAt(save, i);
        }
        return data;
    }

    private SaveData SetQueue(SaveData data)
    {
        data.Queue = OrderHandler.Instance.QueueData;
        return data;
    }

    private SaveData SetCloud(SaveData data)
    {
        data.CloudPosition = JsonUtility.ToJson(new SerializableVector3(CloudMoveScript.Instance.transform.position));
        return data;
    }

    private SaveData SetScoring(SaveData data)
    {
        data.Scoring = new ScoringData(
            Scoring.survivalTime,
            Scoring.totalTime,
            Scoring.systems,
            Scoring.naturalResourcesUnits,
            Scoring.advancedResourcesUnits,
            Scoring.facilitiesCount,
            Scoring.transformativeFacilitiesCount,
            Scoring.Instance.StartTime,
            Scoring.Instance.LastTime);
        return data;
    }

    private void SaveSave(SaveData save, int slot)
    {
        // most of the structure is from https://videlais.com/2021/02/28/encrypting-game-data-with-unity/
        Aes iAes = Aes.Create();

        FileStream file = File.Create(Application.persistentDataPath
                     + "/MarchSave" + slot + ".dat");


        byte[] savedKey = iAes.Key;

        PlayerPrefs.SetString("saveKey"+slot, System.Convert.ToBase64String(savedKey));

        byte[] inputIV = iAes.IV;

        file.Write(inputIV, 0, inputIV.Length);

        CryptoStream iStream = new CryptoStream(
              file,
              iAes.CreateEncryptor(iAes.Key, iAes.IV),
              CryptoStreamMode.Write);

        StreamWriter sWriter = new StreamWriter(iStream);

        var data = JsonConvert.SerializeObject(save);

        sWriter.Write(data);
        sWriter.Close();
        iStream.Close();
        file.Close();
    }

    #endregion Save


    public static void Replace<T>(T x, T y)
    where T : class
    {
        // replaces 'x' with 'y'
        if (x == null) throw new ArgumentNullException("x");
        if (y == null) throw new ArgumentNullException("y");

        var size = Marshal.SizeOf(typeof(T));
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(y, ptr, false);
        Marshal.PtrToStructure(ptr, x);
        Marshal.FreeHGlobal(ptr);
    }
}
