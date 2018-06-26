using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using MultiplayerARPG;

[System.Serializable]
public class WorldSaveData
{
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();

    public void SavePersistentWorldData(string id, string map)
    {
        var binaryFormatter = new BinaryFormatter();
        var surrogateSelector = new SurrogateSelector();
        surrogateSelector.AddAllUnitySurrogate();
        var buildingSaveDataSS = new BuildingSaveDataSerializationSurrogate();
        var worldSaveDataSS = new WorldSaveDataSerializationSurrogate();
        surrogateSelector.AddSurrogate(typeof(BuildingSaveData), new StreamingContext(StreamingContextStates.All), buildingSaveDataSS);
        surrogateSelector.AddSurrogate(typeof(WorldSaveData), new StreamingContext(StreamingContextStates.All), worldSaveDataSS);
        binaryFormatter.SurrogateSelector = surrogateSelector;
        var path = Application.persistentDataPath + "/" + id + "_world_" + map + ".sav";
        var file = File.Open(path, FileMode.OpenOrCreate);
        binaryFormatter.Serialize(file, this);
        file.Close();
    }

    public void LoadPersistentData(string id, string map)
    {
        var path = Application.persistentDataPath + "/" + id + "_world_" + map + ".sav";
        if (File.Exists(path))
        {
            var binaryFormatter = new BinaryFormatter();
            var surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddAllUnitySurrogate();
            var buildingSaveDataSS = new BuildingSaveDataSerializationSurrogate();
            var worldSaveDataSS = new WorldSaveDataSerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(BuildingSaveData), new StreamingContext(StreamingContextStates.All), buildingSaveDataSS);
            surrogateSelector.AddSurrogate(typeof(WorldSaveData), new StreamingContext(StreamingContextStates.All), worldSaveDataSS);
            binaryFormatter.SurrogateSelector = surrogateSelector;
            var file = File.Open(path, FileMode.Open);
            var result = (WorldSaveData)binaryFormatter.Deserialize(file);
            buildings = result.buildings;
            file.Close();
        }
    }
}
