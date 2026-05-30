namespace Client
{
    internal static class Offsets
    {
        internal static uint Il2Cpp;
        internal static uint InitBase = 0x9EC1C48;

        internal static uint StaticClass = 0x5C; // NEVER CHANGES
        internal static uint CurrentMatch = 0x50; // protected NFJPHMKKEBF m_Match; IN class MatchGame
        internal static uint MatchStatus = 0x8c; // protected NFJPHMKKEBF.LICPHHNNPPF ILGECLEFCCO; IN class NFJPHMKKEBF
        internal static uint LocalPlayer = 0x94; // protected Player FJPEHEGICBO; IN class NFJPHMKKEBF
        internal static uint DictionaryEntities = 0x68; // protected Dictionary<uint, ReplicationEntity> m_ReplicationEntitis; IN class MatchGame

        internal static uint Player_IsDead = 0x50; // private bool FHMPKFMFEPM; IN class AttackableEntity
        internal static uint Player_Name = 0x2e4; // protected string OIAJCBLDHKP; IN class Player
        internal static uint Player_Data = 0x48; // protected IPRIDataPool m_PRIDataPool; IN class ReplicationEntity
        internal static uint Player_ShadowBase = 0x16bc; // public PlayerNetwork.HHCBNAPCKHF m_ShadowState; IN class PlayerNetwork 
        internal static uint XPose = 0x78; // public FBCAHNCLMDC ADFIDIPODGK; IN class PlayerNetwork.HHCBNAPCKHF
        internal static uint PlayerAttributes = 0x4c0; // protected PlayerAttributes JKPFFNEMJIF; IN class Player

        internal static uint AvatarManager = 0x4c4; // protected AvatarManager FOGJNGDMJKJ; IN class Player
        internal static uint Avatar = 0xa0; // internal IUmaAvatar EEAGBKBMBLD; IN class AvatarManager
        internal static uint Avatar_IsVisible = 0x95; // private bool IsVisible; IN class UmaAvatarSimple
        internal static uint Avatar_Data = 0x14; // public UMAData umaData; IN class UMAAvatarBase
        internal static uint Avatar_Data_IsTeam = 0x59; // public bool isTeammate; IN class UMAData

        internal static uint FollowCamera = 0x454; // protected FollowCamera CHDOHNOEBML; IN class Player
        internal static uint Camera = 0x18; // protected Camera FCKFGJMEECI IN class CameraControllerBase
        internal static uint MainCameraTransform = 0x254; // public Transform MainCameraTransform; IN class Player
        internal static uint AimRotation = 0x404; // private Quaternion <KCFEHMAIINO>k__BackingField; IN class Player
        internal static uint ViewMatrix = 0xe8; // internal Matrix4x4 panelToWorld; IN class BaseRuntimePanel

        internal static uint CurrentObserver = 0xb4; // protected FNCMBMMKLLI BGGJJKKKFDC; IN class NFJPHMKKEBF
        internal static uint ObserverPlayer = 0x28; // private Player NJMDHHGDNPJ; IN class FNCMBMMKLLI

        internal static uint Weapon = 0x3f8; // public GPBDEDFKJNA ActiveUISightingWeapon; IN class Player
        internal static uint WeaponData = 0x58; // private CHEJCCHHDMH <NOAOCMKGLAH>k__BackingField; IN class GPBDEDFKJNA 
        internal static uint WeaponRecoil = 0xc; // private float EFMCDHABKGP; IN class OACEDDHKLIM 
        internal static uint NoReload = 0x99; // public Boolean ShootNoReload; IN class PlayerAttributes
        internal static uint InventoryManager = 0x4ac; // protected NPCNMJAGIKI COLEAPKGFLK; IN class Player
        internal static uint WeaponOnHand = 0x54; // private AAHMJHHPECM LFEPIIENLAF; IN class NPCNMJAGIKI
        internal static uint WeaponInfo = 0x64; // protected OOIPMACFIFL LAEMLAPIAFD; IN class GPBDEDFKJNA
        internal static uint WeaponID = 0x14; // public uint HEONOMOEOLN; IN class NPCNMJAGIKI.CHBEAKBLDPI
        internal static uint BuffWeaponAmmoClip = 0xD0; // public int BuffWeaponAmmoClip; IN class PlayerAttributes

        internal static uint IsClientBot = 0x2EC; // public bool IsClientBot; IN class Player
        internal static uint InfinitySkyler = 0x120; // private float DPFCEOKBPPP; IN class PlayerAttributes
        internal static uint BaseProfileInfo = 0x16d0; // protected BaseProfileInfo OJAFLKJINPJ; IN class PlayerNetwork

        internal static uint FallingSpeedUpScale = 0x1CC;
        internal static uint RunSpeedUpScale = 0x1D0;

        internal static uint HealingWalkSpeedScale = 0x2950;
        internal static uint IsUseMedkitForceStand = 0x2A88;
        internal static uint IsUseMedkitForceStandOnlyEffectSprint = 0x2A89;

        internal static uint RightcameraOffset = 0x48;
        internal static uint FOVcameraoffset = 0x4C;

        internal static uint MediKit = 0x94;

        internal static uint FixedDeltaTime = 0x24; // private Single m_DeltaTime; IN class TimeService
        internal static uint GameTimer = 0x10; // private Single m_FixedDeltaTime; IN class TimeService

        internal static uint sAim1 = 0x544; // private bool <LPEIEILIKGC>k__BackingField; IN class Player
        internal static uint sAim2 = 0x948; // pprivate MADMMIICBNN GEGFCFDGGGP; IN class Player
        internal static uint sAim3 = 0x38;  //	public Vector3 BOGOIAMJFDN; IN class MADMMIICBNN
        internal static uint sAim4 = 0x2C; // public Vector3 NHKKHPLFMNG; IN class MADMMIICBNN

        internal static uint AimbotVisible = 0x4A8;  // protected Collider HECFNHJKOMN; IN  class Player
        internal static uint HeadCollider = 0x54; // private Collider<INICDNFOFJB> k__BackingField; IN  class AttackableEntity
    }
}