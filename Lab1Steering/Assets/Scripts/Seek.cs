using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Seek : MonoBehaviour
{
    [SerializeField] private GameObject target;
    private NavMeshAgent ai;
    
    private void Awake()
    {
        ai = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        ai.SetDestination(target.transform.position);
    }

    void Update()
    {
        
    }
}
