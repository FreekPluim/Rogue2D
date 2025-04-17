using Cinemachine;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;

public class RealtimeDungeonGenerator : MonoBehaviour
{
    public static RealtimeDungeonGenerator instance;

    [Header("General Refferences")]
    public Room currentRoom;
    public CinemachineVirtualCamera currentCam;
    public int maxRoomCount = 20;

    [Header("Seed")]
    [SerializeField] int GenerationSeed;

    [Header("Generation Objects")]
    [SerializeField] GameObject doorPrefab;
    [SerializeField] GameObject floorTile;
    [SerializeField] GameObject virtualCameraPrefab;
    [SerializeField] GameObject promiseBoxPrefab;

    [Header("Nav mesh")]
    [SerializeField] NavMeshSurface navMeshSurface;

    [HideInInspector] public UnityEvent<Room, Door> changedRoom;

    List<Room> rooms = new List<Room>();
    List<Door> allDoors = new List<Door>();

    #region Testing
    List<Vector3> testList = new List<Vector3>();
    [SerializeField] Transform point;
    public void TestBuildingOverlap()
    {
        testList.Clear();
        Debug.Log(point.position);

        testList.Add(point.position);

        foreach (var room in rooms)
        {
            print(room.CheckInRoomBounds(testList));
        }
    }
    #endregion

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        Random.InitState(GenerationSeed);

        //GenRoom();
        GenerateDungeon();
    }
    public void GenRoom(Door interactedDoor = null)
    {
        rooms.Add(CreateRoom(interactedDoor));
        CreateRoomVisuals(rooms[rooms.Count - 1], interactedDoor);
    }

    //Old code
    /*
        Room GenerateRoom(Door interactedDoor = null)
        {
            if (interactedDoor != null && !currentRoom.doors.Contains(interactedDoor.gameObject))
            {
                Debug.LogError("Door not in this room");
                return null;
            }

            //Create new room
            Room newRoom = new Room();

            //Set room type to start if its the first room
            if (rooms.Count == 0)
            {
                newRoom.roomType = RoomType.START;
                currentRoom = newRoom;
            }

            GameObject room = new GameObject("Room");
            newRoom.roomParent = room;

            //Set room size
            Vector3 roomSize = new(Random.Range(20, 30), 1, Random.Range(20, 30));
            newRoom.roomSize = roomSize;

            //Set room position
            Vector3 roomPosition = Vector3.zero;
            if (interactedDoor != null)
            {
                interactedDoor.room = newRoom;
                //Change room position based on door pressed
                //Also set the door back and exclude back direction from doors being placed
                Door newDoor;
                switch (interactedDoor.dir)
                {
                    //z+
                    case Direction.North:
                        //Works
                        roomPosition = new(
                            currentRoom.roomParent.transform.position.x,
                            0,
                            currentRoom.roomParent.transform.position.z + currentRoom.roomSize.z / 2 + (roomSize.z / 2));

                        //Places door on the same position under parent, not in world space.
                        placeDoor(newRoom, new(0, 0, -(roomSize.z / 2)), Direction.South, currentRoom);

                        break;
                    //x+
                    case Direction.East:
                        roomPosition = new(
                            currentRoom.roomParent.transform.position.x + currentRoom.roomSize.x / 2 + (roomSize.x / 2),
                            0,
                            currentRoom.roomParent.transform.position.z);

                        placeDoor(newRoom, new(-(roomSize.x / 2), 0, 0), Direction.West, currentRoom);

                        break;
                    //z-
                    case Direction.South:
                        roomPosition = new(
                            currentRoom.roomParent.transform.position.x,
                            0,
                            currentRoom.roomParent.transform.position.z - currentRoom.roomSize.z / 2 - (roomSize.z / 2));

                        placeDoor(newRoom, new(0, 0, (roomSize.z / 2) - 1), Direction.North, currentRoom);

                        break;
                    //x-
                    case Direction.West:
                        roomPosition = new(
                            currentRoom.roomParent.transform.position.x - currentRoom.roomSize.x / 2 - (roomSize.x / 2),
                            0,
                            currentRoom.roomParent.transform.position.z);

                        placeDoor(newRoom, new((roomSize.x / 2) - 1, 0, 0), Direction.East, currentRoom);

                        break;
                }

            }
            room.transform.position = roomPosition;

            //Place Floor
            float halfX = roomSize.x / 2;
            float halfZ = roomSize.z / 2;
            for (float x = 0; x < roomSize.x; x++)
            {
                for (float z = 0; z < roomSize.z; z++)
                {
                    Instantiate(floorTile, roomPosition + (new Vector3(x - halfX, 0, z - halfZ)), Quaternion.identity, room.transform);
                }
            }

            PlaceDoors(newRoom);
            PlaceCameras(newRoom);

            //currentRoom.roomParent.SetActive(false);


            ChangeRoom(interactedDoor, newRoom);

            return newRoom;
        }
    */

    void GenerateDungeon()
    {
        GenRoom();
        for (int i = 0; i < maxRoomCount; ++i)
        {
            foreach (var door in rooms[i].doors)
            {
                GenRoom(door.GetComponent<Door>());
            }
        }

        List<Door> temp = new List<Door>();
        foreach (var door in allDoors)
        {
            if (door.promiseRoom != null)
            {
                temp.Add(door);
            }
        }

        for (int i = temp.Count - 1; i >= 0; i--)
        {
            Destroy(temp[i].promiseRoom);
            Destroy(temp[i].gameObject);
        }
    }

    Direction GetOppositeDirection(Direction dir)
    {
        Direction temp = Direction.North;

        switch (dir)
        {
            case Direction.North:
                temp = Direction.South;
                break;
            case Direction.East:
                temp = Direction.West;
                break;
            case Direction.South:
                temp = Direction.North;
                break;
            case Direction.West:
                temp = Direction.East;
                break;
        }

        return temp;
    }
    Room CreateRoom(Door interactedDoor = null)
    {
        //Create new room
        Room newRoom = new Room();
        if (interactedDoor != null) interactedDoor.room = newRoom;

        //Set room type to start if its the first room
        if (rooms.Count == 0)
        {
            newRoom.roomType = RoomType.START;
            currentRoom = newRoom;
        }

        SetRoomData(interactedDoor, newRoom);

        if (interactedDoor != null)
        {
            Destroy(interactedDoor.promiseRoom);
            interactedDoor.promiseRoom = null;
            bool isValidRoom = false;
            int failSafe = 0;
            while (!isValidRoom)
            {
                failSafe++;
                isValidRoom = true;
                foreach (Room room in rooms)
                {
                    if (room.CheckInRoomBounds(newRoom.GetCorners()))
                    {
                        isValidRoom = false;
                    }
                }
                if (!isValidRoom) SetRoomData(interactedDoor, newRoom);
                if (failSafe == 50)
                {
                    isValidRoom = true;
                    break;
                }
            }

            newRoom.possibleDirections.Remove(GetOppositeDirection(interactedDoor.dir));

        }

        return newRoom;
    }

    void CreateRoomVisuals(Room newRoom, Door interactedDoor = null)
    {
        GameObject room = new GameObject("room");
        room.transform.position = newRoom.roomPosition;
        newRoom.roomParent = room;

        //Place Floor
        float halfX = newRoom.roomSize.x / 2;
        float halfZ = newRoom.roomSize.z / 2;
        for (float x = 0; x < newRoom.roomSize.x; x++)
        {
            for (float z = 0; z < newRoom.roomSize.z; z++)
            {
                Instantiate(floorTile, newRoom.roomPosition + (new Vector3(x - halfX, 0, z - halfZ)), Quaternion.identity, room.transform);
            }
        }

        newRoom.RemoveImpossibleSides();
        PlaceDoors(newRoom);
        PlaceCameras(newRoom);

        UpdateNavmesh();
        //ChangeRoom(interactedDoor, newRoom);
    }

    private void SetRoomData(Door interactedDoor, Room newRoom)
    {
        //Set room size
        Vector3 roomSize = new(Random.Range(20, 30), 1, Random.Range(20, 30));
        newRoom.roomSize = roomSize;

        int RoomOffset = Random.Range(-8, 8);
        newRoom.roomOffset = RoomOffset;

        //Get room position
        Vector3 roomPosition = Vector3.zero;
        if (interactedDoor != null)
        {
            //Change room position based on door pressed
            //Also set the door back and exclude back direction from doors being placed
            roomPosition = GetRoomPosition(interactedDoor, roomSize, newRoom.roomOffset);
        }
        newRoom.roomPosition = roomPosition;
        newRoom.SetRoomBounds();
    }
    private Vector3 GetRoomPosition(Door interactedDoor, Vector3 roomSize, int roomOffset)
    {
        Vector3 roomPos = Vector3.zero;

        switch (interactedDoor.dir)
        {
            //z+
            case Direction.North:
                //Works
                roomPos = new(
                    interactedDoor.transform.position.x + roomOffset,
                    0,
                    interactedDoor.transform.position.z + 0.5f + (roomSize.z / 2));
                break;
            //x+
            case Direction.East:
                roomPos = new(
                    interactedDoor.transform.position.x + 0.5f + (roomSize.x / 2),
                    0,
                    interactedDoor.transform.position.z + roomOffset);

                break;
            //z-
            case Direction.South:
                roomPos = new(
                    interactedDoor.transform.position.x + roomOffset,
                    0,
                    interactedDoor.transform.position.z - (roomSize.z / 2));

                break;
            //x-
            case Direction.West:
                roomPos = new(
                    interactedDoor.transform.position.x - (roomSize.x / 2),
                    0,
                    interactedDoor.transform.position.z + roomOffset);
                break;
        }

        return roomPos;
    }
    public void ChangeRoom(Door interactedDoor, Room enteringRoom)
    {
        /*if (interactedDoor != null && !currentRoom.doors.Contains(interactedDoor.gameObject))
        {
            Debug.LogError("Door not in this room");
            return;
        }*/

        if (interactedDoor != null) ChangeCamera(enteringRoom, interactedDoor.dir);
        else ChangeCamera(enteringRoom, Direction.North);

        currentRoom = enteringRoom;
        changedRoom.Invoke(currentRoom, interactedDoor);
    }

    #region Doors
    private void PlaceDoors(Room newRoom)
    {
        int min = 0;
        if (newRoom.roomType == RoomType.START)
        {
            min = 2;
        }
        if (allDoors.Count <= maxRoomCount - 1)
        {
            min = 1;
        }

        //Place doors
        for (int i = 0; i < Random.Range(min, newRoom.possibleDirections.Count + 1); i++)
        {
            Direction doorDir = newRoom.possibleDirections[Random.Range(0, newRoom.possibleDirections.Count)];

            Door newDoor = new();
            switch (doorDir)
            {
                case Direction.North:
                    newDoor = placeDoor(newRoom, new(newRoom.roomOffset, 0, (newRoom.roomSize.z / 2) - 1), doorDir);
                    newDoor.promiseRoom = Instantiate(promiseBoxPrefab, newRoom.roomPosition + new Vector3(newRoom.roomOffset, 0, ((newRoom.roomSize.z / 2) - 1) + 15), Quaternion.identity);
                    break;
                case Direction.East:
                    newDoor = placeDoor(newRoom, new(((newRoom.roomSize.x / 2) - 1), 0, newRoom.roomOffset), doorDir);
                    newDoor.promiseRoom = Instantiate(promiseBoxPrefab, newRoom.roomPosition + new Vector3(((newRoom.roomSize.x / 2) - 1) + 15, 0, newRoom.roomOffset), Quaternion.identity);
                    break;
                case Direction.South:
                    newDoor = placeDoor(newRoom, new(newRoom.roomOffset, 0, -(newRoom.roomSize.z / 2)), doorDir);
                    newDoor.promiseRoom = Instantiate(promiseBoxPrefab, newRoom.roomPosition + new Vector3(newRoom.roomOffset, 0, -(newRoom.roomSize.z / 2) - 15), Quaternion.identity);
                    break;
                case Direction.West:
                    newDoor = placeDoor(newRoom, new(-(newRoom.roomSize.x / 2), 0, newRoom.roomOffset), doorDir);
                    newDoor.promiseRoom = Instantiate(promiseBoxPrefab, newRoom.roomPosition + new Vector3(-(newRoom.roomSize.x / 2) - 15, 0, newRoom.roomOffset), Quaternion.identity);
                    break;
            }
            allDoors.Add(newDoor);
        }
    }
    Dictionary<Direction, Vector3> dirToRot = new Dictionary<Direction, Vector3>()
    {
        {Direction.North, Vector3.zero },
        {Direction.East, new(0,90,0)},
        {Direction.South, new(0,180,0) },
        {Direction.West, new(0,-90,0) }
    };
    Door placeDoor(Room room, Vector3 position, Direction direction, Room previousRoom = null)
    {
        Door newDoor = Instantiate(doorPrefab,
            room.roomParent.transform.position + position, Quaternion.Euler(dirToRot[direction]),
            room.roomParent.transform).GetComponent<Door>();

        newDoor.dir = direction;
        newDoor.room = previousRoom;

        room.doors.Add(newDoor.gameObject);

        room.possibleDirections.Remove(direction);

        return newDoor;
    }
    #endregion
    #region Cameras
    void PlaceCameras(Room room)
    {
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    pos = new(0, 12, -(room.roomSize.z / 2));
                    break;
                case 1:
                    pos = new(-(room.roomSize.x / 2), 12, 0);
                    break;
                case 2:
                    pos = new(0, 12, (room.roomSize.z / 2));
                    break;
                case 3:
                    pos = new((room.roomSize.x / 2), 12, 0);
                    break;
                default:
                    break;
            }

            room.virtualCameras.Add((Direction)i,
                Instantiate(
                    virtualCameraPrefab,
                    room.roomParent.transform.position + pos,
                    Quaternion.identity,
                    room.roomParent.transform).GetComponent<CinemachineVirtualCamera>());
            room.virtualCameras[(Direction)i].LookAt = room.roomParent.transform;
            room.virtualCameras[(Direction)i].gameObject.SetActive(false);
        }
    }
    public void ChangeCamera(Room room, Direction newDirection)
    {
        if (currentCam != null) currentCam.gameObject.SetActive(false);
        room.virtualCameras[newDirection].gameObject.SetActive(true);
        currentCam = room.virtualCameras[newDirection];
    }
    #endregion
    #region Navigation Surface
    private void UpdateNavmesh()
    {
        navMeshSurface.BuildNavMesh();
    }
    #endregion
}

#region Room stuff
public enum RoomType { NONE, START, END, TREASURE, COMBAT }
public enum RoomSides { LEFT, RIGHT, TOP, BOTTOM }
public class Room
{
    public GameObject roomParent;

    //Room data
    public Vector3 roomSize;
    public Vector3 roomPosition;
    public RoomType roomType;
    public int roomOffset;

    public List<GameObject> doors = new List<GameObject>();
    public Dictionary<Direction, CinemachineVirtualCamera> virtualCameras = new Dictionary<Direction, CinemachineVirtualCamera>();
    public List<Direction> possibleDirections = new() {
        Direction.North, Direction.East, Direction.South, Direction.West
    };
    public Dictionary<RoomSides, float> RoomBounds = new Dictionary<RoomSides, float>();

    public bool CheckInRoomBounds(List<Vector3> positions)
    {
        if (RoomBounds.Count == 0) SetRoomBounds();
        bool ret = false;
        foreach (var pos in positions)
        {
            if (pos.x > RoomBounds[RoomSides.LEFT] && //Check right +x
                pos.x < RoomBounds[RoomSides.RIGHT] && //Check Left -x
                pos.z < RoomBounds[RoomSides.TOP] && //Check Top +z
                pos.z > RoomBounds[RoomSides.BOTTOM]) //Check Bottom -z
            {
                ret = true;
            }
        }
        return ret;
    }
    public void SetRoomBounds()
    {
        RoomBounds.Clear();
        if (roomSize != null && roomParent != null)
        {
            RoomBounds.Add(RoomSides.LEFT, roomPosition.x - roomSize.x / 2);
            RoomBounds.Add(RoomSides.RIGHT, roomPosition.x + roomSize.x / 2);
            RoomBounds.Add(RoomSides.TOP, roomPosition.z + roomSize.z / 2);
            RoomBounds.Add(RoomSides.BOTTOM, roomPosition.z - roomSize.z / 2);
        }
    }
    public List<Vector3> GetCorners()
    {
        List<Vector3> corners = new List<Vector3>();

        corners.Add(new(roomPosition.x - roomSize.x / 2, 0, roomPosition.z + roomSize.z / 2));
        corners.Add(new(roomPosition.x + roomSize.x / 2, 0, roomPosition.z + roomSize.z / 2));
        corners.Add(new(roomPosition.x - roomSize.x / 2, 0, roomPosition.z - roomSize.z / 2));
        corners.Add(new(roomPosition.x + roomSize.x / 2, 0, roomPosition.z - roomSize.z / 2));
        corners.Add(roomPosition);

        return corners;
    }
    public void RemoveImpossibleSides()
    {
        SetRoomBounds();
        //Check north
        if (possibleDirections.Contains(Direction.North))
        {
            if (Physics.Raycast(new Vector3(RoomBounds[RoomSides.LEFT], -0.01f, RoomBounds[RoomSides.TOP]), Vector3.forward, 30))
            {
                possibleDirections.Remove(Direction.North);
            }
            else if (Physics.Raycast(new Vector3(RoomBounds[RoomSides.RIGHT], -0.01f, RoomBounds[RoomSides.TOP]), Vector3.forward, 30))
            {
                possibleDirections.Remove(Direction.North);
            }
        }

        //Check east
        if (possibleDirections.Contains(Direction.East))
        {
            if (Physics.Raycast(new Vector3(RoomBounds[RoomSides.RIGHT], -0.01f, RoomBounds[RoomSides.TOP]), Vector3.right, 30))
            {
                possibleDirections.Remove(Direction.East);
            }
            else if (Physics.Raycast(new Vector3(RoomBounds[RoomSides.RIGHT], -0.01f, RoomBounds[RoomSides.BOTTOM]), Vector3.right, 30))
            {
                possibleDirections.Remove(Direction.East);
            }
        }

        //Check south
        if (possibleDirections.Contains(Direction.South))
        {
            if (Physics.Raycast(new Vector3(RoomBounds[RoomSides.LEFT], -0.01f, RoomBounds[RoomSides.BOTTOM]), -Vector3.forward, 30))
            {
                possibleDirections.Remove(Direction.South);
            }
            else if (Physics.Raycast(new Vector3(RoomBounds[RoomSides.RIGHT], -0.01f, RoomBounds[RoomSides.BOTTOM]), -Vector3.forward, 30))
            {
                possibleDirections.Remove(Direction.South);
            }
        }

        //Check west
        if (possibleDirections.Contains(Direction.West))
        {
            if (Physics.Raycast(new Vector3(RoomBounds[RoomSides.LEFT], -0.01f, RoomBounds[RoomSides.TOP]), -Vector3.right, 30))
            {
                possibleDirections.Remove(Direction.West);
            }
            else if (Physics.Raycast(new Vector3(RoomBounds[RoomSides.LEFT], -0.01f, RoomBounds[RoomSides.BOTTOM]), -Vector3.right, 30))
            {
                possibleDirections.Remove(Direction.West);
            }
        }
    }
}
#endregion
