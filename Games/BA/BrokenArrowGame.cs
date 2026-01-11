using ArmaReforgerFeeder;
using ImGuiNET;
using MamboDMA.Games.Reforger;
using MamboDMA.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static MamboDMA.Games.BA.Engine;
using static MamboDMA.Games.BA.Il2CppSystem;

namespace MamboDMA.Games.BA
{
    public sealed class BrokenArrowGame : IGame
    {
        public string Name => "BrokenArrow";
        private bool _initialized;
        private bool _running;

        private string _baExe = "BrokenArrow.exe";

        private static BrokenArrowConfig Cfg => Config<BrokenArrowConfig>.Settings;

        public static EntityRaw[] _entitiesRaw = Array.Empty<EntityRaw>();
        public static EntityUnit[] entityUnits = Array.Empty<EntityUnit>();

        //this is atomic memory write, it allows all threads instant access to result
        //the ?? also forces new empty array if null array is passed in instead of null values which avoids errors
        public static void UpdateEntityRaw(EntityRaw[] array)
        {
            System.Threading.Volatile.Write(ref _entitiesRaw, array ?? Array.Empty<EntityRaw>());
        }

        //put this on a thread
        public void FastUpdateLoop()
        {
            UpdateCamera();

        }


        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Draw(ImGuiWindowFlags winFlags)
        {
            throw new NotImplementedException();
        }
    }
}
