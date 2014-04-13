using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.AI
{
    class GoalQueue
    {
        LinkedList<Goal> goals;

        public IEnumerable<Goal> Goals { get { return goals; } }

        public void Update()
        {
            if (!goals.Any()) return;
            var currentGoal = goals.First.Value;

            if (currentGoal.IsComplete())
            {
                goals.RemoveFirst();
            }
            else
            {
                var newGoals = currentGoal.Execute();
                goals = new LinkedList<Goal>(newGoals.Concat(goals));
            }
        }
    }
}
