using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Clio.Utilities;
using static System.Globalization.CultureInfo;


namespace GatherUp.Order
{
    public class Profile
    {
        public enum CordialType
        {
            None,
            Cordial,
            HiCordial,
            Auto
        }

        public string Name { get; set; }
        public Teleport TeleportOnStart;
        public Teleport TeleportOnComplete;
        public List<HotSpot> Hotspots;
        public List<HotSpot> Blackspots;
        public List<string> Items;
        public List<string> Gatherskills;
        public Gear gear;
        public Gather gather;
        public string Killradius = "50";

        public class Teleport
        {
            public bool Enabled = false;
            public uint AetheryteId = 0;
            public ushort ZoneId = 0;
            public string Name = "";
        }

        public class Gear
        {
            public bool Enabled = false;
            public int GearSet = 0;
        }

        public class FlyTo
        {
            public bool Enabled => Destinations != null && Destinations.Any();
            public List<Destination> Destinations { get; set; } = new List<Destination>();

            public class Destination
            {
                public Vector3 Position { get; set; }
                public double AllowedVariance { get; set; } = 0.0f;
                public bool Land { get; set; } = true;

                public string GetXYZ()
                {
                    return string.Format("{0}, {1}, {2}",
                        Position.X.ToString(InvariantCulture),
                        Position.Y.ToString(InvariantCulture),
                        Position.Z.ToString(InvariantCulture));
                }
            }
        }

        public class Gather
        {
            public bool Infinite = true; //gather forever.
            public ExGather exGather = new ExGather();
            public int Quantity = 0;
            public bool Hq = false;
            public string ItemId;
            public string Target;

            public class ExGather
            {
                public bool Enabled = false;

                public bool UseCordial
                {
                    get { return (CordialType != CordialType.None); }
                }

                public bool DiscoverUnknowns = false;
                public string CordialTime = "Auto";
                public CordialType CordialType = CordialType.None;
            }
        }

        public class HotSpot
        {
            public Vector3 Coord { get; set; }
            public int Radius { get; set; }
            public FlyTo FlyTo { get; set; }
            public bool DisableMount { get; set; }

            public HotSpot(Vector3 coord, int radius) : this(coord, radius, new FlyTo())
            {
            }

            public HotSpot(Vector3 coord, int radius, FlyTo flyTo)
            {
                Coord = coord;
                Radius = radius;
                FlyTo = flyTo;
            }

            public HotSpot(Vector3 coord) : this(coord, 100, new FlyTo())
            {
            }

            public string GetXYZ()
            {
                return string.Format("{0}, {1}, {2}",
                    Coord.X.ToString(InvariantCulture),
                    Coord.Y.ToString(InvariantCulture),
                    Coord.Z.ToString(InvariantCulture));
            }

            public override string ToString()
            {
                return string.Format("{0}, {1}, {2} | {3}",
                    Coord.X.ToString(InvariantCulture),
                    Coord.Y.ToString(InvariantCulture),
                    Coord.Z.ToString(InvariantCulture),
                    Radius.ToString(InvariantCulture));
            }
        }


        public Profile()
        {
            Hotspots = new List<HotSpot>();
            Blackspots = new List<HotSpot>();
            Items = new List<string>();
            Gatherskills = new List<string>();
            TeleportOnComplete = new Teleport();
            TeleportOnStart = new Teleport();
            gear = new Gear();
            gather = new Gather();
        }

        public XDocument ToXml()
        {
            return new ProfileTransformer(this).ToXDocument();
        }
    }
}