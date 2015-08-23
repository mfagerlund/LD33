using System;
using System.Collections.Generic;
using Beehive.Lisp;

namespace Beehive.BehaviorTrees
{
    // Co-routines allow you to write more complex tasks in a more compact and coding friendly manner. 
    public class Coroutine<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        private readonly Func<TBb, IEnumerator<TaskState>> _coroutineCreator;
        private IEnumerator<TaskState> _coroutine;

        public Coroutine(string name, Func<TBb, IEnumerator<TaskState>> coroutineCreator)
            : base(name)
        {
            _coroutineCreator = coroutineCreator;
        }

        public Coroutine(Func<TBb, IEnumerator<TaskState>> coroutineCreator)
            : this(null, coroutineCreator)
        {
        }     

        public override TaskState Execute()
        {
            if (!_coroutine.MoveNext())
            {
                _coroutine.Dispose();
                _coroutine = null;
                return TaskState.Failure;
            }

            TaskState taskState = _coroutine.Current;
            return taskState;
        }

        public override LispParser.MethodNode BuildParseTree()
        {
            throw new NotImplementedException("Not implemented, use NamedCoroutine instead?");
        }

        protected override void OnEnter()
        {
            if (_coroutine == null)
            {
                _coroutine = _coroutineCreator(Context.Blackboard);
            }
        }
        
        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            throw new NotImplementedException("Not implemented, use NamedCoroutine instead?");
        }

        protected override void OnExit()
        {
            base.OnExit();
            if (State != TaskState.Running && _coroutine != null)
            {
                _coroutine.Dispose();
                _coroutine = null;
            }
        }
    }

    public class NamedCoroutine<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        private IEnumerator<TaskState> _coroutine;

        public NamedCoroutine(string name, NamedCoroutineCreator namedCoroutineCreator)
            : base(name)
        {
            NamedCoroutineCreator = namedCoroutineCreator;
        }

        public override string DebugText
        {
            get { return NamedCoroutineCreator.Name; }
        }

        private NamedCoroutineCreator NamedCoroutineCreator { get; set; }
      
        public override TaskState Execute()
        {
            if (!_coroutine.MoveNext())
            {
                _coroutine.Dispose();
                _coroutine = null;
                return TaskState.Success;
            }

            TaskState taskState = _coroutine.Current;
            return taskState;
        }

        public override LispParser.MethodNode BuildParseTree()
        {
            // Named Coroutine is hidden in the code, so this behaves slightly differently than would be expected
            //LispParser.MethodNode node = base.BuildParseTree();
            //node.Add(NamedCoroutineCreator.BuildParseTree());
            //return node;
            LispParser.MethodNode node = new LispParser.MethodNode(NamedCoroutineCreator.Name);
            node.Add("Name", Name, true);
            return node;
        }

        protected override void OnEnter()
        {
            if (_coroutine == null)
            {
                _coroutine = NamedCoroutineCreator.CreateCoroutine();
            }
        }
        
        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            NamedCoroutine<TBb> namedCoroutine = new NamedCoroutine<TBb>(null, null);

            foreach (LispParser.Node child in method.Children)
            {
                LispParser.IdentifierNode identifier = child as LispParser.IdentifierNode;
                if (identifier != null)
                {
                    Func<IEnumerator<TaskState>> func = blackboard.GetCoRoutineFunc(identifier.Value);

                    if (func == null)
                    {
                        throw new InvalidOperationException("Unrecognized couroutine creator: " + identifier.Value);
                    }

                    namedCoroutine.NamedCoroutineCreator = new NamedCoroutineCreator(identifier.Value, func);
                }
            }

            return namedCoroutine;
        }

        protected override void OnExit()
        {
            base.OnExit();
            if (State != TaskState.Running && _coroutine != null)
            {
                _coroutine.Dispose();
                _coroutine = null;
            }
        }
    }

    public class NamedCoroutineCreator : LispParser.ICanBuildLispParseTree
    {
        private readonly Func<IEnumerator<TaskState>> _func;

        public NamedCoroutineCreator(string name, Func<IEnumerator<TaskState>> func)
        {
            Name = name;
            _func = func;
        }

        public string Name { get; set; }

        public IEnumerator<TaskState> CreateCoroutine()
        {
            return _func();
        }

        public LispParser.Node BuildParseTree()
        {
            return new LispParser.IdentifierNode(Name);
        }
    }
}