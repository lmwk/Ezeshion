using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float frequency = 3f;

    private void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(frequency);

        if (EnemyMob.enemies.Count < EnemyMob.maxEnemies)
        {
            NetworkManager.instance.InstantiateEnemy(transform.position);
        }

        StartCoroutine(SpawnEnemy());
    }
}
