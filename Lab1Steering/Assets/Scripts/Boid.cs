using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;
    public bool isLeader = false;

    private float orbitDir = 1f;
    private float orbitRadiusFactor = 1f;
    private float orbitPhase = 0.0f;

    private Vector3 wanderTarget = Vector3.zero;
    private float nextWanderTime = 0.0f;

    private const float skin = 0.05f;
    private static readonly float[] WhiskerYaw = { 0f, 35f, -35f };
    
    private FlockManager flock;
    private Renderer render;

    private void Awake()
    {
        render = GetComponentInChildren<Renderer>();
    }

    public void Init(FlockManager manager)
    {
        flock = manager;
        velocity = Random.onUnitSphere * flock.minSpeed;
        orbitDir = Random.value < 0.5f ? -1f : 1f;
        orbitRadiusFactor = Random.Range(0.9f, 1.3f);
        orbitPhase = Random.Range(0f, Mathf.PI * 2f);
        ApplyRole(false);
    }

    public void SetLeader(bool leader)
    {
        if (isLeader == leader) return;
        ApplyRole(leader);
    }

    private void ApplyRole(bool leader)
    {
        isLeader = leader;
        nextWanderTime = 0f;

        if (render)
            render.material.color = leader ? flock.leaderColor : flock.followerColor;
    }

    private void Update()
    {
        if (!flock) return;

        float dt = Time.deltaTime;

        Vector3 desired = isLeader ? LeaderDesired() : FollowerDesired();

        float blend = Mathf.Clamp01(flock.turnResponsiveness * dt);
        Vector3 v = Vector3.Lerp(velocity, desired, blend);

        float speed = v.magnitude;
        if (speed < 0.001f) v = transform.forward * flock.minSpeed;
        else v = v / speed * Mathf.Clamp(speed, flock.minSpeed, flock.maxSpeed);
        velocity = v;

        MoveWithCollision(dt);
        Facevelocity(dt);
    }

    #region Leader
    private Vector3 LeaderDesired()
    {
        Vector3 pos = transform.position;
        Vector3 toTarget = GetLeaderTarget(pos) - pos;
        Vector3 dir = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : transform.forward;

        Vector3 steer = dir + Avoid(Heading()) * flock.obstacleWeight + Bounds() * flock.boundsWeight;

        if (steer.sqrMagnitude < 0.0001f) steer = dir;
        return steer.normalized * flock.leaderSpeed;
    }

    private Vector3 GetLeaderTarget(Vector3 pos)
    {
        if (flock.orbitCenter)
        {
            Vector3 c = flock.orbitCenter.position;
            Vector3 rel = pos - c;
            rel.y = 0f;

            float angle = Mathf.Atan2(rel.z, rel.x) + orbitDir * 0.6f;
            float r = flock.orbitRadius * orbitRadiusFactor;
            float y = c.y + Mathf.Sin(Time.time * flock.orbitVerticalSpeed + orbitPhase) * flock.orbitHeightAmplitude;

            return new Vector3(c.x + Mathf.Cos(angle) * r, y, c.z + Mathf.Sin(angle) * r);
        }

        if (Time.time >= nextWanderTime || (wanderTarget - pos).sqrMagnitude < 4f)
            PickWanderTarget();

        return wanderTarget;
    }

    private void PickWanderTarget()
    {
        Vector3 half = flock.areaSize * 0.5f - Vector3.one * flock.boundsMargin;
        half = Vector3.Max(half, Vector3.one);
        Vector3 center = flock.transform.position;

        for (int i = 0; i < 10; i++)
        {
            Vector3 p = center + new Vector3(
                Random.Range(-half.x, half.x),
                Random.Range(-half.y, half.y),
                Random.Range(-half.z, half.z));

            if (!Physics.CheckSphere(p, flock.boidRadius * 3f, flock.obstacleMask, QueryTriggerInteraction.Ignore))
            {
                wanderTarget = p;
                break;
            }
        }

        nextWanderTime = Time.time + Random.Range(5f, 10f);
    }
    #endregion

    #region Follower
    private Vector3 FollowerDesired()
    {
        Vector3 pos = transform.position;

        Vector3 cohesion = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 separation = Vector3.zero;

        int neighbours = 0;
        int separationCount = 0;

        Boid nearestLeader = null;
        float nearestLeaderSqr = float.MaxValue;

        float neighbourSqr = flock.neighborRadius * flock.neighborRadius;
        float separationSqr = flock.separationRadius * flock.separationRadius;

        foreach (Boid other in flock.Boids)
        {
            if (other == this) continue;

            Vector3 offset = other.transform.position - pos;
            float sqr = offset.sqrMagnitude;

            if (other.isLeader && sqr < nearestLeaderSqr)
            {
                nearestLeaderSqr = sqr;
                nearestLeader = other;
            }

            if (sqr > neighbourSqr) continue;

            neighbours++;
            cohesion += other.transform.position;
            alignment += other.velocity;

            if (sqr < separationSqr)
            {
                if (sqr > 0.0001f) separation -= offset / sqr;
                else separation += Random.onUnitSphere;
                separationCount++;
            }
        }

        if (neighbours > 0)
        {
            cohesion = (cohesion / neighbours - pos).normalized;
            alignment = (alignment / neighbours).normalized;
        }

        if (separationCount > 0)
            separation = Vector3.ClampMagnitude(separation / separationCount * flock.separationRadius, 1f);

        Vector3 leaderForce = Vector3.zero;
        if (nearestLeader)
        {
            Vector3 toLeader = nearestLeader.transform.position - pos;
            if (toLeader.magnitude > flock.leaderFollowDistance)
                leaderForce = toLeader.normalized;
        }

        Vector3 desired =
            cohesion * flock.cohesionWeight +
            alignment * flock.alignmentWeight +
            separation * flock.separationWeight +
            leaderForce * flock.leaderWeight +
            Avoid(Heading()) * flock.obstacleWeight +
            Bounds() * flock.boundsWeight;

        return Vector3.ClampMagnitude(desired, 1f) * flock.maxSpeed;
    }
    #endregion

    private Vector3 Heading()
    {
        return velocity.sqrMagnitude > 0.01f ? velocity.normalized : transform.forward;
    }

    private Vector3 Avoid(Vector3 heading)
    {
        Vector3 result = Vector3.zero;
        Vector3 pos = transform.position;
        float look = flock.obstacleLookAhead;

        foreach (float yaw in WhiskerYaw)
        {
            Vector3 dir = Quaternion.AngleAxis(yaw, Vector3.up) * heading;

            if (Physics.SphereCast(pos, flock.boidRadius, dir, out RaycastHit hit, look, flock.obstacleMask, QueryTriggerInteraction.Ignore))
            {
                float strength = 1f - Mathf.Clamp01(hit.distance / look);

                Vector3 slide = Vector3.ProjectOnPlane(dir, hit.normal);
                if (slide.sqrMagnitude < 0.01f) slide = Vector3.Cross(hit.normal, Vector3.up);
                if (slide.sqrMagnitude < 0.01f) slide = Vector3.Cross(hit.normal, Vector3.right);

                result += (hit.normal + slide.normalized) * strength;
            }
        }

        return result;
    }

    private Vector3 Bounds()
    {
        Vector3 local = transform.position - flock.transform.position;
        Vector3 half = flock.areaSize * 0.5f;
        float margin = Mathf.Max(0.1f, flock.boundsMargin);
        Vector3 force = Vector3.zero;

        for (int i = 0; i < 3; i++)
        {
            float limit = half[i] - margin;
            float a = Mathf.Abs(local[i]);
            if (a > limit)
                force[i] = -Mathf.Sign(local[i]) * Mathf.Clamp01((a - limit) / margin);
        }

        return force;
    }

    private void MoveWithCollision(float dt)
    {
        Vector3 step = velocity * dt;
        float dist = step.magnitude;

        if (dist < 0.0001f) return;

        Vector3 dir = step / dist;

        if (Physics.SphereCast(transform.position, flock.boidRadius, dir, out RaycastHit hit, dist + skin, flock.obstacleMask, QueryTriggerInteraction.Ignore))
        {
            float allowed = Mathf.Max(0f, hit.distance - skin);
            transform.position += dir * allowed;

            Vector3 remaining = dir * (dist - allowed);
            transform.position += Vector3.ProjectOnPlane(remaining, hit.normal);

            velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
        }

        else transform.position += step;
    }

    private void Facevelocity(float dt)
    {
        if (velocity.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * dt);
    }
}