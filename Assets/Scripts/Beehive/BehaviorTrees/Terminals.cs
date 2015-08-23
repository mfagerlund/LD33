using System;
using Beehive.Lisp;

namespace Beehive.BehaviorTrees
{
    public class FunctionTask<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        private readonly Func<TBb, TaskState> _function;

        public FunctionTask(string name, Func<TBb, TaskState> function)
            : base(name)
        {
            _function = function;
        }

        public FunctionTask(Func<TBb, TaskState> function)
            : this(null, function)
        {
            _function = function;
        }

        public override TaskState Execute()
        {
            return _function(Context.Blackboard);
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            throw new InvalidOperationException("Not implemented!");
        }
    }

    public class BoolFunctionTask<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        private readonly Func<TBb, bool> _function;

        public BoolFunctionTask(string name, Func<TBb, bool> function)
            : base(name)
        {
            _function = function;
        }

        public BoolFunctionTask(Func<TBb, bool> function)
            : this(null, function)
        {
        }

        public override TaskState Execute()
        {
            return _function(Context.Blackboard) ? TaskState.Success : TaskState.Failure;
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            throw new NotImplementedException();
        }
    }

    //public class BooleanToState<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    //{
    //    public BooleanToState(string name, LispOperator<TBb> boolComputer)
    //        : base(name)
    //    {
    //        BoolComputer = boolComputer;
    //    }

    //    public BooleanToState(LispOperator<TBb> boolComputer)
    //        : this(null, boolComputer)
    //    {
    //    }

    //    public LispOperator<TBb> BoolComputer { get; set; }

    //    public override TaskState Execute()
    //    {
    //        return ((ICanEvaluateToBool)BoolComputer).EvaluateToBool() ? TaskState.Success : TaskState.Failure;
    //    }

    //    public override LispParser.MethodNode BuildParseTree()
    //    {
    //        LispParser.MethodNode node = new LispParser.MethodNode("BooleanToState");
    //        node.Add(BoolComputer.BuildParseTree());
    //        return node;
    //    }

    //    protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
    //    {
    //        BooleanToState<TBb> booleanToState = new BooleanToState<TBb>(null, null);
    //        booleanToState.BoolComputer = ((BehaviourTreeCompiler<TBb>)compiler).LispCompiler.Compile(blackboard, method.Children[0]);
    //        if (!(booleanToState.BoolComputer is ICanEvaluateToBool))
    //        {
    //            throw new InvalidOperationException("The bool computer must evaluate to a bool!");
    //        }
    //        return booleanToState;
    //    }
    //}
}