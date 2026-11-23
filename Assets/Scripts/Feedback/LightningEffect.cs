using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// A jagged bolt between two points that flickers and fades. Used by Arc and Seeker.
    /// </summary>
    public class LightningEffect : MonoBehaviour
    {
        private const int Segments = 7;
        private const float Duration = 0.18f;

        private LineRenderer line;
        private Vector2 from, to;
        private Color color;
        private float timer;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            line = ShapeFactory.CreateLine(transform, "Bolt", Color.white, 0.07f, 7);
            line.useWorldSpace = true;
        }

        public void Play(Vector2 start, Vector2 end, Color boltColor)
        {
            from = start;
            to = end;
            color = boltColor;
            timer = 0f;
            IsPlaying = true;
            gameObject.SetActive(true);
            Redraw();
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= Duration)
            {
                IsPlaying = false;
                gameObject.SetActive(false);
                return;
            }

            // Re-jag every frame so the bolt crackles.
            Redraw();
        }

        private void Redraw()
        {
            Vector2 direction = to - from;
            Vector2 side = new Vector2(-direction.y, direction.x).normalized;
            float jag = Mathf.Min(0.5f, direction.magnitude * 0.12f);

            line.positionCount = Segments + 1;
            for (int i = 0; i <= Segments; i++)
            {
                float t = i / (float)Segments;
                float offset = (i == 0 || i == Segments) ? 0f : Random.Range(-jag, jag);
                line.SetPosition(i, from + direction * t + side * offset);
            }

            Color faded = NeonColors.WithAlpha(color, 1f - timer / Duration);
            ShapeFactory.SetColor(line, faded);
        }
    }

}
