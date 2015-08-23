using System;
using System.Collections.Generic;
using System.Linq;
using Beehive.Lisp;

namespace Beehive.BehaviorTrees
{
    // See http://stackoverflow.com/questions/14360893/intelligent-agents-utility-function
    // Utility is a way of handling the fact that different tasks have different utility at different times.
    // Traditional BehaviourTrees order Tasks in a static manner, which doesn't allow for the fact
    // that factors that change over time can influence the proper ordering of the tasks.

    // Associates a utility function (computes how important a task is) with the task itself. Typically,
    // the function should return values in the range 0 (don't do it at all) to 1 (must do now).
    public class TaskUtility<TBb> : LispParser.ICanBuildLispParseTree, LispParser.ICanCreateFromLispParseTree<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public TaskUtility(LispOperator<TBb> utilityComputer, TreeTask<TBb> task)
        {
            UtilityComputer = utilityComputer;
            Task = task;
        }

        public float Utility { get; set; }
        public LispOperator<TBb> UtilityComputer { get; set; }
        public TreeTask<TBb> Task { get; set; }

        public void UpdateUtility()
        {
            Utility = UtilityComputer.TryEvaluateToFloat();
        }

        public LispParser.Node BuildParseTree()
        {
            LispParser.MethodNode node = new LispParser.MethodNode("TaskUtility");
            node.Add(UtilityComputer.BuildParseTree());
            node.Add(Task.BuildParseTree());
            return node;
        }

        public LispParser.ICanCreateFromLispParseTree<TBb> CreateFromParseTree(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.MethodNode method = node as LispParser.MethodNode;

            if (method == null || method.MethodName != "TaskUtility")
            {
                return null;
            }

            TaskUtility<TBb> taskUtility = new TaskUtility<TBb>(null, null);
            taskUtility.UtilityComputer = ((BehaviourTreeCompiler<TBb>)compiler).LispCompiler.Compile(blackboard, method.Children[0]);
            if (!(taskUtility.UtilityComputer is ICanEvaluateToFloat))
            {
                throw new InvalidOperationException("The utility computer must evaluate to a float!");
            }
            taskUtility.Task = (TreeTask<TBb>)compiler.Compile(blackboard, method.Children[1]); 
            return taskUtility;
        }
    }

    // A control flow node for managing utility tasks
    public abstract class UtilityControlFlowNode<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        protected UtilityControlFlowNode(string name, params TaskUtility<TBb>[] children)
            : base(name)
        {
            Children = new List<TaskUtility<TBb>>(children);
        }

        protected UtilityControlFlowNode(params TaskUtility<TBb>[] children)
            : this(null, children)
        {
        }

        public List<TaskUtility<TBb>> Children { get; private set; }
        public override void ForEachTask(Action<TreeTask<TBb>> action)
        {
            base.ForEachTask(action);
            foreach (TaskUtility<TBb> child in Children)
            {
                child.Task.ForEachTask(action);
            }
        }

        public override LispParser.MethodNode BuildParseTree()
        {
            LispParser.MethodNode node = base.BuildParseTree();
            foreach (TaskUtility<TBb> taskUtility in Children)
            {
                node.Children.Add(taskUtility.BuildParseTree());
            }
            return node;
        }

        protected void UpdateUtilities()
        {
            Children.ForEach(c => c.UpdateUtility());
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            UtilityControlFlowNode<TBb> utilityControlFlowNode = CreateUtilityControlFlow(blackboard, method, compiler);

            if (utilityControlFlowNode == null)
            {
                return null;
            }

            foreach (LispParser.Node child in method.Children)
            {
                child.NamedParamX<LispParser.StringNode>("Name", v => utilityControlFlowNode.Name = v.Value);
                child.ParamX<LispParser.MethodNode>(null, v => utilityControlFlowNode.Children.Add((TaskUtility<TBb>)compiler.Compile(blackboard, child)));
            }

            return utilityControlFlowNode;
        }

        protected abstract UtilityControlFlowNode<TBb> CreateUtilityControlFlow(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler);
    }

    // Orders tasks by descending utility (highest first) and executes them in order until one
    // succeeds or returns running. Se regular Selector for details.
    public class UtilitySelector<TBb> : UtilityControlFlowNode<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public UtilitySelector(params TaskUtility<TBb>[] children)
            : base(children)
        {
        }

        public override TaskState Execute()
        {
            UpdateUtilities();
            foreach (TaskUtility<TBb> tu in Children.OrderByDescending(tu => tu.Utility))
            {
                if (tu.Utility <= 0)
                {
                    break;
                }

                TaskState childStatus = tu.Task.Tick();
                switch (childStatus)
                {
                    case TaskState.Running:
                        return TaskState.Running;
                    case TaskState.Success:
                        return TaskState.Success;
                }
            }
            return TaskState.Failure;
        }

        protected override UtilityControlFlowNode<TBb> CreateUtilityControlFlow(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new UtilitySelector<TBb>();
        }
    }

    // Orders tasks by descending utility (highest first) and executes them in order until one
    // fails or returns running. See regular Sequence for details.
    public class UtilitySequence<TBb> : UtilityControlFlowNode<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public UtilitySequence(params TaskUtility<TBb>[] children)
            : base(children)
        {
        }

        public override TaskState Execute()
        {
            UpdateUtilities();
            foreach (TaskUtility<TBb> tu in Children.OrderByDescending(tu => tu.Utility))
            {
                if (tu.Utility <= 0)
                {
                    break;
                }
                TaskState childStatus = tu.Task.Tick();
                switch (childStatus)
                {
                    case TaskState.Running:
                        return TaskState.Running;
                    case TaskState.Failure:
                        return TaskState.Failure;
                }
            }
            return TaskState.Success;
        }

        protected override UtilityControlFlowNode<TBb> CreateUtilityControlFlow(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new UtilitySequence<TBb>();
        }
    }
}