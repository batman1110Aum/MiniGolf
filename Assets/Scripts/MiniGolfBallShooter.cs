using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGolfBallShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float maxPower = 15f;
    public float powerMultiplier = 8f;
    public float minPower = 2f;

    [Header("Trajectory Settings")]
    public GameObject dotPrefab;
    public int dotCount = 20;
    public float dotSpacing = 0.3f;
    public LayerMask collisionMask;

    [Header("Respawn Settings")]
    public float respawnTime = 5f;
    public Vector3 respawnPosition = new Vector3(0, 0.5f, 0);

    private Rigidbody rb;
    private bool isAiming = false;
    private bool isMoving = false;
    private Vector3 startTouchPosition;
    private Vector3 currentTouchPosition;
    private List<GameObject> trajectoryDots = new List<GameObject>();
    private Camera mainCamera;
    private Coroutine respawnCoroutine;
    private bool canShoot = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        // Create trajectory dots
        CreateTrajectoryDots();
        HideTrajectoryDots();

        // Set initial position
        transform.position = respawnPosition;
    }

    void Update()
    {
        HandleInput();

        // Check if ball has stopped moving
        if (isMoving && rb.linearVelocity.magnitude < 0.1f)
        {
            BallStopped();
        }
    }

    private void HandleInput()
    {
        if (!canShoot || isMoving) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartAiming(touch.position);
                    break;

                case TouchPhase.Moved:
                    UpdateAiming(touch.position);
                    break;

                case TouchPhase.Ended:
                    EndAiming(touch.position);
                    break;

                case TouchPhase.Canceled:
                    CancelAiming();
                    break;
            }
        }

        // Mouse input for testing in editor
#if UNITY_EDITOR
        if (canShoot && !isMoving)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartAiming(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && isAiming)
            {
                UpdateAiming(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && isAiming)
            {
                EndAiming(Input.mousePosition);
            }
        }
#endif
    }

    private void StartAiming(Vector2 screenPosition)
    {
        isAiming = true;
        startTouchPosition = GetWorldTouchPosition(screenPosition);
        ShowTrajectoryDots();
    }

    private void UpdateAiming(Vector2 screenPosition)
    {
        if (!isAiming) return;

        currentTouchPosition = GetWorldTouchPosition(screenPosition);
        UpdateTrajectoryDots();
    }

    private void EndAiming(Vector2 screenPosition)
    {
        if (!isAiming) return;

        Vector3 endPosition = GetWorldTouchPosition(screenPosition);
        ShootBall(endPosition);

        isAiming = false;
        HideTrajectoryDots();
    }

    private void CancelAiming()
    {
        isAiming = false;
        HideTrajectoryDots();
    }

    private Vector3 GetWorldTouchPosition(Vector2 screenPosition)
    {
        // Create a plane at the ball's height
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // Fallback: use a fixed distance if plane raycast fails
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
    }

    private void ShootBall(Vector3 endPosition)
    {
        Vector3 direction = (startTouchPosition - endPosition).normalized;
        float distance = Vector3.Distance(startTouchPosition, endPosition);
        float power = Mathf.Clamp(distance * powerMultiplier, minPower, maxPower);

        rb.AddForce(direction * power, ForceMode.Impulse);
        isMoving = true;
        canShoot = false;

        Debug.Log($"Shot ball with power: {power}, direction: {direction}");

        // Start respawn timer
        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);
        respawnCoroutine = StartCoroutine(RespawnAfterTime());
    }

    private void BallStopped()
    {
        isMoving = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private IEnumerator RespawnAfterTime()
    {
        yield return new WaitForSeconds(respawnTime);
        RespawnBall();
    }

    private void RespawnBall()
    {
        // Stop any movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset position
        transform.position = respawnPosition;

        // Reset rotation
        transform.rotation = Quaternion.identity;

        // Enable shooting
        canShoot = true;
        isMoving = false;
        isAiming = false;

        Debug.Log("Ball respawned at start position");
    }

    // Public method to manually trigger respawn (useful for holes or obstacles)
    public void ForceRespawn()
    {
        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);

        RespawnBall();
    }

    // Public method to respawn immediately without waiting
    public void RespawnImmediately()
    {
        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);

        RespawnBall();
    }

    private void CreateTrajectoryDots()
    {
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, transform.position, Quaternion.identity);
            dot.transform.localScale = Vector3.one * 0.1f;
            trajectoryDots.Add(dot);
        }
    }

    private void UpdateTrajectoryDots()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = currentTouchPosition;

        // Calculate shot direction and power
        Vector3 shotDirection = (startTouchPosition - endPos).normalized;
        float distance = Vector3.Distance(startTouchPosition, endPos);
        float power = Mathf.Clamp(distance * powerMultiplier, minPower, maxPower);

        // Update each dot position along the trajectory
        for (int i = 0; i < trajectoryDots.Count; i++)
        {
            float time = i * dotSpacing;
            Vector3 dotPosition = CalculateTrajectoryPoint(startPos, shotDirection * power, time);

            trajectoryDots[i].transform.position = dotPosition;

            // Check for collisions and hide dots beyond collision point
            if (i > 0)
            {
                Vector3 prevDotPosition = trajectoryDots[i - 1].transform.position;
                if (Physics.Linecast(prevDotPosition, dotPosition, out RaycastHit hit, collisionMask))
                {
                    trajectoryDots[i].transform.position = hit.point;
                    // Hide remaining dots
                    for (int j = i + 1; j < trajectoryDots.Count; j++)
                    {
                        trajectoryDots[j].SetActive(false);
                    }
                    return;
                }
            }

            trajectoryDots[i].SetActive(true);
        }
    }

    private Vector3 CalculateTrajectoryPoint(Vector3 startPos, Vector3 initialVelocity, float time)
    {
        // Simple trajectory calculation (you can add gravity later if needed)
        return startPos + initialVelocity * time;
    }

    private void ShowTrajectoryDots()
    {
        foreach (GameObject dot in trajectoryDots)
        {
            dot.SetActive(true);
        }
    }

    private void HideTrajectoryDots()
    {
        foreach (GameObject dot in trajectoryDots)
        {
            dot.SetActive(false);
        }
    }

    // Public properties to check ball state
    public bool CanShoot => canShoot;
    public bool IsMoving => isMoving;
    public bool IsAiming => isAiming;

    void OnDestroy()
    {
        // Clean up trajectory dots
        foreach (GameObject dot in trajectoryDots)
        {
            if (dot != null)
                Destroy(dot);
        }
        trajectoryDots.Clear();

        // Stop coroutines
        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);
    }
}