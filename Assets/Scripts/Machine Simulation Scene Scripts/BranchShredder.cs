using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchShredder : MonoBehaviour
{
    [Header("Shredding Settings")]
    public Transform shredderBladesRotor;
    public float bladeRotationSpeed = 1000f;
    public Transform woodChipSpawnPoint;
    public Transform woodChipContainer;

    [Header("Effects")]
    public ParticleSystem shredEffect;
    public AudioClip shredSound;

    [Header("Prefabs")]
    public GameObject woodChipPrefab;
    public int chipsPerCut = 3;
    public float fallSpeed = 2f;

    [Header("Shredding Parameters")]
    public float cutInterval = 0.3f;
    public float cutDistance = 0.2f;
    public int maxWoodChips = 100;

    [Header("Progressive Scaling")]
    public float scaleReductionSpeed = 0.5f;
    public float minScale = 0.1f;
    public float movementSpeed = 0.5f;

    [Header("Chip Spawning")]
    public Vector3 chipSpawnOffset = new Vector3(0, -0.2f, 0);
    public float chipSpawnRadius = 0.3f;
    public bool spawnAtShredderOutput = true;

    [Header("Destruction Settings")]
    public string destructionColliderTag = "DestructionZone";
    public bool destroyOnAnyCollision = false;

    private AudioSource audioSource;
    private Dictionary<GameObject, float> branchesInShredder = new Dictionary<GameObject, float>();
    private HashSet<GameObject> currentlyShredding = new HashSet<GameObject>();
    private List<GameObject> woodChipsList = new List<GameObject>();
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, float> branchProgress = new Dictionary<GameObject, float>();

    [Header("Branch Tracking")]
    public int totalBranches = 5;
    public int shreddedBranches = 0;
    public bool AllBranchesShredded => shreddedBranches >= totalBranches;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (shredderBladesRotor != null)
        {
            shredderBladesRotor.Rotate(Vector3.up, bladeRotationSpeed * Time.deltaTime);
        }

        ProcessShredding();
        UpdateProgressiveScaling();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Branch") && !branchesInShredder.ContainsKey(other.gameObject))
        {
            branchesInShredder.Add(other.gameObject, 0f);
            branchProgress.Add(other.gameObject, 0f);

            if (!originalScales.ContainsKey(other.gameObject))
            {
                originalScales.Add(other.gameObject, other.transform.localScale);
            }
        }

        HandleDestructionCollision(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Branch") && branchesInShredder.ContainsKey(other.gameObject))
        {
            branchesInShredder.Remove(other.gameObject);
            branchProgress.Remove(other.gameObject);

            if (originalScales.ContainsKey(other.gameObject))
            {
                other.transform.localScale = originalScales[other.gameObject];
                originalScales.Remove(other.gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleDestructionCollision(collision.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleDestructionCollision(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        HandleDestructionCollision(collider.gameObject);
    }

    void HandleDestructionCollision(GameObject collidedObject)
    {
        if (collidedObject.CompareTag("Branch"))
        {
            if (destroyOnAnyCollision)
            {
                DestroyBranchImmediately(collidedObject);
                return;
            }
        }

        if (collidedObject.CompareTag(destructionColliderTag))
        {
            foreach (var branch in new List<GameObject>(branchesInShredder.Keys))
            {
                if (branch != null && Vector3.Distance(branch.transform.position, collidedObject.transform.position) < 2.0f)
                {
                    DestroyBranchImmediately(branch);
                    break;
                }
            }
        }
    }

    void DestroyBranchImmediately(GameObject branch)
    {
        Debug.Log("Branch destroyed by collision: " + branch.name);

        if (shredEffect != null)
        {
            shredEffect.transform.position = branch.transform.position;
            shredEffect.Play();
        }

        if (shredSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shredSound);
        }

        for (int i = 0; i < chipsPerCut * 2; i++)
        {
            SpawnWoodChip(branch.transform.position);
        }

        if (branchesInShredder.ContainsKey(branch))
            branchesInShredder.Remove(branch);
        if (branchProgress.ContainsKey(branch))
            branchProgress.Remove(branch);
        if (originalScales.ContainsKey(branch))
            originalScales.Remove(branch);

        shreddedBranches++;
        Destroy(branch);
    }

    void ProcessShredding()
    {
        List<GameObject> branchesToProcess = new List<GameObject>(branchesInShredder.Keys);

        foreach (GameObject branch in branchesToProcess)
        {
            if (branch == null || currentlyShredding.Contains(branch))
                continue;

            branchesInShredder[branch] += Time.deltaTime;
            if (branchesInShredder[branch] >= cutInterval)
            {
                StartCoroutine(ShredBranchPiece(branch));
                branchesInShredder[branch] = 0f;
            }
        }
    }

    void UpdateProgressiveScaling()
    {
        foreach (GameObject branch in new List<GameObject>(branchProgress.Keys))
        {
            if (branch == null) continue;

            branchProgress[branch] += scaleReductionSpeed * Time.deltaTime;
            ApplyProgressiveScale(branch, branchProgress[branch]);
            MoveBranchTowardsShredder(branch, branchProgress[branch]);

            if (branchProgress[branch] >= 1f)
            {
                DestroyBranch(branch);
            }
        }
    }

    void ApplyProgressiveScale(GameObject branch, float progress)
    {
        if (originalScales.ContainsKey(branch))
        {
            Vector3 originalScale = originalScales[branch];
            float scaleFactor = Mathf.Lerp(1f, minScale, progress);
            branch.transform.localScale = originalScale * scaleFactor;
        }
    }

    void MoveBranchTowardsShredder(GameObject branch, float progress)
    {
        Vector3 directionToShredder = (transform.position - branch.transform.position).normalized;
        float moveDistance = movementSpeed * Time.deltaTime * progress;
        branch.transform.position += directionToShredder * moveDistance;
    }

    IEnumerator ShredBranchPiece(GameObject branch)
    {
        if (currentlyShredding.Contains(branch))
            yield break;

        currentlyShredding.Add(branch);

        Vector3 spawnPosition = GetOptimalSpawnPosition(branch);

        if (shredEffect != null)
        {
            shredEffect.transform.position = spawnPosition;
            shredEffect.Play();
        }

        if (shredSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shredSound);
        }

        for (int i = 0; i < chipsPerCut; i++)
        {
            SpawnWoodChip(spawnPosition);
            yield return new WaitForSeconds(0.1f);
        }

        currentlyShredding.Remove(branch);
    }

    Vector3 GetOptimalSpawnPosition(GameObject branch)
    {
        if (spawnAtShredderOutput && woodChipSpawnPoint != null)
        {
            return woodChipSpawnPoint.position + chipSpawnOffset;
        }
        else
        {
            Vector3 branchPos = branch.transform.position;
            Vector3 randomOffset = new Vector3(
                Random.Range(-chipSpawnRadius, chipSpawnRadius),
                chipSpawnOffset.y,
                Random.Range(-chipSpawnRadius, chipSpawnRadius)
            );
            return branchPos + randomOffset;
        }
    }

    void SpawnWoodChip(Vector3 spawnPosition)
    {
        if (woodChipPrefab == null || woodChipsList.Count >= maxWoodChips)
            return;

        Vector3 finalSpawnPos = spawnPosition + new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.05f, 0.05f),
            Random.Range(-0.1f, 0.1f)
        );

        GameObject woodChip = Instantiate(woodChipPrefab, finalSpawnPos, Random.rotation);
        woodChip.SetActive(true);
        woodChipsList.Add(woodChip);

        Rigidbody rb = woodChip.GetComponent<Rigidbody>();
        if (rb == null) rb = woodChip.AddComponent<Rigidbody>();

        rb.drag = 1f;
        rb.angularDrag = 0.5f;
        rb.useGravity = true;

        rb.velocity = new Vector3(
            Random.Range(-0.2f, 0.2f),
            -fallSpeed * Random.Range(0.8f, 1.2f),
            Random.Range(-0.2f, 0.2f)
        );

        rb.AddTorque(new Vector3(
            Random.Range(-3f, 3f),
            Random.Range(-3f, 3f),
            Random.Range(-3f, 3f)
        ), ForceMode.Impulse);

        if (woodChipContainer != null)
        {
            woodChip.transform.parent = woodChipContainer;
        }
    }

    void DestroyBranch(GameObject branch)
    {
        if (branchesInShredder.ContainsKey(branch))
            branchesInShredder.Remove(branch);
        if (branchProgress.ContainsKey(branch))
            branchProgress.Remove(branch);
        if (originalScales.ContainsKey(branch))
            originalScales.Remove(branch);

        shreddedBranches++;
        Destroy(branch);
    }

    public void ClearAllWoodChips()
    {
        foreach (GameObject woodChip in woodChipsList)
        {
            if (woodChip != null) Destroy(woodChip);
        }
        woodChipsList.Clear();
    }

    public int GetWoodChipCount()
    {
        return woodChipsList.Count;
    }

    void OnDestroy()
    {
        branchesInShredder.Clear();
        currentlyShredding.Clear();
        originalScales.Clear();
        branchProgress.Clear();
    }

    public void ResetBranches()
    {
        shreddedBranches = 0;
    }
}