using UnityEngine;
using UnityEngine.AI;

public class NinaDeLaTierra : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    [SerializeField]private Transform Player;
    public float targetRange = 4.5f;
    private Transform T;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        T = this.transform;
    }

    private void Update()
    {
        if (Distance() < targetRange && PlayerController.Instance.GetComponent<HideComponent>().IsVisible)
        {
            if (!CheckForObstructions(Player.position))
            {
                navMeshAgent.destination = Player.position;
                Debug.Log("can approach player");
            }
            else
            {
                Debug.Log("obstructed");
            }
        }
    }

    private bool CheckForObstructions(Vector3 targetPos)
    {
        RaycastHit hit;
        Vector3 dir = (T.position - targetPos);
        if (Physics.Raycast(T.position, dir, out hit, targetRange))
        {
            if (hit.collider.gameObject.tag == "Wall" || hit.collider.gameObject.tag == "DiggableWall")
                return true;
        }
        return false;
    }

    private float Distance()
    {
        return Vector3.Distance(T.position, Player.position);
    }
}
