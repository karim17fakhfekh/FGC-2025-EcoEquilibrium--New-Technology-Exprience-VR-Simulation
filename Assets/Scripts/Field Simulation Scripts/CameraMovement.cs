using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] targetPoints;
    public float[] moveSpeeds;
    public float[] waitDurations;

    void Start()
    {
        if (targetPoints.Length > 0 &&
            moveSpeeds.Length == targetPoints.Length &&
            waitDurations.Length == targetPoints.Length)
        {
            transform.position = targetPoints[0].position;
            transform.rotation = targetPoints[0].rotation;

            StartCoroutine(WaitThenMove(1));
        }
        else
        {
            Debug.LogError("Target points, move speeds, and wait durations must all have the same length!");
        }
    }

    IEnumerator WaitThenMove(int nextIndex)
    {
        yield return new WaitForSeconds(waitDurations[nextIndex - 1]);
        StartCoroutine(MoveAlongPath(nextIndex));
    }

    IEnumerator MoveAlongPath(int startIndex)
    {
        for (int currentIndex = startIndex; currentIndex < targetPoints.Length; currentIndex++)
        {
            Transform target = targetPoints[currentIndex];
            float speed = moveSpeeds[currentIndex];

            yield return StartCoroutine(MoveToTarget(target, speed));

            yield return new WaitForSeconds(waitDurations[currentIndex]);
        }
    }

    IEnumerator MoveToTarget(Transform target, float speed)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float distance = Vector3.Distance(startPos, target.position);
        float duration = distance / speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, target.position, t);
            transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
