using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;
namespace IceDragon.Registration.Prefabs;


public static class ArcticKelpFactory
{
    private static readonly Color ArcticKelpColor = new(6, 2, 0);
    private static readonly Color ArcticKelpSpec= new(15, 0, 0);

    public static void RegisterArcticKelpVariants()
    {
        RegisterArcticKelp("ee1baf03-0560-4f4d-ad29-13a337bef0d7", "Arctic_kelp_dense_01");
        RegisterArcticKelp("9bfe02bd-60a3-401b-b7a0-627c3bdc4451", "Arctic_kelp_dense_02");
        RegisterArcticKelp("1fd4d86f-3b06-4369-945c-ca65f50b4800", "Arctic_kelp_young_01");
    }

    private static void RegisterArcticKelp(string classId, string techTypeName)
    {
        PrefabInfo info = PrefabInfo.WithTechType(techTypeName);
        CloneTemplate clone = new CloneTemplate(info, classId);
        clone.ModifyPrefab = ModifyKelp;
        CustomPrefab prefab = new CustomPrefab(info);
        prefab.SetGameObject(clone);
        prefab.Register();
    }

    private static void ModifyKelp(GameObject gameobject)
    {
        Transform LodRoot = gameobject.transform.GetChild(0);
        foreach (Transform lodGroup in LodRoot)
        {
            GameObject lod = lodGroup.GetChild(0).gameObject;
            foreach (Material material in lod.GetComponent<MeshRenderer>().materials)
            {
                material.SetColor(ShaderPropertyID._Color, ArcticKelpColor);
                material.SetColor(ShaderPropertyID._SpecColor, ArcticKelpSpec);
            }
        }
    }
}
