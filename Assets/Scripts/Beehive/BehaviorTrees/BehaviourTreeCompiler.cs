using System;
using System.Collections.Generic;
using Beehive.Lisp;

namespace Beehive.BehaviorTrees
{
    // TBb = TBlackboard, but it's too damn long...
    public class BehaviourTreeCompiler<TBb> : LispParser.ICompiler<TBb> where TBb : IBehaviourTreeBlackboard
    {
        private readonly BehaviourTree<TBb> _behaviourTree = new BehaviourTree<TBb>(null, null);
        private readonly NamedCoroutine<TBb> _namedCoroutine = new NamedCoroutine<TBb>(null, null);

        public BehaviourTreeCompiler()
        {
            Creators = new List<LispParser.ICanCreateFromLispParseTree<TBb>>
            {
                // Control Flow
                new Sequence<TBb>(),
                new Selector<TBb>(),

                // Utility Control Flow
                new UtilitySelector<TBb>(),
                new UtilitySequence<TBb>(),
                new TaskUtility<TBb>(null, null),

                // Decorators
                new Counter<TBb>(null),
                new AlwaysFail<TBb>(null),
                new AlwaysSucceed<TBb>(null),
                new Invert<TBb>(null),
                new UntilFail<TBb>(null),
                new UntilSuccess<TBb>(null),
                new RunOnce<TBb>(null),
                new RunNTimes<TBb>(null),
                
                // Leaves
                new Fail<TBb>(),
                new Succeed<TBb>(),
                new KeepRunning<TBb>(),
                new DebugLog<TBb>(),
                new FixedResultState<TBb>(null, TaskState.Running)
            };

            LispCompiler = new LispCompiler<TBb>();
        }

        public List<LispParser.ICanCreateFromLispParseTree<TBb>> Creators { get; set; }
        public LispCompiler<TBb> LispCompiler { get; set; }

        public BehaviourTree<TBb> Compile(TBb blackboard, string code)
        {
            LispParser lispParser = new LispParser();
            LispParser.Node parseTree = lispParser.Parse(code);
            return Compile(blackboard, parseTree);
        }

        public BehaviourTree<TBb> Compile(TBb blackboard, LispParser.Node parseTree)
        {
            BehaviourTree<TBb> behaviourTree = (BehaviourTree<TBb>)_behaviourTree.CreateFromParseTree(blackboard, parseTree, this);
            return behaviourTree;
        }

        LispParser.ICanCreateFromLispParseTree<TBb> LispParser.ICompiler<TBb>.Compile(TBb blackboard, LispParser.Node parseTree)
        {
            foreach (LispParser.ICanCreateFromLispParseTree<TBb> creator in Creators)
            {
                LispParser.ICanCreateFromLispParseTree<TBb> result = creator.CreateFromParseTree(blackboard, parseTree, this);
                if (result != null)
                {
                    return result;
                }
            }

            // For named couritines, we rewrite the parsetree to be able to use the regular flow.
            LispParser.MethodNode method = parseTree as LispParser.MethodNode;
            if (method != null)
            {
                Func<IEnumerator<TaskState>> function = blackboard.GetCoRoutineFunc(method.Identifier.Value);
                if (function != null)
                {
                    method.Add(method.Identifier);
                    method.Identifier = new LispParser.IdentifierNode("NamedCoroutine");
                    LispParser.ICanCreateFromLispParseTree<TBb> result = ((LispParser.ICanCreateFromLispParseTree<TBb>)_namedCoroutine).CreateFromParseTree(blackboard, method, this);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            throw new InvalidOperationException(
                string.Format("Unrecognized token: {0}.", parseTree));
        }
    }
}