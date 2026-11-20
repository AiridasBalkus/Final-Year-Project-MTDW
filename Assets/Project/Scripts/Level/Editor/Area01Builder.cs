using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

// Rebuilds the Area01 grey box under a "Level" root in the open scene, so the layout can be tweaked
// here and rebuilt. Path runs along +Z: start room, checkpoint 1, corridor, hall, antechamber, door, arena.
// Enemies and the player aren't touched, they're placed in the scene.
public static class Area01Builder
{
    const float WallHeight = 4f;
    const float WallThickness = 0.5f;
    const float DoorGap = 4f; // width of the openings between rooms
    const string MaterialFolder = "Assets/Project/Materials/";

    static Transform root;
    static Material wallMat, checkpointMat, doorMat, bossMat;

    [MenuItem("MTDW/Level/Rebuild Area01 Grey Box")]
    static void Build()
    {
        GameObject old = GameObject.Find("Level");
        if (old != null) Object.DestroyImmediate(old);

        root = new GameObject("Level").transform;
        wallMat = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.probuilder/Content/Resources/Materials/ProBuilderDefault.mat");
        checkpointMat = GetMaterial("Greybox_Checkpoint", new Color(1f, 0.55f, 0.15f));
        doorMat = GetMaterial("Greybox_Door", new Color(0.3f, 0.2f, 0.15f));
        bossMat = GetMaterial("Greybox_BossMarker", new Color(0.7f, 0.1f, 0.1f));

        // floors and side walls, x0, x1, z0, z1
        Room("1_StartRoom", -7f, 7f, 0f, 14f);
        Room("2_Checkpoint", -4f, 4f, 14f, 20f);
        Room("3_Corridor", -2f, 2f, 20f, 42f);
        Room("4_Hall", -11f, 11f, 42f, 64f);
        Room("5_Antechamber", -4f, 4f, 64f, 72f);
        Room("6_BossArena", -12f, 12f, 72f, 96f);

        // walls across each boundary, as wide as the wider room, with a centred opening
        Divider(0f, 7f, 0f);
        Divider(14f, 7f, DoorGap);
        Divider(20f, 4f, DoorGap);
        Divider(42f, 11f, DoorGap);
        Divider(64f, 11f, DoorGap);
        Divider(72f, 12f, DoorGap);
        Divider(96f, 12f, 0f);

        Transform hall = root.Find("4_Hall");
        Box("Pillar", hall, new Vector3(-6.5f, WallHeight / 2f, 47.5f), new Vector3(1.5f, WallHeight, 1.5f), wallMat);
        Box("Pillar", hall, new Vector3(6.5f, WallHeight / 2f, 47.5f), new Vector3(1.5f, WallHeight, 1.5f), wallMat);
        Box("Pillar", hall, new Vector3(-6.5f, WallHeight / 2f, 58.5f), new Vector3(1.5f, WallHeight, 1.5f), wallMat);
        Box("Pillar", hall, new Vector3(6.5f, WallHeight / 2f, 58.5f), new Vector3(1.5f, WallHeight, 1.5f), wallMat);

        GameLoop gameLoop = Object.FindAnyObjectByType<GameLoop>();
        CheckpointAt("Checkpoint_01", new Vector3(2.5f, 0.6f, 17f), gameLoop);
        CheckpointAt("Checkpoint_02", new Vector3(2.5f, 0.6f, 68f), gameLoop);

        // left out of the NavMesh so the doorway is walkable once it opens; not static, it moves
        GameObject door = Box("BossDoor", root, new Vector3(0f, WallHeight / 2f, 72f), new Vector3(DoorGap, WallHeight, WallThickness), doorMat);
        GameObjectUtility.SetStaticEditorFlags(door, 0);
        door.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
        door.AddComponent<BossDoor>().range = 2.5f;

        // flat marker, no collider so it doesn't block anything
        GameObject boss = Box("BossSpawn", root, new Vector3(0f, 0.01f, 86f), new Vector3(3f, 0.02f, 3f), bossMat);
        Object.DestroyImmediate(boss.GetComponent<MeshCollider>());
        boss.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

        NavMeshSurface surface = root.gameObject.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;

        Selection.activeGameObject = root.gameObject;
        Debug.Log("[Level] Area01 grey box rebuilt, bake the NavMesh on Level");
    }

    static void Room(string name, float x0, float x1, float z0, float z1)
    {
        Transform room = new GameObject(name).transform;
        room.SetParent(root, false);

        float w = x1 - x0, d = z1 - z0, cx = (x0 + x1) / 2f, cz = (z0 + z1) / 2f;
        Box("Floor", room, new Vector3(cx, -0.1f, cz), new Vector3(w, 0.2f, d), wallMat);
        Box("Wall_W", room, new Vector3(x0 - WallThickness / 2f, WallHeight / 2f, cz), new Vector3(WallThickness, WallHeight, d), wallMat);
        Box("Wall_E", room, new Vector3(x1 + WallThickness / 2f, WallHeight / 2f, cz), new Vector3(WallThickness, WallHeight, d), wallMat);
    }

    // a wall across x = -halfWidth..halfWidth (plus the corners), split around a centred gap
    static void Divider(float z, float halfWidth, float gap)
    {
        float outer = halfWidth + WallThickness;
        float y = WallHeight / 2f;
        if (gap <= 0f)
        {
            Box($"Wall_{z}", root, new Vector3(0f, y, z), new Vector3(outer * 2f, WallHeight, WallThickness), wallMat);
            return;
        }
        float half = gap / 2f;
        float width = outer - half;
        Box($"Wall_{z}_L", root, new Vector3(-(half + width / 2f), y, z), new Vector3(width, WallHeight, WallThickness), wallMat);
        Box($"Wall_{z}_R", root, new Vector3(half + width / 2f, y, z), new Vector3(width, WallHeight, WallThickness), wallMat);
    }

    // marker plus a respawn point beside it in the middle of the room, facing on along +Z
    static void CheckpointAt(string name, Vector3 position, GameLoop gameLoop)
    {
        GameObject marker = Box(name, root, position, new Vector3(0.6f, 1.2f, 0.6f), checkpointMat);
        Transform spawn = new GameObject("RespawnPoint").transform;
        spawn.SetParent(marker.transform, false);
        spawn.position = new Vector3(0f, 0f, position.z);

        Checkpoint checkpoint = marker.AddComponent<Checkpoint>();
        checkpoint.range = 2f;
        checkpoint.respawnPoint = spawn;
        checkpoint.gameLoop = gameLoop;
    }

    static GameObject Box(string name, Transform parent, Vector3 center, Vector3 size, Material mat)
    {
        ProBuilderMesh mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
        mesh.GetComponent<MeshRenderer>().sharedMaterial = mat;
        mesh.ToMesh();
        mesh.Refresh();

        GameObject go = mesh.gameObject;
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = center;
        go.AddComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
        return go;
    }

    static Material GetMaterial(string name, Color color)
    {
        string path = MaterialFolder + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
