using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CandyBeltSort
{
    public class FactoryArena : MonoBehaviour
    {
        public ConveyorBelt[] Belts { get; private set; }
        public ConveyorBelt Belt => Belts != null && Belts.Length > 0 ? Belts[0] : null;
        public BoxRack Rack { get; private set; }
        public Camera Cam { get; private set; }
        public Transform CandyRoot { get; private set; }
        public Transform ArenaRoot { get; private set; }
        public Vector3 ShipDock { get; private set; }

        MeshRenderer _floor;
        MeshRenderer _wall;

        static readonly Color Steel = Palette.Hex("607D8B");
        static readonly Color SteelDark = Palette.Hex("37474F");
        static readonly Color Wood = Palette.Hex("A1887F");
        static readonly Color Brass = Palette.Hex("F4D03F");

        public void Build(WorldInfo world, int laneCount = 1)
        {
            Clear();
            laneCount = Mathf.Clamp(laneCount, 1, 2);

            var root = new GameObject("Arena");
            root.transform.SetParent(transform, false);

            BuildRoom(root.transform, world);
            DecorateKitchen(root.transform, world);
            BuildBelts(root.transform, laneCount, world);

            CandyRoot = new GameObject("Candies").transform;
            CandyRoot.SetParent(root.transform, false);

            var rackGo = new GameObject("BoxRack");
            rackGo.transform.SetParent(root.transform, false);
            Rack = rackGo.AddComponent<BoxRack>();
            ArenaRoot = root.transform;
            ShipDock = new Vector3(6.4f, 0.7f, -0.2f);

            DisableOtherLights();
            BuildLights(root.transform);
            BuildCamera(root.transform, world, laneCount);
            FaceGlows(root.transform);
        }

        static void FaceGlows(Transform root)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var rot = cam.transform.rotation;
            var glows = root.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < glows.Length; i++)
            {
                if (glows[i] == null || glows[i].name.IndexOf("Glow", System.StringComparison.Ordinal) < 0) continue;
                glows[i].transform.rotation = rot;
            }
        }

        public void Tint(WorldInfo world)
        {
            if (_wall != null) _wall.sharedMaterial = MeshFactory.Textured(SpriteFactory.BrickTex(), Color.Lerp(world.Wall, Color.white, 0.15f), 0.18f, 5f, 2.4f);
            if (Cam != null)
                Cam.backgroundColor = Color.Lerp(world.Wall, Palette.Hex("B3E5FC"), 0.35f);
        }

        public ConveyorBelt BeltAt(int lane)
        {
            if (Belts == null || Belts.Length == 0) return null;
            lane = Mathf.Clamp(lane, 0, Belts.Length - 1);
            return Belts[lane];
        }

        void BuildRoom(Transform parent, WorldInfo world)
        {
            var grout = Color.Lerp(world.Floor, Palette.Hex("BCAAA4"), 0.4f);
            var tileA = Color.Lerp(world.Floor, Color.white, 0.18f);
            var tileB = Color.Lerp(world.Floor, Palette.Hex("F8BBD0"), 0.22f);

            var floor = MeshFactory.TexturedSolid("Floor", parent, new Vector3(0f, -0.06f, 2.1f), new Vector3(20f, 0.12f, 22f),
                SpriteFactory.FloorTex(grout, tileA, tileB), Color.white, 0.12f, 6f, 6f);
            _floor = floor.GetComponent<MeshRenderer>();

            var wallTex = SpriteFactory.BrickTex();
            var wallTint = Color.Lerp(world.Wall, Color.white, 0.15f);
            var back = MeshFactory.TexturedSolid("Wall", parent, new Vector3(0f, 3.15f, 9.55f), new Vector3(20f, 6.5f, 0.4f), wallTex, wallTint, 0.16f, 5f, 2.4f);
            _wall = back.GetComponent<MeshRenderer>();
            MeshFactory.TexturedSolid("LeftWall", parent, new Vector3(-9.8f, 3.0f, 2.0f), new Vector3(0.4f, 6.2f, 16f), wallTex, MeshFactory.Shade(wallTint, 0.92f), 0.16f, 4f, 2.2f);
            MeshFactory.TexturedSolid("RightWall", parent, new Vector3(9.8f, 3.0f, 2.0f), new Vector3(0.4f, 6.2f, 16f), wallTex, MeshFactory.Shade(wallTint, 0.88f), 0.16f, 4f, 2.2f);

            var wood = SpriteFactory.WoodTex();
            MeshFactory.TexturedSolid("WainscotB", parent, new Vector3(0f, 0.55f, 9.32f), new Vector3(19.6f, 1.15f, 0.22f), wood, Color.Lerp(world.Wall, Wood, 0.4f), 0.2f, 8f, 1.2f);
            MeshFactory.TexturedSolid("BaseB", parent, new Vector3(0f, 0.08f, 9.28f), new Vector3(19.8f, 0.18f, 0.28f), wood, Wood, 0.18f, 10f, 0.4f);

            MeshFactory.Solid("SkyFill", parent, new Vector3(0f, 4.6f, 11.2f), new Vector3(24f, 8f, 0.2f), Color.Lerp(world.Wall, Palette.Hex("81D4FA"), 0.45f), 0.05f);
        }

        void DecorateKitchen(Transform parent, WorldInfo world)
        {
            var marble = SpriteFactory.MarbleTex();
            var wood = SpriteFactory.WoodTex();
            var enamel = SpriteFactory.EnamelTex();

            MeshFactory.TexturedBlock("SignBoard", parent, new Vector3(0f, 5.35f, 9.22f), new Vector3(5.2f, 1.05f, 0.18f), enamel, Color.Lerp(world.Accent, Palette.Hex("EF5350"), 0.4f), 0.45f, 2f, 1f);
            MakeWorldLabel(parent, "SUGAR KITCHEN", new Vector3(0f, 5.35f, 9.1f), 48, Color.white);

            Window3D(parent, new Vector3(-5.1f, 3.85f, 9.28f));
            Window3D(parent, new Vector3(5.1f, 3.85f, 9.28f));

            MeshFactory.TexturedSolid("Shelf", parent, new Vector3(0f, 1.55f, 8.85f), new Vector3(8.4f, 0.12f, 0.7f), wood, Palette.Hex("EFEBE9"), 0.28f, 4f, 1f);
            MeshFactory.TexturedSolid("ShelfLip", parent, new Vector3(0f, 1.46f, 8.55f), new Vector3(8.4f, 0.08f, 0.12f), wood, Wood, 0.2f, 4f, 0.3f);
            Jar(parent, new Vector3(-3.1f, 1.95f, 8.8f), Palette.Pink);
            Jar(parent, new Vector3(-1.85f, 1.95f, 8.8f), Palette.Mint);
            Jar(parent, new Vector3(-0.6f, 1.95f, 8.8f), Palette.Lemon);
            Jar(parent, new Vector3(0.7f, 1.95f, 8.8f), Palette.Blueberry);
            Jar(parent, new Vector3(2.0f, 1.95f, 8.8f), Palette.Grape);
            Jar(parent, new Vector3(3.2f, 1.95f, 8.8f), Palette.Hex("FF8A65"));

            MeshFactory.TexturedSolid("CounterL", parent, new Vector3(-6.15f, 0.42f, 2.4f), new Vector3(2.5f, 0.84f, 7.2f), marble, Color.white, 0.42f, 2f, 4f);
            MeshFactory.TexturedSolid("CounterLBase", parent, new Vector3(-6.15f, 0.18f, 2.4f), new Vector3(2.35f, 0.36f, 7.0f), wood, Palette.Hex("8D6E63"), 0.18f, 2f, 4f);
            MeshFactory.TexturedSolid("CounterR", parent, new Vector3(6.15f, 0.42f, 2.4f), new Vector3(2.5f, 0.84f, 7.2f), marble, Color.white, 0.42f, 2f, 4f);
            MeshFactory.TexturedSolid("CounterRBase", parent, new Vector3(6.15f, 0.18f, 2.4f), new Vector3(2.35f, 0.36f, 7.0f), wood, Palette.Hex("8D6E63"), 0.18f, 2f, 4f);

            Mixer(parent, new Vector3(-6.1f, 1.15f, 4.4f));
            Pan(parent, new Vector3(-6.05f, 0.92f, 1.6f));
            Bowl(parent, new Vector3(-6.2f, 0.95f, 0.2f), Palette.Pink);
            Oven(parent, new Vector3(6.15f, 1.35f, 4.2f));
            ShipBay(parent, new Vector3(6.35f, 0.9f, -0.25f));

            Pipe(parent, new Vector3(-3.4f, 5.6f, 6.4f), 7.2f);
            Pipe(parent, new Vector3(3.6f, 5.85f, 5.2f), 6.4f);
            Lamp(parent, new Vector3(-2.0f, 4.7f, 3.6f));
            Lamp(parent, new Vector3(2.0f, 4.7f, 3.6f));
            Lamp(parent, new Vector3(0f, 4.85f, 6.2f));

            MeshFactory.TexturedSolid("Sack1", parent, new Vector3(-7.3f, 0.45f, 7.4f), new Vector3(0.9f, 0.9f, 0.9f), SpriteFactory.CardboardTex(), Palette.Hex("FFCC80"), 0.2f, 1f, 1f);
            MeshFactory.TexturedSolid("Sack2", parent, new Vector3(-6.6f, 0.32f, 7.7f), new Vector3(0.7f, 0.64f, 0.7f), SpriteFactory.CardboardTex(), Palette.Hex("EF9A9A"), 0.2f, 1f, 1f);
            MeshFactory.Solid("Barrel", parent, new Vector3(7.35f, 0.5f, 7.3f), new Vector3(0.85f, 0.5f, 0.85f), Palette.Hex("8D6E63"), 0.22f, PrimitiveType.Cylinder);

            MeshFactory.TexturedSolid("Mat", parent, new Vector3(0f, 0.01f, -1.55f), new Vector3(4.8f, 0.04f, 1.7f), SpriteFactory.BeltTex(), Palette.Hex("EF9A9A"), 0.08f, 2f, 1f);

            SpriteFactory.Billboard("GlowOven", parent, new Vector3(6.15f, 1.15f, 3.55f), new Vector2(1.1f, 0.7f), SpriteFactory.Glow(), new Color(1f, 0.55f, 0.15f, 0.55f), 6);
        }

        static void Window3D(Transform parent, Vector3 pos)
        {
            MeshFactory.Block("WindowFrame", parent, pos, new Vector3(2.35f, 2.15f, 0.16f), Palette.Hex("ECEFF1"), 0.4f);
            MeshFactory.Solid("Glass", parent, pos + new Vector3(0f, 0f, -0.06f), new Vector3(1.95f, 1.75f, 0.05f), Palette.Hex("81D4FA"), 0.85f);
            MeshFactory.Solid("MullionV", parent, pos + new Vector3(0f, 0f, -0.08f), new Vector3(0.1f, 1.75f, 0.06f), Palette.Hex("EFEBE9"), 0.3f);
            MeshFactory.Solid("MullionH", parent, pos + new Vector3(0f, 0f, -0.08f), new Vector3(1.95f, 0.1f, 0.06f), Palette.Hex("EFEBE9"), 0.3f);
            MeshFactory.Solid("Sill", parent, pos + new Vector3(0f, -1.15f, -0.12f), new Vector3(2.5f, 0.12f, 0.28f), Palette.Hex("D7CCC8"), 0.25f);
        }

        static void Jar(Transform parent, Vector3 pos, Color jam)
        {
            MeshFactory.Solid("Jar", parent, pos, new Vector3(0.42f, 0.36f, 0.42f), Palette.Hex("E0F7FA"), 0.82f, PrimitiveType.Cylinder);
            MeshFactory.Solid("Jam", parent, pos + new Vector3(0f, -0.04f, 0f), new Vector3(0.34f, 0.2f, 0.34f), jam, 0.55f, PrimitiveType.Cylinder);
            MeshFactory.Solid("Lid", parent, pos + new Vector3(0f, 0.38f, 0f), new Vector3(0.38f, 0.06f, 0.38f), Wood, 0.25f, PrimitiveType.Cylinder);
        }

        static void Mixer(Transform parent, Vector3 pos)
        {
            MeshFactory.Solid("MixerBowl", parent, pos, new Vector3(0.95f, 0.28f, 0.95f), Palette.Hex("B0BEC5"), 0.62f, PrimitiveType.Cylinder);
            MeshFactory.Solid("MixerHead", parent, pos + new Vector3(0f, 0.7f, -0.15f), new Vector3(0.35f, 0.9f, 0.35f), Palette.Hex("EC407A"), 0.4f);
            MeshFactory.Solid("MixerArm", parent, pos + new Vector3(0f, 1.05f, 0.1f), new Vector3(0.22f, 0.18f, 0.7f), Palette.Hex("F48FB1"), 0.35f);
        }

        static void Pan(Transform parent, Vector3 pos)
        {
            MeshFactory.Solid("Pan", parent, pos, new Vector3(0.85f, 0.05f, 0.85f), Palette.Hex("546E7A"), 0.55f, PrimitiveType.Cylinder);
            MeshFactory.Solid("Handle", parent, pos + new Vector3(0.55f, 0.02f, 0f), new Vector3(0.45f, 0.07f, 0.1f), Palette.Hex("90A4AE"), 0.4f);
        }

        static void Bowl(Transform parent, Vector3 pos, Color batter)
        {
            MeshFactory.Solid("Bowl", parent, pos, new Vector3(0.7f, 0.14f, 0.7f), Palette.Hex("EEEEEE"), 0.5f, PrimitiveType.Cylinder);
            MeshFactory.Solid("Batter", parent, pos + new Vector3(0f, 0.1f, 0f), new Vector3(0.52f, 0.05f, 0.52f), batter, 0.45f, PrimitiveType.Cylinder);
        }

        static void Oven(Transform parent, Vector3 pos)
        {
            var enamel = SpriteFactory.EnamelTex();
            MeshFactory.TexturedBlock("Oven", parent, pos, new Vector3(1.7f, 1.7f, 1.35f), enamel, Palette.Hex("90A4AE"), 0.38f, 2f, 2f);
            MeshFactory.TexturedSolid("Door", parent, pos + new Vector3(0f, -0.1f, -0.7f), new Vector3(1.35f, 0.95f, 0.08f), enamel, Palette.Hex("455A64"), 0.3f, 1.5f, 1.5f);
            MeshFactory.Solid("Window", parent, pos + new Vector3(0f, -0.08f, -0.76f), new Vector3(0.95f, 0.55f, 0.04f), Palette.Hex("FF8A50"), 0.7f);
            MeshFactory.Solid("Knob1", parent, pos + new Vector3(-0.4f, 0.62f, -0.7f), new Vector3(0.14f, 0.14f, 0.1f), Palette.Hex("FFCDD2"), 0.4f, PrimitiveType.Cylinder);
            MeshFactory.Solid("Knob2", parent, pos + new Vector3(-0.1f, 0.62f, -0.7f), new Vector3(0.14f, 0.14f, 0.1f), Palette.Hex("FFF59D"), 0.4f, PrimitiveType.Cylinder);
        }

        static void ShipBay(Transform parent, Vector3 pos)
        {
            var wood = SpriteFactory.WoodTex();
            MeshFactory.TexturedBlock("Bay", parent, pos, new Vector3(1.9f, 1.8f, 1.2f), wood, Palette.Hex("8D6E63"), 0.22f, 2f, 2f);
            MeshFactory.Solid("Opening", parent, pos + new Vector3(-0.15f, 0.05f, -0.62f), new Vector3(1.15f, 1.2f, 0.08f), Palette.Hex("3E2723"), 0.05f);
            MeshFactory.TexturedSolid("Ramp", parent, pos + new Vector3(-0.15f, -0.55f, -0.95f), new Vector3(1.1f, 0.08f, 0.7f), wood, Palette.Hex("A1887F"), 0.2f, 1f, 1f);
            MeshFactory.Block("Stamp", parent, pos + new Vector3(0.55f, 0.55f, -0.55f), new Vector3(0.35f, 0.25f, 0.12f), Palette.Hex("FF8A65"), 0.3f);
        }

        static void Pipe(Transform parent, Vector3 pos, float length)
        {
            var pipe = MeshFactory.Solid("Pipe", parent, pos, new Vector3(0.16f, length * 0.5f, 0.16f), Palette.Hex("90A4AE"), 0.55f, PrimitiveType.Cylinder);
            pipe.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            MeshFactory.Solid("Joint", parent, pos + new Vector3(-length * 0.35f, -0.35f, 0f), new Vector3(0.22f, 0.35f, 0.22f), Palette.Hex("78909C"), 0.5f, PrimitiveType.Cylinder);
        }

        static void Lamp(Transform parent, Vector3 pos)
        {
            MeshFactory.Solid("Cord", parent, pos + new Vector3(0f, 0.55f, 0f), new Vector3(0.06f, 0.55f, 0.06f), Palette.Hex("B0BEC5"), 0.3f, PrimitiveType.Cylinder);
            MeshFactory.Solid("Shade", parent, pos, new Vector3(0.7f, 0.18f, 0.7f), Palette.Hex("FFF59D"), 0.45f, PrimitiveType.Cylinder);
            SpriteFactory.Billboard("LampGlow", parent, pos + new Vector3(0f, -0.15f, 0f), new Vector2(1.4f, 1.4f), SpriteFactory.Glow(), new Color(1f, 0.92f, 0.55f, 0.4f), 7);
        }

        static TextMesh MakeWorldLabel(Transform parent, string text, Vector3 pos, int size, Color color)
        {
            var go = new GameObject("SignText");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.08f;
            tm.fontSize = size;
            tm.color = color;
            tm.fontStyle = FontStyle.Bold;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return tm;
        }

        void BuildBelts(Transform parent, int laneCount, WorldInfo world)
        {
            Belts = new ConveyorBelt[laneCount];
            if (laneCount == 1)
            {
                Belts[0] = CreateBelt(parent, 0f, "Belt", 0, 1.7f, world);
                return;
            }

            Belts[0] = CreateBelt(parent, -1.2f, "BeltL", 0, 1.22f, world);
            Belts[1] = CreateBelt(parent, 1.2f, "BeltR", 1, 1.22f, world);
        }

        static ConveyorBelt CreateBelt(Transform parent, float x, string name, int lane, float width, WorldInfo world)
        {
            var beltRoot = new GameObject(name);
            beltRoot.transform.SetParent(parent, false);
            beltRoot.transform.localPosition = new Vector3(x, 0f, 0f);
            var belt = beltRoot.AddComponent<ConveyorBelt>();
            belt.Lane = lane;
            belt.StartZ = 7.32f;
            belt.EndZ = -1.35f;
            belt.Height = 0.5f;

            var metal = SpriteFactory.MetalTex();
            var rubber = SpriteFactory.BeltTex();

            MeshFactory.TexturedSolid("Frame", beltRoot.transform, new Vector3(0f, 0.18f, 3.05f), new Vector3(width + 0.42f, 0.28f, 8.95f), metal, Steel, 0.4f, 2f, 6f);
            MeshFactory.TexturedSolid("Bed", beltRoot.transform, new Vector3(0f, 0.36f, 3.02f), new Vector3(width, 0.07f, 8.7f), rubber, Color.white, 0.12f, 1f, 8f);
            MeshFactory.Solid("RailL", beltRoot.transform, new Vector3(-(width * 0.5f + 0.06f), 0.42f, 3.02f), new Vector3(0.08f, 0.12f, 8.7f), Brass, 0.55f);
            MeshFactory.Solid("RailR", beltRoot.transform, new Vector3(width * 0.5f + 0.06f, 0.42f, 3.02f), new Vector3(0.08f, 0.12f, 8.7f), Brass, 0.55f);

            MeshFactory.Solid("LegA", beltRoot.transform, new Vector3(-width * 0.35f, 0.0f, 6.4f), new Vector3(0.14f, 0.2f, 0.14f), SteelDark, 0.3f, PrimitiveType.Cylinder);
            MeshFactory.Solid("LegB", beltRoot.transform, new Vector3(width * 0.35f, 0.0f, 6.4f), new Vector3(0.14f, 0.2f, 0.14f), SteelDark, 0.3f, PrimitiveType.Cylinder);
            MeshFactory.Solid("LegC", beltRoot.transform, new Vector3(-width * 0.35f, 0.0f, 0.2f), new Vector3(0.14f, 0.2f, 0.14f), SteelDark, 0.3f, PrimitiveType.Cylinder);
            MeshFactory.Solid("LegD", beltRoot.transform, new Vector3(width * 0.35f, 0.0f, 0.2f), new Vector3(0.14f, 0.2f, 0.14f), SteelDark, 0.3f, PrimitiveType.Cylinder);

            var rollers = new GameObject("Rollers").transform;
            rollers.SetParent(beltRoot.transform, false);
            for (int i = 0; i < 9; i++)
            {
                float z = 7.1f - i * 1.05f;
                var roller = MeshFactory.Solid("Roller", rollers, new Vector3(0f, 0.3f, z), new Vector3(0.12f, (width - 0.12f) * 0.5f, 0.12f), Palette.Hex("455A64"), 0.45f, PrimitiveType.Cylinder);
                roller.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            var slats = new GameObject("Slats").transform;
            slats.SetParent(beltRoot.transform, false);
            for (int i = 0; i < 10; i++)
            {
                float z = 7.15f - i * 0.92f;
                MeshFactory.Solid("Slat", slats, new Vector3(0f, 0.41f, z), new Vector3(width - 0.22f, 0.03f, 0.12f), Brass, 0.5f);
            }

            BuildDispenser(beltRoot.transform, width, world);
            BuildCatchBin(beltRoot.transform, width);
            belt.BindMotion(rollers, slats);
            return belt;
        }

        static void BuildDispenser(Transform parent, float width, WorldInfo world)
        {
            float w = width + 0.95f;
            var enamel = SpriteFactory.EnamelTex();
            var accent = Color.Lerp(world.Accent, Palette.Hex("EC407A"), 0.35f);

            MeshFactory.TexturedBlock("Machine", parent, new Vector3(0f, 1.15f, 8.25f), new Vector3(w, 2.15f, 1.7f), enamel, Palette.Hex("90A4AE"), 0.38f, 2.2f, 2.2f);
            MeshFactory.TexturedBlock("Hopper", parent, new Vector3(0f, 2.45f, 8.3f), new Vector3(w * 0.72f, 0.55f, 1.1f), enamel, accent, 0.4f, 1.5f, 1f);
            MeshFactory.TexturedBlock("HopperTop", parent, new Vector3(0f, 2.85f, 8.3f), new Vector3(w * 0.5f, 0.35f, 0.8f), enamel, MeshFactory.Shade(accent, 1.1f), 0.42f, 1f, 1f);
            MeshFactory.Solid("Mouth", parent, new Vector3(0f, 0.62f, 7.38f), new Vector3(width * 0.78f, 0.62f, 0.22f), Palette.Hex("263238"), 0.05f);
            MeshFactory.Solid("Lip", parent, new Vector3(0f, 0.38f, 7.22f), new Vector3(width * 0.85f, 0.1f, 0.28f), Brass, 0.5f);
            MeshFactory.Solid("Light", parent, new Vector3(w * 0.28f, 1.85f, 7.42f), new Vector3(0.18f, 0.18f, 0.12f), Palette.Hex("69F0AE"), 0.7f, PrimitiveType.Cylinder);
            MeshFactory.Solid("LightOff", parent, new Vector3(w * 0.12f, 1.85f, 7.42f), new Vector3(0.18f, 0.18f, 0.12f), Palette.Hex("EF9A9A"), 0.55f, PrimitiveType.Cylinder);
            MeshFactory.Block("Panel", parent, new Vector3(-w * 0.22f, 1.55f, 7.4f), new Vector3(0.55f, 0.7f, 0.08f), Palette.Hex("455A64"), 0.3f);
            SpriteFactory.Billboard("MouthGlow", parent, new Vector3(0f, 0.62f, 7.28f), new Vector2(width * 0.7f, 0.5f), SpriteFactory.Glow(), new Color(1f, 0.85f, 0.4f, 0.35f), 8);
        }

        static void BuildCatchBin(Transform parent, float width)
        {
            var kraft = SpriteFactory.CardboardTex();
            var card = Palette.Hex("E8C39C");
            MeshFactory.TexturedSolid("BinFloor", parent, new Vector3(0f, 0.08f, -1.55f), new Vector3(width + 0.55f, 0.08f, 1.15f), kraft, MeshFactory.Shade(card, 0.75f), 0.12f, 2f, 2f);
            MeshFactory.TexturedSolid("BinF", parent, new Vector3(0f, 0.35f, -2.05f), new Vector3(width + 0.55f, 0.55f, 0.08f), kraft, card, 0.15f, 2f, 1f);
            MeshFactory.TexturedSolid("BinL", parent, new Vector3(-(width * 0.5f + 0.28f), 0.35f, -1.55f), new Vector3(0.08f, 0.55f, 1.15f), kraft, MeshFactory.Shade(card, 0.85f), 0.15f, 1f, 2f);
            MeshFactory.TexturedSolid("BinR", parent, new Vector3(width * 0.5f + 0.28f, 0.35f, -1.55f), new Vector3(0.08f, 0.55f, 1.15f), kraft, MeshFactory.Shade(card, 0.85f), 0.15f, 1f, 2f);
            MeshFactory.Solid("BinPad", parent, new Vector3(0f, 0.13f, -1.55f), new Vector3(width + 0.2f, 0.04f, 0.9f), Palette.Hex("EF9A9A"), 0.1f);
        }

        static void DisableOtherLights()
        {
            foreach (var existing in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (existing.gameObject.scene.name == "DontDestroyOnLoad") continue;
                existing.enabled = false;
            }
        }

        void BuildLights(Transform parent)
        {
            var keyGo = new GameObject("KeyLight");
            keyGo.transform.SetParent(parent, false);
            keyGo.transform.rotation = Quaternion.Euler(48f, -38f, 0f);
            var key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.94f, 0.86f);
            key.intensity = 1.35f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.55f;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(25f, 140f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.85f, 0.92f, 1f);
            fill.intensity = 0.42f;
            fill.shadows = LightShadows.None;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.52f, 0.5f);
        }

        void BuildCamera(Transform parent, WorldInfo world, int laneCount)
        {
            foreach (var existing in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (existing.cameraType != CameraType.Game) continue;
                if (existing.transform.IsChildOf(transform)) continue;
                var listener = existing.GetComponent<AudioListener>();
                if (listener != null) Object.Destroy(listener);
                existing.enabled = false;
            }

            var camGo = new GameObject("GameCamera");
            camGo.transform.SetParent(parent, false);
            var rot = Quaternion.Euler(38f, 40f, 0f);
            var target = new Vector3(0f, 0.55f, 2.55f);
            float dist = laneCount >= 2 ? 21f : 17.8f;
            camGo.transform.position = target + rot * new Vector3(0f, 0f, -dist);
            camGo.transform.rotation = rot;

            Cam = camGo.AddComponent<Camera>();
            var additional = Cam.GetUniversalAdditionalCameraData();
            additional.renderPostProcessing = false;
            Cam.orthographic = true;
            Cam.orthographicSize = laneCount >= 2 ? 7.4f : 6.35f;
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = Color.Lerp(world.Wall, Palette.Hex("B3E5FC"), 0.35f);
            Cam.nearClipPlane = 0.1f;
            Cam.farClipPlane = 80f;
            Cam.transparencySortMode = TransparencySortMode.Orthographic;
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
        }

        void Clear()
        {
            if (ArenaRoot != null)
            {
                ArenaRoot.SetParent(null, true);
                Destroy(ArenaRoot.gameObject);
                ArenaRoot = null;
            }

            Belts = null;
            Rack = null;
            Cam = null;
            CandyRoot = null;
            _floor = null;
            _wall = null;
        }
    }
}
