using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Gives newly spawned monster rewards a short outward toss.</summary>
public sealed class DropSpreadEffect : MonoBehaviour
{
    private const float SearchRadius = 3f;
    private static readonly HashSet<DropItem> ExistingDrops = new HashSet<DropItem>();
    private static DropSpreadEffect runner;
    private static int spreadIndex;

    public static void CaptureExistingDrops()
    {
        ExistingDrops.Clear();
        foreach (DropItem drop in FindObjectsByType<DropItem>(FindObjectsSortMode.None))
        {
            if (drop != null)
                ExistingDrops.Add(drop);
        }
    }

    public static void AnimateAt(Vector3 deathPosition)
    {
        DropItem[] drops = FindObjectsByType<DropItem>(FindObjectsSortMode.None);
        DropSpreadEffect effectRunner = GetRunner();

        foreach (DropItem drop in drops)
        {
            if (drop == null || ExistingDrops.Contains(drop))
                continue;

            ExistingDrops.Add(drop);
            Transform dropVisual = drop.EffectTransform;
            if (dropVisual == null)
                continue;

            Vector3 offset = dropVisual.position - deathPosition;
            offset.y = 0f;
            if (offset.sqrMagnitude > SearchRadius * SearchRadius)
                continue;

            effectRunner.StartCoroutine(effectRunner.Toss(dropVisual, deathPosition));
            effectRunner.StartCoroutine(effectRunner.FloatAndRotate(dropVisual));
        }

        ExistingDrops.Clear();
    }

    private static DropSpreadEffect GetRunner()
    {
        if (runner != null)
            return runner;

        GameObject runnerObject = new GameObject("Drop Spread Effects");
        runner = runnerObject.AddComponent<DropSpreadEffect>();
        return runner;
    }

    private IEnumerator FloatAndRotate(Transform drop)
    {
        // Start idle motion after the initial scatter animation has finished.
        yield return new WaitForSeconds(0.45f);
        if (drop == null)
            yield break;

        Vector3 restingPosition = drop.position;
        Quaternion restingRotation = drop.rotation;
        float phase = Random.Range(0f, Mathf.PI * 2f);
        float bobHeight = Random.Range(0.06f, 0.12f);
        float bobSpeed = Random.Range(1.2f, 1.8f);
        float spinSpeed = Random.Range(25f, 55f);

        while (drop != null)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight;
            drop.position = restingPosition + Vector3.up * bob;
            restingRotation *= Quaternion.Euler(0f, spinSpeed * Time.deltaTime, 0f);
            drop.rotation = restingRotation;
            yield return null;
        }
    }

    private IEnumerator Toss(Transform drop, Vector3 deathPosition)
    {
        Vector3 start = drop.position;
        Vector3 radial = start - deathPosition;
        radial.y = 0f;

        float angle = spreadIndex++ * 137.5f + Random.Range(-28f, 28f);
        Vector3 randomDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        radial = radial.sqrMagnitude > 0.04f
            ? (radial.normalized + randomDirection * 0.45f).normalized
            : randomDirection;

        Vector3 end = start + radial * Random.Range(0.35f, 0.75f);
        float duration = Random.Range(0.28f, 0.42f);
        float arcHeight = Random.Range(0.35f, 0.6f);
        float elapsed = 0f;

        Rigidbody body = drop.GetComponent<Rigidbody>();
        if (body == null)
            body = drop.GetComponentInChildren<Rigidbody>();
        bool wasKinematic = body != null && body.isKinematic;
        if (body != null)
            body.isKinematic = true;

        while (elapsed < duration && drop != null)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            Vector3 position = Vector3.Lerp(start, end, easedProgress);
            position.y += Mathf.Sin(progress * Mathf.PI) * arcHeight;
            drop.position = position;
            yield return null;
        }

        if (drop != null)
        {
            drop.position = end;
            if (body != null)
                body.isKinematic = wasKinematic;
        }
    }
}
