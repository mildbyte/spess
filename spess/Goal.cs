using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.AI
{
    public abstract class Goal : IGoal
    {
        string name;
        ICompositeGoal parent;

        public string Name { get { return name; } }
        
        /// <summary>
        /// The toplevel parent that spawned this goal (not this goal's direct parent)
        /// </summary>
        public ICompositeGoal Parent { get { return parent; } }

        public Goal(string name, ICompositeGoal creator)
        {
            this.name = name;

            // If the goal was created by the user (creator == null), then the goal has no parent.
            // If the creator is a root goal, the new goal's parent becomes its creator.
            // Otherwise, the parent root goal propagates.
            if (creator == null) parent = null;
            else if (creator.Parent == null) parent = creator;
            else parent = creator.Parent;
        }
    }

    public interface IGoal
    {
        string Name { get; }
        ICompositeGoal Parent { get; }
    }

    public interface IFailableGoal : IGoal
    {
        bool Failed();
    }

    public interface ICompositeGoal : IGoal
    {
        /// <summary>
        /// Gets all subgoals that need to be executed for this goal to be completed
        /// </summary>
        /// <returns>An IEnumerable of required goals</returns>
        IEnumerable<Goal> GetSubgoals();
    }

    public interface IBaseGoal : IGoal
    {
        void Execute();
        bool IsComplete();
    }
}
