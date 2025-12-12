using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] float remaining_time;

    [Header("Opciones")]
    public bool useUnscaledTime = false;
    public Action onExpired;  // callback opcional

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

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (remaining_time > 0)
        {
            remaining_time -= dt;
        }
        else
        {
            remaining_time = 0;
            running = false;
            if (onExpired != null) onExpired.Invoke();
            else GameManager.Instance?.OnTimeExpired();
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