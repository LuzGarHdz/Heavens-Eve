using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] float remaining_time;
    void Update()
    {
        if (remaining_time > 0)
        {
            remaining_time -= Time.deltaTime;
        }
        else if (remaining_time < 0)
        {
            remaining_time = 0;
        }

        int minutes = Mathf.FloorToInt(remaining_time / 60);
        int seconds= Mathf.FloorToInt(remaining_time % 60);
        TimerText.text=string.Format("{0:00}:{1:00}",minutes,seconds);
        

    }
}
