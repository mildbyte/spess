using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using spess.ExchangeData;
using spess.AI;

namespace spess
{
    public class Universe
    {
        List<Sector> sectors;
        List<Owner> owners;

        Dictionary<Owner, List<Gate>> ownerKnownGates;

        float gameTime = 0.0f;

        public List<Sector> Sectors { get { return sectors; } }
        public List<Owner> Owners { get { return owners; } }
        public float GameTime { get { return gameTime; } }

        public Owner GetPlayer() { return owners[0]; }
        

        class BFSNode
        {
            Gate prevGate;
            BFSNode prevNode;
            Sector sector;

            public Gate PrevGate { get { return prevGate; } }
            public BFSNode PrevNode { get { return prevNode; } }
            public Sector Sector { get { return sector; } }
            public BFSNode(Gate prevGate, BFSNode prevNode, Sector sector)
            {
                this.sector = sector;
                this.prevGate = prevGate;
                this.prevNode = prevNode;
            }
        }

        /// <summary>
        /// Finds the closest space body to a given location that satisfies a given predicate.
        /// Only takes into account the number of jumps to the body
        /// </summary>
        /// <param name="pred">The predicate, for example, spaceBody is Exchange</param>
        /// <param name="location">The location to search from</param>
        /// <param name="o">The owner (used for known gate-aware calculations)</param>
        /// <returns>The closest body</returns>
        public SpaceBody GetClosestBodyBy(Func<SpaceBody, bool> pred, Location location, Owner o)
        {
            // TODO better performance by not throwing away the best body path

            SpaceBody sectorBestBody = location.Sector.Contents.Where(b => pred(b))
                .OrderBy(b => (b.Location.Coordinates - location.Coordinates).Length()).FirstOrDefault();

            if (sectorBestBody == null)
                return Sectors.Select(s => s.Contents).SelectMany(x => x).Where(b => pred(b))
                    .OrderBy(b => GetGateAwareRoute(o, b.Location.Sector, location.Sector).Count()).FirstOrDefault();
            else return sectorBestBody;
        }

        /// <summary>
        /// Gets a sequence of gates that the owner knows about that allows his ship to get from
        /// one sector to another.
        /// </summary>
        /// <param name="o">Owner of the ship</param>
        /// <param name="s1">Source sector</param>
        /// <param name="s2">Destination sector</param>
        /// <returns>A list of gates the ship has to take or null if there is no such route</returns>
        public List<Gate> GetGateAwareRoute(Owner o, Sector s1, Sector s2)
        {
            // Don't need to find a route if the sectors are the same!
            if (s1 == s2) return new List<Gate>();

            // Drop out if we don't know the owner
            if (!ownerKnownGates.ContainsKey(o)) return null;
            List<Gate> knownGates = ownerKnownGates[o];

            // Initialize the BFS structures
            HashSet<Sector> visitedSectors = new HashSet<Sector>();
            LinkedList<BFSNode> bfsQueue = new LinkedList<BFSNode>();
            bfsQueue.AddLast(new BFSNode(null, null, s1));

            // Perform a breadth-first traversal of the sector graphs, recording the gate we took
            // and the previous BFS node
            BFSNode currNode = null;
            while (bfsQueue.Count > 0)
            {
                // Pop a node from the queue
                currNode = bfsQueue.First.Value;
                bfsQueue.RemoveFirst();
                
                // Ignore the sector if we've been there
                if (visitedSectors.Contains(currNode.Sector)) continue;
                visitedSectors.Add(currNode.Sector);

                // Stop if we've reached the destination
                if (currNode.Sector == s2) break;

                // Take all possible gates we know about
                foreach (Gate g in currNode.Sector.Contents.OfType<Gate>())
                {
                    if (!knownGates.Contains(g)) continue;
                    bfsQueue.AddLast(new BFSNode(g, currNode, g.Destination.Sector));
                }
            }

            // If we haven't reached the sector, we've failed.
            if (currNode.Sector != s2) return null;

            // Reconstruct the list of gates we took
            List<Gate> result = new List<Gate>();
            while (currNode.PrevGate != null)
            {
                result.Add(currNode.PrevGate);
                currNode = currNode.PrevNode;
            }

            result.Reverse();

            return result;
        }

        public Universe()
        {
            sectors = new List<Sector>();
            owners = new List<Owner>();
            owners.Add(new PlayerOwner(this));
            ownerKnownGates = new Dictionary<Owner, List<Gate>>();
        }

        public void DiscoverGate(Owner o, Gate g)
        {
            if (!ownerKnownGates.ContainsKey(o)) ownerKnownGates[o] = new List<Gate>();
            ownerKnownGates[o].Add(g);
        }

        public Owner AddOwner()
        {
            Owner o = new AIOwner(this);
            owners.Add(o);
            return o;
        }

        public Sector AddSector(String name)
        {
            Sector s = new Sector();
            s.Name = name;
            sectors.Add(s);
            return s;
        }

        public Ship AddShip(string name, Sector sector, Vector3 position, Owner owner, float maxSpeed)
        {
            Ship ship = new Ship(name, new Location(sector, position), owner, maxSpeed, this);
            sector.AddItem(ship);
            return ship;
        }

        public AIShip AddAIShip(string name, Sector sector, Vector3 position, Owner owner, float maxSpeed)
        {
            AIShip ship = new AIShip(name, new Location(sector, position), owner, maxSpeed, this);
            sector.AddItem(ship);
            return ship;
        }

        public ProductionStation AddProductionStation(string name, Sector sector, Vector3 position, Owner owner, ProductionRule production, int storageSpace) {
            ProductionStation station = new ProductionStation(name, new Location(sector, position), production, storageSpace, this);
            station.Owner = owner;
            sector.AddItem(station);
            return station;
        }

        public Exchange AddExchange(string name, Sector sector, Vector3 position)
        {
            Exchange exchange = new Exchange(name, new Location(sector, position), this);
            sector.AddItem(exchange);
            return exchange;
        }

        public void JoinSectors(Sector sector1, Sector sector2, Vector3 sector1GateLoc, Vector3 sector2GateLoc)
        {
            Location s1Loc = new Location(sector1, sector1GateLoc);
            Location s2Loc = new Location(sector2, sector2GateLoc);

            Gate s1Gate = new Gate("", s1Loc, s2Loc, this);
            Gate s2Gate = new Gate("", s2Loc, s1Loc, this);

            sector1.AddItem(s1Gate);
            sector2.AddItem(s2Gate);
        }

        public void Update(float timeDifference)
        {
            gameTime += timeDifference;
            Sectors.ForEach(s => s.Update(timeDifference));
            Owners.ForEach(o => o.Update(timeDifference));
        }
    }
}
