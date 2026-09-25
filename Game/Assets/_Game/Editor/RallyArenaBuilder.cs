using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Imports the Rally art library without replacing the live basketball environment.</summary>
public static class RallyArenaBuilder {
    const string Art = "Assets/_Game/Art/Basketball/Rally/";
    const string Prefabs = "Assets/_Game/Prefabs/Basketball/Rally/";
    const string Review = "Assets/_Game/Scenes/RallyArenaReview.unity";
    [Serializable] class ImportData { public MaterialData[] materials; public string[] modules; }
    [Serializable] class MaterialData {
        public string name, base_map, roughness_map;
        public float[] color_srgb;
        public float roughness, metallic, emission;
    }

    [MenuItem("Sports/Basketball/Build Rally art library")]
    public static void Build() {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Prefabs);
        Directory.CreateDirectory(Art + "Materials");
        AssetDatabase.Refresh();
        var data = JsonUtility.FromJson<ImportData>(File.ReadAllText(Art + "unity-import.json"));
        foreach (var path in Directory.GetFiles(Art, "*.png")) {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = !path.Contains("Roughness") && !path.Contains("MetallicSmoothness");
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = path.Contains("Maple") ? 2048 : 512;
            importer.anisoLevel = path.Contains("Maple") ? 8 : 2;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) throw new Exception("URP/Lit is required for the Rally review scene.");
        var materials = new Dictionary<string, Material>();
        foreach (var spec in data.materials) {
            var path = Art + "Materials/" + spec.name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.shader = shader;
            var color = new Color(spec.color_srgb[0], spec.color_srgb[1], spec.color_srgb[2], 1);
            m.SetColor("_BaseColor", color);
            m.SetTexture("_BaseMap", string.IsNullOrEmpty(spec.base_map) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(Art + spec.base_map));
            m.SetFloat("_Metallic", spec.metallic);
            m.SetFloat("_Smoothness", 1 - spec.roughness);
            if (!string.IsNullOrEmpty(spec.roughness_map)) {
                m.EnableKeyword("_METALLICSPECGLOSSMAP");
                m.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "Rally_HoneyMaple_MetallicSmoothness.png"));
                m.SetFloat("_Smoothness", 1);
            }
            if (spec.emission > 0) {
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * spec.emission);
            }
            m.enableInstancing = true; EditorUtility.SetDirty(m); materials[spec.name] = m;
        }
        foreach (var name in data.modules) {
            var importer = (ModelImporter)AssetImporter.GetAtPath(Art + name + ".fbx");
            if (!importer) throw new Exception("Missing module: " + name);
            importer.globalScale = 1; importer.useFileScale = true;
            importer.importCameras = false; importer.importLights = false;
            importer.importAnimation = false; importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.preserveHierarchy = true; importer.isReadable = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.SaveAndReimport();
            foreach (var material in materials.Where(p => p.Key.StartsWith("Rally_" + name + "_")))
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), material.Key), material.Value);
            importer.SaveAndReimport();
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var assembly = new GameObject("Rally Arena — Modular Assembly");
        foreach (var name in data.modules) {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Art + name + ".fbx");
            var root = (GameObject)PrefabUtility.InstantiatePrefab(source); root.name = name;
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true)) {
                renderer.enabled = true; renderer.gameObject.SetActive(true); renderer.gameObject.layer = 8;
                renderer.sharedMaterials = renderer.sharedMaterials.Select(m => {
                    if (!m || !materials.TryGetValue(m.name, out var mapped)) throw new Exception("Unmapped material on " + renderer.name + ": " + (m ? m.name : "null"));
                    return mapped;
                }).ToArray();
                if (renderer.name.Contains("Lettering") || renderer.name.Contains("Markings") || renderer.name.Contains("Emblem"))
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            if (name.EndsWith("Seating")) ConfigureLODs(root, name);
            if (name == "Court") AddBox(root, "Collision_Floor", new Vector3(0, -.23f, 0), new Vector3(24, .46f, 37.5f));
            if (name == "Hoop") AddBox(root, "Collision_PaddedBase", new Vector3(0, 1.20f, -3.08f), new Vector3(1.62f, 2.40f, 1.98f));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Prefabs + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(root);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(assembly.transform, false);
            if (name == "Hoop") {
                instance.name = "Hoop_North"; instance.transform.localPosition = new Vector3(0, 0, -12.7254f);
                var second = (GameObject)PrefabUtility.InstantiatePrefab(prefab); second.name = "Hoop_South";
                second.transform.SetParent(assembly.transform, false);
                second.transform.localPosition = new Vector3(0, 0, 12.7254f);
                second.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
        var boundary = new GameObject("Collision_RunoffBoundary"); boundary.transform.SetParent(assembly.transform, false);
        foreach (var s in new[] { -1f, 1f }) {
            AddBox(boundary, "Sideline", new Vector3(s * 11.85f, 1.5f, 0), new Vector3(.25f, 3, 37.2f));
            AddBox(boundary, "Endline", new Vector3(0, 1.5f, s * 18.55f), new Vector3(23.7f, 3, .25f));
        }
        PrefabUtility.SaveAsPrefabAsset(assembly, Prefabs + "RallyArena.prefab");
        Audit(assembly, data.modules.Length, materials.Count);
        SetupReview(assembly);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Review);
        AssetDatabase.SaveAssets();
        Capture();
        Debug.Log("RALLY_UNITY_COMPLETE " + Review);
    }

    static void ConfigureLODs(GameObject root, string module) {
        foreach (var group in root.GetComponentsInChildren<LODGroup>(true)) UnityEngine.Object.DestroyImmediate(group);
        var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        for (var i = 0; i < 24; i++) {
            var prefix = module + "__Bay_" + i.ToString("00") + "_Seats_LOD";
            var near = renderers.Single(r => r.name == prefix + "0");
            var far = renderers.Single(r => r.name == prefix + "1");
            var go = new GameObject("Seat_Bay_" + i.ToString("00")); go.transform.SetParent(root.transform, false);
            near.transform.SetParent(go.transform, true); far.transform.SetParent(go.transform, true);
            var lod = go.AddComponent<LODGroup>(); lod.fadeMode = LODFadeMode.None;
            lod.SetLODs(new[] { new LOD(.10f, new Renderer[] { near }), new LOD(.005f, new Renderer[] { far }) });
            lod.RecalculateBounds();
        }
    }
    static void AddBox(GameObject parent, string name, Vector3 center, Vector3 size) {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false); go.layer = 8;
        var c = go.AddComponent<BoxCollider>(); c.center = center; c.size = size;
    }
    static void Audit(GameObject root, int moduleCount, int materialCount) {
        var filters = root.GetComponentsInChildren<MeshFilter>(true);
        var wood = filters.Single(f => f.name == "Court__Maple").GetComponent<Renderer>().bounds;
        if (Mathf.Abs(wood.size.x - 15.24f) > .01f || Mathf.Abs(wood.size.z - 28.6512f) > .01f) throw new Exception("Court scale mismatch: " + wood);
        var rims = filters.Where(f => f.name.StartsWith("Hoop__Rim") && !f.name.StartsWith("Hoop__RimMount")).ToArray();
        if (rims.Length != 2 || rims.Any(f => Mathf.Abs(f.GetComponent<Renderer>().bounds.center.y - 3.048f) > .001f)) throw new Exception("Rim placement mismatch");
        var lods = root.GetComponentsInChildren<LODGroup>();
        if (lods.Length != 48) throw new Exception("Expected 48 per-bay LOD groups");
        long near = 0, far = 0;
        foreach (var f in filters) {
            if (f.sharedMesh.vertexCount == 0) throw new Exception("Empty mesh: " + f.name);
            var r = f.GetComponent<Renderer>();
            if (r.sharedMaterials.Any(m => !m || m.shader.name != "Universal Render Pipeline/Lit")) throw new Exception("Invalid material: " + f.name);
            long triangles = 0;
            for (int s = 0; s < f.sharedMesh.subMeshCount; s++) triangles += (long)f.sharedMesh.GetIndexCount(s) / 3;
            if (!f.name.EndsWith("LOD1")) near += triangles;
            if (!f.name.EndsWith("LOD0")) far += triangles;
        }
        var report = "Rally Unity import audit\n" +
            "Modules: " + moduleCount + "; hoop instances: 2; seat LOD groups: 48\n" +
            "Independent material assets: " + materialCount + "; mesh renderers including LODs: " + filters.Length + "\n" +
            "All-near triangles: " + near + "; all-far triangles: " + far + "\n" +
            "Court bounds: " + wood + "; rim height: 3.048 m\n" +
            "Colliders: " + root.GetComponentsInChildren<Collider>().Length + " (floor, padded bases, runoff boundary)\n" +
            "Decorative net/rim do not have ball-physics colliders. Concourse is visual, not navigable.\n" +
            "Live basketball scene is unchanged. No target-device performance claim.\n";
        File.WriteAllText(Path.GetFullPath("../ArtSource/Basketball/Rally/unity-import-audit.txt"), report);
        Debug.Log(report);
    }
    static void SetupReview(GameObject assembly) {
        // Preview lighting is separate from the reusable prefab. Indoor shipping lighting needs baking.
        foreach (var r in assembly.transform.Find("Ceiling").GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
        RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.66f, .69f, .74f); RenderSettings.fog = false;
        var key = new GameObject("Review overhead wash").AddComponent<Light>(); key.type = LightType.Directional;
        key.color = new Color(1, .88f, .73f); key.intensity = 1.1f; key.shadows = LightShadows.Soft;
        key.transform.rotation = Quaternion.Euler(72, -25, 0); RenderSettings.sun = key;
        var fill = new GameObject("Review cool bounce").AddComponent<Light>(); fill.type = LightType.Directional;
        fill.color = new Color(.78f, .88f, 1); fill.intensity = .7f; fill.transform.rotation = Quaternion.Euler(25, 150, 0);
        var cam = new GameObject("Rally court review camera").AddComponent<Camera>(); cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.12f, .20f, .30f);
        cam.nearClipPlane = .05f; cam.farClipPlane = 200; cam.fieldOfView = 58;
        cam.transform.position = new Vector3(-8, 3.3f, 13); cam.transform.LookAt(new Vector3(1, 5, -12));
        cam.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;
    }
    static void Capture() {
        var camera = Camera.main;
        var oldPipeline = QualitySettings.renderPipeline;
        var active = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        var pipeline = UnityEngine.Object.Instantiate(active);
        pipeline.renderScale = 1; pipeline.msaaSampleCount = 4;
        pipeline.shadowDistance = 90; pipeline.mainLightShadowmapResolution = 4096; pipeline.shadowCascadeCount = 4;
        var settings = new SerializedObject(pipeline);
        settings.FindProperty("m_SoftShadowsSupported").boolValue = true;
        settings.ApplyModifiedPropertiesWithoutUndo();
        QualitySettings.renderPipeline = pipeline;
        var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
        var previous = RenderTexture.active; var oldTarget = camera.targetTexture;
        var image = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
        try {
            camera.targetTexture = rt;
            for (int i = 0; i < 3; i++) camera.Render();
            RenderTexture.active = rt;
            image.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); image.Apply();
            File.WriteAllBytes(Path.GetFullPath("../Docs/VisualDirection/Basketball/05_Unity_Court.png"), image.EncodeToPNG());
        } finally {
            camera.targetTexture = oldTarget; RenderTexture.active = previous; QualitySettings.renderPipeline = oldPipeline;
            UnityEngine.Object.DestroyImmediate(pipeline); UnityEngine.Object.DestroyImmediate(image); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
        }
    }

    public static void CaptureSavedReview() {
        EditorSceneManager.OpenScene(Review);
        foreach (var path in Directory.GetFiles(Art + "Materials", "*.mat")) {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path.Replace('\\', '/'));
            if (m.GetColor("_EmissionColor").maxColorComponent > 0) {
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                m.EnableKeyword("_EMISSION"); EditorUtility.SetDirty(m);
            }
        }
        AssetDatabase.SaveAssets();
        Capture();
        Debug.Log("RALLY_SAVED_REVIEW_CAPTURED");
    }
}
