using System;

namespace Com.MMsGamingCo.Core
{

    /// <summary>
    /// Base class for anything saved with SaveSystem or CloudSaveData.
    /// Use public fields (JsonUtility and Cloud Save both serialize them) and give
    /// every field a default so older saves load cleanly. Note: JsonUtility loads a
    /// missing array or list as empty, not null, so check Length in Normalize too.
    ///
    /// When the format changes: bump CurrentSchemaVersion and upgrade old data in
    /// Normalize(), the same way ZombCube's SaveData moved from indexes to IDs.
    /// </summary>
    [Serializable]
    public abstract class SaveDataBase
    {
        /// <summary>Version the data was written with. 0 means it predates versioning.</summary>
        public int schemaVersion = 0;

        /// <summary>UTC ticks of the last save. Used to pick the newer of a local and cloud save.</summary>
        public long lastSavedTicks = 0;

        /// <summary>Version of the format this build writes.</summary>
        public virtual int CurrentSchemaVersion { get { return 1; } }

        public DateTime LastSavedUtc
        {
            get { return new DateTime(lastSavedTicks, DateTimeKind.Utc); }
        }

        /// <summary>
        /// Brings data from any older version up to the current format and fixes
        /// missing values (null arrays, etc). Must be safe to call more than once.
        /// Call base.Normalize() last so schemaVersion is only bumped after upgrading.
        /// </summary>
        public virtual void Normalize()
        {
            schemaVersion = CurrentSchemaVersion;
        }

        /// <summary>Stamps the save time. SaveSystem and CloudSaveData call this for you.</summary>
        public void MarkSaved()
        {
            lastSavedTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>Returns whichever save was written most recently. Either can be null.</summary>
        public static T Newest<T>(T a, T b) where T : SaveDataBase
        {
            if (a == null) return b;
            if (b == null) return a;
            return b.lastSavedTicks > a.lastSavedTicks ? b : a;
        }
    }

}
