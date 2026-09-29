using UnityEngine;

public class DemoWaferController : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color previewColor = Color.cyan;
    [SerializeField] private Color confirmedColor = Color.green;

    private static readonly int PatternTypeId = Shader.PropertyToID("_PatternType");
    private static readonly int OffsetId      = Shader.PropertyToID("_PatternOffset");
    private static readonly int RotDegId      = Shader.PropertyToID("_PatternRotDeg");
    private static readonly int ScaleId       = Shader.PropertyToID("_PatternScale");
    private static readonly int ColorId       = Shader.PropertyToID("_PatternColor");

    private PatternParams confirmed;

    public void SetPreview(in PatternParams p)
    {
        Apply(p, previewColor);
    }

    public void Confirm(in PatternParams p)
    {
        confirmed = p;
        Apply(p, confirmedColor);
    }

    public PatternParams GetConfirmed() => confirmed;

    private void Apply(in PatternParams p, Color c)
    {
        if (!targetRenderer) return;
        var mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(mpb);

        mpb.SetFloat(PatternTypeId, (float)p.patternType);
        mpb.SetVector(OffsetId, p.offsetXY);
        mpb.SetFloat(RotDegId, p.rotationSteps45 * 45f);
        mpb.SetFloat(ScaleId, p.scale);
        mpb.SetColor(ColorId, c);

        targetRenderer.SetPropertyBlock(mpb);
    }
}