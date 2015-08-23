using System;
using Beehive.Lisp;

namespace Beehive.BehaviorTrees
{
    public class Counter<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public Counter(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public int ExecuteCount { get; set; }
        public int OnEnterCount { get; set; }
        public int OnExitCount { get; set; }

        public void Reset()
        {
            OnEnterCount = 0;
            ExecuteCount = 0;
            OnExitCount = 0;
        }

        public override TaskState Execute()
        {
            ExecuteCount++;
            return DecoratedTask.Tick();
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            OnEnterCount++;
        }

        protected override void OnExit()
        {
            base.OnExit();
            OnExitCount++;
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new Counter<TBb>(null);
        }
    }

    public class FixedResultState<TBb> : LeafTreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public FixedResultState(string name, TaskState resultState)
            : base(name)
        {
            ResultState = resultState;
        }

        public FixedResultState(TaskState resultState)
            : this(null, resultState)
        {
        }

        public TaskState ResultState { get; set; }

        public override TaskState Execute()
        {
            return ResultState;
        }

        public override LispParser.MethodNode BuildParseTree()
        {
            LispParser.MethodNode node = base.BuildParseTree();
            node.Add("ResultState", ResultState.ToString());
            return node;
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            FixedResultState<TBb> fixedResultState = new FixedResultState<TBb>(ResultState);
            foreach (LispParser.Node child in method.Children)
            {
                child.NamedParamX<LispParser.StringNode>("ResultState", v => fixedResultState.ResultState = (TaskState)Enum.Parse(typeof(TaskState), v.Value));
            }
            return fixedResultState;
        }
    }
}