using UnityEngine;
using UnityEngine.AI;

public enum PlayerState { Free, Combat }
public class PlayerController : MonoBehaviour
{
    public PlayerState state = PlayerState.Free;

    [SerializeField] NavMeshAgent agent;
    Camera cam;

    GameObject LastInteractedItem;

    private void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                agent.SetDestination(hit.point);
                LastInteractedItem = hit.transform.gameObject;
            }
        }
    }

    public GameObject GetLastInteractedItem()
    {
        return LastInteractedItem;
    }
}
