﻿using System;
using BaseUtils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EDDTest
{
    public static class Journal
    {
        public static void JournalEntry(string filename, string cmdrname, CommandArgs argsentry, int repeatdelay)
        {
            if (argsentry.Left == 0)
            {
                Console.WriteLine("** Minimum 3 parameters of filename, cmdrname, journalentrytype");
                return;
            }

            int repeatcount = 0;

            while (true)
            {
                CommandArgs args = new CommandArgs(argsentry);

                string eventtype = args.Next().ToLower();

                Random rnd = new Random();

                string lineout = null;      //quick writer

                if (eventtype.Equals("fsd"))
                    lineout = FSDJump(args, repeatcount);
                else if (eventtype.Equals("fsdtravel"))
                    lineout = FSDTravel(args);
                else if (eventtype.Equals("loc"))
                    lineout = Loc(args);
                else if (eventtype.Equals("interdiction"))
                    lineout = Interdiction(args);
                else if (eventtype.Equals("docked"))
                {
                    lineout = "{ " + TimeStamp() + "\"event\":\"Docked\", " +
                        "\"StationName\":\"Jameson Memorial\", " +
                        "\"StationType\":\"Orbis\", \"StarSystem\":\"Shinrarta Dezhra\", \"Faction\":\"The Pilots Federation\", \"Allegiance\":\"Independent\", \"Economy\":\"$economy_HighTech;\", \"Economy_Localised\":\"High tech\", \"Government\":\"$government_Democracy;\", \"Government_Localised\":\"Democracy\", \"Security\":\"$SYSTEM_SECURITY_high;\", \"Security_Localised\":\"High Security\" }";
                }
                else if (eventtype.Equals("undocked"))
                    lineout = "{ " + TimeStamp() + "\"event\":\"Undocked\", " + "\"StationName\":\"Jameson Memorial\",\"StationType\":\"Orbis\" }";
                else if (eventtype.Equals("liftoff"))
                    lineout = "{ " + TimeStamp() + "\"event\":\"Liftoff\", " + "\"Latitude\":7.141173, \"Longitude\":95.256424 }";
                else if (eventtype.Equals("touchdown"))
                {
                    lineout = "{ " + TimeStamp() + F("event", "Touchdown") + F("Latitude", 7.141173) + F("Longitude", 95.256424) + FF("PlayerControlled", true) + " }";
                }
                else if (eventtype.Equals("commitcrime"))
                {
                    string f = args.Next();
                    int id = args.Int();
                    lineout = "{ " + TimeStamp() + F("event", "CommitCrime") + F("CrimeType", "collidedAtSpeedInNoFireZone") + F("Faction", f) + FF("Fine", id) + " }";
                }
                else if (eventtype.Equals("missionaccepted") && args.Left == 3)
                {
                    string f = args.Next();
                    string vf = args.Next();
                    int id = args.Int();

                    lineout = "{ " + TimeStamp() + F("event", "MissionAccepted") + F("Faction", f) +
                            F("Name", "Mission_Assassinate_Legal_Corporate") + F("TargetType", "$MissionUtil_FactionTag_PirateLord;") + F("TargetType_Localised", "Pirate lord") + F("TargetFaction", vf)
                            + F("DestinationSystem", "Quapa") + F("DestinationStation", "Grabe Dock") + F("Target", "mamsey") + F("Expiry", DateTime.UtcNow.AddDays(1)) +
                            F("Influence", "Med") + F("Reputation", "Med") + FF("MissionID", id) + "}";
                }
                else if (eventtype.Equals("missioncompleted") && args.Left == 3)
                {
                    string f = args.Next();
                    string vf = args.Next();
                    int id = args.Int();

                    lineout = "{ " + TimeStamp() + F("event", "MissionCompleted") + F("Faction", f) +
                        F("Name", "Mission_Assassinate_Legal_Corporate") + F("TargetType", "$MissionUtil_FactionTag_PirateLord;") + F("TargetType_Localised", "Pirate lord") + F("TargetFaction", vf) +
                         F("MissionID", id) + F("Reward", "82272") + " \"CommodityReward\":[ { \"Name\": \"CoolingHoses\", \"Count\": 4 } ] }";
                }
                else if (eventtype.Equals("missionredirected") && args.Left == 3)
                {
                    string sysn = args.Next();
                    string stationn = args.Next();
                    int id = args.Int();
                    lineout = "{ " + TimeStamp() + F("event", "MissionRedirected") + F("MissionID", id) + F("MissionName", "Mission_Assassinate_Legal_Corporate") +
                        F("NewDestinationStation", stationn) + F("OldDestinationStation", "Cuffey Orbital") +
                        F("NewDestinationSystem", sysn) + FF("OldDestinationSystem", "Vequess") + " }";
                }
                else if (eventtype.Equals("missions") && args.Left == 1)
                {
                    int id = args.Int();
                    BaseUtils.QuickJSONFormatter qj = new QuickJSONFormatter();

                    qj.Object().UTC("timestamp").V("event", "Missions");

                    qj.Array("Active").Object();
                    FMission(qj, id, "Mission_Assassinate_Legal_Corporate", false, 20000);
                    qj.Close(2);

                    qj.Array("Failed").Object();
                    FMission(qj, id + 1000, "Mission_Assassinate_Legal_Corporate", false, 20000);
                    qj.Close(2);

                    qj.Array("Completed").Object();
                    FMission(qj, id + 2000, "Mission_Assassinate_Legal_Corporate", false, 20000);
                    qj.Close(2);

                    lineout = qj.Get();
                }
                else if (eventtype.Equals("marketbuy") && args.Left == 3)
                {
                    string name = args.Next();
                    int count = args.Int();
                    int price = args.Int();

                    BaseUtils.QuickJSONFormatter qj = new QuickJSONFormatter();

                    lineout = qj.Object().UTC("timestamp").V("event", "MarketBuy").V("MarketID", 29029292)
                                .V("Type", name).V("Type_Localised", name + "loc").V("Count", count).V("BuyPrice", price).V("TotalCost", price * count).Get();
                }
                else if (eventtype.Equals("bounty"))
                {
                    string f = args.Next();
                    int rw = args.Int();

                    lineout = "{ " + TimeStamp() + F("event", "Bounty") + F("VictimFaction", f) + F("VictimFaction_Localised", f + "_Loc") +
                        F("TotalReward", rw, true) + "}";
                }
                else if (eventtype.Equals("factionkillbond"))
                {
                    string f = args.Next();
                    string vf = args.Next();
                    int rw = args.Int();

                    lineout = "{ " + TimeStamp() + F("event", "FactionKillBond") + F("VictimFaction", vf) + F("VictimFaction_Localised", vf + "_Loc") +
                        F("AwardingFaction", f) + F("AwardingFaction_Localised", f + "_Loc") +
                        F("Reward", rw, true) + "}";
                }
                else if (eventtype.Equals("capshipbond"))
                {
                    string f = args.Next();
                    string vf = args.Next();
                    int rw = args.Int();

                    lineout = "{ " + TimeStamp() + F("event", "CapShipBond") + F("VictimFaction", vf) + F("VictimFaction_Localised", vf + "_Loc") +
                        F("AwardingFaction", f) + F("AwardingFaction_Localised", f + "_Loc") +
                        F("Reward", rw, true) + "}";
                }
                else if (eventtype.Equals("resurrect"))
                {
                    int ct = args.Int();

                    lineout = "{ " + TimeStamp() + F("event", "Resurrect") + F("Option", "Help me") + F("Cost", ct) + FF("Bankrupt", false) + "}";
                }
                else if (eventtype.Equals("died"))
                {
                    lineout = "{ " + TimeStamp() + F("event", "Died") + F("KillerName", "Evil Jim McDuff") + F("KillerName_Localised", "Evil Jim McDuff The great") + F("KillerShip", "X-Wing") + FF("KillerRank", "Invincible") + "}";
                }
                else if (eventtype.Equals("miningrefined"))
                    lineout = "{ " + TimeStamp() + F("event", "MiningRefined") + FF("Type", "Gold") + " }";
                else if (eventtype.Equals("engineercraft"))
                    lineout = "{ " + TimeStamp() + F("event", "EngineerCraft") + F("Engineer", "Robert") + F("Blueprint", "FSD_LongRange")
                        + F("Level", "5") + "\"Ingredients\":{ \"magneticemittercoil\":1, \"arsenic\":1, \"chemicalmanipulators\":1, \"dataminedwake\":1 } }";
                else if (eventtype.Equals("navbeaconscan"))
                    lineout = "{ " + TimeStamp() + F("event", "NavBeaconScan") + FF("NumBodies", "3") + " }";
                else if (eventtype.Equals("scanplanet") && args.Left >= 1)
                {
                    lineout = "{ " + TimeStamp() + F("event", "Scan") + "\"BodyName\":\"" + args.Next() + "x" + repeatcount + "\", \"DistanceFromArrivalLS\":639.245483, \"TidalLock\":true, \"TerraformState\":\"\", \"PlanetClass\":\"Metal rich body\", \"Atmosphere\":\"\", \"AtmosphereType\":\"None\", \"Volcanism\":\"rocky magma volcanism\", \"MassEM\":0.010663, \"Radius\":1163226.500000, \"SurfaceGravity\":3.140944, \"SurfaceTemperature\":1068.794067, \"SurfacePressure\":0.000000, \"Landable\":true, \"Materials\":[ { \"Name\":\"iron\", \"Percent\":36.824127 }, { \"Name\":\"nickel\", \"Percent\":27.852226 }, { \"Name\":\"chromium\", \"Percent\":16.561033 }, { \"Name\":\"zinc\", \"Percent\":10.007420 }, { \"Name\":\"selenium\", \"Percent\":2.584032 }, { \"Name\":\"tin\", \"Percent\":2.449526 }, { \"Name\":\"molybdenum\", \"Percent\":2.404594 }, { \"Name\":\"technetium\", \"Percent\":1.317050 } ], \"SemiMajorAxis\":1532780800.000000, \"Eccentricity\":0.000842, \"OrbitalInclination\":-1.609496, \"Periapsis\":179.381393, \"OrbitalPeriod\":162753.062500, \"RotationPeriod\":162754.531250, \"AxialTilt\":0.033219 }";
                }
                else if (eventtype.Equals("scanstar"))
                {
                    lineout = "{ " + TimeStamp() + F("event", "Scan") + "\"BodyName\":\"Merope A" + repeatcount + "\", \"DistanceFromArrivalLS\":0.000000, \"StarType\":\"B\", \"StellarMass\":8.597656, \"Radius\":2854249728.000000, \"AbsoluteMagnitude\":1.023468, \"Age_MY\":182, \"SurfaceTemperature\":23810.000000, \"Luminosity\":\"IV\", \"SemiMajorAxis\":12404761034752.000000, \"Eccentricity\":0.160601, \"OrbitalInclination\":18.126791, \"Periapsis\":49.512009, \"OrbitalPeriod\":54231617536.000000, \"RotationPeriod\":110414.359375, \"AxialTilt\":0.000000 }";
                }
                else if (eventtype.Equals("scanearth"))
                {
                    int rn = rnd.Next(10);
                    lineout = "{ " + TimeStamp() + F("event", "Scan") + "\"BodyName\":\"Merope " + rn + "\", \"DistanceFromArrivalLS\":901.789856, \"TidalLock\":false, \"TerraformState\":\"Terraformed\", \"PlanetClass\":\"Earthlike body\", \"Atmosphere\":\"\", \"AtmosphereType\":\"EarthLike\", \"AtmosphereComposition\":[ { \"Name\":\"Nitrogen\", \"Percent\":92.386833 }, { \"Name\":\"Oxygen\", \"Percent\":7.265749 }, { \"Name\":\"Water\", \"Percent\":0.312345 } ], \"Volcanism\":\"\", \"MassEM\":0.840000, \"Radius\":5821451.000000, \"SurfaceGravity\":9.879300, \"SurfaceTemperature\":316.457062, \"SurfacePressure\":209183.453125, \"Landable\":false, \"SemiMajorAxis\":264788426752.000000, \"Eccentricity\":0.021031, \"OrbitalInclination\":13.604733, \"Periapsis\":73.138206, \"OrbitalPeriod\":62498732.000000, \"RotationPeriod\":58967.023438, \"AxialTilt\":-0.175809 }";
                }
                else if (eventtype.Equals("afmurepairs"))
                    lineout = "{ " + TimeStamp() + F("event", "AfmuRepairs") + "\"Module\":\"$modularcargobaydoor_name;\", \"Module_Localised\":\"Cargo Hatch\", \"FullyRepaired\":true, \"Health\":1.000000 }";

                else if (eventtype.Equals("sellshiponrebuy"))
                    lineout = "{ " + TimeStamp() + F("event", "SellShipOnRebuy") + "\"ShipType\":\"Dolphin\", \"System\":\"Shinrarta Dezhra\", \"SellShipId\":4, \"ShipPrice\":4110183 }";

                else if (eventtype.Equals("searchandrescue") && args.Left == 2)
                {
                    string name = args.Next();
                    int count = args.Int();
                    BaseUtils.QuickJSONFormatter qj = new QuickJSONFormatter();
                    lineout = qj.Object().UTC("timestamp").V("event", "SearchAndRescue").V("MarketID", 29029292)
                                .V("Name", name).V("Name_Localised", name + "loc").V("Count", count).V("Reward", 10234).Get();

                }
                else if (eventtype.Equals("repairdrone"))
                    lineout = "{ " + TimeStamp() + F("event", "RepairDrone") + "\"HullRepaired\": 0.23, \"CockpitRepaired\": 0.1,  \"CorrosionRepaired\": 0.5 }";

                else if (eventtype.Equals("communitygoal"))
                    lineout = "{ " + TimeStamp() + F("event", "CommunityGoal") + "\"CurrentGoals\":[ { \"CGID\":726, \"Title\":\"Alliance Research Initiative - Trade\", \"SystemName\":\"Kaushpoos\", \"MarketName\":\"Neville Horizons\", \"Expiry\":\"2017-08-17T14:58:14Z\", \"IsComplete\":false, \"CurrentTotal\":10062, \"PlayerContribution\":562, \"NumContributors\":101, \"TopRankSize\":10, \"PlayerInTopRank\":false, \"TierReached\":\"Tier 1\", \"PlayerPercentileBand\":50, \"Bonus\":200000 } ] }";

                else if (eventtype.Equals("musicnormal"))
                    lineout = "{ " + TimeStamp() + F("event", "Music") + FF("MusicTrack", "NoTrack") + " }";
                else if (eventtype.Equals("musicsysmap"))
                    lineout = "{ " + TimeStamp() + F("event", "Music") + FF("MusicTrack", "SystemMap") + " }";
                else if (eventtype.Equals("musicgalmap"))
                    lineout = "{ " + TimeStamp() + F("event", "Music") + FF("MusicTrack", "GalaxyMap") + " }";
                else if (eventtype.Equals("friends") && args.Left >= 1)
                    lineout = "{ " + TimeStamp() + F("event", "Friends") + F("Status", "Online") + FF("Name", args.Next()) + " }";
                else if (eventtype.Equals("fuelscoop") && args.Left >= 2)
                {
                    string scoop = args.Next();
                    string total = args.Next();
                    lineout = "{ " + TimeStamp() + F("event", "FuelScoop") + F("Scooped", scoop) + FF("Total", total) + " }";
                }
                else if (eventtype.Equals("jetconeboost"))
                    lineout = "{ " + TimeStamp() + F("event", "JetConeBoost") + FF("BoostValue", "1.5") + " }";
                else if (eventtype.Equals("fighterdestroyed"))
                    lineout = "{ " + TimeStamp() + FF("event", "FighterDestroyed") + " }";
                else if (eventtype.Equals("fighterrebuilt"))
                    lineout = "{ " + TimeStamp() + F("event", "FighterRebuilt") + FF("Loadout", "Fred") + " }";
                else if (eventtype.Equals("npccrewpaidwage"))
                    lineout = "{ " + TimeStamp() + F("event", "NpcCrewPaidWage") + F("NpcCrewId", 1921) + FF("Amount", 10292) + " }";
                else if (eventtype.Equals("npccrewrank"))
                    lineout = "{ " + TimeStamp() + F("event", "NpcCrewRank") + F("NpcCrewId", 1921) + FF("RankCombat", 4) + " }";
                else if (eventtype.Equals("launchdrone"))
                    lineout = "{ " + TimeStamp() + F("event", "LaunchDrone") + FF("Type", "FuelTransfer") + " }";
                else if (eventtype.Equals("market"))
                    lineout = Market(Path.GetDirectoryName(filename), args.Next());
                else if (eventtype.Equals("moduleinfo"))
                    lineout = ModuleInfo(Path.GetDirectoryName(filename), args.Next());
                else if (eventtype.Equals("outfitting"))
                    lineout = Outfitting(Path.GetDirectoryName(filename), args.Next());
                else if (eventtype.Equals("shipyard"))
                    lineout = Shipyard(Path.GetDirectoryName(filename), args.Next());
                else if (eventtype.Equals("powerplay"))
                    lineout = "{ " + TimeStamp() + F("event", "PowerPlay") + F("Power", "Fred") + F("Rank", 10) + F("Merits", 10) + F("Votes", 2) + FF("TimePledged", 433024) + " }";
                else if (eventtype.Equals("underattack"))
                    lineout = "{ " + TimeStamp() + F("event", "UnderAttack") + FF("Target", "Fighter") + " }";
                else if (eventtype.Equals("promotion") && args.Left==2)
                    lineout = "{ " + TimeStamp() + F("event", "Promotion") + FF(args.Next(), args.Int()) + " }";
                else if (eventtype.Equals("cargodepot"))
                    lineout = CargoDepot(args);
                else
                {
                    Console.WriteLine("** Unrecognised journal event type or not enough parameters for entry");
                    break;
                }

                if (lineout != null)
                {
                    if (!File.Exists(filename))
                    {
                        using (Stream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (StreamWriter sr = new StreamWriter(fs))
                            {
                                string line = "{ " + TimeStamp() + "\"event\":\"Fileheader\", \"part\":1, \"language\":\"English\\\\UK\", \"gameversion\":\"2.2 (Beta 2)\", \"build\":\"r121783/r0 \" }";
                                sr.WriteLine(line);
                                Console.WriteLine(line);

                                string line2 = "{ " + TimeStamp() + "\"event\":\"LoadGame\", \"Commander\":\"" + cmdrname + "\", \"Ship\":\"Anaconda\", \"ShipID\":14, \"GameMode\":\"Open\", \"Credits\":18670609, \"Loan\":0 }";
                                sr.WriteLine(line2);
                                Console.WriteLine(line2);
                            }
                        }
                    }

                    Write(filename, lineout);
                }
                else
                    break;

                if (repeatdelay == -1)
                {
                    ConsoleKeyInfo k = Console.ReadKey();

                    if (k.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }
                else if (repeatdelay > 0)
                {
                    System.Threading.Thread.Sleep(repeatdelay);

                    if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
                        break;
                }
                else
                    break;

                repeatcount++;
            }
        }

        static string Loc(CommandArgs args)
        {
            if (args.Left < 4)
            {
                Console.WriteLine("** More parameters");
                return null;
            }

            double x = double.NaN, y = 0, z = 0;
            string starnameroot = args.Next();

            if (!double.TryParse(args.Next(), out x) || !double.TryParse(args.Next(), out y) || !double.TryParse(args.Next(), out z))
            {
                Console.WriteLine("X,y,Z must be numbers");
                return null;
            }

            return "{ " + TimeStamp() + "\"event\":\"Location\", " +
                "\"StarSystem\":\"" + starnameroot +
                "\", \"StarPos\":[" + x.ToString("0.000000") + ", " + y.ToString("0.000000") + ", " + z.ToString("0.000000") +
                "], \"Allegiance\":\"\", \"Economy\":\"$economy_None;\", \"Economy_Localised\":\"None\", \"Government\":\"$government_None;\", \"Government_Localised\":\"None\", \"Security\":\"$SYSTEM_SECURITY_low;\", \"Security_Localised\":\"Low Security\" }";
        }

        //                                  "Options: Interdiction Loc name success isplayer combatrank faction power\n" +
        static string Interdiction(CommandArgs args)
        {
            if (args.Left < 6)
            {
                Console.WriteLine("** More parameters");
                return null;
            }

            return "{ " + TimeStamp() + "\"event\":\"Interdiction\", " +
                "\"Success\":\"" + args[1] + "\", " +
                "\"Interdicted\":\"" + args[0] + "\", " +
                "\"IsPlayer\":\"" + args[2] + "\", " +
                "\"CombatRank\":\"" + args[3] + "\", " +
                "\"Faction\":\"" + args[4] + "\", " +
                "\"Power\":\"" + args[5] + "\" }";
        }

        static string FSDJump(CommandArgs args, int repeatcount)
        {
            if (args.Left < 4)
            {
                Console.WriteLine("** More parameters: file cmdrname fsd x y z");
                return null;
            }

            double x = double.NaN, y = 0, z = 0;
            string starnameroot = args.Next();

            if (!double.TryParse(args.Next(), out x) || !double.TryParse(args.Next(), out y) || !double.TryParse(args.Next(), out z))
            {
                Console.WriteLine("** X,Y,Z must be numbers");
                return null;
            }

            z = z + 100 * repeatcount;

            string starname = starnameroot + ((z > 0) ? "_" + z.ToString("0") : "");

            return "{ " + TimeStamp() + "\"event\":\"FSDJump\", \"StarSystem\":\"" + starname +
            "\", \"StarPos\":[" + x.ToString("0.000000") + ", " + y.ToString("0.000000") + ", " + z.ToString("0.000000") +
            "], \"Allegiance\":\"\", \"Economy\":\"$economy_None;\", \"Economy_Localised\":\"None\", \"Government\":\"$government_None;\"," +
            "\"Government_Localised\":\"None\", \"Security\":\"$SYSTEM_SECURITY_low;\", \"Security_Localised\":\"Low Security\"," +
            "\"JumpDist\":10.791, \"FuelUsed\":0.790330, \"FuelLevel\":6.893371 }";
        }

        static string FSDTravel(CommandArgs args)
        {
            if (args.Left < 8)
            {
                Console.WriteLine("** More parameters");
                return null;
            }

            double x = double.NaN, y = 0, z = 0, dx = 0, dy = 0, dz = 0;
            double percent = 0;
            string starnameroot = args.Next();

            if (!double.TryParse(args.Next(), out x) || !double.TryParse(args.Next(), out y) || !double.TryParse(args.Next(), out z) ||
                !double.TryParse(args.Next(), out dx) || !double.TryParse(args.Next(), out dy) || !double.TryParse(args.Next(), out dz) ||
                !double.TryParse(args.Next(), out percent))
            {
                Console.WriteLine("** X,Y,Z,dx,dy,dz,percent must be numbers");
                return null;
            }

            Console.WriteLine("{0} {1} {2}", dx, dy, dz);

            x = (dx - x) * percent / 100.0 + x;
            y = (dy - y) * percent / 100.0 + y;
            z = (dz - z) * percent / 100.0 + z;

            string starname = starnameroot + percent.ToString("0");

            return "{ " + TimeStamp() + "\"event\":\"FSDJump\", \"StarSystem\":\"" + starname +
            "\", \"StarPos\":[" + x.ToString("0.000000") + ", " + y.ToString("0.000000") + ", " + z.ToString("0.000000") +
            "], \"Allegiance\":\"\", \"Economy\":\"$economy_None;\", \"Economy_Localised\":\"None\", \"Government\":\"$government_None;\"," +
            "\"Government_Localised\":\"None\", \"Security\":\"$SYSTEM_SECURITY_low;\", \"Security_Localised\":\"Low Security\"," +
            "\"JumpDist\":10.791, \"FuelUsed\":0.790330, \"FuelLevel\":6.893371 }";
        }

        static string Market(string mpath, string opt)
        {
            string mline = "{ " + TimeStamp() + F("event", "Market") + F("MarketID", 12345678) + F("StationName", "Columbus") + FF("StarSystem", "Sol");
            string market = mline + ", " + EDDTest.Properties.Resources.Market;

            if (opt == null || opt.Equals("NOFILE", StringComparison.InvariantCultureIgnoreCase) == false)
                File.WriteAllText(Path.Combine(mpath, "Market.json"), market);

            return mline + " }";
        }

        static string Outfitting(string mpath, string opt)
        {
            //{ "timestamp":"2018-01-28T23:45:39Z", "event":"Outfitting", "MarketID":3229009408, "StationName":"Mourelle Gateway", "StarSystem":"G 65-9",
            string jline = "{ " + TimeStamp() + F("event", "Outfitting") + F("MarketID", 12345678) + F("StationName", "Columbus") + FF("StarSystem", "Sol");
            string fline = jline + ", " + EDDTest.Properties.Resources.Outfitting;

            if (opt == null || opt.Equals("NOFILE", StringComparison.InvariantCultureIgnoreCase) == false)
                File.WriteAllText(Path.Combine(mpath, "Outfitting.json"), fline);

            return jline + " }";
        }

        static string CargoDepot(CommandArgs args)
        {
            try
            {
                int missid = int.Parse(args.Next());
                string type = args.Next();
                int countcol = int.Parse(args.Next());
                int countdel = int.Parse(args.Next());
                int total = int.Parse(args.Next());

                return "{ " + TimeStamp() + F("event", "CargoDepot") + F("MissionID", missid) + F("UpdateType", type) +
                                F("StartMarketID", 12) + F("EndMarketID", 13) +
                                F("ItemsCollected", countcol) +
                                F("ItemsDelivered", countdel) +
                                F("TotalItemsToDeliver", total) +
                                F("Progress", (double)countcol / (double)total, true) + " }";
            }
            catch
            {
                Console.WriteLine("missionid type col del total");
                return null;
            }
        }

        static string Shipyard(string mpath, string opt)
        {
            // { "timestamp":"2018-01-26T03:47:33Z", "event":"Shipyard", "MarketID":128004608, "StationName":"Vonarburg Co-operative", "StarSystem":"Wyrd",

            string jline = "{ " + TimeStamp() + F("event", "Shipyard") + F("MarketID", 12345678) + F("StationName", "Columbus") + FF("StarSystem", "Sol");
            string fline = jline + ", " + EDDTest.Properties.Resources.Shipyard;

            if (opt == null || opt.Equals("NOFILE", StringComparison.InvariantCultureIgnoreCase) == false)
                File.WriteAllText(Path.Combine(mpath, "Shipyard.json"), fline);

            return jline + " }";
        }


        static string ModuleInfo(string mpath, string opt)
        {
            string mline = "{ " + TimeStamp() + FF("event", "ModuleInfo");
            string market = mline + ", " + EDDTest.Properties.Resources.ModulesInfo;

            if (opt == null || opt.Equals("NOFILE", StringComparison.InvariantCultureIgnoreCase) == false)
                File.WriteAllText(Path.Combine(mpath, "ModulesInfo.json"), market);     // note the plural

            return mline + " }";
        }

        public static void FMission(QuickJSONFormatter q , int id, string name, bool pas, int time)
        {
            q.V("MissionID", id).V("Name",name).V("PassengerMission",pas).V("Expires",time);
        }


        #region DEPRECIATED Helpers for journal writing - USE QuickJSONFormatter!

        public static string TimeStamp()
        {
            DateTime dt = DateTime.Now.ToUniversalTime();
            return "\"timestamp\":\"" + dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'") + "\", ";
        }

        public static string F(string name, long v, bool term = false)
        {
            return "\"" + name + "\":" + v + (term ? " " : ", ");
        }

        public static string F(string name, double v, bool term = false)
        {
            return "\"" + name + "\":" + v.ToString("0.######") + (term ? " " : "\", ");
        }

        public static string F(string name, bool v, bool term = false)
        {
            return "\"" + name + "\":" + (v ? "true" : "false") + (term ? " " : "\", ");
        }

        public static string F(string name, string v, bool term = false)
        {
            return "\"" + name + "\":\"" + v + (term ? "\" " : "\", ");
        }

        public static string F(string name, DateTime v, bool term = false)
        {
            return "\"" + name + "\":\"" + v.ToString("yyyy-MM-ddTHH:mm:ssZ") + (term ? "\" " : "\", ");
        }

        public static string F(string name, int[] array, bool end = false)
        {
            string s = "";
            foreach (int a in array)
            {
                if (s.Length > 0)
                    s += ", ";

                s += a.ToString();
            }

            return "\"" + name + "\":[" + s + "]" + (end ? "" : ", ");
        }

        public static string FF(string name, string v)      // no final comma
        {
            return F(name, v, true);
        }

        public static string FF(string name, bool v)      // no final comma
        {
            return F(name, v, true);
        }

        public static string FF(string name, long v)      // no final comma
        {
            return F(name, v, true);
        }

        public static void Write(string filename, string line)
        {
            using (Stream fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sr = new StreamWriter(fs))
                {
                    sr.WriteLine(line);
                    Console.WriteLine(line);
                }
            }
        }

        #endregion

        #region Help!

        public static string Help()
        {
            return
            "JournalPath CMDRname Options..\n" +
            "Travel   FSD name x y z (x y z is position as double)\n" +
            "         FSDTravel name x y z destx desty destz percentint \n" +
            "         Loc name x y z\n" +
            "         Docked, Undocked, Touchdown, Liftoff\n" +
            "         FuelScoop amount total\n" +
            "         JetConeBoost\n" +
            "Missions MissionAccepted/MissionCompleted faction victimfaction id\n" +
            "         MissionRedirected newsystem newstation id\n" +
            "         Missions activeid\n" +
            "         Bounty faction reward\n" +
            "         CommitCrime faction amount\n" +
            "         Interdiction name success isplayer combatrank faction power\n" +
            "         FactionKillBond faction victimfaction reward\n" +
            "         CapShipBond faction victimfaction reward\n" +
            "Commds   marketbuy fdname count price\n"+
            "Scans    ScanPlanet name\n" +
            "         ScanStar, ScanEarth\n" +
            "         NavBeaconScan\n" +
            "Ships    SellShipOnRebuy\n" +
            "Others   SearchAndRescue fdname count\n" +
            "         MiningRefined\n" +
            "         RepairDrone, CommunityGoal\n" +
            "         MusicNormal, MusicGalMap, MusicSysMap\n" +
            "         Friends name\n" +
            "         Died\n" +
            "         Resurrect cost\n" +
            "         PowerPlay, UnderAttack\n" +
            "         CargoDepot missionid updatetype(Collect,Deliver,WingUpdate) count total\n" +
            "         FighterDestroyed, FigherRebuilt, NpcCrewRank, NpcCrewPaidWage, LaunchDrone\n" +
            "         Market, ModuleInfo, Outfitting, Shipyard (use NOFILE after to say don't write the file)\n" +
            "         Promotion Combat/Trade/Explore/CQC/Federation/Empire Ranknumber\n"
            ;
        }

        #endregion

    }

}
