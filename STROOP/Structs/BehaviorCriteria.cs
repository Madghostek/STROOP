﻿using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STROOP.Structs
{
    public struct BehaviorCriteria
    {
        public uint BehaviorAddress;
        public uint? GfxIdUS;
        public uint? GfxIdJP;
        public uint? GfxIdSH;
        public uint? GfxIdEU;
        public uint? SubType;
        public uint? Appearance;
        public uint? SpawnObjUS;
        public uint? SpawnObjJP;
        public uint? SpawnObjSH;
        public uint? SpawnObjEU;

        public uint? GfxId
        {
            get
            {
                switch (RomVersionConfig.Version)
                {
                    case RomVersion.US:
                        return GfxIdUS;
                    case RomVersion.JP:
                        return GfxIdJP;
                    case RomVersion.SH:
                        return GfxIdSH ?? GfxIdUS; // TODO: Fix once gfx ids are set
                    case RomVersion.EU:
                        return GfxIdEU ?? GfxIdUS; // TODO: Fix once gfx ids are set
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (RomVersionConfig.Version)
                {
                    case RomVersion.US:
                        GfxIdUS = value;
                        break;
                    case RomVersion.JP:
                        GfxIdJP = value;
                        break;
                    case RomVersion.SH:
                        GfxIdSH = value;
                        break;
                    case RomVersion.EU:
                        GfxIdEU = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public uint? SpawnObj
        {
            get
            {
                switch (RomVersionConfig.Version)
                {
                    case RomVersion.US:
                        return SpawnObjUS;
                    case RomVersion.JP:
                        return SpawnObjJP;
                    case RomVersion.SH:
                        return SpawnObjSH ?? GfxIdUS; // TODO: Fix once gfx ids are set
                    case RomVersion.EU:
                        return SpawnObjEU ?? GfxIdUS; // TODO: Fix once gfx ids are set
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (RomVersionConfig.Version)
                {
                    case RomVersion.US:
                        SpawnObjUS = value;
                        break;
                    case RomVersion.JP:
                        SpawnObjJP = value;
                        break;
                    case RomVersion.SH:
                        SpawnObjSH = value;
                        break;
                    case RomVersion.EU:
                        SpawnObjEU = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is BehaviorCriteria))
                return false;

            var otherCriteria = (BehaviorCriteria)obj;

            return otherCriteria == this;
        }

        public int GetNumFields()
        {
            return
                (SubType.HasValue ? 1 : 0) +
                (Appearance.HasValue ? 1 : 0) +
                (GfxId.HasValue ? 1 : 0) +
                (SpawnObj.HasValue ? 1 : 0);
        }

        public bool CongruentTo(BehaviorCriteria otherCriteria)
        {
            if (otherCriteria.BehaviorAddress != BehaviorAddress)
                return false;

            if (SubType.HasValue && otherCriteria.SubType.HasValue && SubType.Value != otherCriteria.SubType.Value)
                return false;

            if (Appearance.HasValue && otherCriteria.Appearance.HasValue && Appearance.Value != otherCriteria.Appearance.Value)
                return false;

            if (GfxId.HasValue && otherCriteria.GfxId.HasValue && GfxId.Value != otherCriteria.GfxId.Value)
                return false;

            if (SpawnObj.HasValue && otherCriteria.SpawnObj.HasValue && SpawnObj.Value != otherCriteria.SpawnObj.Value)
                return false;

            return true;
        }

        public BehaviorCriteria? Generalize(BehaviorCriteria otherCriteria)
        {
            if (otherCriteria.BehaviorAddress != BehaviorAddress)
                return null;

            if (SubType.HasValue && otherCriteria.SubType.HasValue && SubType.Value != otherCriteria.SubType.Value)
                return new BehaviorCriteria() { BehaviorAddress = BehaviorAddress };

            if (Appearance.HasValue && otherCriteria.Appearance.HasValue && Appearance.Value != otherCriteria.Appearance.Value)
                return new BehaviorCriteria() { BehaviorAddress = BehaviorAddress, SubType = SubType };

            if (GfxId.HasValue && otherCriteria.GfxId.HasValue && GfxId.Value != otherCriteria.GfxId.Value)
                return new BehaviorCriteria() { BehaviorAddress = BehaviorAddress, SubType = SubType, Appearance = Appearance };

            if (SpawnObj.HasValue && otherCriteria.SpawnObj.HasValue && SpawnObj.Value != otherCriteria.SpawnObj.Value)
                return new BehaviorCriteria() { BehaviorAddress = BehaviorAddress, SubType = SubType, Appearance = Appearance, GfxId = GfxId };

            return this;
        }

        public static bool operator ==(BehaviorCriteria a, BehaviorCriteria b)
        {
            return
                a.BehaviorAddress == b.BehaviorAddress &&
                a.SubType == b.SubType &&
                a.Appearance == b.Appearance &&
                a.GfxId == b.GfxId &&
                a.SpawnObj == b.SpawnObj;
        }

        public static bool operator !=(BehaviorCriteria a, BehaviorCriteria b)
        {
            return !(a == b);
        }

        public static bool HasSameAssociation(BehaviorCriteria? beh1, BehaviorCriteria? beh2)
        {
            if (beh1 == null && beh2 == null) return true;
            if (beh1 == null || beh2 == null) return false;
            ObjectBehaviorAssociation assoc1 = Config.ObjectAssociations.FindObjectAssociation(beh1.Value);
            ObjectBehaviorAssociation assoc2 = Config.ObjectAssociations.FindObjectAssociation(beh2.Value);
            return assoc1 == assoc2;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + BehaviorAddress.GetHashCode();
            hash = hash * 23 + GfxId.GetHashCode();
            hash = hash * 23 + SubType.GetHashCode();
            hash = hash * 23 + Appearance.GetHashCode();
            hash = hash * 23 + SpawnObj.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return Config.ObjectAssociations.GetObjectName(this);
        }
    }
}
