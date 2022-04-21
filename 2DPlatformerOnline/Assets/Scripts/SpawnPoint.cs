using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField]
    GameObject graphics;

    void Awake()
    {
        //Removes the graphic we use to make the spawnpoints. We are only interested in spawning the right place, not the graphic.
        graphics.SetActive(false);
    }
}
