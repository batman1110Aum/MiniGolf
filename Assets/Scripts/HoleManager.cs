using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleManager : MonoBehaviour
{
    public static HoleManager Instance;

    [Header("Hole Settings")]
    [SerializeField] private GameObject holePrefab;
    [SerializeField] private int numberOfHoles = 5;
    [SerializeField] private float minSpawnDistance = 5f;
    [SerializeField] private float maxSpawnDistance = 30f;

    [Header("Color Change Settings")]
    [SerializeField] private float minColorChangeTime = 2f;
    [SerializeField] private float maxColorChangeTime = 5f;

    [Header("Timer Rewards/Penalties")]
    [SerializeField] private float goodHoleTimeBonus = 5f;
    [SerializeField] private float badHoleTimePenalty = 10f;

    private List<HoleController> holes = new List<HoleController>();
    private Coroutine colorChangeCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnHoles();
        StartColorChanging();
    }

    void Update()
    {
        if (GameManager.GameManagerInstance.moveByTouch)
        {
            FreezeAllHoles();
        }
        else
        {
            UnfreezeAllHoles();
        }
    }

    private void SpawnHoles()
    {
        for (int i = 0; i < numberOfHoles; i++)
        {
            Vector3 randomPos = GetRandomPositionOnFloor();

            GameObject hole = Instantiate(holePrefab, randomPos, Quaternion.identity);
            HoleController controller = hole.GetComponent<HoleController>();

            if (controller != null)
            {
                holes.Add(controller);
            }

            Debug.Log($"Spawned hole {i} at position: {randomPos}");
        }

        Debug.Log($"Spawned {holes.Count} holes in random positions");
    }

    private Vector3 GetRandomPositionOnFloor()
    {
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        Vector3 position = new Vector3(
            Mathf.Cos(randomAngle) * randomDistance,
            0.1f,
            Mathf.Sin(randomAngle) * randomDistance
        );

        return position;
    }

    public void RepositionAllHoles()
    {
        Debug.Log("Repositioning all holes to new random positions");

        foreach (var hole in holes)
        {
            if (hole != null)
            {
                Vector3 newPos = GetRandomPositionOnFloor();
                hole.transform.position = newPos;
            }
        }
    }

    private void StartColorChanging()
    {
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }
        colorChangeCoroutine = StartCoroutine(ChangeColorsRoutine());
    }

    private IEnumerator ChangeColorsRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minColorChangeTime, maxColorChangeTime);
            yield return new WaitForSeconds(waitTime);

            foreach (var hole in holes)
            {
                hole.RandomizeColorWithWarning();
            }
        }
    }

    private void FreezeAllHoles()
    {
        foreach (var hole in holes)
        {
            hole.FreezeColor();
        }
    }

    private void UnfreezeAllHoles()
    {
        foreach (var hole in holes)
        {
            hole.UnfreezeColor();
        }
    }

    public void OnBallTouchedHole(HoleController.HoleType type)
    {
        if (type == HoleController.HoleType.Good)
        {
            GameManager.GameManagerInstance.AddTime(goodHoleTimeBonus);
            Debug.Log($"GOOD SHOT! Ball touched BLUE hole! +{goodHoleTimeBonus} seconds");
        }
        else
        {
            GameManager.GameManagerInstance.SubtractTime(badHoleTimePenalty);
            Debug.Log($"BAD SHOT! Ball touched RED hole! -{badHoleTimePenalty} seconds");
        }

        RepositionAllHoles();
    }
}
