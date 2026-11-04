using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [SerializeField] private Boid boidPrefab;

    [Header("Selectors")]
    [Min(1)] public int totalBoids = 30;
    [Min(0)] public int leaderCount = 2;

    [Header("Weights")]
    [Range(0f, 5f)] public float cohesionWeight = 1f;
    [Range(0f, 5f)] public float alignmentWeight = 1f;
    [Range(0f, 8f)] public float separationWeight = 1.5f;
    [Range(0f, 5f)] public float leaderWeight = 1.5f;
    [Range(0f, 10f)] public float obstacleWeight = 3f;
    [Range(0f, 10f)] public float boundsWeight = 2f;

    [Header("Boids")]
    public float neighborRadius = 5f;
    public float separationRadius = 1.5f;
    public float leaderFollowDistance = 3f;
    public float minSpeed = 2f;
    public float maxSpeed = 6f;
    public float leaderSpeed = 4f;
    public float turnResponsiveness = 4f;

    [Header("Obstacles")]
    public LayerMask obstacleMask;
    public float boidRadius = 0.6f;
    public float obstacleLookAhead = 6f;

    [Header("Area Size")]
    public Vector3 areaSize = new(40f, 20f, 40f);
    public float boundsMargin = 4f;

    [Header("Orbit")]
    public Transform orbitCenter;
    public float orbitRadius = 8f;
    public float orbitHeightAmplitude = 3f;
    public float orbitVerticalSpeed = 0.5f;

    [SerializeField] private float spawnRadius = 6f;

    [SerializeField] private bool showPanel = true;

    [Header("Colores")]
    public Color leaderColor = Color.red;
    public Color followerColor = Color.white;

    private readonly List<Boid> boids = new();
    public List<Boid> Boids => boids;

    private int appliedTotal = -1;
    private int appliedLeaders = -1;
    private float nextRetry = -1.0f;

    private void Update()
    {
        leaderCount = Mathf.Clamp(leaderCount, 0, totalBoids);

        if (boids.Count != totalBoids && Time.time >= nextRetry)
        {
            nextRetry = Time.time + 1f;
            SyncPopulation();
        }

        if (leaderCount != appliedLeaders || boids.Count != appliedTotal) SyncLeaders();

        appliedTotal = boids.Count;
        appliedLeaders = leaderCount;
    }

    private void SyncPopulation()
    {
        int safety = totalBoids * 10;
        while (boids.Count < totalBoids && safety-- > 0)
        {
            if (!TrySpawn()) break;
        }

        while (boids.Count > totalBoids)
        {
            Boid last = boids[^1];
            boids.RemoveAt(boids.Count - 1);
            Destroy(last.gameObject);
        }
    }

    private bool TrySpawn()
    {
        if (!boidPrefab) return false;

        for (int i = 0; i < 10; i++)
        {
            Vector3 p = transform.position + Random.insideUnitSphere * spawnRadius;

            if (Physics.CheckSphere(p, boidRadius * 2f, obstacleMask, QueryTriggerInteraction.Ignore))
                continue;

            Boid boid = Instantiate(boidPrefab, p, Random.rotation, transform);

            boid.Init(this);
            boids.Add(boid);

            return true;
        }

        return false;
    }

    private void SyncLeaders()
    {
        for (int i = 0; i < boids.Count; i++)
            boids[i].SetLeader(i < leaderCount);
    }

    private void OnGUI()
    {
        if (!showPanel) return;

        GUILayout.BeginArea(new Rect(10, 10, 260, 320), GUI.skin.box);
        GUILayout.Label($"Boid: {totalBoids}");
        totalBoids = Mathf.RoundToInt(GUILayout.HorizontalSlider(totalBoids, 1, 300));

        GUILayout.Label($"Leaders: {leaderCount}");
        leaderCount = Mathf.RoundToInt(GUILayout.HorizontalSlider(leaderCount, 0, totalBoids));

        GUILayout.Label($"Cohesion: {cohesionWeight:F1}");
        cohesionWeight = GUILayout.HorizontalSlider(cohesionWeight, 0f, 5f);

        GUILayout.Label($"Alignment: {alignmentWeight:F1}");
        alignmentWeight = GUILayout.HorizontalSlider(alignmentWeight, 0f, 5f);

        GUILayout.Label($"Separation: {separationWeight:F1}");
        separationWeight = GUILayout.HorizontalSlider(separationWeight, 0f, 8f);

        GUILayout.Label($"Follow leader weight: {leaderWeight:F1}");
        leaderWeight = GUILayout.HorizontalSlider(leaderWeight, 0f, 5f);
        GUILayout.EndArea();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, areaSize);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        if (orbitCenter)
        {
            Gizmos.color = Color.red;
            Vector3 c = orbitCenter.position;
            Vector3 prev = c + new Vector3(orbitRadius, 0f, 0f);
            for (int i = 1; i <= 48; i++)
            {
                float a = i / 48f * Mathf.PI * 2f;
                Vector3 next = c + new Vector3(Mathf.Cos(a) * orbitRadius, 0f, Mathf.Sin(a) * orbitRadius);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
    }
}