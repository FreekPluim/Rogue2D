/*public class DungeonFloorGenerator : MonoBehaviour
{
    public GameObject floorPrefab;

    private void Start()
    {
        Floor floor = new Floor(new Vector2Int(120, 120), new Vector2Int(6, 12), this);
        StartCoroutine(floor.GenerateFloor());
    }

}


public class Floor : MonoBehaviour
{
    DungeonFloorGenerator gen;

    Vector2Int floorSize;
    Vector2Int roomsMinMax;

    public Dictionary<Room, GameObject> rooms = new Dictionary<Room, GameObject>();

    public Floor(Vector2Int pFloorSize, Vector2Int pRoomsMinMax, DungeonFloorGenerator pGen)
    {
        floorSize = pFloorSize;
        roomsMinMax = pRoomsMinMax;
        gen = pGen;
    }

    public IEnumerator GenerateFloor()
    {
        GameObject floor = new GameObject("Floor");
        floor.transform.localScale = new Vector3(floorSize.x, 1, floorSize.y);

        Room room = new Room(0, floorSize.x, floorSize.y, Vector3Int.zero);
        GameObject r0 = new GameObject("Room 0");
        rooms.Add(room, r0);

        for (int i = 0; i < Random.Range(2, 3); i++)
        {
            int selectedRoom = Random.Range(0, rooms.Count);
            rooms.AddRange(rooms.ElementAt(selectedRoom).Key.Split());
            //Destroy(rooms.ElementAt(selectedRoom).Value);
            rooms.Remove(rooms.ElementAt(selectedRoom).Key);
            yield return new WaitForSeconds(5);
        }

    }
}

public class Room
{
    DungeonFloorGenerator gen;
    int ID;
    Vector2Int roomSize;

    Vector3Int roomPosition;
    public Room(int pID, int pX, int pZ, Vector3Int pRoomPosition)
    {
        ID = pID;
        roomSize = new(pX, pZ);
        roomPosition = pRoomPosition;
    }

    public Dictionary<Room, GameObject> Split()
    {
        Dictionary<Room, GameObject> retRoom = new Dictionary<Room, GameObject>();
        bool vertical = Random.Range(1, 2) == 1;

        // Create a horizontal wall to split the room
        if (vertical)
        {
            //Get room height
            int roomHeight = roomSize.y;

            //Get Middle
            int middle = roomPosition.z;

            //add Offset
            int offset = Random.Range(-5, 5);

            //get new room height
            int newRoomHeight = (roomHeight / 2) + offset;

            //Create new rooms
            Room room1 = new Room(
                ID + 1,
                roomSize.x, newRoomHeight,
                new(roomPosition.x, roomPosition.y, middle + (newRoomHeight / 2)));

            Room room2 = new Room(
                ID + 2,
                roomSize.x, (roomHeight - newRoomHeight),
                new(roomPosition.x, roomPosition.y, middle - ((newRoomHeight + -(offset * 2)) / 2)));


            #region Visualize
            GameObject r1 = new GameObject("Room " + room1.ID);
            r1.transform.position = room1.roomPosition;
            r1.transform.localScale = new(room1.roomSize.x, 1, room1.roomSize.y);

            GameObject r2 = new GameObject("Room " + room2.ID);
            r2.transform.position = room2.roomPosition;
            r2.transform.localScale = new(room2.roomSize.x, 1, room2.roomSize.y);
            #endregion


            retRoom.Add(room1, r1);
            retRoom.Add(room2, r2);
        }
        else // Create a vertical wall to split the room
        {
            //Get room Width
            int roomWidth = roomSize.x;

            //Get Middle
            int middle = roomPosition.x;

            //add Offset
            int newWall = middle + Random.Range(-5, 5);

            //get new room height
            int newRoomWidth = roomWidth / 2 - newWall;

            //Create new rooms
            Room room1 = new Room(
                ID + 1,
                newRoomWidth, roomSize.y,
                new(newRoomWidth / 2, roomPosition.y, roomPosition.z));

            Room room2 = new Room(
                ID + 2,
                (roomWidth - newRoomWidth), roomSize.y,
                new(-(newRoomWidth / 2 + newWall), roomPosition.y, roomPosition.z));

            #region Visualize
            GameObject r1 = new GameObject("Room " + room1.ID);
            r1.transform.position = room1.roomPosition;
            r1.transform.localScale = new(room1.roomSize.x, 1, room1.roomSize.y);

            GameObject r2 = new GameObject("Room " + room2.ID);
            r2.transform.position = room2.roomPosition;
            r2.transform.localScale = new(room2.roomSize.x, 1, room2.roomSize.y);
            #endregion

            retRoom.Add(room1, r1);
            retRoom.Add(room2, r2);
        }

        return retRoom;
    }
}*/
