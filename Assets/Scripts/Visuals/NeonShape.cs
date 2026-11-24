using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// A glowing shape on a child object: a neon sprite from SpriteLibrary, or the old
    /// LineRenderer outline if the art is missing. Callers just set color and transform.
    /// </summary>
    public class NeonShape : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private LineRenderer lineRenderer;
        private float radius;

        public bool UsesSprite { get { return spriteRenderer != null; } }

        /// <summary>
        /// Creates a shape. fallbackPoints (in local units) are drawn as lines when the art is missing.
        /// </summary>
        public static NeonShape Create(Transform parent, string artName, float radius, Vector2[] fallbackPoints, Color color, int sortingOrder, float fallbackWidth = 0.09f)
        {
            GameObject shapeObject = new GameObject(artName);
            shapeObject.transform.SetParent(parent, false);
            NeonShape shape = shapeObject.AddComponent<NeonShape>();
            shape.radius = radius;

            Sprite sprite = SpriteLibrary.Shape(artName, radius);
            if (sprite != null)
            {
                shape.spriteRenderer = shapeObject.AddComponent<SpriteRenderer>();
                shape.spriteRenderer.sprite = sprite;
                shape.spriteRenderer.sortingOrder = sortingOrder;
            }
            else
            {
                shape.lineRenderer = ShapeFactory.CreatePolygon(shapeObject.transform, "Line", fallbackPoints, color, fallbackWidth, sortingOrder);
            }

            shape.SetColor(color);
            return shape;
        }

        public void SetColor(Color color)
        {
            if (spriteRenderer != null) spriteRenderer.color = color;
            else if (lineRenderer != null) ShapeFactory.SetColor(lineRenderer, color);
        }

        /// <summary>Swaps the art (e.g. a Splitter showing cracks). Ignored in line fallback mode.</summary>
        public void SetArt(string artName)
        {
            if (spriteRenderer == null) return;
            Sprite sprite = SpriteLibrary.Shape(artName, radius);
            if (sprite != null) spriteRenderer.sprite = sprite;
        }

        public void SetVisible(bool visible)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = visible;
            if (lineRenderer != null) lineRenderer.enabled = visible;
        }
    }

}
