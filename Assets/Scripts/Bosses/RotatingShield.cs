using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// A ring of segments around a boss that spins. Solid segments deflect dashes;
    /// gaps let the comet through. Pure math so it can be tested.
    /// </summary>
    public class RotatingShield
    {
        /// <summary>Ring radius in world units.</summary>
        public float Radius { get; set; }

        /// <summary>Current rotation in degrees.</summary>
        public float Rotation { get; set; }

        /// <summary>True = solid. Each entry covers 360 / Length degrees, starting at Rotation.</summary>
        public bool[] Segments { get; private set; }

        public float SegmentAngle { get { return 360f / Mathf.Max(1, Segments.Length); } }

        public RotatingShield(float radius, bool[] segments)
        {
            Radius = radius;
            SetSegments(segments);
        }

        public void SetSegments(bool[] segments)
        {
            Segments = segments != null && segments.Length > 0 ? segments : new[] { true };
        }

        /// <summary>Builds a pattern from a string, e.g. "XXX-XXX-" (X solid, - gap).</summary>
        public static bool[] Pattern(string pattern)
        {
            var segments = new bool[pattern.Length];
            for (int i = 0; i < pattern.Length; i++)
                segments[i] = pattern[i] != '-';
            return segments;
        }

        /// <summary>Is the ring solid at this world angle (degrees, 0 = +X, counter-clockwise)?</summary>
        public bool IsSolidAt(float worldAngle)
        {
            float local = Mathf.Repeat(worldAngle - Rotation, 360f);
            int index = Mathf.Min(Segments.Length - 1, (int)(local / SegmentAngle));
            return Segments[index];
        }

        /// <summary>
        /// Checks a comet of cometRadius moving from start to end against the ring around center.
        /// The ring is one-way: it bounces dashes coming in from outside, but anything
        /// inside can always dash out, so the player can never be trapped with the boss.
        /// </summary>
        public bool TryDeflect(Vector2 center, Vector2 start, Vector2 end, float cometRadius, out Vector2 hitPoint, out Vector2 normal)
        {
            hitPoint = end;
            normal = Vector2.zero;

            // Grow the ring by the comet's size, so its edge (not center) touches.
            float contactRadius = Radius + cometRadius;
            if ((start - center).sqrMagnitude < contactRadius * contactRadius) return false;
            if (!Geometry2D.SegmentCrossesCircle(start, end, center, contactRadius, out float t)) return false;

            Vector2 contact = start + (end - start) * t;
            Vector2 radial = contact - center;
            float angle = Mathf.Atan2(radial.y, radial.x) * Mathf.Rad2Deg;
            if (!IsSolidAt(angle)) return false;

            normal = radial.normalized;
            // Nudge off the surface so the next step doesn't hit it again.
            hitPoint = contact + normal * 0.02f;
            return true;
        }
    }

}
