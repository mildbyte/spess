﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.AI
{
    public delegate void OrderCompleted(IGoal goal);
    public delegate void OrderStarted(IBaseGoal goal);
    public delegate void OrderFailed(IGoal goal);

    public static class Extensions
    {
        public static void RemoveAll<T>(this LinkedList<T> list, Func<T, bool> p)
        {
            var currNode = list.First;
            while (currNode != null)
            {
                if (p(currNode.Value))
                {
                    var toRemove = currNode;
                    currNode = currNode.Next;
                    list.Remove(toRemove);
                }
                else currNode = currNode.Next;
            }
        }
    }

    public class GoalQueue
    {
        LinkedList<Goal> goals;
        LinkedList<IBaseGoal> pendingGoals;
        Dictionary<ICompositeGoal, int> pendingChildren;

        public IEnumerable<Goal> Goals { get { return goals; } }
        public IEnumerable<IBaseGoal> PendingGoals { get { return pendingGoals; } }
        public OrderCompleted OnOrderCompleted { get; set; }
        public OrderStarted OnOrderStarted { get; set; }
        public OrderFailed OnOrderFailed { get; set; }

        public void AddGoal(Goal g)
        {
            if (g.Parent != null) return; // User can only add root goals
            goals.AddLast(g);
        }

        public bool IsEmpty()
        {
            return (!goals.Any() && !pendingGoals.Any());
        }

        public GoalQueue()
        {
            goals = new LinkedList<Goal>();
            pendingGoals = new LinkedList<IBaseGoal>();
            pendingChildren = new Dictionary<ICompositeGoal, int>();
        }

        public void CancelAllOrders() {
            pendingGoals.Clear();
            goals.Clear();
        }

        public void CancelPendingOrders()
        {
            pendingGoals.Clear();
        }

        public void Update()
        {
            // Keep removing complete goals from the pending goal queue
            while (pendingGoals.Any() && pendingGoals.First.Value.IsComplete())
            {
                // If the goal is a root goal, notify about its completion.
                IBaseGoal goal = pendingGoals.First();
                if (goal.Parent == null)
                {
                    if (OnOrderCompleted != null) OnOrderCompleted(goal);
                }
                else
                {
                    // Otherwise, if this is the last goal spawned by a parent, notify about the parent goal completion.
                    pendingChildren[goal.Parent]--;
                    if (pendingChildren[goal.Parent] == 0)
                    {
                        pendingChildren.Remove(goal.Parent);
                        if (OnOrderCompleted != null) OnOrderCompleted(goal.Parent);
                    }
                }
                pendingGoals.RemoveFirst();
            }
            
            // If we still have pending goals or there are no goals that we can execute, return
            if (pendingGoals.Any() || !goals.Any()) return;

            // Execute a goal from the goals' list
            Goal currGoal = goals.First.Value;
            goals.RemoveFirst();

            // If the goal failed, add it to the failed goals' set (if it's a root goal)
            // or add its ultimate parent to the set otherwise.
            if (currGoal is IFailableGoal && ((IFailableGoal)currGoal).Failed())
            {
                // The parent goal whose children we should cancel.
                // Direct iteration is used here instead of keeping a list of parents
                // who failed, since the parents all of whose children have been deleted
                // also need to be deleted and the goal queue shouldn't be too large anyway.
                IGoal soughtGoal;
                if (currGoal.Parent == null) soughtGoal = currGoal;
                else soughtGoal = currGoal.Parent;

                pendingGoals.RemoveAll(g => g.Parent == soughtGoal);
                goals.RemoveAll(g => g.Parent == soughtGoal);

                // Notify that the root goal failed
                if (OnOrderFailed != null) OnOrderFailed(soughtGoal);

                return;
            }

            // For a composite goal, add all of its children to the goal queue.
            // For a base goal, execute it and move it to the pending goals' queue.
            if (currGoal is ICompositeGoal)
            {
                var newGoals = ((ICompositeGoal)currGoal).GetSubgoals();

                // Increase the number of base goals that the root goal depends on
                foreach (Goal g in newGoals)
                {
                    if (g is IBaseGoal)
                    {
                        if (!pendingChildren.ContainsKey(g.Parent)) pendingChildren[g.Parent] = 0;
                        pendingChildren[g.Parent]++;
                    }
                }

                goals = new LinkedList<Goal>(newGoals.Concat(goals));
            }
            else if (currGoal is IBaseGoal) {
                ((IBaseGoal)currGoal).Execute();
                pendingGoals.AddLast((IBaseGoal)currGoal);
                if (OnOrderStarted != null) OnOrderStarted(currGoal as IBaseGoal);
            }
        }
    }
}
