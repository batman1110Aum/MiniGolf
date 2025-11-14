using System.Collections;
using UnityEngine;

public class HoleController : MonoBehaviour
{
    public enum HoleType { Good, Bad }

    public HoleType currentType { get; private set; }

    private Material holeMaterial;
    private Color goodColor = Color.blue;
    private Color badColor = Color.red;
    private Color warningColor = Color.yellow;
    private bool isFrozen = false;
    private bool isFlashing = false;

    void Start()
    {
        holeMaterial = GetComponent<Renderer>().material;
        RandomizeColor();
    }

    public void RandomizeColor()
    {
        if (isFrozen || isFlashing) return;

        currentType = Random.value > 0.5f ? HoleType.Good : HoleType.Bad;
        UpdateColor();
    }

    public void RandomizeColorWithWarning()
    {
        if (isFrozen) return;

        StartCoroutine(FlashBeforeChange());
    }

    private IEnumerator FlashBeforeChange()
    {
        isFlashing = true;

        for (int i = 0; i < 3; i++)
        {
            holeMaterial.color = warningColor;
            yield return new WaitForSeconds(0.2f);
            UpdateColor();
            yield return new WaitForSeconds(0.2f);
        }

        currentType = Random.value > 0.5f ? HoleType.Good : HoleType.Bad;
        UpdateColor();
        isFlashing = false;
    }

    public void FreezeColor()
    {
        isFrozen = true;
    }

    public void UnfreezeColor()
    {
        isFrozen = false;
    }

    private void UpdateColor()
    {
        holeMaterial.color = currentType == HoleType.Good ? goodColor : badColor;
    }
}
