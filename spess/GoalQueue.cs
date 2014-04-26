using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.AI
{
    public delegate void OrderCompleted(IGoal goal);
    public delegate void OrderStarted(IBaseGoal goal);
    public delegate void OrderFailed(IGoal goal);

    public class GoalQueue
    {
        LinkedList<Goal> goals;
        LinkedList<IBaseGoal> pendingGoals;

        /// <summary>
        /// Failed toplevel goals. This is used to cancel all goals spawned by a goal
        /// (not only its immediate children) if one child fails.
        /// 
        /// For example, MoveTo consists of MoveToSector and MoveInSector.
        /// If MoveToSector fails (ship can't find a route), MoveInSector is still
        /// executed, so the ships move to the target position in the same sector.
        /// </summary>
        HashSet<IGoal> failedGoals;

        public IEnumerable<Goal> Goals { get { return goals; } }
        public IEnumerable<IBaseGoal> PendingGoals { get { return pendingGoals; } }
        public OrderCompleted OnOrderCompleted { get; set; }
        public OrderStarted OnOrderStarted { get; set; }
        public OrderFailed OnOrderFailed { get; set; }

        public void AddGoal(Goal g)
        {
            goals.AddLast(g);
        }

        public GoalQueue()
        {
            goals = new LinkedList<Goal>();
            pendingGoals = new LinkedList<IBaseGoal>();
            failedGoals = new HashSet<IGoal>();
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
                if (OnOrderCompleted != null) OnOrderCompleted(pendingGoals.First());
                pendingGoals.RemoveFirst();
            }
            
            // If we still have pending goals or there are no goals that we can execute, return
            if (pendingGoals.Any() || !goals.Any()) return;

            // Execute a goal from the goals' list
            Goal currGoal = goals.First.Value;
            goals.RemoveFirst();

            // If one of the cousins of this goal failed, ignore this goal.
            if (failedGoals.Contains(currGoal.Parent)) return;

            // If the goal failed, add it to the failed goals' set (if it's a root goal)
            // or add its ultimate parent to the set otherwise.
            if (currGoal is IFailableGoal && ((IFailableGoal)currGoal).Failed())
            {
                if (currGoal.Parent == null) failedGoals.Add(currGoal);
                else failedGoals.Add(currGoal.Parent);
                if (OnOrderFailed != null) OnOrderFailed(currGoal);
            }

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
                if (OnOrderStarted != null) OnOrderStarted(currGoal as IBaseGoal);
            }
        }
    }
}
