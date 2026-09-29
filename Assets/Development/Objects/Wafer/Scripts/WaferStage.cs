using UnityEngine;
public enum WaferStage
{
    Uncoated = 0,  // до Machine 1
    Coated   = 1,  // после Machine 1 (лак нанесён)
    Exposed  = 2,  // после Machine 2 (паттерн выбран/подтверждён)
    Etched   = 3   // после Machine 3 (травление выполнено)
}
