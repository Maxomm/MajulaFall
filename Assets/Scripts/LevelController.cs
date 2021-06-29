using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private bool generateNew, reloadLevel;
        [SerializeField] private readonly List<Vector2> levelSeed = new List<Vector2>();
        [SerializeField] private readonly List<GameObject> levelTiles = new List<GameObject>();
        [SerializeField] private float offSet;
        [SerializeField] private int tileAmount;

        [SerializeField] private readonly List<GameObject> tilePrefabs = new List<GameObject>();

        // Start is called before the first frame update
        private void Update()
        {
            if (generateNew)
            {
                CreateLevel();
                generateNew = false;
            }

            if (!reloadLevel) return;
            ReloadLevel();
            reloadLevel = false;
        }

        private void ReloadLevel()
        {
            for (var i = levelTiles.Count - 1; i >= 0; i--) DestroyImmediate(levelTiles[i].gameObject);

            levelTiles.Clear();
            tileAmount = levelSeed.Count;

            for (var i = 0; i < tileAmount; i++)
            {
                var randomTile = (int) levelSeed[i].x;
                var randomRot = levelSeed[i].y;

                var newTile = Instantiate(tilePrefabs[randomTile], Vector3.zero + Vector3.down * offSet * i,
                    Quaternion.Euler(90, 0, 0), transform);
                newTile.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 0, randomRot);
                levelTiles.Add(newTile);
            }
        }

        private void CreateLevel()
        {
            for (var i = levelTiles.Count - 1; i >= 0; i--) DestroyImmediate(levelTiles[i].gameObject);

            levelTiles.Clear();
            levelSeed.Clear();

            for (var i = 0; i < tileAmount; i++)
            {
                var randomTile = Random.Range(0, tilePrefabs.Count);
                float randomRot = Random.Range(0, 360);
                var newTile = Instantiate(tilePrefabs[randomTile], Vector3.zero + Vector3.down * offSet * i,
                    Quaternion.Euler(90, 0, 0), transform);
                newTile.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 0, randomRot);
                levelTiles.Add(newTile);
                levelSeed.Add(new Vector2(randomTile, randomRot));
            }
        }
    }
}