using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using UnityEngine;

namespace Com.MorganHouston.Core.Services
{

    /// <summary>
    /// Thin wrapper around Unity Cloud Save (player data). Pulled out of ZombCube's
    /// CloudSaveLogin so saving doesn't depend on a login MonoBehaviour or the UI.
    /// Every call needs a signed-in player (see AuthenticationManager), returns
    /// safely when there isn't one, and reports problems through cloudSaveError.
    /// </summary>
    public static class CloudSaveData
    {

        /// <summary>Raised with a player-friendly message when a call fails. Hook ErrorManager up to this.</summary>
        public static event Action<string> cloudSaveError;

        public static bool CanUseCloud
        {
            get
            {
                // AuthenticationService.Instance throws if services haven't been initialized.
                return UnityServices.State == ServicesInitializationState.Initialized
                    && AuthenticationService.Instance.IsSignedIn;
            }
        }

        #region Save

        /// <summary>
        /// Saves an object to the cloud under a key.
        /// </summary>
        /// <returns>True if the save went through.</returns>
        public static async Task<bool> SaveObjectData<T>(string key, T value) where T : SaveDataBase
        {
            if (!CanUseCloud || value == null) return false;

            value.schemaVersion = value.CurrentSchemaVersion;
            value.MarkSaved();

            return await SaveData(new Dictionary<string, object> { { key, value } });
        }

        /// <summary>Saves a single value (int, float, string, etc) to the cloud.</summary>
        public static async Task<bool> SaveSingleData(string key, object value)
        {
            if (!CanUseCloud) return false;
            return await SaveData(new Dictionary<string, object> { { key, value } });
        }

        /// <summary>Saves several keys in one request.</summary>
        public static async Task<bool> SaveData(Dictionary<string, object> data)
        {
            if (!CanUseCloud) return false;

            try
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                return true;
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
                RaiseError("Error: Failed to save cloud data.");
            }
            catch (CloudSaveRateLimitedException e)
            {
                Debug.LogWarning(e);
                RaiseError("Error: Saving too often, try again shortly.");
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
                RaiseError("Error: Failed to save cloud data.");
            }

            return false;
        }

        #endregion

        #region Load

        /// <summary>
        /// Gets an object from the cloud. Returns null if the key doesn't exist or the
        /// request fails. The result is already normalized to the current format.
        /// </summary>
        public static async Task<T> RetrieveObjectData<T>(string key) where T : SaveDataBase
        {
            T data = await RetrieveSpecificData<T>(key);
            if (data != null)
                data.Normalize();
            return data;
        }

        /// <summary>
        /// Gets any value from the cloud. Returns default(T) if the key doesn't exist
        /// or the request fails.
        /// </summary>
        public static async Task<T> RetrieveSpecificData<T>(string key)
        {
            if (!CanUseCloud) return default;

            try
            {
                Dictionary<string, Item> results =
                    await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

                if (results.TryGetValue(key, out Item item))
                    return item.Value.GetAs<T>();

                Debug.Log($"CloudSaveData: there is no key named {key}.");
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
                RaiseError("Error: " + e.Message);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
                RaiseError("Error: Failed to load cloud data, loading local profile.");
            }

            return default;
        }

        /// <summary>Lists every key saved for this player.</summary>
        public static async Task<List<string>> ListAllKeys()
        {
            var keys = new List<string>();
            if (!CanUseCloud) return keys;

            try
            {
                List<ItemKey> results = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
                foreach (ItemKey itemKey in results)
                    keys.Add(itemKey.Key);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }

            return keys;
        }

        #endregion

        #region Sync

        /// <summary>
        /// Compares a local save with the cloud copy and returns the newest one.
        /// If the local save wins it is uploaded, so both copies match afterwards.
        /// Offline or signed out, the local save is returned untouched.
        /// </summary>
        public static async Task<T> Sync<T>(string key, T local) where T : SaveDataBase
        {
            if (!CanUseCloud) return local;

            T cloud = await RetrieveObjectData<T>(key);
            T newest = SaveDataBase.Newest(local, cloud);

            if (newest != null && newest == local)
                await SaveObjectData(key, local);

            return newest;
        }

        #endregion

        #region Delete

        /// <summary>Deletes one key from the cloud.</summary>
        public static async Task<bool> DeleteSpecificData(string key)
        {
            if (!CanUseCloud) return false;

            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
                return true;
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        /// <summary>
        /// Deletes every key saved for this player. Used by "Delete Account", which
        /// Apple requires for any app that lets players create an account.
        /// </summary>
        public static async Task DeleteEverything()
        {
            List<string> keys = await ListAllKeys();
            foreach (string key in keys)
                await DeleteSpecificData(key);
        }

        #endregion

        private static void RaiseError(string message)
        {
            cloudSaveError?.Invoke(message);
        }

    }

}
