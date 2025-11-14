using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiceManager : MonoBehaviour
{
    private Rigidbody diceRb;
    private int diceValue;
    private bool ground;
    private float groundTimer = 0f;
    private const float MAX_ROLL_TIME = 5f;
    private const float FALL_LIMIT = -10f;
    private bool hasBeenDestroyed = false;

    [Header("Dice Settings")]
    [SerializeField] private LayerMask lMask;
    [SerializeField] private float radius;
    [SerializeField] private GameObject[] players;
    [SerializeField] private ParticleSystem introParticle;

    [Header("Mode Selection")]
    [SerializeField] private bool isDiceMode = true;

    void Start()
    {
        diceRb = GetComponent<Rigidbody>();
        Debug.Log($"{gameObject.name}: DiceManager started. Mode: {(isDiceMode ? "Dice" : "Golf Ball")}");
    }

    void Update()
    {
        if (hasBeenDestroyed) return;

        if (!diceRb.isKinematic && transform.position.y < FALL_LIMIT)
        {
            Debug.LogWarning($"{gameObject.name}: Fell off the floor! Repositioning holes and preparing next ball");
            HoleManager.Instance?.RepositionAllHoles();
            hasBeenDestroyed = true;
            ProjectileManager.ProjectileManagerInstance.PrepareNextBall();
            Destroy(gameObject);
            return;
        }

        if (isDiceMode && !diceRb.isKinematic && ground && diceValue == 0)
        {
            groundTimer += Time.deltaTime;

            if (diceRb.IsSleeping() || groundTimer >= MAX_ROLL_TIME)
            {
                if (groundTimer >= MAX_ROLL_TIME)
                {
                    Debug.Log($"{gameObject.name}: Took too long to stop, forcing it to sleep");
                    diceRb.linearVelocity = Vector3.zero;
                    diceRb.angularVelocity = Vector3.zero;
                }

                Debug.Log($"{gameObject.name}: Stopped on ground, casting ray to read dice value");
                if (Physics.Raycast(transform.position, Vector3.up, out var hit, Mathf.Infinity, lMask))
                {
                    diceValue = hit.collider.GetComponent<GetValue>().value;

                    Debug.Log($"{gameObject.name}: Dice value is {diceValue}. Starting CreateAgent coroutine");
                    StartCoroutine(CreateAgent());
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: Raycast failed to detect dice value!");
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasBeenDestroyed) return;

        if (other.CompareTag("Hole"))
        {
            HoleController holeController = other.GetComponent<HoleController>();

            if (holeController != null)
            {
                string colorName = holeController.currentType == HoleController.HoleType.Good ? "BLUE (Good)" : "RED (Bad)";
                Debug.Log($"{gameObject.name} touched {colorName} hole!");

                HoleManager.Instance?.OnBallTouchedHole(holeController.currentType);
            }

            Debug.Log($"{gameObject.name}: Destroying and preparing next ball");
            hasBeenDestroyed = true;

            ProjectileManager.ProjectileManagerInstance.PrepareNextBall();
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (hasBeenDestroyed) return;

        Debug.Log($"{gameObject.name}: Collision with {other.collider.name}, tag: {other.collider.tag}");

        if (other.collider.CompareTag("ground") && !ground)
        {
            ground = true;
            groundTimer = 0f;
            Debug.Log($"{gameObject.name}: Ground collision detected! Timer started");
        }
    }

    private IEnumerator CreateAgent()
    {
        var i = 0;

        Debug.Log($"{gameObject.name}: Starting to spawn {diceValue} agents");

        while (i < diceValue)
        {
            float angle = i * Mathf.PI * 2f / diceValue;

            Vector3 newPos = new Vector3(transform.position.x + Mathf.Cos(angle) * radius, 0f, transform.position.z + Mathf.Sin(angle) * radius);

            Instantiate(players[Random.Range(0, 4)], newPos, Quaternion.identity);

            i++;

            yield return new WaitForSecondsRealtime(0.2f);
        }

        Debug.Log($"{gameObject.name}: All agents spawned, waiting 1 second");
        yield return new WaitForSecondsRealtime(1f);

        Debug.Log($"{gameObject.name}: Resetting state");
        diceRb.isKinematic = true;
        diceValue = 0;
        ground = false;
        groundTimer = 0f;

        Debug.Log($"{gameObject.name}: Calling PrepareNextBall");
        ProjectileManager.ProjectileManagerInstance.PrepareNextBall();
    }
}
