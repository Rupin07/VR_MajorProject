using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoMenu : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "MAIN";

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
