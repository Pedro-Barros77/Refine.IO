using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class GameController : MonoBehaviour
{

    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private int enemyCount, enemyMax;
    [SerializeField] private float delayToSpawn;
    public int EnemyCount { get { return enemyCount; } }
    DateTime LastSpawn;

    Color32[] EnemyColors, TurretColors;

    Transform EnemiesHolder;

    void Start()
    {
        LastSpawn = DateTime.Now;
        EnemiesHolder = GameObject.Find("Environment").transform.Find("Enemies");

        #region colors
        EnemyColors = new Color32[]
        {
            new Color32(171,22,44,255), //Red
            new Color32(22,26,171,255), //Blue
            new Color32(179,119,16,255), //Orange
            new Color32(121,16,178,255), //Purple
            new Color32(16,176,178,255), // Cyan
            new Color32(17,17,17,255) // Black
        };

        TurretColors = new Color32[]
        {
            new Color32(128,26,38,255), //Red
            new Color32(26,22,128,255), //Blue
            new Color32(130,100,20,255), //Orange
            new Color32(110,20,130,255), //Purple
            new Color32(20,130,130,255), // Cyan
            new Color32(5,5,5,255) // Black
        };
        #endregion
    }

    void Update()
    {
        if (LastSpawn.AddMilliseconds(delayToSpawn) < DateTime.Now && enemyCount < enemyMax)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        int randomSpawnZone = UnityEngine.Random.Range(0, 4);
        float randomXposition = 0, randomYposition = 0;

        switch (randomSpawnZone)
        {
            case 0:
                randomXposition = UnityEngine.Random.Range(-20f, 13f);
                randomYposition = UnityEngine.Random.Range(20f, 13f);
                break;
            case 1:
                randomXposition = UnityEngine.Random.Range(13f, 20f);
                randomYposition = UnityEngine.Random.Range(-7f, 7f);
                break;
            case 2:
                randomXposition = UnityEngine.Random.Range(-20f, 13f);
                randomYposition = UnityEngine.Random.Range(-13f, 20f);
                break;
            case 3:
                randomXposition = UnityEngine.Random.Range(-13, -20f);
                randomYposition = UnityEngine.Random.Range(-7f, 7f);
                break;
        }

        var spawnPosition = new Vector2(randomXposition, randomYposition);
        var entities = WorldBuilder.WorldEntities;
        if (entities.Any(e => Vector3.Distance(spawnPosition, e.transform.position) < 2))
        {
            SpawnEnemy();
            return;
        }

        var enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, EnemiesHolder);
        int randomColor = UnityEngine.Random.Range(0, 6);
        var bodyColor = enemy.transform.Find("Tank").Find("Body_Color").GetComponent<SpriteRenderer>();
        var turretColor = enemy.transform.Find("Tank").Find("Turret").Find("Turret_Color").GetComponent<SpriteRenderer>();
        bodyColor.color = EnemyColors[randomColor];
        turretColor.color = TurretColors[randomColor];
        LastSpawn = DateTime.Now;

        enemyCount++;
    }

    public void KillEnemy()
    {
        enemyCount--;
    }
}
