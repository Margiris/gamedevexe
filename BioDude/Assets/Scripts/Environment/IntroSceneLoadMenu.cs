using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Environment
{
    public class IntroSceneLoadMenu : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
            StartCoroutine(WaitForVideoEnd());
        }

        private IEnumerator WaitForVideoEnd()
        {
            var length = gameObject.GetComponent<VideoPlayer>().clip.length;
        
            yield return new WaitForSeconds((float) length);

            Destroy(GameObject.FindGameObjectWithTag("VideoSurface"));
            SceneManager.LoadScene(1);
        }
    }
}