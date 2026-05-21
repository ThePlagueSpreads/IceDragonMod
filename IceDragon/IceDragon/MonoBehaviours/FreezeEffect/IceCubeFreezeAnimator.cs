using UnityEngine;

namespace IceDragon.MonoBehaviours.FreezeEffect;

public class IceCubeFreezeAnimator : MonoBehaviour
{
    private const float expandTimeLength = 0.75f;
    private const float sitTimeLength = 4;
    private const float thawTimeLength = 5;

    private const float fadeStartTime = 3.5f;
    private static float fadeTotalTime = GetTotalAnimationTime() - fadeStartTime;
    
    public float targetScale;
    private float animTime;

    private Renderer render;

    public void Start()
    {
        render = GetComponentInChildren<Renderer>();
    }

    public void Update()
    {
        animTime += Time.deltaTime;
        UpdateScale();
        UpdateFade();
    }

    public void UpdateScale()
    {
        float phaseScale;
        if (animTime <= expandTimeLength)
        {
            phaseScale = Mathf.Lerp(0, 1, animTime / expandTimeLength);
        }
        else if (animTime <= expandTimeLength + sitTimeLength)
        {
            phaseScale = 1;
        }
        else if (animTime >= expandTimeLength + sitTimeLength)
        {
            float thawTime = animTime - (expandTimeLength + sitTimeLength);
            phaseScale = Mathf.Lerp(1, 0, thawTime / thawTimeLength);
        }
        else
        {
            phaseScale = 0;
        }
        transform.localScale = Vector3.one * (targetScale * phaseScale);
    }

    public void UpdateFade()
    {
        float fadeAmount = 0.001f;
        if (animTime >= fadeStartTime)
        {
            float fadeTime = animTime - fadeStartTime;
            fadeAmount = Mathf.Lerp(0.001f, 0.999f, fadeTime / fadeTotalTime);
        }
        
        render.SetFadeAmount(fadeAmount);
    }
    

    public static float GetTotalAnimationTime() => expandTimeLength + sitTimeLength + thawTimeLength;
}
