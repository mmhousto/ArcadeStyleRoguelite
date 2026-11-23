using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// An expanding, fading ring plus a few flying sparks. Used for kills, landings
    /// and spawn warnings until real particle effects are made.
    /// </summary>
    public class BurstEffect : MonoBehaviour
    {
        private const int SparkCount = 6;

        private LineRenderer ring;
        private LineRenderer[] sparks;
        private Vector2[] sparkDirections;
        private Color color;
        private float startRadius, endRadius, duration, timer;
        private bool reverse;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            ring = ShapeFactory.CreatePolygon(transform, "Ring", ShapeFactory.Circle(1f, 20), Color.white, 0.08f, 5);

            sparks = new LineRenderer[SparkCount];
            sparkDirections = new Vector2[SparkCount];
            for (int i = 0; i < SparkCount; i++)
                sparks[i] = ShapeFactory.CreateLine(transform, "Spark", Color.white, 0.07f, 5);
        }

        /// <summary>
        /// Plays the burst. With reverse the ring shrinks inward instead (spawn warning).
        /// </summary>
        public void Play(Vector2 position, Color burstColor, float fromRadius, float toRadius, float seconds, bool withSparks, bool shrink = false)
        {
            transform.position = position;
            color = burstColor;
            startRadius = fromRadius;
            endRadius = toRadius;
            duration = Mathf.Max(0.01f, seconds);
            reverse = shrink;
            timer = 0f;
            IsPlaying = true;
            gameObject.SetActive(true);

            for (int i = 0; i < SparkCount; i++)
            {
                sparks[i].enabled = withSparks;
                float angle = (i + Random.value * 0.6f) * Mathf.PI * 2f / SparkCount;
                sparkDirections[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            UpdateVisuals(0f);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (t >= 1f)
            {
                IsPlaying = false;
                gameObject.SetActive(false);
                return;
            }

            UpdateVisuals(t);
        }

        private void UpdateVisuals(float t)
        {
            float eased = 1f - (1f - t) * (1f - t);
            float radius = Mathf.Lerp(startRadius, endRadius, reverse ? t : eased);
            float alpha = reverse ? Mathf.Lerp(0.3f, 1f, t) : 1f - t;

            ring.transform.localScale = Vector3.one * radius;
            ShapeFactory.SetColor(ring, NeonColors.WithAlpha(color, alpha));

            for (int i = 0; i < SparkCount; i++)
            {
                if (!sparks[i].enabled) continue;

                Vector2 inner = sparkDirections[i] * (radius * 0.9f);
                Vector2 outer = sparkDirections[i] * (radius * 0.9f + 0.4f * (1f - t));
                ShapeFactory.SetPoints(sparks[i], new[] { inner, outer });
                ShapeFactory.SetColor(sparks[i], NeonColors.WithAlpha(color, alpha));
            }
        }
    }

}
