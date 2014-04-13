﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace spess
{
    class Universe
    {
        List<Sector> sectors;
        List<Owner> owners;

        Dictionary<Owner, List<Gate>> ownerKnownGates;

        public List<Sector> Sectors { get { return sectors; } }
        public List<Owner> Owners { get { return owners; } }

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
        /// Gets a sequence of gates that the owner knows about that allows his ship to get from
        /// one sector to another.
        /// </summary>
        /// <param name="o">Owner of the ship</param>
        /// <param name="s1">Source sector</param>
        /// <param name="s2">Destination sector</param>
        /// <returns>A list of gates the ship has to take or null if there is no such route</returns>
        public List<Gate> GetGateAwareRoute(Owner o, Sector s1, Sector s2)
        {
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
                foreach (Gate g in currNode.Sector.Gates)
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
            owners.Add(new Owner());
        }

        public Sector AddSector()
        {
            Sector s = new Sector();
            sectors.Add(s);
            return s;
        }

        public Ship AddShip(Sector sector, Vector3 position, Owner owner, float maxSpeed)
        {
            Ship ship = new Ship("", new Location(sector, position), owner, maxSpeed);
            sector.AddShip(ship);
            return ship;
        }

        public ProductionStation AddProductionStation(Sector sector, Vector3 position, ProductionRule production, int storageSpace) {
            ProductionStation station = new ProductionStation("", new Location(sector, position), production, storageSpace);
            sector.Stations.Add(station);
            return station;
        }

        public void JoinSectors(Sector sector1, Sector sector2, Vector3 sector1GateLoc, Vector3 sector2GateLoc)
        {
            Location s1Loc = new Location(sector1, sector1GateLoc);
            Location s2Loc = new Location(sector2, sector2GateLoc);

            Gate s1Gate = new Gate("", s1Loc, s2Loc);
            Gate s2Gate = new Gate("", s2Loc, s1Loc);

            sector1.Gates.Add(s1Gate);
            sector2.Gates.Add(s2Gate);
        }

        public void Update(float timeDifference)
        {
            foreach (Sector s in sectors)
            {
                s.Update(timeDifference);
            }
        }
    }
}
