using System.Collections;
using UnityEngine;

public class Machine2_UIController : MonoBehaviour
{
    [Header("Machine 2: Zones")]
    [Tooltip("Trigger collider зоны приёма (InputZone). Можно оставить пустым, если этот скрипт висит прямо на объекте с TriggerCollider.")]
    [SerializeField] private Collider inputZone;

    [Tooltip("Точка спавна выходного WaferExposed.")]
    [SerializeField] private Transform outputSpawnPoint;

    [Header("Machine 2: Prefabs")]
    [Tooltip("Префаб WaferExposed (Type 3). Должен содержать компоненты Wafer + WaferExposedData.")]
    [SerializeField] private GameObject waferExposedPrefab;

    [Header("Demo Wafer")]
    [SerializeField] private DemoWaferController demoWafer;

    [Header("Process")]
    [Tooltip("Время процесса экспонирования (сек). Можно оставить 6 для консистентности с Machine 1.")]
    [SerializeField] private float processTimeSec = 6f;

    [Header("UI Ranges")]
    [SerializeField] private Vector2 moveRange = new Vector2(-0.5f, 0.5f);
    [SerializeField] private Vector2 scaleRange = new Vector2(0.5f, 1.5f);

    [Header("Debug")]
    [SerializeField] private bool logEvents = false;

    private bool isBusy;
    private bool hasLoadedCoatedWafer;

    // Текущие (редактируемые UI) параметры паттерна
    private PatternParams current;

    private void Reset()
    {
        // Если скрипт повесили прямо на InputZone с TriggerCollider — подцепим автоматически
        if (inputZone == null)
            inputZone = GetComponent<Collider>();
    }

    private void Awake()
    {
        // Дефолты (можешь поменять под свой UX)
        current.patternType = PatternType.Cross;
        current.offsetXY = Vector2.zero;
        current.rotationSteps45 = 0; // 0 = 0°
        current.scale = 1f;

        // Показать стартовое превью
        if (demoWafer != null)
            demoWafer.SetPreview(current);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Работает только если этот скрипт висит на объекте с TriggerCollider
        // или если trigger-события приходят сюда через Rigidbody/иерархию.
        TryLoadWafer(other);
    }

    // Если хочешь, можешь вручную прокинуть событие из отдельного триггер-скрипта:
    public void TryLoadWafer(Collider other)
    {
        if (isBusy) return;
        if (hasLoadedCoatedWafer) return;
        if (other == null) return;

        var wafer = other.GetComponent<Wafer>();
        if (wafer == null) return;

        if (wafer.Stage != WaferStage.Coated)
        {
            if (logEvents) Debug.Log($"[Machine2] Ignored wafer with stage: {wafer.Stage}", this);
            return;
        }

        hasLoadedCoatedWafer = true;

        if (logEvents) Debug.Log("[Machine2] Coated wafer loaded. Ready for pattern selection.", this);

        // По принятому шаблону “загрузили → входной вафер исчез” (как делали для Machine 1) [1]
        Destroy(wafer.gameObject);
    }

    // --------------------
    // UI Callbacks: Pattern
    // --------------------
    public void UI_SelectCross()
    {
        current.patternType = PatternType.Cross;
        PushPreview();
    }

    public void UI_SelectFrame()
    {
        current.patternType = PatternType.Frame;
        PushPreview();
    }

    public void UI_SelectFilledSquare()
    {
        current.patternType = PatternType.FilledSquare;
        PushPreview();
    }

    // --------------------
    // UI Callbacks: Move/Scale/Rotate
    // --------------------
    public void UI_SetMoveX(float value)
    {
        current.offsetXY.x = Mathf.Clamp(value, moveRange.x, moveRange.y);
        PushPreview();
    }

    public void UI_SetMoveY(float value)
    {
        current.offsetXY.y = Mathf.Clamp(value, moveRange.x, moveRange.y);
        PushPreview();
    }

    public void UI_SetScale(float value)
    {
        current.scale = Mathf.Clamp(value, scaleRange.x, scaleRange.y);
        PushPreview();
    }

    /// <summary>
    /// Поворот шагом 45 градусов: 0..7 (0°,45°,90°...315°)
    /// </summary>
    public void UI_Rotate45()
    {
        current.rotationSteps45 = (current.rotationSteps45 + 1) % 8;
        PushPreview();
    }

    // --------------------
    // UI Callbacks: Confirm
    // --------------------
    public void UI_ConfirmExpose()
    {
        if (isBusy) return;

        if (!hasLoadedCoatedWafer)
        {
            if (logEvents) Debug.Log("[Machine2] Confirm ignored: no coated wafer loaded.", this);
            return;
        }

        StartCoroutine(ExposeRoutine());
    }

    private IEnumerator ExposeRoutine()
    {
        isBusy = true;

        // Фиксация паттерна на демо-вафере (зелёный) [5]
        if (demoWafer != null)
            demoWafer.Confirm(current);

        if (logEvents) Debug.Log("[Machine2] Exposure started...", this);

        yield return new WaitForSeconds(processTimeSec);

        SpawnExposedWafer(current);

        // Машина снова готова
        hasLoadedCoatedWafer = false;
        isBusy = false;

        if (logEvents) Debug.Log("[Machine2] Exposure finished. WaferExposed spawned.", this);
    }

    private void SpawnExposedWafer(in PatternParams pattern)
    {
        if (waferExposedPrefab == null || outputSpawnPoint == null)
        {
            Debug.LogWarning("[Machine2] Missing waferExposedPrefab or outputSpawnPoint", this);
            return;
        }

        Vector3 spawnPos = outputSpawnPoint.position + Vector3.up * 0.02f;
        Quaternion spawnRot = outputSpawnPoint.rotation;

        GameObject go = Instantiate(waferExposedPrefab, spawnPos, spawnRot);

        // Проставляем стадию (если префаб уже с Exposed — можно убрать)
        var wafer = go.GetComponent<Wafer>();
        if (wafer != null)
        {
            // Если у тебя есть SetStage — используй его; иначе присвой поле напрямую.
            // wafer.SetStage(WaferStage.Exposed);
            wafer.SetStage(WaferStage.Exposed);
        }

        // Записываем параметры паттерна в выходной вафер (Wafer Type 3) [5]
        var data = go.GetComponent<WaferExposedData>();
        if (data != null)
            data.pattern = pattern;
        else
            Debug.LogWarning("[Machine2] WaferExposedData component not found on waferExposedPrefab", this);
    }

    private void PushPreview()
    {
        if (demoWafer != null)
            demoWafer.SetPreview(current);

        if (logEvents)
            Debug.Log($"[Machine2] Preview: {current.patternType} offset={current.offsetXY} rot45={current.rotationSteps45} scale={current.scale}", this);
    }

    // (Опционально) полезно для отладки/внешних систем
    public PatternParams GetCurrentPattern() => current;
    public bool IsBusy() => isBusy;
    public bool HasLoadedWafer() => hasLoadedCoatedWafer;
}