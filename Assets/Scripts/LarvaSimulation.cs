using System.Collections.Generic;
using Drugs;
using Larvae;
using UnityEngine;

public class LarvaSimulation : MonoBehaviour
{
    public GameObject larvaPrefab;

    public int larvaCount = 5;
    public Vector2 spawnArea = new(10, 10);

    public bool autoMove = true;

    [SerializeField] private float simulationSpeed = 1;

    [SerializeField] private int targetFrameRate = 120;
    [SerializeField] private float fixedDeltaTime = 0.01f;

    [SerializeField] private bool mutateLarva;

    [Header("Drug Testing")]
    [SerializeField] private CocaineEffect cocaineEffect;

    [SerializeField] private float drugDosage = 1f;

    [SerializeField] private Transform larvaeParent;
    [SerializeField] private Transform larvaeSegmentParent;

    private readonly List<Larva> _larvae = new();
    private Camera _camera;
    private float _nextDirectionChange;
    public IReadOnlyList<Larva> Larvae => _larvae;

    private void Start()
    {
        _camera = Camera.main;
        OnValidate();

        SpawnLarvae();

        if (autoMove) StartAllMovement();

        Application.runInBackground = true;
    }

    private void Update()
    {
        HandleInput();
    }

    private void OnDrawGizmosSelected()
    {
        DrawSpawnAreaGizmos();
    }

    private void OnValidate()
    {
        Application.targetFrameRate = targetFrameRate;
        Time.fixedDeltaTime = fixedDeltaTime;
        SetSimulationSpeed(simulationSpeed);
    }

    private void DrawSpawnAreaGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0));
    }

    public void OnSimulationSpeedChanged(float newValue)
    {
        SetSimulationSpeed(newValue);
    }

    private static void SetSimulationSpeed(float newSimulationSpeed)
    {
        Time.timeScale = newSimulationSpeed;
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
        var larvaObj = Instantiate(larvaPrefab, position, Quaternion.identity, larvaeParent);

        var larva = larvaObj.GetComponent<Larva>();

        larva.Initialize(larvaeSegmentParent);

        _larvae.Add(larva);

        if (mutateLarva) MutateLarva(larva);
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

        // Drug testing controls
        if (Input.GetKeyDown(KeyCode.C))
        {
            ApplyCocaineToAllLarvae();
            Debug.Log($"Applied cocaine (dosage: {drugDosage}) to all larvae");
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ClearAllDrugsFromLarvae();
            Debug.Log("Cleared all drugs from all larvae");
        }

        if (Input.GetMouseButton(1))
        {
            var mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            foreach (var larva in _larvae)
            {
                var larvaPos = larva.GetCenter();
                var directionToMouse = ((Vector2)mouseWorldPos - larvaPos).normalized;
                larva.SetMovementDirection(directionToMouse);
            }
        }
    }

    private void ApplyCocaineToAllLarvae()
    {
        if (!cocaineEffect)
        {
            Debug.LogError("CocaineEffect not assigned! Please assign it in the inspector.");
            return;
        }

        foreach (var larva in _larvae)
            if (larva)
                larva.AddDrugEffect(cocaineEffect, drugDosage);
    }

    private void ClearAllDrugsFromLarvae()
    {
        foreach (var larva in _larvae)
            if (larva)
                larva.ClearAllDrugEffects();
    }
}