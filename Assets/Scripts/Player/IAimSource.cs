using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Where the comet's aim comes from: the player's touch/mouse/gamepad (AimInput)
    /// or the playtest bot (BotPilot). The comet treats both the same way.
    /// </summary>
    public interface IAimSource
    {
        bool IsAiming { get; }
        bool InDeadZone { get; }
        Vector2 Direction { get; }
        float Power { get; }
        bool DashRequested { get; }

        /// <summary>Called once per frame before the values are read.</summary>
        void Tick();

        /// <summary>Drops the current aim.</summary>
        void Cancel();
    }

}
