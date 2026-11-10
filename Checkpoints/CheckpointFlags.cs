using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSpeed.GhostCar;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Vector2List = Il2CppSystem.Collections.Generic.List<UnityEngine.Vector2>;

namespace WtcPractice;

public class CheckpointFlags
{
    private const string DungeonScene = "OverworldDungeon-Entrance";
    private const string RoomsRoot = "--- ROOMS ---";
    private const string PoleName = "FinishLine.Flag.Pole";
    private const string ClothName = "FinishLine.Flag.Cloth";
    private const string GhostBodyName = "Body";
    private const int PaletteSlotChannel = 3;
    private const float GhostBodySlot = 34.09375f;
    private const int SettleFrames = 5;

    private const string NumberFontName = "MuseoSansRounded1000 SDF";
    private const float NumberX = -0.355f;
    private const float NumberDepth = 0.02f;
    private const float NumberAreaWidth = 0.71f;
    private const float NumberAreaHeight = 0.54f;
    private const float NumberFontSize = 4.5f;

    private const float ClothHeight = 1.33f;
    private const float CurrentScale = 1.5f;
    private const float OtherScale = 1.05f;

    private static readonly Color NumberColor = new(0f, 0f, 0f, 0.65f);

    private static readonly string[] FlagPaths =
    {
        "Start Room/Levels/SpecialAreaToggler/HomeDecoration_Bestof2025/Prop_AwardBear_BestOf/AwardBearStatue/SignBestOf/FinishLine_Flag",
        "Start Room/Levels/SpecialAreaToggler/HomeDecoration_Bestof2024/Prop_AwardBear_BestOf/AwardBearStatue/SignBestOf/FinishLine_Flag",
        "Start Room/Levels/SpecialAreaToggler/HomeDecoration_BestOf2023/Prop_AwardBear_BestOf/AwardBearStatue/SignBestOf/FinishLine_Flag",
    };

    private readonly MelonLogger.Instance logger;
    private readonly Dictionary<int, Mesh> slotStreams = new();

    private GameObject template;
    private GameObject root;
    private Material paint;
    private ShadowCastingMode shadowCastingMode;
    private int paintLayer;
    private bool paintStale = true;
    private bool paintWarned;
    private TMP_FontAsset numberFont;
    private bool fontWarned;
    private int framesUntilHarvest;

    public CheckpointFlags(MelonLogger.Instance logger)
    {
        this.logger = logger;
    }

    public void OnSceneInitialized(string sceneName)
    {
        paintStale = true;

        if (sceneName == DungeonScene && template == null)
            framesUntilHarvest = SettleFrames;
    }

    public void Update(bool inLevel, CheckpointList checkpoints)
    {
        if (framesUntilHarvest > 0 && --framesUntilHarvest == 0)
            Harvest();

        if (root != null && root.activeSelf != inLevel)
            root.SetActive(inLevel);

        if (inLevel && paintStale && checkpoints.Count > 0)
            Sync(checkpoints);
    }

    public void Sync(CheckpointList checkpoints)
    {
        if (template == null || !EnsurePaint())
            return;

        EnsureRoot();

        for (int i = 0; i < checkpoints.Count; i++)
        {
            Checkpoint checkpoint = checkpoints[i];

            if (checkpoint.Flag == null)
                checkpoint.Flag = CreateFlag(checkpoint.Car, i + 1);

            Paint(checkpoint.Flag);
            float scale = checkpoint == checkpoints.Current ? CurrentScale : OtherScale;
            checkpoint.Flag.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private void Harvest()
    {
        Transform source = FindSource();
        Mesh pole = MeshOf(source, PoleName);
        Mesh cloth = MeshOf(source, ClothName);

        if (pole == null || cloth == null)
        {
            logger.Warning("Couldn't find the flag in the dungeon, checkpoints won't show flags");
            return;
        }

        template = new GameObject("Practice checkpoint flag");
        template.SetActive(false);
        Object.DontDestroyOnLoad(template);

        AddPart(pole, Vector3.zero);
        AddPart(cloth, new Vector3(0f, ClothHeight, 0f));
    }

    private static Transform FindSource()
    {
        Scene scene = SceneManager.GetSceneByName(DungeonScene);
        if (!scene.IsValid() || !scene.isLoaded)
            return null;

        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name != RoomsRoot)
                continue;

            foreach (string path in FlagPaths)
            {
                Transform found = FindPath(roots[i].transform, path);
                if (found != null)
                    return found;
            }
        }

        return null;
    }

    private static Transform FindPath(Transform from, string path)
    {
        Transform current = from;
        foreach (string name in path.Split('/'))
        {
            current = FindChild(current, name);
            if (current == null)
                return null;
        }

        return current;
    }

    private static Transform FindChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
                return child;
        }

        return null;
    }

    private static Transform FindDescendant(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
                return child;

            Transform found = FindDescendant(child, name);
            if (found != null)
                return found;
        }

        return null;
    }

    private static Mesh MeshOf(Transform source, string name)
    {
        if (source == null)
            return null;

        Transform part = FindChild(source, name);
        MeshFilter filter = part == null ? null : part.GetComponent<MeshFilter>();
        return filter == null ? null : filter.sharedMesh;
    }

    private void AddPart(Mesh mesh, Vector3 localPosition)
    {
        GameObject part = new(mesh.name);
        part.transform.SetParent(template.transform, false);
        part.transform.localPosition = localPosition;
        part.AddComponent<MeshFilter>().sharedMesh = mesh;
        part.AddComponent<MeshRenderer>();
    }

    private bool EnsurePaint()
    {
        if (!paintStale && paint != null)
            return true;

        GhostCarManager manager = Object.FindObjectOfType<GhostCarManager>();
        if (manager == null || manager.baseGhostCarPrefab == null)
            return false;

        Transform body = FindDescendant(manager.baseGhostCarPrefab.transform, GhostBodyName);
        Renderer ghostRenderer = body == null ? null : body.GetComponent<Renderer>();
        if (ghostRenderer == null)
        {
            if (!paintWarned)
                logger.Warning("Couldn't find the ghost material, checkpoints won't show flags");

            paintWarned = true;
            return false;
        }

        paint = ghostRenderer.sharedMaterial;
        shadowCastingMode = ghostRenderer.shadowCastingMode;
        paintLayer = ghostRenderer.gameObject.layer;
        paintStale = false;
        return true;
    }

    private void EnsureRoot()
    {
        if (root != null)
            return;

        root = new GameObject("Practice checkpoint flags");
        Object.DontDestroyOnLoad(root);
    }

    private GameObject CreateFlag(CarSnapshot car, int number)
    {
        GameObject flag = Object.Instantiate(template, root.transform);

        Vector3 forward = Flat(car.Rotation * Vector3.forward);
        Quaternion rotation = forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(-forward);
        flag.transform.SetPositionAndRotation(car.Position, rotation);
        flag.SetActive(true);

        if (EnsureFont())
            AddNumber(flag, number);

        return flag;
    }

    private bool EnsureFont()
    {
        if (numberFont != null)
            return true;

        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        for (int i = 0; i < fonts.Length; i++)
        {
            if (fonts[i] != null && fonts[i].name == NumberFontName)
                numberFont = fonts[i];
        }

        if (numberFont == null && !fontWarned)
        {
            logger.Warning("Couldn't find the game's font, flags won't show numbers");
            fontWarned = true;
        }

        return numberFont != null;
    }

    private void AddNumber(GameObject flag, int number)
    {
        GameObject holder = new("Number");
        holder.layer = paintLayer;

        RectTransform rect = holder.AddComponent<RectTransform>();
        rect.SetParent(flag.transform, false);
        rect.localPosition = new Vector3(NumberX, ClothHeight, NumberDepth);
        rect.localRotation = Quaternion.Euler(0f, 180f, 0f);
        rect.sizeDelta = new Vector2(NumberAreaWidth, NumberAreaHeight);

        TextMeshPro label = holder.AddComponent<TextMeshPro>();
        label.font = numberFont;
        label.fontSize = NumberFontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = NumberColor;
        label.text = number.ToString();
    }

    private static Vector3 Flat(Vector3 direction)
    {
        direction.y = 0f;
        return direction.normalized;
    }

    private void Paint(GameObject flag)
    {
        var renderers = flag.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].GetComponent<TextMeshPro>() != null)
                continue;

            Mesh mesh = renderers[i].GetComponent<MeshFilter>().sharedMesh;

            Il2CppReferenceArray<Material> materials = new(mesh.subMeshCount);
            for (int slot = 0; slot < mesh.subMeshCount; slot++)
                materials[slot] = paint;

            renderers[i].sharedMaterials = materials;
            renderers[i].shadowCastingMode = shadowCastingMode;
            renderers[i].gameObject.layer = paintLayer;
            renderers[i].additionalVertexStreams = SlotStream(mesh.vertexCount);
        }
    }

    private Mesh SlotStream(int vertexCount)
    {
        if (slotStreams.TryGetValue(vertexCount, out Mesh existing) && existing != null)
            return existing;

        Il2CppStructArray<VertexAttributeDescriptor> layout = new(1);
        layout[0] = new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, 2, 0);

        Vector2List slots = new(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            slots.Add(new Vector2(GhostBodySlot, 0f));

        Mesh stream = new() { hideFlags = HideFlags.HideAndDontSave };
        stream.SetVertexBufferParams(vertexCount, layout);
        stream.SetUVs(PaletteSlotChannel, slots);
        slotStreams[vertexCount] = stream;
        return stream;
    }
}
