using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Knockback math. Enemies close to the source are pushed hardest; at the edge of
    /// the radius they still get half the push, so nobody is left sitting on the comet.
    /// </summary>
    public static class Knockback
    {
        /// <summary>
        /// Velocity to push a target at targetPosition away from source.
        /// Zero if the target is outside the radius.
        /// </summary>
        public static Vector2 Velocity(Vector2 source, Vector2 targetPosition, float radius, float speed)
        {
            Vector2 away = targetPosition - source;
            float distance = away.magnitude;
            if (distance > radius || radius <= 0f) return Vector2.zero;

            // Something exactly on top of the source gets pushed straight up.
            Vector2 direction = distance > 0.0001f ? away / distance : Vector2.up;
            float falloff = 0.5f + 0.5f * (1f - distance / radius);
            return direction * speed * falloff;
        }

        /// <summary>Slows a knockback velocity down over time (exponential drag).</summary>
        public static Vector2 Decay(Vector2 velocity, float drag, float deltaTime)
        {
            return velocity * Mathf.Exp(-drag * deltaTime);
        }
    }

}
