using UnityEngine;

public class Wafer : MonoBehaviour
{
    [field: SerializeField] public WaferStage Stage { get; private set; } = WaferStage.Uncoated;

    public void SetStage(WaferStage newStage)
    {
        Stage = newStage;
        Debug.Log($"Wafer {name} stage => {Stage}"); // опциональный дебаг о том, что ваферу присвоена новая стадия

        // (опционально) здесь можно обновлять визуал: материал/цвет
        // ApplyVisual();
    }
}