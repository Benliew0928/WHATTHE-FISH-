using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Imports the modular football art into an isolated, inspectable review prefab.</summary>
public static class SunvaleStadiumBuilder {
    const string Art = "Assets/_Game/Art/Football/Sunvale/";
    const string Prefabs = "Assets/_Game/Prefabs/Football/Sunvale/";
    const string ScenePath = "Assets/_Game/Scenes/SunvaleStadiumReview.unity";
    static readonly string[] Modules = { "Stadium", "Lawn", "Goal", "Perimeter", "Banners", "Landscape", "Floodlights", "Scoreboard", "CornerFlags", "Dugouts" };

    [MenuItem("Sports/Football/Build Sunvale review assets")]
    public static void Build() {
        // The menu may be used interactively: never discard an unsaved scene.
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Prefabs);
        AssetDatabase.Refresh();
        var textureImporter = (TextureImporter)AssetImporter.GetAtPath(Art + "Sunvale_PaintedPalette.png");
        textureImporter.textureType = TextureImporterType.Default;
        textureImporter.sRGBTexture = true;
        textureImporter.mipmapEnabled = true;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.maxTextureSize = 1024;
        textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
        textureImporter.anisoLevel = 2;
        textureImporter.SaveAndReimport();
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) throw new Exception("URP Lit shader missing");
        var matPath = Art + "Sunvale_Painted.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); }
        mat.shader = shader;
        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "Sunvale_PaintedPalette.png"));
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Smoothness", .22f);
        mat.SetFloat("_Metallic", 0);
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);

        foreach (var name in Modules) {
            var importer = (ModelImporter)AssetImporter.GetAtPath(Art + name + ".fbx");
            if (!importer) throw new Exception("Missing module: " + name);
            importer.globalScale = 1;
            importer.useFileScale = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.preserveHierarchy = true;
            importer.isReadable = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.SaveAndReimport();
        }
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var assembly = new GameObject("Sunvale Stadium — Modular Assembly");
        var modulePrefabs = new Dictionary<string, GameObject>();
        foreach (var name in Modules) {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Art + name + ".fbx");
            var root = (GameObject)PrefabUtility.InstantiatePrefab(source);
            root.name = name;
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true)) {
                renderer.sharedMaterials = Enumerable.Repeat(mat, renderer.sharedMaterials.Length).ToArray();
                renderer.enabled = true;
                renderer.gameObject.SetActive(true);
                renderer.gameObject.layer = 8;
                // Keep module transforms editable; renderer-level static batching can be chosen at integration.
                renderer.shadowCastingMode = ShadowCastingMode.On;
                if (renderer.name == "Scoreboard__Lettering") renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            if (name == "Stadium") ConfigureSeatLODs(root);
            if (name == "Lawn") AddBox(root, "Collision_Lawn", new Vector3(0, -.18f, 0), new Vector3(78, .36f, 115));
            if (name == "Goal") {
                foreach (float x in new[] { -3.72f, 3.72f }) AddBox(root, "Collision_Post", new Vector3(x, 1.25f, 0), new Vector3(.12f, 2.5f, .12f));
                AddBox(root, "Collision_Crossbar", new Vector3(0, 2.5f, 0), new Vector3(7.56f, .12f, .12f));
            }
            modulePrefabs[name] = PrefabUtility.SaveAsPrefabAsset(root, Prefabs + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }
        foreach (var name in Modules) {
            var root = (GameObject)PrefabUtility.InstantiatePrefab(modulePrefabs[name]);
            root.transform.SetParent(assembly.transform, false);
            if (name == "Goal") {
                root.name = "Goal_North";
                root.transform.localPosition = new Vector3(0, 0, -52.56f);
                var south = (GameObject)PrefabUtility.InstantiatePrefab(modulePrefabs[name]);
                south.name = "Goal_South";
                south.transform.SetParent(assembly.transform, false);
                south.transform.localPosition = new Vector3(0, 0, 52.56f);
                south.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
        var collisions = new GameObject("Collision_RunoffBoundary");
        collisions.transform.SetParent(assembly.transform, false);
        foreach (float s in new[] { -1f, 1f }) {
            AddBox(collisions, "Touchline runoff", new Vector3(s * 38.7f, 1.5f, 0), new Vector3(.3f, 3, 116));
            AddBox(collisions, "End runoff", new Vector3(0, 1.5f, s * 58), new Vector3(77.4f, 3, .3f));
        }
        PrefabUtility.SaveAsPrefabAsset(assembly, Prefabs + "SunvaleStadium.prefab");
        SetupReviewScene();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets();
        Audit(assembly, mat);
        RenderReview();
        Debug.Log("SUNVALE_UNITY_COMPLETE " + ScenePath);
    }

    static void ConfigureSeatLODs(GameObject root) {
        // Unity infers a whole-model LODGroup from FBX name suffixes; replace it
        // with per-bay groups so nearby seats do not force the full bowl to LOD0.
        foreach (var imported in root.GetComponentsInChildren<LODGroup>(true)) UnityEngine.Object.DestroyImmediate(imported);
        var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        for (int bay = 0; bay < 32; bay++) {
            var prefix = "Stadium__Bay_" + bay.ToString("00") + "_Seats_LOD";
            var near = renderers.Single(r => r.name == prefix + "0");
            var far = renderers.Single(r => r.name == prefix + "1");
            var go = new GameObject("Seating_Bay_" + bay.ToString("00"));
            go.transform.SetParent(near.transform.parent, false);
            near.transform.SetParent(go.transform, true);
            far.transform.SetParent(go.transform, true);
            var lod = go.AddComponent<LODGroup>();
            lod.fadeMode = LODFadeMode.None;
            lod.SetLODs(new[] { new LOD(.13f, new Renderer[] { near }), new LOD(.008f, new Renderer[] { far }) });
            lod.RecalculateBounds();
        }
    }

    static void AddBox(GameObject parent, string name, Vector3 centre, Vector3 size) {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.layer = 8;
        var collider = go.AddComponent<BoxCollider>();collider.center = centre;collider.size = size;
    }

    static void SetupReviewScene() {
        var sun = new GameObject("Sunvale warm daylight").AddComponent<Light>();
        sun.type = LightType.Directional;sun.intensity = 1.25f;sun.color = new Color(1, .95f, .83f);
        sun.shadows = LightShadows.Soft;sun.transform.rotation = Quaternion.Euler(56, -35, 0);
        RenderSettings.sun = sun;RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.48f, .58f, .70f);RenderSettings.fog = false;
        var camera = new GameObject("Sunvale pitch review camera").AddComponent<Camera>();
        camera.tag = "MainCamera";camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.36f, .67f, .92f);camera.fieldOfView = 69;
        camera.nearClipPlane = .1f;camera.farClipPlane = 500;
        // Unity's FBX handedness conversion maps Blender (x,y,z) to (-x,z,-y).
        camera.transform.position = new Vector3(-13, 2.8f, 19);
        camera.transform.LookAt(new Vector3(3, 8, -56));
        camera.AdditionalCameraData().renderPostProcessing = false;
    }

    static UniversalAdditionalCameraData AdditionalCameraData(this Camera camera) {
        return camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
    }

    static void Audit(GameObject assembly, Material material) {
        var filters = assembly.GetComponentsInChildren<MeshFilter>(true);
        var bounds = new Bounds();bool first = true;
        long near = 0, far = 0;
        foreach (var filter in filters) {
            var renderer = filter.GetComponent<MeshRenderer>();
            if (renderer.sharedMaterials.Any(m => m != material)) throw new Exception("Material not remapped: " + filter.name);
            var mesh = filter.sharedMesh;
            long triangles = 0;
            for (int s = 0; s < mesh.subMeshCount; s++) triangles += (long)mesh.GetIndexCount(s) / 3;
            if (!filter.name.EndsWith("LOD1")) near += triangles;
            if (!filter.name.EndsWith("LOD0")) far += triangles;
            if (first) { bounds = renderer.bounds;first = false; } else bounds.Encapsulate(renderer.bounds);
        }
        var lawn = assembly.transform.Find("Lawn").GetComponentsInChildren<MeshRenderer>().Single(r => r.name == "Lawn__Pitch");
        if (Mathf.Abs(lawn.bounds.size.x - 68) > .05f || Mathf.Abs(lawn.bounds.size.z - 105) > .05f)
            throw new Exception("FBX pitch scale/axis mismatch: " + lawn.bounds);
        if (assembly.GetComponentsInChildren<LODGroup>().Length != 32) throw new Exception("Missing seat LOD groups");
        var goals = assembly.GetComponentsInChildren<MeshRenderer>().Where(r => r.name == "Goal__Frame").ToArray();
        if (goals.Length != 2 || goals.Any(g => Mathf.Abs(g.bounds.size.x - 7.56f) > .05f)) throw new Exception("Goal import mismatch");
        var report = "Sunvale Unity import audit\n" +
            "Mesh renderers: " + filters.Length + "\nMaterials: 1, URP/Lit, shared painted atlas\n" +
            "All-near triangles: " + near + "\nAll-far triangles: " + far + "\n" +
            "Seat LOD groups: 32\nPitch bounds: " + lawn.bounds + "\nAssembly bounds: " + bounds + "\n" +
            "Goals: 2; 7.32 x 2.44 m internal opening\nColliders: " + assembly.GetComponentsInChildren<Collider>().Length + "\n" +
            "Runtime performance has not been profiled on Android.\n";
        File.WriteAllText(Path.GetFullPath("../ArtSource/Football/Sunvale/unity-import-audit.txt"), report);
        Debug.Log(report);
    }

    static void RenderReview() {
        var camera = Camera.main;
        // Desktop art capture only. Restore the game's mobile pipeline afterwards.
        var oldPipeline = QualitySettings.renderPipeline;
        var active = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        var reviewPipeline = UnityEngine.Object.Instantiate(active);
        reviewPipeline.renderScale = 1;
        reviewPipeline.msaaSampleCount = 4;
        reviewPipeline.shadowDistance = 180;
        reviewPipeline.mainLightShadowmapResolution = 4096;
        reviewPipeline.shadowCascadeCount = 4;
        var reviewSettings = new SerializedObject(reviewPipeline);
        reviewSettings.FindProperty("m_SoftShadowsSupported").boolValue = true;
        reviewSettings.ApplyModifiedPropertiesWithoutUndo();
        QualitySettings.renderPipeline = reviewPipeline;
        var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
        var previous = RenderTexture.active;
        var oldTarget = camera.targetTexture;
        var oldPosition = camera.transform.position;
        var oldRotation = camera.transform.rotation;
        var oldFov = camera.fieldOfView;
        var image = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
        try {
            camera.targetTexture = rt;camera.Render();RenderTexture.active = rt;
            image.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0);image.Apply();
            File.WriteAllBytes(Path.GetFullPath("../Docs/VisualDirection/Stadium/04_Unity_Pitch_View.png"), image.EncodeToPNG());
            camera.transform.position = new Vector3(-24, 4.4f, -28);
            camera.transform.LookAt(new Vector3(-40, 9, -37));
            camera.fieldOfView = 65;
            camera.Render();RenderTexture.active = rt;
            image.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0);image.Apply();
            File.WriteAllBytes(Path.GetFullPath("../Docs/VisualDirection/Stadium/05_Unity_Architecture_Detail.png"), image.EncodeToPNG());
        } finally {
            camera.targetTexture = oldTarget;RenderTexture.active = previous;
            camera.transform.SetPositionAndRotation(oldPosition, oldRotation);camera.fieldOfView = oldFov;
            QualitySettings.renderPipeline = oldPipeline;
            UnityEngine.Object.DestroyImmediate(reviewPipeline);
            UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
