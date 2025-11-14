using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager GameManagerInstance;

    private Vector2 lastMousePosition;
    private GameObject currentLevel;
    private Vector3 _mouseStartPos, playerStartPos;
    private Camera cam;

    [Header("Pointer Settings")]
    [SerializeField][Range(0f, 100f)] public float pointerSpeed;
    [HideInInspector] public Vector3 controller;
    [HideInInspector] public bool moveByTouch, readyToToss;

    [Header("Progress Bar")]
    public Image ProgressBar_Img;
    [SerializeField] private float loadTime;

    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float gameTime = 30f;
    private float currentTime;
    private bool gameActive = true;

    void Start()
    {
        GameManagerInstance = this;
        readyToToss = true;
        cam = Camera.main;
        currentTime = gameTime;
    }

    void Update()
    {
        if (gameActive)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                gameActive = false;
                moveByTouch = false;
                readyToToss = false;
                Debug.Log("GAME OVER! Time's up!");
            }
            UpdateTimerUI();
        }

        if (!gameActive) return;

        if (Input.GetMouseButtonDown(0) && readyToToss)
        {
            moveByTouch = true;

            Plane plane = new Plane(Vector3.up, 5f);

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var distance))
            {
                _mouseStartPos = ray.GetPoint(distance);
                playerStartPos = transform.position;
            }
        }

        if (moveByTouch)
        {
            Plane plane = new Plane(Vector3.up, 5f);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var distance))
            {
                Vector3 mousePos = ray.GetPoint(distance);
                Vector3 move = mousePos - _mouseStartPos;
                controller = playerStartPos + move;
                controller.x = Mathf.Clamp(controller.x, -7f, 7f);
                controller.z = Mathf.Clamp(controller.z, -18.75f, 18.75f);
                transform.position = Vector3.MoveTowards(transform.position, controller, Time.deltaTime * pointerSpeed);
            }
        }

        if (!readyToToss)
        {
            Invoke("FillProgressBar", 1f);
        }
    }

    private void FillProgressBar()
    {
        if (!gameActive) return;

        if (ProgressBar_Img.fillAmount != 1f)
        {
            ProgressBar_Img.fillAmount += loadTime * Time.deltaTime;
        }
        else
        {
            readyToToss = true;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (currentTime <= 10f && currentTime > 0f)
            {
                timerText.color = Color.red;
            }
        }
    }

    public void AddTime(float seconds)
    {
        currentTime += seconds;
        Debug.Log($"Added {seconds} seconds! Current time: {currentTime:F1}");
    }

    public void SubtractTime(float seconds)
    {
        currentTime -= seconds;
        if (currentTime < 0f) currentTime = 0f;
        Debug.Log($"Subtracted {seconds} seconds! Current time: {currentTime:F1}");
    }

    public bool IsGameActive()
    {
        return gameActive;
    }
}
