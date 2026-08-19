using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CandyBeltSort
{
    public class FactoryArena : MonoBehaviour
    {
        public ConveyorBelt Belt { get; private set; }
        public BoxRack Rack { get; private set; }
        public Camera Cam { get; private set; }
        public Transform CandyRoot { get; private set; }

        MeshRenderer _floor;
        MeshRenderer _wall;

        public void Build(WorldInfo world)
        {
            Clear();

            var root = new GameObject("Arena");
            root.transform.SetParent(transform, false);

            _floor = MeshFactory.Primitive(PrimitiveType.Cube, "Floor", root.transform, new Vector3(0f, -0.05f, 2.2f), new Vector3(14f, 0.1f, 16f), world.Floor).GetComponent<MeshRenderer>();
            _wall = MeshFactory.Primitive(PrimitiveType.Cube, "Wall", root.transform, new Vector3(0f, 3f, 8.6f), new Vector3(14f, 7f, 0.3f), world.Wall).GetComponent<MeshRenderer>();
            MeshFactory.Primitive(PrimitiveType.Cube, "LeftWall", root.transform, new Vector3(-7f, 2.2f, 2f), new Vector3(0.3f, 5f, 16f), Color.Lerp(world.Wall, Color.white, 0.1f));
            MeshFactory.Primitive(PrimitiveType.Cube, "RightWall", root.transform, new Vector3(7f, 2.2f, 2f), new Vector3(0.3f, 5f, 16f), Color.Lerp(world.Wall, Color.white, 0.1f));

            BuildBelt(root.transform);

            CandyRoot = new GameObject("Candies").transform;
            CandyRoot.SetParent(root.transform, false);

            var rackGo = new GameObject("BoxRack");
            rackGo.transform.SetParent(root.transform, false);
            Rack = rackGo.AddComponent<BoxRack>();

            BuildLights(root.transform);
            BuildCamera(root.transform, world);
        }

        public void Tint(WorldInfo world)
        {
            if (_floor != null)
            {
                var m = _floor.material;
                m.color = world.Floor;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", world.Floor);
            }
            if (_wall != null)
            {
                var m = _wall.material;
                m.color = world.Wall;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", world.Wall);
            }
            if (Cam != null)
                Cam.backgroundColor = Color.Lerp(world.Wall, Color.white, 0.25f);
        }

        void BuildBelt(Transform parent)
        {
            var beltRoot = new GameObject("Belt");
            beltRoot.transform.SetParent(parent, false);
            Belt = beltRoot.AddComponent<ConveyorBelt>();

            MeshFactory.Primitive(PrimitiveType.Cube, "Bed", beltRoot.transform, new Vector3(0f, 0.28f, 3f), new Vector3(1.7f, 0.22f, 9.2f), Palette.Hex("455A64"));
            MeshFactory.Primitive(PrimitiveType.Cube, "RailL", beltRoot.transform, new Vector3(-0.92f, 0.48f, 3f), new Vector3(0.12f, 0.28f, 9.2f), Palette.Hex("FFD54F"));
            MeshFactory.Primitive(PrimitiveType.Cube, "RailR", beltRoot.transform, new Vector3(0.92f, 0.48f, 3f), new Vector3(0.12f, 0.28f, 9.2f), Palette.Hex("FFD54F"));
            MeshFactory.Primitive(PrimitiveType.Cube, "Hopper", beltRoot.transform, new Vector3(0f, 1.1f, 7.35f), new Vector3(1.6f, 1.4f, 1.1f), Palette.Hex("90A4AE"));
            MeshFactory.Primitive(PrimitiveType.Cube, "FailBin", beltRoot.transform, new Vector3(0f, 0.15f, -1.55f), new Vector3(2.2f, 0.2f, 1.1f), Palette.Hex("EF9A9A"));

            for (int i = 0; i < 8; i++)
            {
                float z = 7f - i * 1.15f;
                MeshFactory.Primitive(PrimitiveType.Cube, "Slat", beltRoot.transform, new Vector3(0f, 0.41f, z), new Vector3(1.5f, 0.04f, 0.18f), Palette.Hex("FFE082"));
            }
        }

        void BuildLights(Transform parent)
        {
            foreach (var existing in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (existing.transform.root != transform)
                    existing.enabled = false;
            }

            var sun = new GameObject("Sun");
            sun.transform.SetParent(parent, false);
            sun.transform.rotation = Quaternion.Euler(48f, -30f, 0f);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.color = new Color(1f, 0.97f, 0.92f);
            light.shadows = LightShadows.Soft;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(parent, false);
            fill.transform.position = new Vector3(-3f, 4f, -2f);
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.range = 18f;
            fillLight.intensity = 2.2f;
            fillLight.color = new Color(1f, 0.85f, 0.95f);
        }

        void BuildCamera(Transform parent, WorldInfo world)
        {
            foreach (var existing in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (existing.GetComponent<AudioListener>() != null)
                    Destroy(existing.GetComponent<AudioListener>());
                existing.enabled = false;
            }

            var camGo = new GameObject("GameCamera");
            camGo.transform.SetParent(parent, false);
            camGo.transform.position = new Vector3(0f, 9.6f, -8.4f);
            camGo.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            Cam = camGo.AddComponent<Camera>();
            var additional = Cam.GetUniversalAdditionalCameraData();
            additional.renderPostProcessing = false;
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = Color.Lerp(world.Wall, Color.white, 0.25f);
            Cam.fieldOfView = 48f;
            Cam.nearClipPlane = 0.1f;
            Cam.farClipPlane = 60f;
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
        }

        void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }
    }
}
