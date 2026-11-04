using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Patrol : MonoBehaviour
{
    [SerializeField] private float stoppedTime = 1.5f;
    [SerializeField] private GameObject[] waypoints;
    
    private Vector3 target = Vector3.zero;
    private NavMeshAgent ai;

    private void Awake()
    {
        ai = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(SetNewTarget());
    }

    private void Update()
    {
        if (Vector3.Distance(target, transform.position) <= 0.1f)
        {
            StartCoroutine(SetNewTarget());
        }
    }

    private IEnumerator SetNewTarget()
    {
        if (waypoints.Length <= 0) yield break;
        
        int r = Random.Range(0, waypoints.Length);
        target = waypoints[r].transform.position;
        yield return new WaitForSeconds(stoppedTime);
        ai.SetDestination(target);
    }
}