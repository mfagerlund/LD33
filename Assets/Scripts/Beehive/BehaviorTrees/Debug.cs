using System;
using Beehive.Lisp;

namespace Beehive.BehaviorTrees
{
    public class DebugLog<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public DebugLog(string name = null)
            : base(name)
        {
        }

        public LispOperator<TBb> StringComputer { get; set; }

        public override TaskState Execute()
        {
            Console.WriteLine(StringComputer.TryEvaluateToString());
            return TaskState.Success;
        }

        public override LispParser.MethodNode BuildParseTree()
        {
            LispParser.MethodNode node = new LispParser.MethodNode("DebugLog");
            node.Add(StringComputer.BuildParseTree());
            return node;
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            DebugLog<TBb> debugLog = new DebugLog<TBb>(null);
            debugLog.StringComputer = ((BehaviourTreeCompiler<TBb>)compiler).LispCompiler.Compile(blackboard, method.Children[0]);
            if (!(debugLog.StringComputer is ICanEvaluateToString))
            {
                throw new InvalidOperationException("The string computer must evaluate to a string!");
            }
            return debugLog;
        }
    }
}