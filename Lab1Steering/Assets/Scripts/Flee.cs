using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Flee : MonoBehaviour
{
    [SerializeField] private GameObject target;
    private NavMeshAgent ai;
    
    private void Awake()
    {
        ai = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
