using UnityEngine;

namespace GUI_scripts
{
    public class DialogueColliderTrigger : MonoBehaviour
    {
        public Dialogue[] dialogue;
        public bool active = true;
        public bool multi_use;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            //Debug.Log("enter");
            if (collision.gameObject.CompareTag("Player"))
                FindObjectOfType<DialogueManager>().StartDialogue(this);
        }
    }
}