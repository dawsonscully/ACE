﻿using ACE.Entity.Enum;
using ACE.Network.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Entity
{
    public abstract class CollidableObject : WorldObject
    {
        public CollidableObject(ObjectType type, ObjectGuid guid)
            : base(type, guid)
        {
        }

        public CollidableObject(ObjectType type, ObjectGuid guid, string name, ushort weenieClassId, ObjectDescriptionFlag descriptionFlag, WeenieHeaderFlag weenieFlag, Position position)
            : base(type, guid)
        {
            this.Name = name;
            this.DescriptionFlags = descriptionFlag;
            this.WeenieFlags = weenieFlag;
            this.Location = position;
            this.WeenieClassid = weenieClassId;
        }

        public CollidableObject(AceObject aceO)
            : base((ObjectType)aceO.TypeId, new ObjectGuid(aceO.AceObjectId))
        {
            this.Name = aceO.Name;
            this.DescriptionFlags = (ObjectDescriptionFlag)aceO.WdescBitField;
            this.Location = aceO.Position;
            this.WeenieClassid = aceO.WeenieClassId;
            this.WeenieFlags = (WeenieHeaderFlag)aceO.WeenieFlags;

            this.PhysicsData.MTableResourceId = aceO.MotionTableId;
            this.PhysicsData.Stable = aceO.SoundTableId;
            this.PhysicsData.CSetup = aceO.ModelTableId;

            // this should probably be determined based on the presence of data.
            this.PhysicsData.PhysicsDescriptionFlag = (PhysicsDescriptionFlag)aceO.PhysicsBitField;
            this.PhysicsData.PhysicsState = (PhysicsState)aceO.PhysicsState;

            // game data min required flags;
            this.Icon = aceO.IconId;

            this.GameData.Usable = (Usable)aceO.Usability;
            this.GameData.RadarColour = (RadarColor)aceO.BlipColor;
            this.GameData.RadarBehavior = (RadarBehavior)aceO.Radar;
            this.GameData.UseRadius = aceO.UseRadius;

            aceO.AnimationOverrides.ForEach(ao => this.ModelData.AddModel(ao.Index, (ushort)ao.AnimationId));
            aceO.TextureOverrides.ForEach(to => this.ModelData.AddTexture(to.Index, (ushort)to.OldId, (ushort)to.NewId));
            aceO.PaletteOverrides.ForEach(po => this.ModelData.AddPalette(po.SubPaletteId, po.Offset, po.Length));
        }

        public abstract void OnCollide(Player player);
    }
}
