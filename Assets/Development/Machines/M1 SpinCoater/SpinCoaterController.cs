using System.Collections;
using UnityEngine;

public class SpinCoaterController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Transform outputSpawnPoint;
    [SerializeField] private GameObject waferCoatedPrefab;
    [SerializeField] private Animator coatingAnimator;

    [Header("Process")]
    [SerializeField] private float processTimeSec = 6f;

    private bool isProcessing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isProcessing) return;

        Wafer wafer = other.GetComponent<Wafer>();
        if (wafer == null) return;

        if (wafer.Stage != WaferStage.Uncoated) return;

        // старт анимации
        if (coatingAnimator != null)
            coatingAnimator.SetTrigger("DoCoating");

        StartCoroutine(ProcessWaferRoutine(other.gameObject));
    }

    private IEnumerator ProcessWaferRoutine(GameObject waferUncoatedGO)
    {
        isProcessing = true;

        // 1) "Исчезает" входной вафер (можно Destroy, можно SetActive(false))
        Destroy(waferUncoatedGO);

        // 2) Таймер процесса
        yield return new WaitForSeconds(processTimeSec);

        // 3) Спавн обработанного вафера
        Vector3 spawnPos = outputSpawnPoint.position + Vector3.up * 0.02f;
        Instantiate(waferCoatedPrefab, spawnPos, outputSpawnPoint.rotation);

        isProcessing = false;
    }
}