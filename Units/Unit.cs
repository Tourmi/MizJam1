﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MizJam1.Rendering;
using MizJam1.Units;
using MizJam1.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

using static MizJam1.Units.Stats;

namespace MizJam1.Units
{
    public class Unit
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;
        const float ROTATION_RADIANS = (float)(90 * Math.PI / 180);

        private static readonly Random rand = new Random();

        public Unit(uint id, string name, UnitClass unitClass, bool enemy)
        {
            ID = id & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);
            FlippedHorizontally = (id & FLIPPED_HORIZONTALLY_FLAG) != 0;
            FlippedVertically = (id & FLIPPED_VERTICALLY_FLAG) != 0;
            FlippedDiagonally = (id & FLIPPED_DIAGONALLY_FLAG) != 0;
            Name = name;
            UnitClass = unitClass;

            Stats = UnitClass.GetStats((ushort)rand.Next(1, 7));
            Enemy = enemy;

            Health = Stats[MaxHealth];

            Acted = false;
        }

        public Dictionary<Stats, ushort> Stats;

        public uint ID { get; set; }
        public string Name { get; set; }
        public UnitClass UnitClass { get; set; }
        public bool Enemy { get; set; }
        public bool Ally { get => !Enemy; set => Enemy = !value; }

        public float Rotation => FlippedDiagonally ? ROTATION_RADIANS : 0f;
        public bool FlippedDiagonally { get; set; }
        public bool FlippedHorizontally { get; set; }
        public bool FlippedVertically { get; set; }

        public Point Position { get; set; }
        public bool Acted { get; set; }
        public bool Defending { get; set; }
        public bool Hide { get; set; }

        private ushort health;
        public ushort Health { get { return health; } set { if (value < Stats[MaxHealth]) health = value; else health = Stats[MaxHealth]; } }

        public void Reroll()
        {
            Stats = UnitClass.GetStats((ushort)rand.Next(1, 7));
        }

        public Sprite GetSprite(Texture2D[] gameTextures)
        {
            Sprite sprite = new Sprite(gameTextures[TextureIDInterpreter.GetTextureID(ID)], Global.UnitSize, TextureIDInterpreter.GetUnitSourceRectangle(ID), Enemy ? Global.Colors.Accent4 : Global.Colors.Accent2);

            return sprite;
        }
    }
}
