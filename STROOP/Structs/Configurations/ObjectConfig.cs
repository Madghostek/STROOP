﻿using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STROOP.Structs
{
    public static class ObjectConfig
    {
        public static readonly uint StructSize = 0x0260;

        public static readonly uint HeaderOffset = 0x00;
        public static readonly uint NextLinkOffset = 0x08;
        public static readonly uint PreviousLinkOffset = 0x04;
        public static readonly uint ProcessedNextLinkOffset = 0x60;
        public static readonly uint ProcessedPreviousLinkOffset = 0x64;
        public static readonly uint ParentOffset = 0x68;

        public static readonly uint BehaviorScriptOffset = 0x020C;
        public static readonly uint BehaviorGfxOffset = 0x14;
        public static readonly uint BehaviorSubtypeOffset = 0x144;
        public static readonly uint BehaviorParamsOffset = 0x188;
        public static readonly uint BehaviorAppearanceOffset = 0xF0;
        public static readonly uint BehaviorSpawnObjOffset = 0xFC;

        public static readonly uint ModelPointerOffset = 0x218;
        public static readonly uint AnimationOffset = 0x3C;
        public static readonly uint ActiveOffset = 0x74;
        public static readonly uint ActionOffset = 0x14C;
        public static readonly uint TimerOffset = 0x154;

        public static readonly uint XOffset = 0xA0;
        public static readonly uint YOffset = 0xA4;
        public static readonly uint ZOffset = 0xA8;

        public static readonly uint XSpeedOffset = 0xAC;
        public static readonly uint YSpeedOffset = 0xB0;
        public static readonly uint ZSpeedOffset = 0xB4;
        public static readonly uint HSpeedOffset = 0xB8;

        public static readonly uint YawFacingOffset = 0xD6;
        public static readonly uint PitchFacingOffset = 0xD2;
        public static readonly uint RollFacingOffset = 0xDA;
        public static readonly uint YawMovingOffset = 0xCA;
        public static readonly uint PitchMovingOffset = 0xC6;
        public static readonly uint RollMovingOffset = 0xCE;

        public static readonly uint YawVelocityOffset = 0x118;
        public static readonly uint PitchVelocityOffset = 0x114;
        public static readonly uint RollVelocityOffset = 0x11C;

        public static readonly uint YawFacingOffsetUInt = 0xD4;
        public static readonly uint PitchFacingOffsetUInt = 0xD0;
        public static readonly uint RollFacingOffsetUInt = 0xD8;
        public static readonly uint YawMovingOffsetUInt = 0xC8;
        public static readonly uint PitchMovingOffsetUInt = 0xC4;
        public static readonly uint RollMovingOffsetUInt = 0xCC;

        public static readonly uint HomeXOffset = 0x164;
        public static readonly uint HomeYOffset = 0x168;
        public static readonly uint HomeZOffset = 0x16C;

        public static readonly uint ScaleWidthOffset = 0x2C;
        public static readonly uint ScaleHeightOffset = 0x30;
        public static readonly uint ScaleDepthOffset = 0x34;

        public static readonly uint HitboxRadiusOffset = 0x1F8;
        public static readonly uint HitboxHeightOffset = 0x1FC;
        public static readonly uint HurtboxRadiusOffset = 0x200;
        public static readonly uint HurtboxHeightOffset = 0x204;
        public static readonly uint HitboxDownOffsetOffset = 0x208;

        public static readonly uint TangibleDistOffset = 0x194;
        public static readonly uint DrawDistOffset = 0x19C;

        public static readonly uint GraphicsXOffset = 0x20;
        public static readonly uint GraphicsYOffset = 0x24;
        public static readonly uint GraphicsZOffset = 0x28;
        public static readonly uint GraphicsYawOffset = 0x1C;
        public static readonly uint GraphicsPitchOffset = 0x1A;
        public static readonly uint GraphicsRollOffset = 0x1E;

        public static readonly uint ReleaseStatusOffset = 0x1CC;
        public static readonly uint StackIndexOffset = 0x1D0;
        public static readonly uint StackIndexReleasedValue = 0;
        public static readonly uint StackIndexUnReleasedValue = 1;
        public static readonly uint InitialReleaseStatusOffset = 0x1D4;
        public static readonly uint InteractionStatusOffset = 0x134;

        public static uint ReleaseStatusThrownValue
        {
            get
            {
                return SpecialConfig.CustomReleaseStatus != 0 ?
                    SpecialConfig.CustomReleaseStatus :
                    RomVersionConfig.SwitchMap(ReleaseStatusThrownValueUS, ReleaseStatusThrownValueJP, ReleaseStatusThrownValueSH);
            }
        }
        public static readonly uint ReleaseStatusThrownValueUS = 0x800EE5F8;
        public static readonly uint ReleaseStatusThrownValueJP = 0x800EB778;
        public static readonly uint ReleaseStatusThrownValueSH = 0x800EB798;

        public static uint ReleaseStatusDroppedValue
        {
            get
            {
                return SpecialConfig.CustomReleaseStatus != 0 ?
                    SpecialConfig.CustomReleaseStatus :
                    RomVersionConfig.SwitchMap(ReleaseStatusDroppedValueUS, ReleaseStatusDroppedValueJP, ReleaseStatusDroppedValueSH);
            }
        }
        public static readonly uint ReleaseStatusDroppedValueUS = 0x800EE5F0;
        public static readonly uint ReleaseStatusDroppedValueJP = 0x800EB770;
        public static readonly uint ReleaseStatusDroppedValueSH = 0x800EB790;

        public static readonly uint NativeRoomOffset = 0x1A0;
        public static readonly uint NumCollidedObjectsOffset = 0x76;
        public static readonly uint CollidedObjectsListStartOffset = 0x78;

        public static readonly uint DistanceToMarioOffset = 0x15C;
        public static readonly uint AngleToMarioOffset = 0x162;

        // Object specific vars

        public static readonly uint DustSpawnerBehaviorValue = 0x130024AC;
        public static readonly uint DustBallBehaviorValue = 0x130024DC;
        public static readonly uint DustBehaviorValue = 0x13002500;
        public static readonly uint UnderwaterBubbleSpawnerBehaviorValue = 0x130002B8;
        public static readonly uint UnderwaterBubbleBehaviorValue = 0x13000338;

        public static readonly uint PendulumAccelerationDirectionOffset = 0xF4;
        public static readonly uint PendulumAccelerationMagnitudeOffset = 0x100;
        public static readonly uint PendulumAngularVelocityOffset = 0xFC;
        public static readonly uint PendulumAngleOffset = 0xF8;
        public static readonly uint PendulumWaitingTimerOffset = 0x104;

        public static readonly uint CogCurrentYawVelocity = 0xF8;
        public static readonly uint CogTargetYawVelocity = 0xFC;

        public static readonly uint WaypointOffset = 0x100;
        public static readonly uint PitchToWaypointOffset = 0x10A;
        public static readonly uint RacingPenguinEffortOffset = 0x110;
        public static readonly uint KoopaTheQuickHSpeedMultiplierOffset = 0xF4;

        public static readonly uint FlyGuyOscillationTimerOffset = 0xF8;

        public static readonly uint ScuttlebugPhaseOffset = 0x150;
        public static readonly uint ScuttlebugTargetAngleOffset = 0x162;
        public static readonly uint ScuttlebugTargetLungingOffset = 0xF8;
        public static readonly uint ScuttlebugTargetLungingTimerOffset = 0xFC;

        public static readonly uint GoombaCountdownOffset = 0xFC;
        public static readonly uint GoombaTargetAngleOffset = 0x100;

        public static readonly uint BitfsPlatformGroupTimerOffset = 0xF4;

        public static readonly uint HootLastReleaseTimeOffset = 0x110;

        public static readonly uint PowerStarMissionIndexOffset = 0x188;

        public static readonly uint RollingLogXCenterOffset = 0xF8;
        public static readonly uint RollingLogZCenterOffset = 0xFC;
        public static readonly uint RollingLogDistLimitSquaredOffset = 0xF4;

        public static readonly uint ObjectSpawnerRadiusOffset = 0xF8;

        public static readonly uint SwooperTargetYawOffset = 0xFC;

        public static readonly uint PyramidPlatformNormalXOffset = 0xF4;
        public static readonly uint PyramidPlatformNormalYOffset = 0xF8;
        public static readonly uint PyramidPlatformNormalZOffset = 0xFC;
    }
}
