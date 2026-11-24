using UnityEngine;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// One shared particle system for every spark burst (kills, hits, landings).
    /// Built from code with the generated spark sprite.
    /// </summary>
    public class ParticleBursts : MonoBehaviour
    {
        private static ParticleBursts _instance;

        public static ParticleBursts Instance { get { return _instance; } }

        private ParticleSystem system;

        private void Awake()
        {
            _instance = this;
            system = BuildSystem();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>Throws count sparks out from a point. Safe to call when no instance exists.</summary>
        public static void Burst(Vector2 position, Color color, int count, float speedScale = 1f)
        {
            if (_instance == null || count <= 0) return;

            var emit = new ParticleSystem.EmitParams { applyShapeToPosition = true };
            for (int i = 0; i < count; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float speed = Random.Range(3f, 11f) * speedScale;
                emit.position = position;
                emit.velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * speed;
                emit.startColor = Color.Lerp(color, Color.white, Random.value * 0.35f);
                emit.startSize = Random.Range(0.12f, 0.3f);
                emit.startLifetime = Random.Range(0.25f, 0.6f);
                _instance.system.Emit(emit, 1);
            }
        }

        private ParticleSystem BuildSystem()
        {
            ParticleSystem particles = gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = 800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;

            // Sparks slow down and shrink as they fade.
            ParticleSystem.LimitVelocityOverLifetimeModule drag = particles.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.drag = 3f;

            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.1f));

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            ParticleSystemRenderer particleRenderer = GetComponent<ParticleSystemRenderer>();
            Material material = new Material(Shader.Find("Sprites/Default"));
            Texture2D spark = SpriteLibrary.Texture("spark");
            if (spark != null) material.mainTexture = spark;
            particleRenderer.sharedMaterial = material;
            particleRenderer.sortingOrder = 12;

            particles.Play();
            return particles;
        }
    }

}
