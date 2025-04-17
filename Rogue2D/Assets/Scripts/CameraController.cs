using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Room currentRoom;
    public int facingDirection = 0;

    private void Start()
    {
        RealtimeDungeonGenerator.instance.changedRoom.AddListener(OnRoomChanged);
        if (RealtimeDungeonGenerator.instance.currentRoom != null) currentRoom = RealtimeDungeonGenerator.instance.currentRoom;
    }

    public void OnRoomChanged(Room room, Door door)
    {
        currentRoom = room;
        if (door != null) facingDirection = (int)door.dir;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (facingDirection == 0) facingDirection = 3;
            else facingDirection--;
            RealtimeDungeonGenerator.instance.ChangeCamera(currentRoom, (Direction)facingDirection);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (facingDirection == 3) facingDirection = 0;
            else facingDirection++;
            RealtimeDungeonGenerator.instance.ChangeCamera(currentRoom, (Direction)facingDirection);

        }
    }

}
