using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject treePrefab;          // The tree prefab
    public int maxTrees = 50;              // Maximum number of trees at a time
    public Vector2 mapMin;                 // Bottom-left corner of the map
    public Vector2 mapMax;                 // Top-right corner of the map
    public float spawnRadius = 1.5f;       // Minimum distance between trees

    [Header("Respawn Settings")]
    public float respawnCooldown = 30f;    // Seconds before a chopped tree spot can respawn

    [Header("Wood Settings")]
    public GameObject woodDropPrefab;      // Prefab for the wood/log dropped when tree is chopped
    public Transform dropPointPrefab;      // Optional: point where wood drops (can be empty child)

    // Tracks where trees were chopped down and their cooldown
    private List<Vector3> occupiedSpots = new List<Vector3>();
    private List<float> spotTimers = new List<float>();

    private List<GameObject> currentTrees = new List<GameObject>();

    private void Start()
    {
        // Initial spawn
        for (int i = 0; i < maxTrees; i++)
        {
            SpawnTree();
        }
    }

    private void Update()
    {
        // Reduce cooldown timers and free spots
        for (int i = spotTimers.Count - 1; i >= 0; i--)
        {
            spotTimers[i] -= Time.deltaTime;
            if (spotTimers[i] <= 0f)
            {
                spotTimers.RemoveAt(i);
                occupiedSpots.RemoveAt(i);
            }
        }

        // Spawn new trees if under max
        while (currentTrees.Count < maxTrees)
        {
            SpawnTree();
        }
    }

    private void SpawnTree()
    {
        Vector3 pos = GetRandomPosition();

        if (pos != Vector3.zero)
        {
            GameObject tree = Instantiate(treePrefab, pos, Quaternion.identity);
            currentTrees.Add(tree);

            // Assign wood drop prefab to the Tree component
            Tree treeScript = tree.GetComponent<Tree>();
            if(treeScript != null)
            {
                treeScript.woodDropPrefab = woodDropPrefab;

                // Optional: assign drop point if defined
                if(dropPointPrefab != null)
                {
                    Transform drop = Instantiate(dropPointPrefab, tree.transform);
                    drop.localPosition = Vector3.zero;
                    treeScript.dropPoint = drop;
                }
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        for (int attempts = 0; attempts < 20; attempts++)
        {
            float x = Random.Range(mapMin.x, mapMax.x);
            float y = Random.Range(mapMin.y, mapMax.y);
            Vector3 pos = new Vector3(x, y, 0f);

            // Check distance to existing trees
            bool tooClose = false;
            foreach (Vector3 spot in occupiedSpots)
            {
                if (Vector3.Distance(pos, spot) < spawnRadius)
                {
                    tooClose = true;
                    break;
                }
            }

            foreach (GameObject tree in currentTrees)
            {
                if (tree != null && Vector3.Distance(pos, tree.transform.position) < spawnRadius)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                return pos;
        }

        return Vector3.zero; // Could not find a valid spot
    }

    // Called by Tree when chopped down
    public void TreeChopped(Vector3 position)
    {
        occupiedSpots.Add(position);
        spotTimers.Add(respawnCooldown);
    }

    // Remove tree from the current list (called when chopped)
    public void RemoveTree(GameObject tree)
    {
        if (currentTrees.Contains(tree))
            currentTrees.Remove(tree);
    }
}
