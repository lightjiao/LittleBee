﻿using Config.Static.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Config.Static
{
    public class MapElementCFG : ICFG,IObjectCFG, ICombativeCFG
    {
        public int ConfigId { set; get; }
        public string Name { set; get; }
        public string Desc { set; get; }
        public int ResKey { set; get; }
        public int Mass { set; get; }
        public int Diameter { set; get; }
        public int HaloType { set; get; }
        public int HaloEffectRadius { set; get; }
        public int AttackType { set; get; }
        public int DefenceType { set; get; }
        public int Attack { set; get; }
        public int Defence { set; get; }
    }
}
