using ArmaReforgerFeeder;
using System;
using static MamboDMA.Misc;

namespace MamboDMA.Games.BA
{
    public static class Engine
    {
        public static CamereraData Camera;
        public static ScreenSettings Screen = new(ScreenService.Current.W, ScreenService.Current.H);

        private static ulong _gamePtr, _camMgr, _playerCam;
        private static EntityInfo[] _entitiesRaw = Array.Empty<EntityInfo>();

    }

    public struct CamereraData
    {
        CameraType cameraType;
        // TO-DO finish basic structs which will be my custom types for reading memory in chunks


    }

    public struct EntityInfo
    {
        public ulong Ptr;
        public UnitCategoryType categoryType;
        public UnitType unitType;
        public UnitRole role;
        public int TeamID;
        public Vector3f Position;
        public Vector2f Projected;

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
}