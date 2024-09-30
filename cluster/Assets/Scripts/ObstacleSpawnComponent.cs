using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawnComponent : MonoBehaviour
{
    public GameObject prefabObj;
    public float spawnRate = 0;
    public float minRate = 3;
    public float maxRate = 6;

    public float spawnYPos = 0;
    public float spawnMinXPos = 0;
    public float spawnMaxXPos = 0;

    public string spawnname;

    // Start is called before the first frame update
    public void ObstacleSpawnStart()
    {
        StartCoroutine(SpawnCoroutune());
    }

    void Spawn()
    {
        float x = Random.Range(spawnMinXPos, spawnMaxXPos);
        Instantiate(prefabObj, new Vector3(x, spawnYPos, 0), Quaternion.identity);
    }

    IEnumerator SpawnCoroutune()
    {
        while(true)
        {
            if (GameManager.i.IsGameOver)
            {
                Debug.Log(spawnname + " end");
                yield break;
            }

            spawnRate = Random.Range(minRate, maxRate);
            yield return new WaitForSeconds(spawnRate);
            Spawn();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
