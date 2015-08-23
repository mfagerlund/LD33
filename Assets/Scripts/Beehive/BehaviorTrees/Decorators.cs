using System;
using System.Linq;
using Beehive.Lisp;
using Beehive.Utilities;

namespace Beehive.BehaviorTrees
{
    /* From https://github.com/libgdx/gdx-ai/wiki/Behavior-Trees#decorator:
  
     * Decorator

  The name "decorator" is taken from object-oriented software engineering. The decorator pattern 
  refers to a class that wraps another class, modifying its behavior. If the decorator has 
  the same interface as the class it wraps, then the rest of the software doesn't need to know 
  if it is dealing with the original class or the decorator.

  In the context of a behavior tree, a decorator is a task that has one single child task and 
  modifies its behavior in some way. You could think of it like a composite task with 
  a single child.

  decorator

  There are many different types of useful decorators:
     * ..
  There are many more decorators you might want to use when building behavior trees, but I think these are enough for now.*/

    public abstract class Decorator<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        protected Decorator(string name, TreeTask<TBb> decoratedTask)
            : base(name)
        {
            DecoratedTask = decoratedTask;
        }

        protected Decorator(TreeTask<TBb> decoratedTask)
            : this(null, decoratedTask)
        {
        }

        public TreeTask<TBb> DecoratedTask { get; set; }

        public override void ForEachTask(Action<TreeTask<TBb>> action)
        {
            base.ForEachTask(action);
            DecoratedTask.ForEachTask(action);
        }

        public override LispParser.MethodNode BuildParseTree()
        {
            LispParser.MethodNode node = base.BuildParseTree();
            node.Add(DecoratedTask.BuildParseTree());
            return node;
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            Decorator<TBb> decorator = CreateDecoratorFromParseTree(blackboard, method, compiler);
            if (decorator != null)
            {
                if (decorator.GetType() != GetType())
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Expected {0} but compiled to {1}!",
                            TypeHelper.GetFriendlyTypeName(GetType()),
                            TypeHelper.GetFriendlyTypeName(decorator.GetType())));
                }

                decorator.DecoratedTask = (TreeTask<TBb>)compiler.Compile(blackboard, method.Children.OfType<LispParser.MethodNode>().Single());
            }
            return decorator;
        }

        protected abstract Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler);
    }

    // AlwaysFail will always fail no matter the wrapped task fails or succeeds.
    public class AlwaysFail<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public AlwaysFail(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public override TaskState Execute()
        {
            DecoratedTask.Tick();
            return TaskState.Failure;
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new AlwaysFail<TBb>(null);
        }
    }

    // AlwaysSucceed will always succeed no matter the wrapped task succeeds or fails.
    public class AlwaysSucceed<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public AlwaysSucceed(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public override TaskState Execute()
        {
            DecoratedTask.Tick();
            return TaskState.Success;
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new AlwaysSucceed<TBb>(null);
        }
    }

    // Invert will succeed if the wrapped task fails and will fail if the wrapped task succeeds.
    public class Invert<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public Invert(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public override TaskState Execute()
        {
            switch (DecoratedTask.Tick())
            {
                case TaskState.Success:
                    return TaskState.Failure;
                case TaskState.Running:
                    return TaskState.Running;
                case TaskState.Failure:
                    return TaskState.Success;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new Invert<TBb>(null);
        }
    }

    // UntilFail will repeat the wrapped task until that task fails.
    public class UntilFail<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public UntilFail(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public override TaskState Execute()
        {
            switch (DecoratedTask.Tick())
            {
                case TaskState.Failure:
                    return TaskState.Success;
                case TaskState.Success:
                case TaskState.Running:
                    return TaskState.Running;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new UntilFail<TBb>(null);
        }
    }

    // UntilSuccess will repeat the wrapped task until that task succeeds.
    public class UntilSuccess<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public UntilSuccess(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public override TaskState Execute()
        {
            switch (DecoratedTask.Tick())
            {
                case TaskState.Success:
                    return TaskState.Success;
                case TaskState.Failure:
                case TaskState.Running:
                    return TaskState.Running;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new UntilSuccess<TBb>(null);
        }
    }

    // Run Once will run one time and then never run again
    public class RunOnce<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public RunOnce(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public bool HasBeenRun { get; set; }
        public TaskState? Result { get; set; }

        public override TaskState Execute()
        {
            if (HasBeenRun)
            {
                return Result.Value;
            }

            Result = DecoratedTask.Tick();
            switch (Result.Value)
            {
                case TaskState.Success:
                case TaskState.Failure:
                    HasBeenRun = true;
                    return Result.Value;
                case TaskState.Running:
                    return TaskState.Running;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new RunOnce<TBb>(null);
        }
    }

    public class RunNTimes<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public RunNTimes(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public int RunTimes { get; private set; }
        public int MaxRunTimes { get; set; }

        public override TaskState Execute()
        {
            if (RunTimes >= MaxRunTimes)
            {
                RunTimes++;
                return TaskState.Failure;
            }

            TaskState result = DecoratedTask.Tick();
            switch (result)
            {
                case TaskState.Success:
                case TaskState.Failure:
                    RunTimes++;
                    return result;
                case TaskState.Running:
                    return TaskState.Running;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override LispParser.MethodNode BuildParseTree()
        {
            LispParser.MethodNode method = base.BuildParseTree();
            method.Children.Insert(0, new LispParser.NamedParameterNode("MaxRunTimes", MaxRunTimes));
            return method;
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            RunNTimes<TBb> runNTimes = new RunNTimes<TBb>(null);
            foreach (LispParser.Node child in method.Children)
            {
                child.NamedParamX<LispParser.IntegerNode>("MaxRunTimes", x => runNTimes.MaxRunTimes = x.Value);
            }
            return runNTimes;
        }
    }

    // SemaphoreGuard allows you to specify how many characters should be allowed to concurrently use the wrapped task which 
    //  represents a limited resource used in different behavior trees (note that this does not necessarily involve 
    //  multithreading concurrency). This is a simple mechanism for ensuring that a limited shared resource is not 
    //  over subscribed. You might have a pool of 5 pathfinders, for example, meaning at most 5 characters can be 
    //  pathfinding at a time. Or you can associate a semaphore to the player character to ensure that at most 3 
    //  enemies can simultaneously attack him. This decorator fails when it cannot acquire the semaphore. This 
    //  allows a select task higher up the tree to find a different action that doesn't involve the contested resource.
    public class SemaphoreGuard<TBb> : Decorator<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public SemaphoreGuard(TreeTask<TBb> decoratedTask)
            : base(decoratedTask)
        {
        }

        public bool HasBeenRun { get; set; }
        public TaskState? Result { get; set; }

        public override TaskState Execute()
        {
            if (HasBeenRun)
            {
                return Result.Value;
            }

            Result = DecoratedTask.Tick();
            switch (Result.Value)
            {
                case TaskState.Success:
                case TaskState.Failure:
                    HasBeenRun = true;
                    return Result.Value;
                case TaskState.Running:
                    return TaskState.Running;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Decorator<TBb> CreateDecoratorFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new SemaphoreGuard<TBb>(null);
        }
    }

    // Include grafts an external subtree. This decorator enhances behavior trees with modularity and reusability.
    // Limit controls the maximum number of times a task can be run, which could be used to make sure that a character doesn't keep 
    //   trying to barge through a door that the player has reinforced.
}