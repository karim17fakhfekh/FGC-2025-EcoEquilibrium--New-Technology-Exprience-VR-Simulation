using UnityEngine;
using System.Collections;

public class BranchActivator2 : MonoBehaviour
{
    [Header("Branches to Activate")]
    public GameObject[] branches;

    [Header("Timing Settings")]
    public float initialDelay = 2f;
    public float interval = 1f;

    private int currentIndex = 0;

    void Start()
    {
        StartCoroutine(ActivateBranches());
    }

    IEnumerator ActivateBranches()
    {
        yield return new WaitForSeconds(initialDelay);

        while (currentIndex < branches.Length)
        {
            GameObject branch = branches[currentIndex];
            if (branch != null) branch.SetActive(true);

            currentIndex++;

            yield return new WaitForSeconds(interval);
        }
    }
}
