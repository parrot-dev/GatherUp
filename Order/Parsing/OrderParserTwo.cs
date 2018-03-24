using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Clio.Utilities;
using GatherUp.Order.Parsing.Exceptions;

namespace GatherUp.Order.Parsing
{
    class OrderParserTwo : IOrderParser
    {
        private readonly XDocument _document;
        public Version MinVersion { get; } = new Version(1, 5, 0);
        public Version MaxVersion => GatherUp.version;
        public Version ProfileVersion { get; }
        public bool IsExGather { get; }
        public bool IsValidVersion => ProfileVersion.CompareTo(MinVersion) >= 0 && ProfileVersion.CompareTo(MaxVersion) <= 0;
        private string GatherTagName => IsExGather ? "ExGather" : "Gather";

        ///<exception cref="ParsingException"></exception>
        public OrderParserTwo(string path)
        {
            try
            {
                _document = XDocument.Load(path);
                ProfileVersion = GetVersion();
                IsExGather = _document.Descendants("ExGather").Any();
            }
            catch (Exception err)
            {
                throw new ParsingException(err.Message);
            }
        }

        ///<exception cref="ParsingException"></exception>
        public Profile ToProfile()
        {
            if (!IsValidVersion) throw new ParsingException("Invalid version");
            try
            {
                var profile = new Profile
                {
                    Name = GetProfileName(),
                    Killradius = GetKillRadius(),
                    gear = GetGear(),
                    TeleportOnStart = GetTeleportOnStart(),
                    TeleportOnComplete = GetTeleportOnComplete(),
                    gather = GetGather(),
                    Hotspots = GetHotSpots(),
                    Blackspots = GetBlackSpots(),
                    Gatherskills = GetGatheringSkills(),
                    Items = GetItemNames()
                };
                return profile;
            }
            catch (ParsingException)
            {
                throw;
            }
            catch (Exception err)
            {
                throw new ParsingException($"Unexpected error while parsing profile: {err.Message}");
            }
        }

        private List<Profile.HotSpot> GetBlackSpots()
        {
            try
            {
                var blackSpotElements = _document.Descendants(GatherTagName).FirstOrDefault()?.Descendants("BlackSpot");
                return blackSpotElements?.Select(GetBasicHotSpot).ToList() ?? new List<Profile.HotSpot>();
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }
            throw new ParsingException("Unexpected error while parsing BlackSpots");
        }

        private List<Profile.HotSpot> GetHotSpots()
        {
            try
            {
                var gatherElements = _document.Descendants(GatherTagName);
                var retval = new List<Profile.HotSpot>();
                foreach (var gatherElement in gatherElements)
                {
                    var hotSpot = GetBasicHotSpot(gatherElement.Descendants("HotSpot").First());
                    hotSpot.FlyTo = GetFlyTo(gatherElement);
                    hotSpot.IsStealth = UsesStealth(gatherElement);
                    hotSpot.DisableMount = DisablesMount(gatherElement);
                    retval.Add(hotSpot);
                }

                return retval;
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }
            throw new ParsingException("Unexpected error while parsing HotSpots");
        }

        private bool UsesStealth(XElement gatherElement)
        {
            try
            {
                var applyStealthElem = gatherElement.ElementsBeforeSelf().LastOrDefault(elem =>
                    elem.Attribute("Name")?.Value == "ApplyStealth" || elem.Name == GatherTagName);
                return applyStealthElem?.Name == "RunCode";
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }
            throw new ParsingException("Unexpected error while looking for stealth tag");
        }

        private bool DisablesMount(XElement gatherElement)
        {
            try
            {
                var disableMountElem = gatherElement.ElementsBeforeSelf().LastOrDefault(elem =>
                    elem.Attribute("Name")?.Value == "DisableMount" || elem.Name == GatherTagName);
                return disableMountElem?.Name == "RunCode";
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }
            throw new ParsingException("Unexpected error while looking for disableMount tag");
        }

        private Profile.FlyTo GetFlyTo(XElement gatherElement)
        {
            try
            {
                var flyTo = new Profile.FlyTo();
                var flyToElem = gatherElement.ElementsBeforeSelf().LastOrDefault(elem => elem.Name == "FlyTo" || elem.Name == GatherTagName);
                if (flyToElem?.Name != "FlyTo") return flyTo;
                if (flyToElem.HasAttributes) //one line flyto.
                {
                    flyTo.Destinations.Add(GetFlyToDestination(flyToElem.Attributes()));
                }
                else
                {
                    flyTo.Destinations = flyToElem.Descendants("HotSpot")
                        .Select(flyingHs => GetFlyToDestination(flyingHs.Attributes())).ToList();
                }

                return flyTo;
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }
            throw new ParsingException("Unexpected error while parsing FlyTo tag");
        }

        private static Profile.FlyTo.Destination GetFlyToDestination(IEnumerable<XAttribute> attributes)
        {
            try
            {
                var destination = new Profile.FlyTo.Destination();
                foreach (var xAttribute in attributes)
                {                  
                    switch (xAttribute.Name.LocalName)
                    {
                        case "XYZ":
                            destination.Position = new Vector3(xAttribute.Value);
                            break;
                        case "AllowedVariance":
                            destination.AllowedVariance = double.Parse(xAttribute.Value);
                            break;
                        case "Land":
                            destination.Land =
                                xAttribute.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                            break;
                        default:
                            throw new ParsingException("DestinationChoice had an unexpected attribute");
                    }
                }
                return destination;
            }
            catch (ParsingException)
            {
                throw;
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
                throw new ParsingException("Unexpected error while parsing flying destination");
            }
        }

        private Profile.HotSpot GetBasicHotSpot(XElement hotSpotElem)
        {
            var xyz = hotSpotElem.Attribute("XYZ")?.Value;
            var radiusStr = hotSpotElem.Attribute("Radius")?.Value;
            if (xyz == null) throw new ParsingException("HotSpot is missing XYZ attribute");
            try
            {
                var hotSpot = new Profile.HotSpot(new Vector3(xyz));
                if (int.TryParse(radiusStr, out var radius))
                {
                    hotSpot.Radius = radius;
                }
                return hotSpot;
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
                throw new ParsingException("Unexpected error while parsing HotSpot");
            }
        }


        private List<string> GetGatheringSkills()
        {
            try
            {
                var gatheringSkillOrder = _document.Root?.Descendants("GatheringSkillOrder").FirstOrDefault();
                return gatheringSkillOrder?.Elements().Select(e => e.Attribute("SpellName")?.Value).Where(o => o != null).ToList();
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }
            throw new ParsingException("Could not parse GatheringSkills");
        }

        private Version GetVersion()
        {
            var comment = _document.DescendantNodes().OfType<XComment>()
                .FirstOrDefault(c => c.Value.Contains("GatherUp"));
            if (comment == null) throw new ParsingException("Could not find version comment");

            var match = new Regex(@"([\d\.]+\d)").Match(comment.ToString());
            if (!match.Success) return new Version(0, 0, 0);
            try
            {
                return new Version(match.Groups[1].Value);
            }
            catch (Exception)
            {
                throw new ParsingException("Version comment is malformed");
            }
        }

        private string GetProfileName()
        {
            return _document.Descendants("Name").FirstOrDefault()?.Value
                   ?? throw new ParsingException("Could not parse Profile name");
        }

        private string GetKillRadius()
        {
            return _document.Descendants("KillRadius").FirstOrDefault()?.Value
                   ?? throw new ParsingException("Could not parse kill radius");
        }


        private Profile.Gather GetGather()
        {
            try
            {
                var gather = new Profile.Gather();
                var mainLoop = _document.Descendants("While").First();
                string condition = mainLoop.Attribute("Condition").Value;
                gather.Infinite = condition.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                gather.ItemId = GetItemId(condition);
                gather.Quantity = GetItemQuantity(condition);
                gather.Target = GetGatherObject();
                gather.exGather = GetExGather();
                return gather;
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }
            throw new ParsingException("Could not parse gathering conditions");
        }

        private Profile.Gather.ExGather GetExGather()
        {
            var exGather = new Profile.Gather.ExGather();
            if (!IsExGather) return exGather;
            try
            {
                var gatherElem = _document.Descendants("ExGather").First();
                exGather.Enabled = true;
                exGather.DiscoverUnknowns = gatherElem.Attribute("DiscoverUnknowns")?.Value.ToLowerInvariant() == "true";
                exGather.CordialType = GetCordialType(gatherElem);
                exGather.CordialTime = gatherElem.Attribute("CordialTime")?.Value ?? "Auto";
                return exGather;
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }
            throw new ParsingException("Could not parse ExGather element");
        }

        private Profile.CordialType GetCordialType(XElement exGatherElement)
        {
            var cordialTypeAttr = exGatherElement.Attribute("CordialType");
            if (cordialTypeAttr?.Value != null &&
                Enum.TryParse(cordialTypeAttr.Value, true, out Profile.CordialType cordialType))
            {
                return cordialType;
            }

            return Profile.CordialType.None;
        }

        private string GetItemId(string whileCondition)
        {
            var match = new Regex(@"ItemCount\((\d+)\).*").Match(whileCondition);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private int GetItemQuantity(string whileCondition)
        {
            var match = new Regex(@"ItemCount\(\d+\) .* (\d+)").Match(whileCondition);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var quantity))
            {
                return quantity;
            }
            return 0;
        }

        private string GetGatherObject()
        {
            var gatherObject = _document.Descendants("GatherObject").FirstOrDefault()?.Value;
            return gatherObject ?? throw new ParsingException("Profile has not gatherObject");
        }

        private Profile.Gear GetGear()
        {
            var gear = new Profile.Gear();
            var cdata = _document.DescendantNodes().FirstOrDefault(node =>
                node.NodeType == XmlNodeType.CDATA && node.Parent?.Attribute("Name")?.Value == "GearSetChange");
            if (cdata == null) return gear;

            var match = new Regex(@"ChangeGearset\((\d+)\)").Match(cdata.ToString());
            if (match.Success && Int32.TryParse(match.Groups[1].Value, out var number))
            {
                gear.GearSet = number;
                gear.Enabled = true;
                return gear;
            }

            throw new ParsingException("Could not parse gearset");
        }

        private List<string> GetItemNames()
        {
            try
            {
                var itemNames = _document.Descendants("ItemNames").First().Elements();
                return itemNames.Select(itemName => itemName.Value).ToList();
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
                throw new ParsingException("Could not find any items to gather");
            }
        }

        private Profile.Teleport GetTeleportOnStart()
        {
            var teleportElement =
                _document.Root?.Descendants("TeleportTo").FirstOrDefault(e => e.Parent?.Parent?.Name == "While");
            if (teleportElement != null)
            {
                return GetTeleport(teleportElement);
            }

            return new Profile.Teleport();
        }

        private Profile.Teleport GetTeleportOnComplete()
        {
            var teleportElement =
                _document.Root?.Descendants("TeleportTo").FirstOrDefault(e => e.Parent?.Parent?.Name == "Order");
            if (teleportElement != null)
            {
                return GetTeleport(teleportElement);
            }

            return new Profile.Teleport();
        }

        private Profile.Teleport GetTeleport(XElement teleportElement)
        {
            try
            {
                var match = new Regex(@"not IsOnMap\((\d+)\)").Match(
                    teleportElement.Parent.Attribute("Condition").Value);
                if (match.Success)
                {
                    var zoneId = ushort.Parse(match.Groups[1].Value);
                    var aetheryteId = uint.Parse(teleportElement.Attribute("AetheryteId").Value);
                    var name = teleportElement.Attribute("Name").Value;
                    return new Profile.Teleport
                    {
                        AetheryteId = aetheryteId,
                        Enabled = true,
                        Name = name,
                        ZoneId = zoneId
                    };
                }
            }
            catch (Exception err)
            {
                Log.Bot.print(err.Message);
            }

            throw new ParsingException("Could not parse teleport");
        }
    }
}