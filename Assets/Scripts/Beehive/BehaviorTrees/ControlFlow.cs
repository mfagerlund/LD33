using System;
using System.Collections.Generic;
using Beehive.Lisp;

namespace Beehive.BehaviorTrees
{
    /* From Wikipedia:
       * Control flow node
       * A control flow node is used to control the subtasks of which it is composed. A control flow node 
       * may be either a selector (fallback) node or a sequence node. They run each of their subtasks in turn. 
       * When a subtask is completed and returns its status (success or failure), the control flow node 
       * decides whether to execute the next subtask or not.*/
    public abstract class ControlFlowNode<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        protected ControlFlowNode(string name, params TreeTask<TBb>[] children)
            : base(name)
        {
            Children = new List<TreeTask<TBb>>(children);
        }

        public List<TreeTask<TBb>> Children { get; set; }
        public LispOperator<TBb> Predicate { get; set; }

        public override void ForEachTask(Action<TreeTask<TBb>> action)
        {
            base.ForEachTask(action);
            foreach (TreeTask<TBb> child in Children)
            {
                child.ForEachTask(action);
            }
        }

        public override LispParser.MethodNode BuildParseTree()
        {
            LispParser.MethodNode node = (LispParser.MethodNode)base.BuildParseTree();
            if (Predicate != null)
            {
                node.Add("if", Predicate.BuildParseTree());
            }

            foreach (TreeTask<TBb> treeTask in Children)
            {
                node.Add(treeTask.BuildParseTree());
            }
            return node;
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            ControlFlowNode<TBb> controlFlowNode = CreateControlFlow(blackboard, method, compiler);

            if (controlFlowNode == null)
            {
                return null;
            }

            foreach (LispParser.Node child in method.Children)
            {
                bool handled =
                    child.NamedParamX<LispParser.StringNode>("Name", v => controlFlowNode.Name = v.Value) ||
                    child.ParamX<LispParser.MethodNode>(null, v => controlFlowNode.Children.Add((TreeTask<TBb>)compiler.Compile(blackboard, child)));

                if (!handled)
                {
                    LispParser.NamedParameterNode named = child as LispParser.NamedParameterNode;

                    if (named != null && named.Identifier.Value == "if")
                    {
                        controlFlowNode.Predicate = ((BehaviourTreeCompiler<TBb>)compiler).LispCompiler.Compile(blackboard, named.Value);
                        if (!(controlFlowNode.Predicate is ICanEvaluateToBool))
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    "The if predicate must evaluate to a bool, {0} doesn't!",
                                    named.Value));
                        }
                        continue;
                    }

                    throw new InvalidOperationException(
                        string.Format(
                            "Unhandled child {0} on {1}!",
                            child,
                            this));
                }
            }

            return controlFlowNode;
        }

        protected abstract ControlFlowNode<TBb> CreateControlFlow(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler);
    }

    /* From Wikipedia:
     * Selector (fallback) node
     * Fallback nodes are used to find and execute the first child that does not fail. 
     * A fallback node will return immediately with a status code of success or running when one of 
     * its children returns success or running (see Figure I and the pseudocode below). 
     * The children are ticked in order of importance, from left to right.*/
    // Selector is symbolised by "?"
    public class Selector<TBb> : ControlFlowNode<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public Selector(params TreeTask<TBb>[] children)
            : this(null, children)
        {
        }

        public Selector(string name, params TreeTask<TBb>[] children)
            : base(name, children)
        {
        }

        public override TaskState Execute()
        {
            if (Predicate != null)
            {
                if (Predicate.TryEvaluateToBool())
                {
                    return TaskState.Failure;
                }
            }         
            foreach (TreeTask<TBb> task in Children)
            {
                TaskState childStatus = task.Tick();
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

        protected override string GetClassName()
        {
            return "?";
        }

        protected override ControlFlowNode<TBb> CreateControlFlow(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new Selector<TBb>();
        }
    }

    /* From Wikipedia:
     * Sequence node
     * Sequence nodes are used to find and execute the first child that has not yet succeeded. 
     * A sequence node will return immediately with a status code of failure or running when 
     * one of its children returns failure or running (see Figure II and the pseudocode below). 
     * The children are ticked in order, from left to right.*/
    // Sequence is symbolised by "→"
    public class Sequence<TBb> : ControlFlowNode<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public Sequence(params TreeTask<TBb>[] children)
            : this(null, children)
        {
        }

        public Sequence(string name, params TreeTask<TBb>[] children)
            : base(name, children)
        {
        }

        public override TaskState Execute()
        {
            if (Predicate != null)
            {
                if (!Predicate.TryEvaluateToBool())
                {
                    return TaskState.Failure;
                }
            }
            foreach (TreeTask<TBb> task in Children)
            {
                TaskState childStatus = task.Tick();
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

        protected override string GetClassName()
        {
            return "→";
        }

        protected override ControlFlowNode<TBb> CreateControlFlow(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new Sequence<TBb>();
        }
    }
}