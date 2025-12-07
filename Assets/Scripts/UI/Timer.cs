using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] float remaining_time;

    private bool running = false;

    public void SetTime(float seconds)
    {
        remaining_time = seconds;
        UpdateText();
    }

    public void StartCountdown()
    {
        running = true;
        gameObject.SetActive(true);
    }

    public void StopCountdown()
    {
        running = false;
    }

    void Update()
    {
        if (!running) return;

        if (remaining_time > 0)
        {
            remaining_time -= Time.deltaTime;
        }
        else
        {
            remaining_time = 0;
            running = false;
            // Avisar al GameManager que se acabó el tiempo
            GameManager.Instance.OnTimeExpired();
        }

        UpdateText();
    }

    private void UpdateText()
    {
        int minutes = Mathf.FloorToInt(remaining_time / 60);
        int seconds = Mathf.FloorToInt(remaining_time % 60);
        if (TimerText != null)
        {
            TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}