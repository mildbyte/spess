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

        public string Name { get { return name; } }

        public Goal(string name)
        {
            this.name = name;
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
