using System;
using System.Collections.Generic;
using Beehive.Lisp;

namespace Beehive.BehaviorTrees
{
    public interface IBehaviourTreeBlackboard : IBlackboard
    {
        Func<IEnumerator<TaskState>> GetCoRoutineFunc(string name);
    }

    public class BehaviourReflectionTreeBlackboard<TOwner> : ReflectionBlackboard<TOwner>, IBehaviourTreeBlackboard where TOwner : class
    {
        private static readonly Dictionary<string, Func<TOwner, IEnumerator<TaskState>>> CoroutineFunctionDelegates = CreateFunctionDelegates<IEnumerator<TaskState>>();

        public BehaviourReflectionTreeBlackboard(TOwner owner) :
            base(owner)
        {
        }

        public Func<IEnumerator<TaskState>> GetCoRoutineFunc(string name)
        {
            Func<TOwner, IEnumerator<TaskState>> res;
            if (CoroutineFunctionDelegates.TryGetValue(name, out res) || CoroutineFunctionDelegates.TryGetValue("get_" + name, out res))
            {
                return () => res(Owner);
            }

            return null;
        }
    }
}