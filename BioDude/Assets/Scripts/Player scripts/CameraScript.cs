using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable StringLiteralTypo

namespace Player_scripts
{
    public class CameraScript : MonoBehaviour
    {
        private enum Following
        {
            Player,
            Object,
            Point
        }

        private Following following;
        private GameObject Player;
        [SerializeField] private GameObject objectToFollow;
        [SerializeField] private float StabilisationSpeed = 0.09f;

        [FormerlySerializedAs("InMovmentSpeed")] [SerializeField]
        private float InMovementSpeed = 0.09f;

        [FormerlySerializedAs("OutMovmentSpeed")] [SerializeField]
        private float OutMovementSpeed = 0.15f;

        [SerializeField] private bool DoCameraRecoil = true;

        public float MinCamZoom = 7;
        public float MaxCamZoom = 10;
        public bool DoDynamicCameraZoom = true;
        [SerializeField] private float MaxOffset = 10;

        private Vector2 FollowingPos;
        private Vector2 Offset;

        private void Start()
        {
            Player = GameObject.Find("player");
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            //fot TESTING:
            //Imitate_actions();
            /////////////

            UpdateTargetPosition();
            //following 
            Vector2 cameraPos = transform.position;

            Vector2 Pos;
            if (following == Following.Player)
            {
                var distance = (cameraPos - (Vector2) Player.transform.position).magnitude;
                if (DoDynamicCameraZoom)
                {
                    gameObject.GetComponent<Camera>().orthographicSize =
                        (1 - distance / MaxOffset) * (MaxCamZoom - MinCamZoom) + MinCamZoom;
                }

                Pos = Vector2.Lerp(cameraPos, FollowingPos,
                    distance > Offset.magnitude ? InMovementSpeed : OutMovementSpeed);
            }
            else //following point or object
            {
                Pos = Vector2.Lerp(cameraPos, FollowingPos, InMovementSpeed);
            }

            transform.position = new Vector3(Pos.x, Pos.y, -10);
            //Debug.DrawLine(cameraPos, FollowingPos, Color.green, 2); // what camera is fllowing
            //Debug.DrawLine(Player.transform.position, FollowingPos, Color.blue); // offset from player
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
        //magnitude - how powerful recoil
        //direction - to what direction (if not defined - opposite to what player is facing) 
        public void AddOffset(float magnitude, Vector2 direction = default(Vector2))
        {
            if (following != Following.Player || !DoCameraRecoil) return;
            if (direction == Vector2.zero)
            {
                var dirAngle = Player.GetComponent<player>().GetDirectionAngle() * Mathf.Deg2Rad;
                direction = new Vector2(-Mathf.Cos(dirAngle), -Mathf.Sin(dirAngle));
            }

            Offset = transform.position - Player.transform.position;
            Offset += (direction * magnitude *
                       (1 - Vector2.Dot(Offset, direction) / MaxOffset)
                ); // further from player camera is - less powerful recoil
            var limit = Offset.normalized * MaxOffset;
            if (Offset.magnitude > limit.magnitude)
            {
                Offset = limit;
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

        public void FollowObject(GameObject objectToFollow1)
        {
            objectToFollow = objectToFollow1;
            following = Following.Object;
        }
    }
}