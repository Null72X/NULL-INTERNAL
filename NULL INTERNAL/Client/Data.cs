using AotForms;
using System;
using System.Numerics;

namespace Client
{
    internal static class Data
    {
        internal static void Work()
        {
            while (true)
            {
                Core.HaveMatrix = false;

                var rBaseGameFacade = InternalMemory.Read<uint>(Offsets.Il2Cpp + Offsets.InitBase, out var baseGameFacade);
                if (!rBaseGameFacade || baseGameFacade == 0)
                {
                    ResetCache();
                    continue;
                }

                var rGameFacade = InternalMemory.Read<uint>(baseGameFacade, out var gameFacade);
                if (!rGameFacade || gameFacade == 0)
                {
                    ResetCache();
                    continue;
                }

                var rStaticGameFacade = InternalMemory.Read<uint>(gameFacade + Offsets.StaticClass, out var staticGameFacade);
                if (!rStaticGameFacade || staticGameFacade == 0)
                {
                    ResetCache();
                    continue;
                }

                var rCurrentGame = InternalMemory.Read<uint>(staticGameFacade, out var currentGame);
                if (!rCurrentGame || currentGame == 0)
                {
                    ResetCache();
                    continue;
                }

                SpeedTimer.PublishGameTimer(currentGame);

                var rCurrentMatch = InternalMemory.Read<uint>(currentGame + Offsets.CurrentMatch, out var currentMatch);
                if (!rCurrentMatch || currentMatch == 0)
                {
                    ResetCache();
                    continue;
                }

                uint localPlayer;
                var rLocalPlayer = InternalMemory.Read<uint>(currentMatch + Offsets.LocalPlayer, out localPlayer);
                if (!rLocalPlayer || localPlayer == 0)
                {
                    Core.IsSpectating = true;

                    var rCurrentObserver = InternalMemory.Read<uint>(currentMatch + Offsets.CurrentObserver, out var currentObserver);
                    if (!rCurrentObserver || currentObserver == 0)
                        continue;

                    var rObserverPlayer = InternalMemory.Read<uint>(currentObserver + Offsets.ObserverPlayer, out var observerPlayer);
                    if (!rObserverPlayer || observerPlayer == 0)
                        continue;

                    localPlayer = observerPlayer;
                }
                else
                {
                    Core.IsSpectating = false;
                }

                Core.LocalPlayer = localPlayer;

                var rMainTransform = InternalMemory.Read<uint>(localPlayer + Offsets.MainCameraTransform, out var mainTransform);
                if (!rMainTransform || mainTransform == 0)
                {
                    continue;
                }

                var rMainTransformPos = Transform.GetPosition(mainTransform, out var mainPos);
                if (rMainTransformPos)
                {
                    Core.LocalMainCamera = mainPos;
                }

                var rFollowCamera = InternalMemory.Read<uint>(localPlayer + Offsets.FollowCamera, out var followCamera);
                if (!rFollowCamera || followCamera == 0)
                {
                    continue;
                }

                var rCamera = InternalMemory.Read<uint>(followCamera + Offsets.Camera, out var camera);
                if (!rCamera || camera == 0)
                {
                    continue;
                }

                var rCameraBase = InternalMemory.Read<uint>(camera + 0x8, out var cameraBase);
                if (!rCameraBase || cameraBase == 0)
                {
                    continue;
                }
                Core.HaveMatrix = true;

                var rViewMatrix = InternalMemory.Read<Matrix4x4>(cameraBase + Offsets.ViewMatrix, out var viewMatrix);
                if (!rViewMatrix)
                {
                    continue;
                }
                Core.CameraMatrix = viewMatrix;

                foreach (var entity in GetEntities(currentGame, Offsets.DictionaryEntities))
                {
                    if (entity == 0) continue;
                    if (entity == localPlayer) continue;

                    Entity player;

                    if (Core.Entities.TryGetValue(entity, out player))
                    {
                        player.Address = entity;

                        if (player.IsTeam == Bool.True) continue;

                        if (player.IsTeam == Bool.Unknown)
                        {
                            var rAvatarManager = InternalMemory.Read<uint>(entity + Offsets.AvatarManager, out var avatarManager);

                            if (rAvatarManager && avatarManager != 0)
                            {
                                var rAvatar = InternalMemory.Read<uint>(avatarManager + Offsets.Avatar, out var avatar);

                                if (rAvatar && avatar != 0)
                                {
                                    var rIsVisible = InternalMemory.Read<bool>(avatar + Offsets.Avatar_IsVisible, out var isVisible);

                                    if (rIsVisible && isVisible)
                                    {
                                        var rAvatarData = InternalMemory.Read<uint>(avatar + Offsets.Avatar_Data, out var avatarData);

                                        if (rAvatarData && avatarData != 0)
                                        {
                                            var rIsTeam = InternalMemory.Read<bool>(avatarData + Offsets.Avatar_Data_IsTeam, out var isTeam);
                                            if (rIsTeam)
                                            {
                                                if (isTeam)
                                                {
                                                    player.IsTeam = Bool.True;
                                                }
                                                else
                                                {
                                                    player.IsTeam = Bool.False;
                                                    player.IsKnown = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!player.IsKnown) continue;
                        if (Config.IgnoreKnocked)
                        {
                            if (InternalMemory.Read<uint>(entity + Offsets.Player_ShadowBase, out var shadowBase))
                            {
                                if (shadowBase != 0)
                                {
                                    if (InternalMemory.Read<int>(shadowBase + Offsets.XPose, out var xpose))
                                    {
                                        player.IsKnocked = xpose == 8;
                                    }
                                }
                            }
                        }

                        var rIsDead = InternalMemory.Read<bool>(entity + Offsets.Player_IsDead, out var isDead);
                        if (rIsDead)
                        {
                            player.IsDead = isDead;
                        }

                        if (Config.ESPName)
                        {
                            var rNameAddr = InternalMemory.Read<uint>(entity + Offsets.Player_Name, out var nameAddr);
                            if (rNameAddr && nameAddr != 0)
                            {
                                var rNameLen = InternalMemory.Read<int>(nameAddr + 0x8, out var nameLen);
                                if (rNameLen)
                                {
                                    if (nameLen > 0)
                                    {
                                        var name = InternalMemory.ReadString(nameAddr + 0xC, nameLen);
                                        if (name != "" && player != null)
                                        {
                                            player.Name = name;
                                        }
                                    }
                                }
                            }
                        }

                        if (Config.ESPHealth || Config.ESPHealthText)
                        {
                            var rDataPool = InternalMemory.Read<uint>(entity + Offsets.Player_Data, out var dataPool);
                            if (rDataPool && dataPool != 0)
                            {
                                var rPoolObj = InternalMemory.Read<uint>(dataPool + 0x8, out var poolObj);
                                if (rPoolObj && poolObj != 0)
                                {
                                    var rPool = InternalMemory.Read<uint>(poolObj + 0x10, out var pool);
                                    if (rPool && pool != 0)
                                    {
                                        var rHealthAddr = InternalMemory.Read<short>(pool + 0x10, out var Health);
                                        if (rHealthAddr && Health != 0)
                                        {
                                            if (player != null)
                                            {
                                                player.Health = Health;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (Config.ESPWeapon)
                        {
                            if (InternalMemory.Read<uint>(entity + Offsets.InventoryManager, out var weaponClass) &&weaponClass != 0)
                            {
                                if (InternalMemory.Read<uint>(weaponClass + Offsets.WeaponOnHand, out var weaponOnHand) && weaponOnHand != 0)
                                {
                                    if (InternalMemory.Read<uint>(weaponOnHand + Offsets.WeaponInfo, out var weaponInfo) && weaponInfo != 0)
                                    {
                                        if (InternalMemory.Read<int>(weaponInfo + Offsets.WeaponID, out var weaponId))
                                        {
                                            player.WeaponID = weaponId;
                                        }
                                    }
                                }
                            }
                        }

                        if (Config.ESPLevel)
                        {
                            if (InternalMemory.Read<uint>(entity + Offsets.BaseProfileInfo,out var baseProfileInfo) && baseProfileInfo != 0)
                            {
                                if (InternalMemory.Read<int>(baseProfileInfo + 0x14,out var level))
                                {
                                    if (level <= 0 || level > 100) level = 1;
                                    player.Level = level;
                                }
                                else
                                {
                                    player.Level = 1;
                                }
                            }
                            else
                            {
                                player.Level = 0;
                            }
                        }

                        if (Config.NoRecoil)
                        {
                            var readWeapon = InternalMemory.Read<uint>(localPlayer + Offsets.Weapon, out var weapon);

                            if (readWeapon && weapon != 0)
                            {
                                var readWeaponData = InternalMemory.Read<uint>(weapon + Offsets.WeaponData, out var weaponData);

                                if (readWeaponData && weaponData != 0)
                                {
                                    var readRecoil = InternalMemory.Read<float>(weaponData + Offsets.WeaponRecoil, out var recoil);
                                    if (readRecoil && recoil != 0)
                                    {
                                        InternalMemory.Write(weaponData + Offsets.WeaponRecoil, 0f);
                                    }
                                }
                            }
                        }

                        uint playerAttribute = 0;
                        if (InternalMemory.Read<uint>(localPlayer + Offsets.PlayerAttributes, out playerAttribute) && playerAttribute != 0)
                        {
                            if (Config.UnlimitedAmmo)
                            {
                                uint currentAmmoBuff = 0;
                                if (InternalMemory.Read<uint>(playerAttribute + Offsets.BuffWeaponAmmoClip, out currentAmmoBuff))
                                {
                                    if (currentAmmoBuff < 140)
                                    {
                                        InternalMemory.Write<uint>(playerAttribute + Offsets.BuffWeaponAmmoClip, 160);
                                    }
                                }
                            }
                            else
                            {
                                InternalMemory.Write<uint>(playerAttribute + Offsets.BuffWeaponAmmoClip, 0);
                            }
                        }

                        if (Config.InfinitySkyler)
                        {
                            if (InternalMemory.Read<uint>(localPlayer + Offsets.PlayerAttributes, out var attributes) && attributes != 0)
                            {
                                InternalMemory.Write(attributes + Offsets.InfinitySkyler, 1.0f);
                            }
                        }
                        else
                        {
                            if (InternalMemory.Read<uint>(localPlayer + Offsets.PlayerAttributes, out var attributes) && attributes != 0)
                            {
                                InternalMemory.Write(attributes + Offsets.InfinitySkyler, 0.0f);
                            }
                        }

                        var rHeadBone = InternalMemory.Read<uint>(entity + (uint)Bones.Head, out var headBone);
                        if (rHeadBone && headBone != 0)
                        {
                            var rHeadTrans = Transform.GetNodePosition(headBone, out var headTransform);

                            if (rHeadTrans)
                            {
                                player.Head = headTransform;
                                player.Distance = Vector3.Distance(mainPos, headTransform);
                            }
                        }

                        var rRootBone = InternalMemory.Read<uint>(entity + (uint)Bones.Root, out var rootBone);
                        if (rRootBone || rootBone != 0)
                        {
                            var rRootTrans = Transform.GetNodePosition(rootBone, out var rootTransform);

                            if (rRootTrans)
                            {
                                player.Root = rootTransform;
                            }
                        }
                        var boneOffsets = new[]
                        {
                            Bones.Head,
                            Bones.Breast,
                            Bones.Root,
                            Bones.Hip,

                            Bones.LeftBiceps,
                            Bones.RightBiceps,

                            Bones.LeftWristJoint,
                            Bones.RightWristJoint,

                            Bones.LeftShoulder,
                            Bones.RightShoulder,

                            Bones.LeftLeg,
                            Bones.RightLeg,

                            Bones.LeftFoot,
                            Bones.RightFoot,
                        };
                        foreach (var offset in boneOffsets)
                        {
                            var rBone = InternalMemory.Read<uint>(entity + (uint)offset, out var bone);
                            if (rBone && bone != 0)
                            {
                                var rBonePos = Transform.GetNodePosition(bone, out var boneTransform);
                                if (rBonePos)
                                {
                                    switch (offset)
                                    {
                                        case Bones.Head:
                                            player.Head = boneTransform;
                                            break;

                                        case Bones.Breast:
                                            player.Breast = boneTransform;
                                            break;

                                        case Bones.Root:
                                            player.Root = boneTransform;
                                            break;

                                        case Bones.Hip:
                                            player.Hip = boneTransform;
                                            break;

                                        case Bones.LeftBiceps:
                                            player.LeftBiceps = boneTransform;
                                            break;

                                        case Bones.RightBiceps:
                                            player.RightBiceps = boneTransform;
                                            break;

                                        case Bones.LeftShoulder:
                                            player.LeftShoulder = boneTransform;
                                            break;

                                        case Bones.RightShoulder:
                                            player.RightShoulder = boneTransform;
                                            break;

                                        case Bones.LeftLeg:
                                            player.LeftLeg = boneTransform;
                                            break;

                                        case Bones.RightLeg:
                                            player.RightLeg = boneTransform;
                                            break;

                                        case Bones.LeftFoot:
                                            player.LeftFoot = boneTransform;
                                            break;

                                        case Bones.RightFoot:
                                            player.RightFoot = boneTransform;
                                            break;

                                        case Bones.LeftWristJoint:
                                            player.LeftWristJoint = boneTransform;
                                            break;

                                        case Bones.RightWristJoint:
                                            player.RightWristJoint = boneTransform;
                                            break;
                                    }
                                    player.Distance = Vector3.Distance(Core.LocalMainCamera, player.Head);
                                }
                            }
                        }
                    }
                    else
                    {
                        Core.Entities[entity] = new Entity
                        {
                            IsTeam = Bool.Unknown,
                            IsKnown = false,
                            IsDead = false,
                            Health = 0,
                            IsKnocked = false,

                            // Bone positions
                            Head = Vector3.Zero,
                            Breast = Vector3.Zero,
                            Root = Vector3.Zero,
                            Hip = Vector3.Zero,

                            LeftBiceps = Vector3.Zero,
                            RightBiceps = Vector3.Zero,

                            LeftWristJoint = Vector3.Zero,
                            RightWristJoint = Vector3.Zero,

                            LeftShoulder = Vector3.Zero,
                            RightShoulder = Vector3.Zero,

                            LeftLeg = Vector3.Zero,
                            RightLeg = Vector3.Zero,

                            LeftFoot = Vector3.Zero,
                            RightFoot = Vector3.Zero,

                            Name = "",      // Default name as empty
                        };
                    }
                }
            }
        }
        static List<uint> GetEntities(uint baseGame, uint offset)
        {
            List<uint> entityList = new List<uint>();

            if (!InternalMemory.Read<uint>(baseGame + offset, out uint dict) || dict == 0)
                return entityList;

            if (!InternalMemory.Read<int>(dict + 0x10, out int count) || count < 1 || count > 10000)
                return entityList;

            if (!InternalMemory.Read<uint>(dict + 0xC, out uint entries) || entries == 0)
                return entityList;

            uint start = entries + 0x10;

            for (uint i = 0; i < count; i++)
            {
                uint entry = start + (i * 0x10);

                if (!InternalMemory.Read<int>(entry + 0x0, out int hash) || hash < 0)
                    continue;

                if (!InternalMemory.Read<uint>(entry + 0xC, out uint entity) || entity == 0)
                    continue;

                entityList.Add(entity);
            }

            return entityList;
        }

        static void ResetCache()
        {
            Core.Entities = new();
            InternalMemory.Cache = new();
        }
    }
}