using System;
using System.Collections.Generic;
using System.Text;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Planets;
using SpaceWar2006.Pickups;
using SpaceWar2006.Effects;

using Cheetah;

namespace SpaceWar2006.Maps
{
    public partial class SWCTF01 : Map
    {
        public SWCTF01()
        {
        }

        public SWCTF01(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }
        public override void CreateCustom()
        {
            Light l = new Light();
            l.directional = true;
            l.Position = new Vector3(-1, 1, 4).GetUnit();
            Spawn(l, true);
        }
    }
}
