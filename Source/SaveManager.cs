using HarmonyLib;
using Il2Cpp;
using System.Text.Json;
using UnityEngine;

namespace TaijiRandomizer
{
    internal class SaveManager
    {
        public class SaveInfo
        {
            public int Slot { get; set; }
            public uint Index { get; set; }
            public int Seed { get; set; }
        }

        private static List<SaveInfo> _saves = new();
        private static Dictionary<(int slot, uint index), SaveInfo> _lookup = new();

        public static string GetDataPath()
        {
            return Application.persistentDataPath + "/randomizer_saves.json";
        }

        public static void Load()
        {
            string path = GetDataPath();

            if (!File.Exists(path))
            {
                _saves.Clear();
                _lookup.Clear();

                return;
            }

            string fileText = File.ReadAllText(path);

            _saves = JsonSerializer.Deserialize<List<SaveInfo>>(fileText);

            _lookup.Clear();
            foreach (SaveInfo saveInfo in _saves)
            {
                _lookup[(saveInfo.Slot, saveInfo.Index)] = saveInfo;
            }
        }

        public static void Save()
        {
            string path = GetDataPath();
            string json = JsonSerializer.Serialize(_saves);
            File.WriteAllText(path, json);
        }

        public static bool IsRandomizedFile(int slot, uint index)
        {
            return _lookup.ContainsKey((slot, index));
        }

        public static SaveInfo GetSaveInfo(int slot, uint index)
        {
            if (!_lookup.ContainsKey((slot, index)))
            {
                SaveInfo newSave = new()
                {
                    Slot = slot,
                    Index = index
                };

                _saves.Add(newSave);
                _lookup[(slot, index)] = newSave;
            }

            return _lookup[(slot, index)];
        }

        [HarmonyPatch(typeof(SaveSystem), nameof(SaveSystem.Save))]
        static class SavePatch
        {
            public static void Postfix()
            {
                if (Randomizer.Instance != null && Randomizer.Instance.CurrentGame != null)
                {
                    SaveManager.Load();

                    SaveInfo saveInfo = SaveManager.GetSaveInfo(Globals.currentSaveSlot, Globals.activeSaveIndex);
                    saveInfo.Seed = Randomizer.Instance.CurrentGame.Value.seed;

                    SaveManager.Save();
                }
            }
        }
    }
}
