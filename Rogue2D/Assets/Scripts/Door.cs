using UnityEngine;

public enum Direction { North, East, South, West }

public class Door : MonoBehaviour
{
    public Room room;
    public GameObject promiseRoom;
    public int offset;
    public Direction dir;

    public void EnteringNewRoom()
    {
        if (room != null)
        {
            //Change room
            RealtimeDungeonGenerator.instance.ChangeRoom(this, room);
        }
        else
        {
            RealtimeDungeonGenerator.instance.GenRoom(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (player.GetLastInteractedItem() == gameObject)
            {
                EnteringNewRoom();

            }
        }
    }

}
