using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager ProjectileManagerInstance;

    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int lineSegment = 50;
    [SerializeField] private float PointerHight;
    public Transform BallInitialPosition;

    private GameObject Pointer;
    private Rigidbody currentBall;
    private bool isFirstShot = true;
    private int ballCount = 0;

    void Awake()
    {
        ProjectileManagerInstance = this;
    }

    void Start()
    {
        Pointer = GameObject.FindWithTag("Player");
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = lineSegment + 1;

        SpawnNewBall();
        ShowCurrentBall();
        Debug.Log("First ball spawned and ready to shoot");
    }

    void Update()
    {
        if (!GameManager.GameManagerInstance.IsGameActive())
        {
            if (lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
            }
            return;
        }

        if (currentBall == null)
        {
            Debug.Log("Current ball is destroyed, waiting for progress bar");
            return;
        }

        if (GameManager.GameManagerInstance.readyToToss && !isFirstShot && currentBall.transform.position.y < -100f)
        {
            Debug.Log("Progress bar filled! Showing ball");
            ShowCurrentBall();
        }

        if (GameManager.GameManagerInstance.moveByTouch && GameManager.GameManagerInstance.readyToToss && currentBall != null)
        {
            UpdateProjectile();
        }
    }

    private void SpawnNewBall()
    {
        GameObject newBallObject = Instantiate(ballPrefab, new Vector3(0, -1000, 0), Quaternion.identity);
        currentBall = newBallObject.GetComponent<Rigidbody>();

        if (currentBall != null)
        {
            currentBall.isKinematic = true;
            currentBall.collisionDetectionMode = CollisionDetectionMode.Continuous;
            ballCount++;
            Debug.Log($"Ball #{ballCount} spawned");
        }
        else
        {
            Debug.LogError("Ball prefab doesn't have a Rigidbody component!");
        }
    }

    private void ShowCurrentBall()
    {
        if (currentBall != null)
        {
            currentBall.transform.position = BallInitialPosition.position;
            currentBall.transform.rotation = Quaternion.identity;
            Debug.Log($"Ball #{ballCount} is now visible and ready to shoot");
        }
    }

    public void PrepareNextBall()
    {
        if (!GameManager.GameManagerInstance.IsGameActive())
        {
            Debug.Log("Game over! No more balls will spawn");
            return;
        }

        isFirstShot = false;
        Debug.Log("PrepareNextBall called! Spawning new ball...");

        SpawnNewBall();
    }

    private void UpdateProjectile()
    {
        Vector3 velocity = CalculateVelocity(Pointer.transform.position, BallInitialPosition.position, PointerHight);

        Visualize(velocity, Pointer.transform.position);

        transform.rotation = Quaternion.LookRotation(velocity);

        lineRenderer.enabled = true;

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log($"Ball #{ballCount} shot! Setting isKinematic to false");
            lineRenderer.enabled = false;
            currentBall.isKinematic = false;
            currentBall.linearVelocity = velocity;
            GameManager.GameManagerInstance.moveByTouch = false;
            GameManager.GameManagerInstance.readyToToss = false;

            GameManager.GameManagerInstance.ProgressBar_Img.fillAmount = 0f;
        }
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float height)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXz = distance;
        distanceXz.y = 0f;

        float sY = distance.y;
        float sXz = distanceXz.magnitude;

        float vxz = sXz / height;
        float vy = sY / height + 0.5f * Mathf.Abs(Physics.gravity.y) * height;

        Vector3 result = distanceXz.normalized;
        result *= vxz;
        result.y = vy;

        return result;
    }

    void Visualize(Vector3 vo, Vector3 finalPos)
    {
        for (int i = 0; i < lineSegment; i++)
        {
            Vector3 pos = CalculatePosInTime(vo, i / (float)lineSegment * PointerHight);
            lineRenderer.SetPosition(i, pos);
        }

        lineRenderer.SetPosition(lineSegment, finalPos);
    }

    Vector3 CalculatePosInTime(Vector3 vo, float height)
    {
        Vector3 Vxz = vo;
        Vxz.y = 0f;

        Vector3 result = BallInitialPosition.position + vo * height;
        float sY = -0.5f * Mathf.Abs(Physics.gravity.y) * (height * height) + vo.y * height + BallInitialPosition.position.y;

        result.y = sY;

        return result;
    }
}
