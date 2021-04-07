using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ABLoader : MonoBehaviour
{
    public Shader MRTKShader;
    public Material MRTKMaterial;

    void Start()
    {
        StartCoroutine(LoadAssetBundle());
    }

    IEnumerator LoadAssetBundle()
    {
        yield return null;

        var bundleLoadRequest = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "assetBundle.unity3d"));
        yield return bundleLoadRequest;

        var assetLoadRequest = bundleLoadRequest.LoadAllAssetsAsync<GameObject>();
        yield return assetLoadRequest;

        var sourceGameObjects = assetLoadRequest.allAssets;
        var sourceGameObject = sourceGameObjects[0] as GameObject;

        var originalInstance = Instantiate(sourceGameObject, new Vector3(-0.45f, -0.02f, 1.0f), Quaternion.identity);
        originalInstance.name = "UnityShaderNoReplacement";
        AddBoxCollider(originalInstance);

        var replacedShaderInstance = Instantiate(sourceGameObject, new Vector3(-0.15f, -0.02f, 1.0f), Quaternion.identity);
        replacedShaderInstance.name = "MRTKShaderReplacedRuntime";
        AddBoxCollider(replacedShaderInstance);
        ReplaceShader(replacedShaderInstance);
        
        var manualReplacedMaterialsInstance = Instantiate(sourceGameObject, new Vector3(0.15f, -0.02f, 1.0f), Quaternion.identity);
        manualReplacedMaterialsInstance.name = "MaterialReplacementCopyColorAndMainText";
        AddBoxCollider(manualReplacedMaterialsInstance);
        ReplaceMaterialManualPropertiesCopy(manualReplacedMaterialsInstance);

        var automaticReplacedMaterialsInstance = Instantiate(sourceGameObject, new Vector3(0.45f, -0.02f, 1.0f), Quaternion.identity);
        automaticReplacedMaterialsInstance.name = "MaterialReplacementCopyEveryProperty";
        AddBoxCollider(automaticReplacedMaterialsInstance);
        ReplaceMaterialAutomaticPropertiesCopy(automaticReplacedMaterialsInstance);

        bundleLoadRequest.Unload(false);
        Resources.UnloadUnusedAssets();
    }

    void AddBoxCollider(GameObject sourceGameObject)
    {
        var boxCollider = sourceGameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0.0f, 0.008f, 0.0f);
        boxCollider.size = new Vector3(0.175f, 0.02f, 0.165f);
    }

    void ReplaceShader(GameObject sourceGameObject)
    {
        var meshRenderers = sourceGameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            var materials = meshRenderer.materials;
            foreach (var aMaterial in materials)
            {
                aMaterial.shader = MRTKShader;
            }

            meshRenderer.materials = materials;
        }
    }

    void ReplaceMaterialManualPropertiesCopy(GameObject sourceGameObject)
    {
        var meshRenderers = sourceGameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            var materials = meshRenderer.materials;

            var index = 0;
            foreach (var aMaterial in materials)
            {
                var replacementMaterial = Instantiate(MRTKMaterial);

                replacementMaterial.SetColor("_Color", aMaterial.GetColor("_Color"));
                replacementMaterial.SetTexture("_MainTex", aMaterial.GetTexture("_MainTex"));

                materials[index++] = replacementMaterial;
            }

            meshRenderer.materials = materials;
        }
    }

    void ReplaceMaterialAutomaticPropertiesCopy(GameObject sourceGameObject)
    {
        var meshRenderers = sourceGameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            var materials = meshRenderer.materials;

            var index = 0;
            foreach (var aMaterial in materials)
            {
                var replaceMaterial = Instantiate(MRTKMaterial);

                replaceMaterial.CopyPropertiesFromMaterial(aMaterial);

                materials[index++] = replaceMaterial;
            }

            meshRenderer.materials = materials;
        }
    }
}
