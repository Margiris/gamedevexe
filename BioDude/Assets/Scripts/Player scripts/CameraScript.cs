using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraScript : NetworkBehaviour {
    
    public enum Following
    {
        Player,
        Object,
        Point
    }
    Following following;
    public GameObject Player;
    [SerializeField]
    GameObject objectToFollow;
    [SerializeField]
    float StabilisationSpeed = 0.09f;
    [SerializeField]
    float InMovmentSpeed = 0.09f;
    [SerializeField]
    float OutMovmentSpeed = 0.15f;
    [SerializeField]
    bool DoCameraRecoil = true;
    
    public float MinCamZoom = 7;
    public float MaxCamZoom = 10;
    public bool DoDynamicCameraZoom = true;
    [SerializeField]
    float MaxOffset = 10;

    //for TESTING
    [SerializeField]
    float TestingForce = 10;
    /////////

    private Vector2 FollowingPos;
    private Vector2 Offset;

    private void Start()
    {
        //Player = GameObject.Find("player");
        //Player = GameObject.Find("SpawnPoint1");
        following = Following.Player;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        //fot TESTING:
        //Imitate_actions();
        /////////////
        if (Player != null)
        {
            UpdateTargetPosition();
            //following 
            Vector2 cameraPos = transform.position;

            Vector2 LerpPos = new Vector2(0, 0);
            if (following == Following.Player)
            {
                float distance = (cameraPos - (Vector2)Player.transform.position).magnitude;
                if (DoDynamicCameraZoom)
                {
                    gameObject.GetComponent<Camera>().orthographicSize = (1 - distance / MaxOffset) * (MaxCamZoom - MinCamZoom) + MinCamZoom;
                }

                if (distance > Offset.magnitude)
                    LerpPos = Vector2.Lerp(cameraPos, FollowingPos, InMovmentSpeed);
                else
                    LerpPos = Vector2.Lerp(cameraPos, FollowingPos, OutMovmentSpeed);
            }
            else //folowing point or object
            {
                LerpPos = Vector2.Lerp(cameraPos, FollowingPos, InMovmentSpeed);
            }

            transform.position = new Vector3(LerpPos.x, LerpPos.y, -10);
            //Debug.DrawLine(cameraPos, FollowingPos, Color.green, 2); // what camera is fllowing
            //Debug.DrawLine(Player.transform.position, FollowingPos, Color.blue); // offset from player
        }
    }

    //private void Imitate_actions()
    //{
    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        AddOffset(TestingForce);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.G))
    //    {
    //        GoToPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    //    }
    //    else if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        FollowPlayer();
    //    }
    //}

    // Update position of player or other object
    private void UpdateTargetPosition()
    {
        if (following == Following.Player)
        {
            if (DoCameraRecoil)
            {
                Vector2 PlayerPos = Player.transform.position;
                Offset = Vector2.Lerp(Offset, Vector2.zero, StabilisationSpeed);
                FollowingPos = (Vector3) (PlayerPos + Offset);
            }
            else
                FollowingPos = Player.transform.position;
        }
        else if (following == Following.Object)
        {
            FollowingPos = objectToFollow.transform.position;
        }
    }

    // API:

    //Add camera recoil effect
    //magnitutde - how powerfull recoil
    //direction - to what direction (if not defined - oposite to what player is facing) 
    public void AddOffset(float magnitude, Vector2 direction = default(Vector2))
    {
        if (following == Following.Player && DoCameraRecoil) // only if following player with recoil
        {
            if (direction == Vector2.zero)
            {
                float dirAngle = Player.transform.parent.GetComponent<player>().GetDirectionAngle() * Mathf.Deg2Rad;
                direction = new Vector2(-Mathf.Cos(dirAngle), -Mathf.Sin(dirAngle));
            }
            Offset = transform.position - Player.transform.position;
            Offset += (direction * magnitude * (1 -  Vector2.Dot(Offset, direction) / MaxOffset)); // further from player camera is - less powerfull recoil
            Vector2 limit = Offset.normalized * MaxOffset;
            if (Offset.magnitude > limit.magnitude)
            {
                Offset = limit;
            }
        }
    }

    public void GoToPosition(Vector3 position)
    {
        following = Following.Point;
        FollowingPos = position;
    }

    public void FollowPlayer()
    {
        following = Following.Player;
    }

    public void FollowObject(GameObject objectToFollow)
    {
        this.objectToFollow = objectToFollow;
        following = Following.Object;
    }
}
