using UnityEngine;

public class FPSLogger : MonoBehaviour
{
    private float timer = 0f;
    private int frames = 0;

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        frames++;

        if (timer >= 1f)
        {
            float fps = frames / timer;
            float frameMs = 1000f / fps;

            Debug.Log("PERFORMANCE; time=" + Time.time.ToString("F1") +
                      "; fps=" + fps.ToString("F1") +
                      "; frameMs=" + frameMs.ToString("F1"));

            timer = 0f;
            frames = 0;
        }
    }
}
