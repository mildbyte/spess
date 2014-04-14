using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.AI
{
    abstract class Goal
    {
        string name;
        ICompositeGoal parent;

        public string Name { get { return name; } }
        
        /// <summary>
        /// The toplevel parent that spawned this goal (not this goal's direct parent)
        /// </summary>
        public ICompositeGoal Parent { get { return parent; } }

        public Goal(string name, ICompositeGoal parent)
        {
            this.name = name;
            this.parent = parent;
        }
    }

    interface ICompositeGoal
    {
        IEnumerable<Goal> GetSubgoals();
    }

    interface IBaseGoal
    {
        void Execute();
        bool IsComplete();
    }
}
