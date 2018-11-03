using UnityEngine;

namespace GUI_scripts
{
    public class DialogueTrigger : MonoBehaviour
    {
        public void TriggerDialogue()
        {
            //FindObjectOfType<DialogueManager>().StartDialogue();
            Destroy(gameObject);
        }
    }
}