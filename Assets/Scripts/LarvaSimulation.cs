using System.Collections.Generic;
using UnityEngine;

public class LarvaSimulation : MonoBehaviour
{
    public GameObject larvaPrefab;

    public int larvaCount = 5;
    public Vector2 spawnArea = new(10, 10);

    public bool autoMove = true;

    public float directionChangeInterval = 5.0f;

    [SerializeField] private float simulationSpeed = 1;

    [SerializeField] private int targetFrameRate = 120;
    [SerializeField] private float fixedDeltaTime = 0.01f;

    private readonly List<Larva> _larvae = new();
    private float _nextDirectionChange;

    private void Start()
    {
        OnValidate();

        SpawnLarvae();

        if (autoMove) StartAllMovement();
    }

    private void Update()
    {
        if (autoMove && Time.time > _nextDirectionChange)
        {
            ChangeRandomDirections();
            _nextDirectionChange = Time.time + directionChangeInterval;
        }

        HandleInput();
    }

    private void OnDrawGizmosSelected()
    {
        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0));
    }

    private void OnValidate()
    {
        Application.targetFrameRate = targetFrameRate;
        Time.fixedDeltaTime = fixedDeltaTime;
        Time.timeScale = simulationSpeed;
    }

    private void SpawnLarvae()
    {
        for (var i = 0; i < larvaCount; i++)
        {
            var spawnPos = new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(-spawnArea.y / 2, spawnArea.y / 2),
                0
            );

            SpawnLarva(spawnPos);
        }
    }

    private void SpawnLarva(Vector2 position)
    {
        var larvaObj = Instantiate(larvaPrefab, position, Quaternion.identity);

        var larva = larvaObj.GetComponent<Larva>();

        _larvae.Add(larva);

        MutateLarva(larva);
    }

    private static void MutateLarva(Larva larva)
    {
        larva.segmentLength *= Random.Range(0.8f, 1.2f);
        larva.headForwardForce *= Random.Range(0.8f, 1.2f);
        larva.dampening *= Random.Range(0.95f, 1.05f);
        larva.restoreForce *= Random.Range(0.8f, 1.2f);
    }

    private void StartAllMovement()
    {
        foreach (var larva in _larvae)
        {
            var randomDir = Random.insideUnitCircle.normalized;
            larva.StartMoving(randomDir);
        }
    }

    private void StopAllMovement()
    {
        foreach (var larva in _larvae) larva.StopMoving();
    }

    private void ChangeRandomDirections()
    {
        foreach (var larva in _larvae)
        {
            var randomDir = Random.insideUnitCircle.normalized;
            larva.SetMovementDirection(randomDir);
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            autoMove = !autoMove;
            if (autoMove)
            {
                StartAllMovement();
                Debug.Log("Auto movement enabled");
            }
            else
            {
                StopAllMovement();
                Debug.Log("Auto movement disabled");
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeRandomDirections();
            Debug.Log("Changed all larvae directions randomly");
        }
    }
}