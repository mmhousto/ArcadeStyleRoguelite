using System;
using System.IO;
using UnityEngine;

namespace Com.MorganHouston.Core
{

    /// <summary>
    /// Saves and loads SaveDataBase objects as JSON in Application.persistentDataPath.
    /// Writes go to a temp file first and the previous save is kept as a .bak, so a
    /// crash or iOS killing the app mid-write can't wipe a player's progress.
    /// </summary>
    public static class SaveSystem
    {

        public const string DefaultSlot = "playerData";
        private const string Extension = ".json";
        private const string TempExtension = ".tmp";
        private const string BackupExtension = ".bak";

        /// <summary>Folder saves are written to. Override in tests.</summary>
        public static string saveFolder = null;

        /// <summary>Pretty-print JSON. Handy while developing, off by default for size.</summary>
        public static bool prettyPrint = false;

        public static event Action<string> saved;
        public static event Action<string> loaded;

        private static string Folder
        {
            get { return string.IsNullOrEmpty(saveFolder) ? Application.persistentDataPath : saveFolder; }
        }

        /// <summary>Full path of a save slot's file.</summary>
        public static string GetPath(string slot = DefaultSlot)
        {
            return Path.Combine(Folder, slot + Extension);
        }

        /// <summary>
        /// Saves data to a slot.
        /// </summary>
        /// <returns>True if the file was written.</returns>
        public static bool Save<T>(T data, string slot = DefaultSlot) where T : SaveDataBase
        {
            if (data == null) return false;

            string path = GetPath(slot);
            string tempPath = path + TempExtension;
            string backupPath = path + BackupExtension;

            try
            {
                data.schemaVersion = data.CurrentSchemaVersion;
                data.MarkSaved();

                Directory.CreateDirectory(Folder);
                File.WriteAllText(tempPath, JsonUtility.ToJson(data, prettyPrint));

                if (File.Exists(path))
                {
                    File.Copy(path, backupPath, true);
                    File.Delete(path);
                }
                File.Move(tempPath, path);

                saved?.Invoke(slot);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("SaveSystem: could not save " + slot + ". " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Loads a slot and returns it in the current format. Falls back to the backup
        /// if the main file is missing or corrupt, and to new data if neither loads.
        /// </summary>
        public static T Load<T>(string slot = DefaultSlot) where T : SaveDataBase, new()
        {
            T data = TryLoad<T>(slot);
            if (data == null)
            {
                data = new T();
                data.Normalize();
            }
            return data;
        }

        /// <summary>
        /// Same as Load, but returns null when there is no readable save. Use this
        /// when you need to know if a save existed (e.g. to compare with the cloud).
        /// </summary>
        public static T TryLoad<T>(string slot = DefaultSlot) where T : SaveDataBase
        {
            string path = GetPath(slot);

            T data = ReadFile<T>(path);
            if (data == null)
            {
                data = ReadFile<T>(path + BackupExtension);
                if (data != null)
                    Debug.LogWarning("SaveSystem: " + slot + " was unreadable, restored from backup.");
            }

            if (data == null) return null;

            data.Normalize();
            loaded?.Invoke(slot);
            return data;
        }

        public static bool Exists(string slot = DefaultSlot)
        {
            string path = GetPath(slot);
            return File.Exists(path) || File.Exists(path + BackupExtension);
        }

        /// <summary>Deletes a slot and its backup.</summary>
        public static void Delete(string slot = DefaultSlot)
        {
            string path = GetPath(slot);
            DeleteIfExists(path);
            DeleteIfExists(path + BackupExtension);
            DeleteIfExists(path + TempExtension);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        private static T ReadFile<T>(string path) where T : SaveDataBase
        {
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return null;
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("SaveSystem: could not read " + path + ". " + e.Message);
                return null;
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

    }

}
