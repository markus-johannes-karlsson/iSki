using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loops the 3 tree containers.
/// </summary>
public class MapController : MonoBehaviour {

    public List<Transform> pools;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        foreach(Transform pool in transform)
        {
            pools.Add(pool);
        }

        GenerateObstacles(0);
        GenerateObstacles(2);
    }

    private void Update()
    {
        for(int i=0; i<pools.Count; i++)
        {
            // Move pool to bottom if it's out of screen
            if(pools[i].position.y - player.position.y > 30.0f) {
                pools[i].position = new Vector3(0.0f, transform.GetChild(2).position.y - 28.0f, 0.0f);
                pools[i].SetAsLastSibling();

                GenerateObstacles(i);
            }
        }
    }

    /// <summary>
    /// Generates different set of trees each time.
    /// </summary>
    private void GenerateObstacles(int poolIndex)
    {
        int r = Random.Range(0, 1000);
        int poolDifficulty = r > 950 ? 1 : r > 850 ? 2 : r > 700 ? 3 : r > 500 ? 4 : 5;
        int counter = 0;

        foreach (Transform obstacle in pools[poolIndex])
        {
            if (counter % poolDifficulty != 0)
            {
                obstacle.gameObject.SetActive(false);
            }
            else
            {
                obstacle.gameObject.SetActive(true);
            }

            counter++;
        }
    }
}
