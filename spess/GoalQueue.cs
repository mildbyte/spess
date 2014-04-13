﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.AI
{
    class GoalQueue
    {
        LinkedList<Goal> goals;
        LinkedList<IBaseGoal> pendingGoals;

        public IEnumerable<Goal> Goals { get { return goals; } }
        public IEnumerable<IBaseGoal> PendingGoals { get { return pendingGoals; } }

        public void AddGoal(Goal g)
        {
            goals.AddLast(g);
        }

        public GoalQueue()
        {
            goals = new LinkedList<Goal>();
            pendingGoals = new LinkedList<IBaseGoal>();
        }

        public void Update()
        {
            // Keep removing complete goals from the pending goal queue
            while (pendingGoals.Any() && pendingGoals.First.Value.IsComplete()) pendingGoals.RemoveFirst();
            
            // If we still have pending goals or there are no goals that we can execute, return
            if (pendingGoals.Any() || !goals.Any()) return;

            // Execute a goal from the goals' list
            Goal currGoal = goals.First.Value;
            goals.RemoveFirst();

            // For a composite goal, add all of its children to the goal queue.
            // For a base goal, execute it and move it to the pending goals' queue.
            if (currGoal is ICompositeGoal)
            {
                var newGoals = ((ICompositeGoal)currGoal).GetSubgoals();
                goals = new LinkedList<Goal>(newGoals.Concat(goals));
            }
            else if (currGoal is IBaseGoal) {
                ((IBaseGoal)currGoal).Execute();
                pendingGoals.AddLast((IBaseGoal)currGoal);
            }
        }
    }
}
