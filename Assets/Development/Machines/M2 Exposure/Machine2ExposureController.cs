using System.Collections;
using UnityEngine;

public class Machine2ExposureController : MonoBehaviour
{
    [Header("Zones")]
    [SerializeField] private Collider inputZone;
    [SerializeField] private Transform outputSpawnPoint;

    [Header("Prefabs")]
    [SerializeField] private GameObject waferExposedPrefab;

    [Header("Demo wafer")]
    [SerializeField] private DemoWaferController demo;

    [Header("Timing")]
    [SerializeField] private float processTimeSec = 6f;

    private bool isBusy;
    private Wafer loadedWafer;

    private PatternParams current;

    private void Awake()
    {
        // дефолты
        current.patternType = PatternType.Cross;
        current.offsetXY = Vector2.zero;
        current.rotationSteps45 = 0;
        current.scale = 1f;

        demo.SetPreview(current);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBusy) return;
        if (loadedWafer != null) return;

        var wafer = other.GetComponent<Wafer>();
        if (wafer == null) return;
        if (wafer.Stage != WaferStage.Coated) return;

        loadedWafer = wafer;

        // по ТЗ: "коated исчезает" после загрузки
        Destroy(wafer.gameObject);

        // теперь UI доступен, превью уже показывается
    }

    // UI callbacks:
    public void UI_SelectCross() { current.patternType = PatternType.Cross; demo.SetPreview(current); }
    public void UI_SelectFrame() { current.patternType = PatternType.Frame; demo.SetPreview(current); }
    public void UI_SelectFilled() { current.patternType = PatternType.FilledSquare; demo.SetPreview(current); }

    public void UI_SetOffsetX(float v) { current.offsetXY.x = v; demo.SetPreview(current); }
    public void UI_SetOffsetY(float v) { current.offsetXY.y = v; demo.SetPreview(current); }
    public void UI_SetScale(float v)   { current.scale = v; demo.SetPreview(current); }

    public void UI_Rotate45()
    {
        current.rotationSteps45 = (current.rotationSteps45 + 1) % 8;
        demo.SetPreview(current);
    }

    public void UI_ConfirmExpose()
    {
        if (isBusy) return;
        if (loadedWafer == null) return; // нет вафера в машине

        StartCoroutine(ExposeRoutine());
    }

    private IEnumerator ExposeRoutine()
    {
        isBusy = true;

        // фиксируем демо-вафер зелёным
        demo.Confirm(current);

        // процесс экспонирования (анимации/звук можно сюда)
        yield return new WaitForSeconds(processTimeSec);

        // спавним WaferExposed и записываем параметры
        var go = Instantiate(waferExposedPrefab,
            outputSpawnPoint.position + Vector3.up * 0.02f,
            outputSpawnPoint.rotation);

        var wafer = go.GetComponent<Wafer>();
        if (wafer) wafer.SetStage(WaferStage.Exposed); // если у тебя Stage через SetStage()

        var data = go.GetComponent<WaferExposedData>();
        if (data) data.pattern = current;

        // машина готова к следующему
        loadedWafer = null;
        isBusy = false;
    }
}