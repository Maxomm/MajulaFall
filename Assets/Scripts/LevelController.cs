using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelController : MonoBehaviour
{
    [SerializeField] private int tileAmount;
    [SerializeField] private float offSet;
    [SerializeField] private List<GameObject> tilePrefabs = new List<GameObject>();
    [SerializeField] private bool generateNew,reloadLevel;
    [SerializeField] private List<GameObject> levelTiles = new List<GameObject>();
    [SerializeField] private List<Vector2> levelSeed = new List<Vector2>();
    // Start is called before the first frame update
    void Update()
    {
        if (generateNew)
        {
            CreateLevel();
            generateNew = false;
        }

        if (reloadLevel)
        {
            ReloadLevel();
            reloadLevel = false;
        }

       

    }

    void ReloadLevel()
    {
    
        for (int i = 0; i < levelTiles.Count; i++)
        {
            DestroyImmediate(levelTiles[i].gameObject);
        }

        levelTiles.Clear();
        tileAmount = levelSeed.Count;

        for (int i = 0; i < tileAmount; i++)
        {
            int randomTile = (int)levelSeed[i].x;
            float randomRot = levelSeed[i].y;
          
            GameObject newTile = Instantiate(tilePrefabs[randomTile], Vector3.zero + (Vector3.down * offSet * i), Quaternion.Euler(90, 0, 0),transform);
            newTile.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 0, randomRot);
            levelTiles.Add(newTile);
            
        }
    }

    void CreateLevel()
    {

        

        for(int i = 0; i < levelTiles.Count; i++)
        {
            DestroyImmediate(levelTiles[i].gameObject);
        }

        levelTiles.Clear();
        levelSeed.Clear();

        for (int i = 0; i < tileAmount; i++)
        {
            int randomTile = Random.Range(0, tilePrefabs.Count);
            float randomRot = Random.Range(0, 360);
            GameObject newTile = Instantiate(tilePrefabs[randomTile], Vector3.zero + (Vector3.down * offSet*i),Quaternion.Euler(90,0,0),transform);
            newTile.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 0, randomRot);
            levelTiles.Add(newTile);
            levelSeed.Add(new Vector2(randomTile, randomRot));
        }
    }
}
