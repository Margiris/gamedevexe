using System.Collections;
using GUI_scripts;
using Player_scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment
{
    public class LevelManagerTrigger : MonoBehaviour
    {
        //public float rotationSpeed = 10;
        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("dialoguetext")] public Dialogue[] DialogueText;
        private DialogueColliderTrigger dialogue;
        private LevelManager lvlManager;
        private Transform door_one;
        private Transform door_two;
        private const float open = 0.825f;

        public float openSpeed = 1;
        //public bool isDoorOpenVertical = true;

        private void Start()
        {
            dialogue = gameObject.AddComponent<DialogueColliderTrigger>();
            dialogue.dialogue = DialogueText;
            lvlManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
            door_one = transform.Find("Doors_one");
            door_two = transform.Find("Doors_two");
        }

        public void OpenExit()
        {
            StartCoroutine(MoveFromTo(door_one, door_one.position, door_one.position + Vector3.down * open, openSpeed));
            StartCoroutine(MoveFromTo(door_two, door_two.position, door_two.position + Vector3.up * open, openSpeed));
        }

        private static IEnumerator MoveFromTo(Transform objectToMove, Vector3 a, Vector3 b, float speed)
        {
            var step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
            float t = 0;
            while (t <= 1.0f)
            {
                t += step; // Goes from 0 to 1, incrementing by step each time
                objectToMove.position = Vector3.Lerp(a, b, t); // Move objectToMove closer to b
                yield return new WaitForFixedUpdate(); // Leave the routine and return here in the next frame
            }

            objectToMove.position = b;
        }

        // Use this for initialization
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var Player = other.GetComponent<player>();
            if (Player != null)
            {
                if (lvlManager.clear)
                {
                    Player.SavePlayerStats();
                    lvlManager.LevelCleared();
                }
                else
                {
                    dialogue.active = true;
                    FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
                }
            }
            else
                Debug.Log("Character script not found on player");
        }
    }
}