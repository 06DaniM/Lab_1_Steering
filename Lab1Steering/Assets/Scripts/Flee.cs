using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Flee : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float distanceToRun = 3.0f;
    private Vector3 newPosition;
    private NavMeshAgent ai;
    
    private void Awake()
    {
        ai = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if (distanceToTarget < distanceToRun)
        {
            Vector3 directionToTarget = transform.position - target.transform.position;
            newPosition = directionToTarget + transform.position;
            ai.SetDestination(newPosition);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(newPosition, 0.2f);
    }
}
