using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

//CameraMovement
//this script makes the camera follow the player in rooms.
//each room is the size of the camera view.
//when the player moves into a new room, the camera moves to the center of that room.
public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    //the object the camera should follow (usually the player)
    public Transform target;

    [Header("Smoothing")]
    //how fast the camera moves toward the new room.
    //0 = instant snap, 1 = very slow movement.
    [Range(0f, 1f)]
    public float smoothSpeed = 0.15f;

    [Header("Room Size")]
    //the width of one room
    public float roomWidth = 16f;

    //the height of one room
    public float roomHeight = 9f;

    //the room index the camera is currently tyoe int
    private int currentRoomX;
    private int currentRoomY;

    //the world position of the center of the current room
    private Vector3 currentRoomCenter;

    //the bottom left corner of room (0,0) in world space
    private Vector2 gridOrigin;


    //called once when the script is first enabled this is a Unity builtin function
    private void Awake()
    {
        //get the Camera component on this GameObject
        Camera cam = GetComponent<Camera>();
        //current room x and y will store 0
        currentRoomX = 0;
        currentRoomY = 0;

        //if we have an orthographic camera, use it to auto set the room size
        //orthographic cameras are used for 2D
        //objects do not get smaller with distance.
        if (cam != null && cam.orthographic)
        {
            //the height of the camera view in world units is orthographicSize * 2
            roomHeight = cam.orthographicSize * 2f;

            //the width depends on the screen aspect ratio.
            roomWidth = roomHeight * cam.aspect;
        }

        //treat the current camera position as the center of room (0,0)
        //so the bottom left of the grid is half a room downleft from the camera
        gridOrigin = new Vector2(
            transform.position.x - roomWidth * 0.5f,
            transform.position.y - roomHeight * 0.5f
        );


        //the current room center is where the camera starts
        currentRoomCenter = transform.position;
    }


    //called every frame, after all other Update function
    //this is a Unity builtin function
    private void LateUpdate()
    {
        //if we have no target, return nothing
        if (target == null)
        {
            return;
        }

        //figure out which room the player is currently in and store it in an int vector 
        //note that vector2int stores only whole numbers integers and is a unity builtin struct that represents a 2d vector with integer components
        Vector2Int targetRoom = roomFromPosition(target.position);
        int targetRoomX = targetRoom.x;
        int targetRoomY = targetRoom.y;

        //if the player moved into a different room, update the camera's target room.
        if (targetRoomX != currentRoomX || targetRoomY != currentRoomY)
        {
            currentRoomX = targetRoomX;
            currentRoomY = targetRoomY;

            //set the new room center for the camera to move toward.
            currentRoomCenter = getRoomCenter(currentRoomX, currentRoomY);
        }


        //if smoothSpeed is 0 or less, snap instantly to the position.
        if (smoothSpeed <= 0f)
        {
            transform.position = new Vector3(currentRoomCenter.x, currentRoomCenter.y, transform.position.z);
        }
        else
        {
            //else smoothly move toward the currentRoomCenter position.
            //lerp = linear interpolation, which moves between two positions.
            Vector3 targetPos = new Vector3(currentRoomCenter.x, currentRoomCenter.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);
        }
    }


    //this function use world position as parameter, figure out which room it's in.
    //returns a Vector2Int where:
    //x = room index horizontally, y = room index vertically.
    private Vector2Int roomFromPosition(Vector3 worldPos)
    {
        //distance from the grid origin.
        float dx = worldPos.x - gridOrigin.x;
        float dy = worldPos.y - gridOrigin.y;

        //divide by room size to get which room, then floor to get an integer index
        int roomX = Mathf.FloorToInt(dx / roomWidth);
        int roomY = Mathf.FloorToInt(dy / roomHeight);

        return new Vector2Int(roomX, roomY);
    }

    //this function uses the given room indices as a parameters (int roomX, int roomY)
    //return the center position of that room in world space 
    //since it is returning center position it would need vector 3
    private Vector3 getRoomCenter(int roomX, int roomY)
    {
        //center = origin + full rooms + half a room
        float cx = gridOrigin.x + roomX * roomWidth + roomWidth * 0.5f;
        float cy = gridOrigin.y + roomY * roomHeight + roomHeight * 0.5f;

        //keep the current camera Z (depth)
        return new Vector3(cx, cy, transform.position.z);
    }
}