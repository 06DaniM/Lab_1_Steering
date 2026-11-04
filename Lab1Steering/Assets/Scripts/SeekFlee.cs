using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SeekFlee : MonoBehaviour
{
    private enum State { Seek, Flee }

    [SerializeField] private Transform target;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Distances")]
    [SerializeField] private float safeDistance = 6f;
    [SerializeField] private float hysteresis = 0.5f;
    [SerializeField] private float minFleeDistance = 5f;
    [SerializeField] private float maxFleeDistance = 10f;

    [Header("Speeds")]
    [SerializeField] private float seekSpeed = 3.5f;
    [SerializeField] private float fleeSpeed = 5f;

    [Header("Searching")]
    [SerializeField] private int candidateCount = 16;
    [SerializeField, Range(0f, 180f)] private float maxAngle = 120f;
    [SerializeField] private float sampleRadius = 2f;
    [SerializeField] private float repathInterval = 0.4f;

    private NavMeshAgent ai;
    private NavMeshPath path;
    private State state = State.Seek;
    private Vector3 fleePoint;
    private bool hasFleePoint;
    private float nextRepathTime;

    private void Awake()
    {
        ai = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }

    private void Update()
    {
        if (!target) return;

        UpdateState();

        if (Time.time < nextRepathTime) return;
        nextRepathTime = Time.time + repathInterval;

        if (state == State.Seek) DoSeek();
        else DoFlee();
    }

    private void UpdateState()
    {
        float distance = FlatDistance(transform.position, target.position);
        Vector3 origin = transform.position;
        Vector3 to = target.position - origin;

        bool canSeeTarget = !Physics.Raycast(origin, to.normalized, out _, distance, obstacleMask, QueryTriggerInteraction.Ignore);
        if (canSeeTarget)
        {
            if (state == State.Seek && distance < safeDistance)
            {
                state = State.Flee;
                ai.speed = fleeSpeed;
                hasFleePoint = false;
                nextRepathTime = 0f;
            }
            else if (state == State.Flee && distance > safeDistance + hysteresis)
            {
                state = State.Seek;
                ai.speed = seekSpeed;
                nextRepathTime = 0f;
            }
        }
        else if (state == State.Flee)
        {
            state = State.Seek;
            ai.speed = seekSpeed;
            nextRepathTime = 0f;
        }
    }

    private void DoSeek()
    {
        ai.SetDestination(target.position);
    }

    private void DoFlee()
    {
        bool arrived = hasFleePoint && !ai.pathPending && ai.remainingDistance <= ai.stoppingDistance + 0.5f;
        bool bad = hasFleePoint && FlatDistance(fleePoint, target.position) < FlatDistance(transform.position, target.position);

        if (!hasFleePoint || arrived || bad)
        {
            if (TryFindFleePoint(out Vector3 point))
            {
                fleePoint = point;
                hasFleePoint = true;
                ai.SetDestination(fleePoint);
            }
            else
            {
                hasFleePoint = false;
                ai.ResetPath();
            }
        }
    }

    private bool TryFindFleePoint(out Vector3 result)
    {
        result = transform.position;

        Vector3 away = transform.position - target.position;
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f) away = transform.forward;
        away.Normalize();

        float currentDistToTarget = FlatDistance(transform.position, target.position);
        float[] factors = { 1f, 0.6f, 0.3f };

        NavMeshQueryFilter filter = new()
        {
            agentTypeID = ai.agentTypeID,
            areaMask = NavMesh.AllAreas
        };

        foreach (float factor in factors)
        {
            float minDistance = minFleeDistance * factor;
            float maxDistance = Mathf.Max(maxFleeDistance, minDistance + 1f);
            float bestScore = float.MinValue;
            bool found = false;

            for (int i = 0; i < candidateCount; i++)
            {
                float t = candidateCount > 1 ? i / (candidateCount - 1) : 0.5f;
                float angle = Mathf.Lerp(-maxAngle, maxAngle, t) + Random.Range(-5f, 5f);
                float distance = Random.Range(minDistance, maxDistance);

                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * away;
                Vector3 candidate = transform.position + dir * distance;

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, filter))
                    continue; 

                if (FlatDistance(hit.position, transform.position) < minDistance) continue; 

                float distToTarget = FlatDistance(hit.position, target.position);

                if (distToTarget <= currentDistToTarget) continue; 
                if (!ai.CalculatePath(hit.position, path) ||
                path.status != NavMeshPathStatus.PathComplete) continue; 

                float score = distToTarget - 0.25f * PathLength(path);
                if (score > bestScore)
                {
                    bestScore = score;
                    result = hit.position;
                    found = true;
                }
            }

            if (found) return true;
        }
        
        return false;
    }

    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private float PathLength(NavMeshPath p)
    {
        float length = 0f;
        for (int i = 1; i < p.corners.Length; i++)
            length += Vector3.Distance(p.corners[i - 1], p.corners[i]);
        return length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        Gizmos.color = new Color(1f, 0.6f, 0f);
        Gizmos.DrawWireSphere(transform.position, minFleeDistance);

        if (hasFleePoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(fleePoint, 0.25f);
            Gizmos.DrawLine(transform.position, fleePoint);
        }
    }
}