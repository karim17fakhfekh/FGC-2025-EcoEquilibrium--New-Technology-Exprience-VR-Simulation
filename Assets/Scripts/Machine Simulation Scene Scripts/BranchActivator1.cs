using UnityEngine;

public class BranchActivator1 : MonoBehaviour
{
    [Header("Branches to Activate")]
    public GameObject[] branches;

    private int currentIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ActivateNextBranch();
        }
    }

    void ActivateNextBranch()
    {
        if (branches.Length == 0 || currentIndex >= branches.Length) return;

        GameObject branch = branches[currentIndex];
        branch.SetActive(true);

        currentIndex++;
    }
}
