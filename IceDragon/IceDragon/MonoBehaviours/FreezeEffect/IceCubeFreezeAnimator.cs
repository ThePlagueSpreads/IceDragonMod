using UnityEngine;

namespace IceDragon.MonoBehaviours.FreezeEffect;

public class IceCubeFreezeAnimator : MonoBehaviour
{
    private const float expandTimeLength = 1.5f;
    private const float sitTimeLength = 4;
    private const float thawTimeLength = 5;
    
    public float targetScale;
    private float animTime;

    public void Update()
    {
        animTime += Time.deltaTime;

        float phaseScale;
        if (animTime <= expandTimeLength)
        {
            phaseScale = Mathf.Lerp(0, 1, animTime / expandTimeLength);
        }
        else if (animTime <= expandTimeLength + sitTimeLength)
        {
            phaseScale = 1f;
        }
        else if (animTime >= expandTimeLength + sitTimeLength)
        {
            float thawTime = animTime - (expandTimeLength + sitTimeLength);
            phaseScale = Mathf.Lerp(1f, 0f, thawTime / thawTimeLength);
        }
        else
        {
            phaseScale = 0f;
        }
        transform.localScale = Vector3.one * (targetScale * phaseScale);
    }

    public static float GetTotalAnimationTime() => expandTimeLength + sitTimeLength + thawTimeLength;
}
