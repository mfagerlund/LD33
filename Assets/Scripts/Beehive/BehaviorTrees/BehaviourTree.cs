using System;
using System.Collections.Generic;
using System.Linq;
using Beehive.Lisp;
using Beehive.Utilities;

namespace Beehive.BehaviorTrees
{
    // See https://en.wikipedia.org/wiki/Behavior_Trees_%28artificial_intelligence,_robotics_and_control%29
    // See https://github.com/libgdx/gdx-ai/wiki/Behavior-Trees

    // TODO: Long lived tasks - Sequences which remember where they left off if previously running.

    public enum TaskState
    {
        Success,
        Running,
        Failure
    }
    
    /* From Wikipedia:
     * A Behavior Tree (BT) is a mathematical model of plan execution used in computer science, robotics and
     * control systems. They describe switchings between a finite set of tasks in a modular fashion. 
     * Their strength comes from their ability to create very complex tasks composed of simple tasks, 
     * without worrying how the simple tasks are implemented. BTs present some similarities to
     * hierarchical state machines with the key difference that the main building block of a behavior is a 
     * task rather than a state. Its ease of human understanding make BTs less error prone and very popular 
     * in the game developer community.*/
    public class BehaviourTree<TBb> : LispParser.ICanBuildLispParseTree, LispParser.ICanCreateFromLispParseTree<TBb> where TBb : IBehaviourTreeBlackboard
    {
        private bool _prepared;
        private int _taskHysterisisRuns;
        public BehaviourTree(string name, TreeTask<TBb> rootTask)
        {
            Name = name;
            RootTask = rootTask;
            RunningTaskHysterisis = 10;
        }
        public string Name { get; set; }
        public TreeTask<TBb> RootTask { get; set; }
        public TaskState CurrentState { get { return RootTask.State; } }
        public Context<TBb> Context { get; set; }

        // When we have a task that returns "running", we can quickly tick *only* that task
        // and ignore traversing the entire tree. However, higher priority behaviours
        // may need to get their chance at taking over the execution. Setting TicksPerFullUpdate 
        // to 0 means that we always check from the beginning. Setting it to some really high
        // number makes it less responsive to changes in the environment.
        public int RunningTaskHysterisis { get; set; }
        public int TickCount { get; set; }

        public TaskState Tick()
        {
            if (!_prepared)
            {
                Prepare();
            }

            if (TryToRerunPreviouslyRunningTask())
            {
                return TaskState.Running;
            }

            Context.Switch();
            TaskState taskState = RootTask.Tick();
            CallOnExitOnTasksThatHaveBeenSuperceded();
            return taskState;
        }       

        public void CallOnExitOnTasksThatHaveBeenSuperceded()
        {
            foreach (TreeTask<TBb> task in Context.PreviouslyRunning.ToList())
            {
                if (!Context.CurrentlyRunning.Contains(task))
                {
                    task.WasSupercededCleanup();
                }
            }
        }

        public LispParser.Node BuildParseTree()
        {
            LispParser.MethodNode node = new LispParser.MethodNode("BehaviourTree");
            node.Add("Name", Name, true);
            node.Add("RunningTaskHysterisis", RunningTaskHysterisis);
            node.Add(RootTask.BuildParseTree());
            return node;
        }

        public LispParser.ICanCreateFromLispParseTree<TBb> CreateFromParseTree(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.MethodNode method = node as LispParser.MethodNode;
            if (method == null)
            {
                throw new InvalidOperationException("Expected BehaviourTree but found: null");
            }

            if (method.MethodName != "BehaviourTree")
            {
                throw new InvalidOperationException("Expected BehaviourTree but found: " + method);
            }

            BehaviourTree<TBb> behaviourTree = new BehaviourTree<TBb>(null, null);
            foreach (LispParser.Node child in method.Children)
            {
                child.NamedParamX<LispParser.NumberNode>("RunningTaskHysterisis", v => behaviourTree.RunningTaskHysterisis = (int)v.FloatValue);
                child.NamedParamX<LispParser.StringNode>("Name", v => behaviourTree.Name = v.Value);
                child.ParamX<LispParser.MethodNode>(null, v => behaviourTree.RootTask = (TreeTask<TBb>)compiler.Compile(blackboard, v));
            }

            return behaviourTree;
        }

        public override string ToString()
        {
            return BuildParseTree().ToCode();
        }

        private bool TryToRerunPreviouslyRunningTask()
        {
            if (CurrentState == TaskState.Running
                && RunningTaskHysterisis < _taskHysterisisRuns
                && Context.RunningTask != null)
            {
                TreeTask<TBb> task = Context.RunningTask;
                _taskHysterisisRuns++;
                task.Tick();

                if (task.State == TaskState.Running)
                {
                    return true;
                }
            }
            return false;
        }
        
        private void Prepare()
        {
            Context = new Context<TBb>();
            HashSet<TreeTask<TBb>> tasks = new HashSet<TreeTask<TBb>>();
            RootTask
                .ForEachTask(
                    task =>
                    {
                        if (tasks.Contains(task))
                        {
                            throw new InvalidOperationException("The tree allready contains the task {0}");
                        }
                        tasks.Add(task);
                        task.Context = Context;
                    });
            _prepared = true;
        }
    }

    public class Context<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public Context()
        {
            CurrentlyRunning = new HashSet<TreeTask<TBb>>();
            PreviouslyRunning = new HashSet<TreeTask<TBb>>();
        }

        public HashSet<TreeTask<TBb>> CurrentlyRunning { get; set; }
        public HashSet<TreeTask<TBb>> PreviouslyRunning { get; set; }
        public TreeTask<TBb> RunningTask { get; set; }
        public TBb Blackboard { get; set; }

        public void Switch()
        {
            RunningTask = null;
            HashSet<TreeTask<TBb>> temp = CurrentlyRunning;
            CurrentlyRunning = PreviouslyRunning;
            PreviouslyRunning = temp;
            CurrentlyRunning.Clear();
        }

        public void RegisterRunning(TreeTask<TBb> task)
        {
            CurrentlyRunning.Add(task);
            // We remove it here to spare some cycles later on.
            if (task.PreviousTickState == TaskState.Running)
            {
                PreviouslyRunning.Remove(task);
            }
            // The first one to report running is the deepest running one
            if (RunningTask == null)
            {
                RunningTask = task;
            }
        }

        public void RegisterNoLongerRunning(TreeTask<TBb> task)
        {
            PreviouslyRunning.Remove(task);
        }
    }

    public static class TaskExtensions
    {
        public static TTask SetName<TTask, TBb>(this TTask task, string name)
            where TTask : TreeTask<TBb>
            where TBb : IBehaviourTreeBlackboard
        {
            task.Name = name;
            return task;
        }

        public static TTask Act<TTask, TBb>(this TTask task, Action<TTask> action)
            where TTask : TreeTask<TBb>
            where TBb : IBehaviourTreeBlackboard
        {
            action(task);
            return task;
        }
    }

    public abstract class TreeTask<TBb> : LispParser.ICanBuildLispParseTree, LispParser.ICanCreateFromLispParseTree<TBb> where TBb : IBehaviourTreeBlackboard
    {
        protected TreeTask(string name)
        {
            Name = name;
            State = TaskState.Success;
        }

        public string Name { get; set; }

        public virtual string DebugText
        {
            get
            {
                string text = TypeHelper.GetFriendlyTypeName(GetType());
                if (Name != null)
                {
                    text += " " + Name;
                }
                return text;
            }
        }

        public Context<TBb> Context { get; set; }
        public TaskState State { get; private set; }
        public TaskState PreviousTickState { get; private set; }

        public virtual void ForEachTask(Action<TreeTask<TBb>> action)
        {
            action(this);
        }      

        public TaskState Tick()
        {
            PreviousTickState = State;

            if (PreviousTickState != TaskState.Running)
            {
                OnEnter();
            }

            State = Execute();

            switch (State)
            {
                case TaskState.Running:
                    Context.RegisterRunning(this);
                    break;
                case TaskState.Success:
                case TaskState.Failure:
                    OnExit();
                    if (PreviousTickState == TaskState.Running)
                    {
                        Context.RegisterNoLongerRunning(this);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return State;
        }

        public abstract TaskState Execute();
     
        public virtual void WasSupercededCleanup()
        {
            // It can't be set to running any more; another task as taken over the running state. 
            // Since it wasn't a success, it will have to be a failure...
            State = TaskState.Failure;
            OnExit();
        }

        LispParser.Node LispParser.ICanBuildLispParseTree.BuildParseTree()
        {
            return BuildParseTree();
        }

        LispParser.ICanCreateFromLispParseTree<TBb> LispParser.ICanCreateFromLispParseTree<TBb>.CreateFromParseTree(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.MethodNode method = node as LispParser.MethodNode;
            if (method == null)
            {
                return null;
            }

            if (method.MethodName == GetClassName())
            {
                TreeTask<TBb> treeTask = CreateTreeTaskFromParseTree(blackboard, method, compiler);
                foreach (LispParser.Node child in method.Children)
                {
                    child.NamedParamX<LispParser.StringNode>("Name", v => treeTask.Name = v.Value);
                }
                return treeTask;
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return BuildParseTree().ToCode();
        }

        public virtual LispParser.MethodNode BuildParseTree()
        {
            string className = GetClassName();
            LispParser.MethodNode node = new LispParser.MethodNode(className);
            node.Add("Name", Name, true);
            return node;
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExit()
        {
        }
        
        protected virtual string GetClassName()
        {
            string typeName = GetType().Name;
            int iBacktick = typeName.IndexOf('`');
            typeName = typeName.Remove(iBacktick);
            return typeName;
        }

        protected abstract TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler);
    }

    public abstract class LeafTreeTask<TBb> : TreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        protected LeafTreeTask(string name)
            : base(name)
        {
        }
    }

    public class Fail<TBb> : LeafTreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public Fail(string name = null)
            : base(name)
        {
        }

        public override TaskState Execute()
        {
            return TaskState.Failure;
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new Fail<TBb>();
        }
    }

    public class Succeed<TBb> : LeafTreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public Succeed(string name = null)
            : base(name)
        {
        }

        public override TaskState Execute()
        {
            return TaskState.Success;
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new Succeed<TBb>();
        }
    }

    public class KeepRunning<TBb> : LeafTreeTask<TBb> where TBb : IBehaviourTreeBlackboard
    {
        public KeepRunning(string name = null)
            : base(name)
        {
        }

        public override TaskState Execute()
        {
            return TaskState.Running;
        }

        protected override TreeTask<TBb> CreateTreeTaskFromParseTree(TBb blackboard, LispParser.MethodNode method, LispParser.ICompiler<TBb> compiler)
        {
            return new KeepRunning<TBb>();
        }
    }
}
