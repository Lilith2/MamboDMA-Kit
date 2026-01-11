using ArmaReforgerFeeder;
using MamboDMA;
using System;
using System.Collections.Frozen;
using System.Numerics;
using System.Reflection;
using static MamboDMA.Games.BA.Engine;
using static MamboDMA.Misc;

namespace MamboDMA.Games.BA
{
    public sealed class EngineError
    {
        private enum ErrorList
        { 
            Success = 0,
            ProcessInvalid,
            GomSigNotFound,
            GomResolveFailed,
            ModuleNotFound_BaseProcess,
            ModuleNotFound_BrokenArrowDLL,
            ModuleNotFound_UnityPlayerDLL,
            ModuleNotFound_UnityEngineCoreModuleDLL,
            ModuleNotFound_DefaultEcsDLL,
            LocalPlayerCamera_Unknown,
            ViewMatrix_Unknown,
            LocalPlayer_Unknown,
            EntityList_Unknown = 12
        }
        private readonly ErrorList _errorList;

        private FrozenDictionary<ErrorList, String> _ErrorStrings;
        public EngineError() 
        {
            // 1. Create a temporary source
            var source = new Dictionary<ErrorList, string>
            {
                { ErrorList.Success, "Success." },
                { ErrorList.ProcessInvalid, "Invalid Process Module. Check if Process is running." },
                { ErrorList.GomSigNotFound, "GOM Signature Finder has failed. Check in IDA... Something changed or the sig finder is improperly used." },
                { ErrorList.ModuleNotFound_BaseProcess, "Broken Arrow exe Invalid. Make sure the game is running & DMA card is working."},
                { ErrorList.ModuleNotFound_BrokenArrowDLL, "BrokenArrow.DLL module Invalid. Check the code using this named module. It loads at runtime and is packed when game not running."},
                { ErrorList.ModuleNotFound_UnityPlayerDLL, "UnityPlayer.DLL module Invalid. Check the code using this named module. It's always unpacked in the game's directory & loads on runtime."},
                { ErrorList.ModuleNotFound_UnityEngineCoreModuleDLL, "UnityEngine.CoreModule.DLL module Invalid. Check the code using this module. It loads at runtime and is packed when game not running."},
                { ErrorList.LocalPlayerCamera_Unknown, "LocalPlayer Camera Pointer Unknown."},
                { ErrorList.ViewMatrix_Unknown, "View Matrix of LocalPlayer's Camera Unknown."},
                { ErrorList.LocalPlayer_Unknown, "LocalPlayer pointer Unknown."},
                { ErrorList.EntityList_Unknown, "EntityList Unknown."}
            };

            // 2. "Freeze" it into the FrozenDictionary
            _ErrorStrings = source.ToFrozenDictionary();
        }

        public string GetErrorString(int _err)
        {
            if(CheckErrorCode(_err))
                return _ErrorStrings[(ErrorList)_err];
            return "\n";
        }

        public static bool CheckErrorCode(int _err)
        {
            if (Enum.TryParse(typeof(ErrorList), _err.ToString(), out var result) && Enum.IsDefined(typeof(ErrorList), result))
            {
                return true;
            }
            return false ;
        }

        
    }


    //I used "sealed" so no other part of the program can inherit from it. Engine.cs is a common design in this kit.
    //BA Engine class is fully self contained & can't be accidentally put into other game's classes.
    public sealed class Engine
    {
        private static EngineError _EngineError;
        private static readonly ulong _gomOffset = 0x1D09650;
        //inside UnityPlayer.dll
        private static string _tagNotDefinedSig = "48 8D 15 ?? ?? ?? ?? 4C 0F 45 45 8F 48 8D 4D 0F E8 ?? ?? ?? ?? 4C 8B 00 48 8D 54 24 30 33 C9 48 8B D8 FF 15 ?? ?? ?? ?? 48 8B 43 08 48 89 44 24 38 41 8B C7 48 8B 1D ?? ?? ?? ?? 4C 8D 45 67 48 8B CB 89 45 67 48 8D 55 C7 E8";
        private static CameraData Camera = new();
        public static ScreenSettings Screen = new(ScreenService.Current.W, ScreenService.Current.H);

        private static ulong _ProcessBase;
        private static List<DmaMemory.ModuleInfo> _modules = new();
        private static DmaMemory.ModuleInfo _DefaultEcsDLL, _brokenArrowDLL, _unityPlayerDLL, _unityEngineCoreModuleDLL;

        

        //long sigAddr = DmaMemory.FindSignature(yourSig, base, end);
        private static ulong ResolveGOMSigResult(ulong sigAddr)
        {
            if (sigAddr != 0)
            {
                // Step 2: Move 0x34 bytes to reach the MOV instruction (Green Arrow)
                ulong movAddr = sigAddr + 0x34;

                // Step 3: Read the 4-byte displacement. 
                // It starts 3 bytes into the instruction (after 48 8B 1D)
                byte displacement = DmaMemory.Read<byte>(movAddr + 3);

                // Step 4: Calculate the final pointer
                // RIP is the address of the NEXT instruction (movAddr + 7)
                ulong rip = movAddr + 7;
                ulong finalAddress = rip + displacement;

                Console.WriteLine($"Found GOM Offset at: 0x{finalAddress:X}");
                return finalAddress;
            }
            string s_err = _EngineError.GetErrorString(3);
            Console.WriteLine(s_err);
            return 0;
        }

        private static ulong FindGOM(DmaMemory.ModuleInfo moduleInfo)
        {
            string s_err;
            ulong result = DmaMemory.FindSignature(_tagNotDefinedSig);
            if (result == 0 || result < 0)
            {
                s_err = _EngineError.GetErrorString(2);
                Console.WriteLine(s_err);
                return result;
            }
            return result;
        }

        private static bool CacheModules()
        {
            _modules = DmaMemory.GetModules();
            if (_modules == null) return false;

            _DefaultEcsDLL = _modules.FirstOrDefault(m => string.Equals(m.Name, "DefaultEcs.dll", StringComparison.OrdinalIgnoreCase));
            if (_DefaultEcsDLL == null)
            {
                Console.WriteLine(_EngineError.GetErrorString(8));
            }

            _brokenArrowDLL = _modules.FirstOrDefault(m => string.Equals(m.Name, "BrokenArrow.dll", StringComparison.OrdinalIgnoreCase));
            if (_brokenArrowDLL == null)
            {
                Console.WriteLine(_EngineError.GetErrorString(5));
            }

            _unityPlayerDLL = _modules.FirstOrDefault(m => string.Equals(m.Name, "UnityPlayer.dll", StringComparison.OrdinalIgnoreCase));
            if (_unityPlayerDLL == null)
            {
                Console.WriteLine(_EngineError.GetErrorString(6));
            }

            _unityEngineCoreModuleDLL = _modules.FirstOrDefault(m => string.Equals(m.Name, "UnityEngine.CoreModule.dll", StringComparison.OrdinalIgnoreCase));
            if (_unityEngineCoreModuleDLL == null)
            {
                Console.WriteLine(_EngineError.GetErrorString(6));
            }

            if (_DefaultEcsDLL == null || _brokenArrowDLL == null || _unityPlayerDLL == null || _unityEngineCoreModuleDLL == null)
                return false;
            else return true;
        }

        //run this function in a constant loop in 
        public static void UpdateCamera()
        {
            Camera.busy = 1;
            if (Camera.myCameraPtr != 0 && Camera.cameraManagerPtr != 0)
            {
                //use a scatter read function here to quickly update cam info

                Camera.busy = 0;
                return;
            }
            //if cam pointers invalid, refresh all pointers here
            //use regular dma read

            Camera.busy = 0;
            return;
        }

        private bool WorldToScreen(in Vector3f position, out float _x, out float _y)
        {
            _x = _y = 0f;
            CameraData snap;
            while (true)
            {
                int x = Volatile.Read(ref Camera.busy);
                if ((x & 1) != 0) continue;     // writer in progress
                snap = Camera;               // struct copy
                int y = Volatile.Read(ref Camera.busy);
                if (x == y) break;     // stable snapshot
            }
            //from here we have a good Camera loaded into local copy


            return false;
        }

        public struct CameraData
        {
            public int busy;
            public CameraType cameraType;
            public ulong cameraManagerPtr;
            public ulong myCameraPtr;
            public ulong viewMatrixPtr;
            public ulong positionPtr;
            public Matrix4x4 viewMatrix;
            public Vector3f position;
        }

        public struct EntityUnit
        { 
            public EntityRaw parent;
            public UnitCategoryType _unitCategoryType;
            public UnitType _unitType;
            public UnitRole _unitRole;
            public string unitName;
            public int teamID;
        }

        public struct EntityRaw
        {
            public ulong Ptr;
            public Vector3f Position;
            public Vector2f Projected;

            public EntityRaw()
            {
                Ptr = 0;
                Position = new Vector3f(0,0,0);
                Projected = new Vector2f(0,0);
            }
        }

        public enum UnitType
        {
            // Token: 0x0400072D RID: 1837
            None,
            // Token: 0x0400072E RID: 1838
            Infantry = 2,
            // Token: 0x0400072F RID: 1839
            Vehicle = 4,
            // Token: 0x04000730 RID: 1840
            Helicopter = 8,
            // Token: 0x04000731 RID: 1841
            Aircraft = 16,
            // Token: 0x04000732 RID: 1842
            Ship = 32,
            // Token: 0x04000733 RID: 1843
            Projectile = 128
        }

        public enum UnitRole
        {
            // Token: 0x0400070D RID: 1805
            None,
            // Token: 0x0400070E RID: 1806
            IFV = 10,
            // Token: 0x0400070F RID: 1807
            Tank,
            // Token: 0x04000710 RID: 1808
            APC,
            // Token: 0x04000711 RID: 1809
            LSV,
            // Token: 0x04000712 RID: 1810
            CargoTruck,
            // Token: 0x04000713 RID: 1811
            LRSAM,
            // Token: 0x04000714 RID: 1812
            SRSAM,
            // Token: 0x04000715 RID: 1813
            LineInfantry = 30,
            // Token: 0x04000716 RID: 1814
            AssaultInfantry,
            // Token: 0x04000717 RID: 1815
            ReconInfantry,
            // Token: 0x04000718 RID: 1816
            Snipers,
            // Token: 0x04000719 RID: 1817
            AAInfantry,
            // Token: 0x0400071A RID: 1818
            ATGMInfantry,
            // Token: 0x0400071B RID: 1819
            SpecialForces,
            // Token: 0x0400071C RID: 1820
            ReconHelicopter = 70,
            // Token: 0x0400071D RID: 1821
            MultiRoleHelicopter,
            // Token: 0x0400071E RID: 1822
            HeavyTransportHelicopter,
            // Token: 0x0400071F RID: 1823
            AttackHelicopter,
            // Token: 0x04000720 RID: 1824
            Drone = 100,
            // Token: 0x04000721 RID: 1825
            MLRS = 130,
            // Token: 0x04000722 RID: 1826
            Mortar,
            // Token: 0x04000723 RID: 1827
            LAM,
            // Token: 0x04000724 RID: 1828
            Artillery,
            // Token: 0x04000725 RID: 1829
            AssaultPlane = 160,
            // Token: 0x04000726 RID: 1830
            Bomber,
            // Token: 0x04000727 RID: 1831
            StrategicBomber,
            // Token: 0x04000728 RID: 1832
            TransportPlane,
            // Token: 0x04000729 RID: 1833
            MultiRolePlane,
            // Token: 0x0400072A RID: 1834
            Ship = 200,
            // Token: 0x0400072B RID: 1835
            Hovercraft
        }

        public enum UnitCategoryType
        {
            // Token: 0x04000704 RID: 1796
            None = -1,
            // Token: 0x04000705 RID: 1797
            Recon,
            // Token: 0x04000706 RID: 1798
            Infantry,
            // Token: 0x04000707 RID: 1799
            Vehicles,
            // Token: 0x04000708 RID: 1800
            Support,
            // Token: 0x04000709 RID: 1801
            Logistic,
            // Token: 0x0400070A RID: 1802
            Helicopters,
            // Token: 0x0400070B RID: 1803
            Aircrafts
        }

        public enum CameraType
        {
            // Token: 0x04000231 RID: 561
            Game = 1,
            // Token: 0x04000232 RID: 562
            SceneView,
            // Token: 0x04000233 RID: 563
            Preview = 4,
            // Token: 0x04000234 RID: 564
            VR = 8,
            // Token: 0x04000235 RID: 565
            Reflection = 16
        }





        //end of class
    }
}





