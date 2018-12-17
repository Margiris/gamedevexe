using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroSceneLoadMenu : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        StartCoroutine(WaitForVideoEnd());
    }

    void Update () {
        if (Input.touchCount > 0)
        {
            LoadSceneMain();
        }
    }

    private IEnumerator WaitForVideoEnd()
    {
        double length = gameObject.GetComponent<VideoPlayer>().clip.length;
        
        yield return new WaitForSeconds((float) length);

        LoadSceneMain();
    }

    private void LoadSceneMain()
    {
        GameObject.Destroy(GameObject.FindGameObjectWithTag("VideoSurface"));
        SceneManager.LoadScene(1);
    }
}