using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using UnityEngine;

namespace IceDragon.MaterialModifiers;

public class IceDragonMaterialModifier : MaterialModifier
{
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        if (materialType == MaterialUtils.MaterialType.Transparent)
        {
            material.color = new Color(1, 1, 1);
            material.SetColor("_SpecColor", new Color(1, 1, 0.85f));
            material.SetFloat("_Shininess", 7.5f);
            material.SetFloat("_SpecInt", 20);
            material.SetFloat("_IBLreductionAtNight", 0.25f);
            material.SetFloat("_Fresnel", 0.7f);
        }
        else
        {
            material.EnableKeyword("MARMO_EMISSION");
            material.SetColor("_GlowColor", Color.black);
            material.SetFloat("_GlowStrength", 0);
            material.SetFloat("_GlowStrengthNight", 0);
            material.SetFloat("_EmissionLM", 0.005f);
            material.SetFloat("_EmissionLMNight", 0.005f);
        }

        if (material.name.Contains("Pupil"))
        {
            material.SetFloat("_Shininess", 6f);
            material.SetFloat("_SpecInt", 10);
            material.SetFloat("_Fresnel", 0);
            material.SetFloat("_GlowStrength", 0.2f);
            material.SetFloat("_GlowStrengthNight", 0.2f);
        }
    }
}