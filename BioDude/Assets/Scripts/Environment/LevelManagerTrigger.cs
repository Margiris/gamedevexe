using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LevelManagerTrigger : NetworkBehaviour {

    //public float rotationSpeed = 10;
    public Dialogue[] dialoguetext;
    DialogueColliderTrigger dialogue;
    public LevelManager lvlManager;
    Transform door_one;
    Transform door_two;
    float open = 0.825f;
    public float openSpeed = 1;
    //public bool isDoorOpenVertical = true;

    void Start()
    {
        dialogue = new DialogueColliderTrigger();
        dialogue.dialogue = dialoguetext;
        //lvlManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        door_one = transform.Find("Doors_one");
        door_two = transform.Find("Doors_two");
    }

    private void Update()
    {
        //transform.Rotate(Vector3.forward, 45 * Time.deltaTime * rotationSpeed);
    }
    
    public void OpenExit()
    {
        StartCoroutine(MoveFromTo(door_one, door_one.position, door_one.position + Vector3.down * open, openSpeed));
        StartCoroutine(MoveFromTo(door_two, door_two.position, door_two.position + Vector3.up * open, openSpeed));
    }

    IEnumerator MoveFromTo(Transform objectToMove, Vector3 a, Vector3 b, float speed)
    {
        float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        while (t <= 1.0f)
        {
            t += step; // Goes from 0 to 1, incrementing by step each time
            objectToMove.position = Vector3.Lerp(a, b, t); // Move objectToMove closer to b
            yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
        }
        objectToMove.position = b;
    }

    // Use this for initialization
    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            player Player = other.transform.parent.GetComponent<player>();
            if(Player != null)
            {
                if (lvlManager.clear)
                {
                    Player.SavePlayerStats();
                    lvlManager.LevelCleared(Player);
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
