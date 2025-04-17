using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                Debug.Log(hit.transform.name);
                if (hit.transform.TryGetComponent(out Door door))
                {

                }
            }
        }
    }
}
