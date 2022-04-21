using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{

    //Singeton part 1
    public static SpawnManager Instance;

    SpawnPoint[] spawnpoints;

    private void Awake()
    {
        //Singleton part 2
        Instance = this;
        spawnpoints = GetComponentsInChildren<SpawnPoint>();
    }

    public Transform GetSpawnPoint()
    {
        return spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
    }
}
