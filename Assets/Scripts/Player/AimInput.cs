using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Turns touch, mouse and gamepad input into a slingshot aim.
    /// Touch and mouse: press anywhere, drag back, release to dash the opposite way.
    /// Gamepad: point the left stick where you want to go, press A or RT to dash.
    /// Call Tick() once per frame before reading the values.
    /// </summary>
    public class AimInput
    {
        private const float StickDeadZone = 0.25f;

        /// <summary>True while the player is holding an aim.</summary>
        public bool IsAiming { get; private set; }

        /// <summary>True while aiming but inside the cancel dead zone.</summary>
        public bool InDeadZone { get; private set; }

        /// <summary>Direction the comet will travel. Normalized.</summary>
        public Vector2 Direction { get; private set; } = Vector2.right;

        /// <summary>Dash power from minPower to 1.</summary>
        public float Power { get; private set; }

        /// <summary>True on the frame the player let go to dash.</summary>
        public bool DashRequested { get; private set; }

        /// <summary>Screen position where the current drag started (touch/mouse only).</summary>
        public Vector2 DragStart { get; private set; }

        /// <summary>True on the frame any button or touch was pressed. Used for "tap to restart".</summary>
        public bool AnyPressed { get; private set; }

        private readonly PrototypeTuning tuning;
        private bool pointerAiming = false;

        public AimInput(PrototypeTuning tuning)
        {
            this.tuning = tuning;
        }

        public void Tick()
        {
            DashRequested = false;
            AnyPressed = false;

            bool aimingThisFrame = TickPointer();
            if (!aimingThisFrame)
                aimingThisFrame = TickGamepad();

            IsAiming = aimingThisFrame;
        }

        /// <summary>Drops the current aim, e.g. when the comet dies mid-drag.</summary>
        public void Cancel()
        {
            pointerAiming = false;
            IsAiming = false;
            DashRequested = false;
        }

        private bool TickPointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null) return false;

            Vector2 position = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                AnyPressed = true;
                pointerAiming = true;
                DragStart = position;
            }

            if (!pointerAiming) return false;

            Vector2 drag = position - DragStart;
            float fullDrag = Mathf.Max(1f, Screen.height * tuning.fullDragScreenFraction);
            float dragFraction = drag.magnitude / fullDrag;

            InDeadZone = dragFraction < tuning.cancelDeadZone;
            if (!InDeadZone)
            {
                // Slingshot: pull back to fire forward.
                Direction = -drag.normalized;
                Power = Mathf.Lerp(tuning.minDashPower, 1f, Mathf.InverseLerp(tuning.cancelDeadZone, 1f, dragFraction));
            }

            if (pointer.press.wasReleasedThisFrame || !pointer.press.isPressed)
            {
                pointerAiming = false;
                DashRequested = !InDeadZone;
                return false;
            }

            return true;
        }

        private bool TickGamepad()
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null) return false;

            bool dashPressed = gamepad.buttonSouth.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame;
            if (dashPressed || gamepad.startButton.wasPressedThisFrame)
                AnyPressed = true;

            Vector2 stick = gamepad.leftStick.ReadValue();
            float magnitude = stick.magnitude;

            InDeadZone = magnitude < StickDeadZone;
            if (InDeadZone) return false;

            Direction = stick / magnitude;
            Power = Mathf.Lerp(tuning.minDashPower, 1f, Mathf.InverseLerp(StickDeadZone, 0.95f, magnitude));

            if (dashPressed)
                DashRequested = true;

            return true;
        }
    }

}
