using System.Numerics;

namespace MamboDMA.Games.BA
{
    public sealed class BrokenArrowConfig
    {
        // UI Config
        private static bool _drawNames = true;
        private static bool _drawDistance = true;
        private static bool _showDebug = false;
        private static bool _showEntityDebug = false;
        private static bool _showLocalPlayerDebug = false;

        //Color Config
        public Vector4 ColorPlayer = new(0.0f, 1f, 0.75f, 1f);
        public Vector4 NameColor = new(1f, 1f, 1f, 1f);
        public Vector4 BoxColor = new(212 / 255f, 175 / 255f, 55 / 255f, 1f);
        public Vector4 HpTextColor = new(1f, 0.86f, 0.63f, 1f);
        public Vector4 DistanceColor = new(180 / 255f, 255 / 255f, 190 / 255f, 1f);

        //Filters?
        public bool DropPoints = false;
        public bool PlayerMarkers = false;
        public bool bFog = false;

        // Perforamnce / Loops
        public float MaxDrawDistance = 500f;
        public int FrameCap = 128;
        public int FastIntervalMs = 4;
        public int SlowIntervalMs = 200;



    }
}
