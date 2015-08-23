using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beehive.Lisp
{
    public interface IBlackboard
    {
        Func<float> GetFloatFunction(string name);
        Func<bool> GetBoolFunction(string name);
    }

    public class ReflectionBlackboard<TOwner> : IBlackboard where TOwner : class
    {
        private static readonly Dictionary<string, Func<TOwner, bool>> BoolFunctionDelegates = CreateFunctionDelegates<bool>();
        private static readonly Dictionary<string, Func<TOwner, float>> FloatFunctionDelegates = CreateFunctionDelegates<float>();

        public ReflectionBlackboard(TOwner owner)
        {
            Owner = owner;
        }

        public TOwner Owner { get; set; }

        public virtual Func<float> GetFloatFunction(string name)
        {
            Func<TOwner, float> res;
            if (FloatFunctionDelegates.TryGetValue(name, out res) || FloatFunctionDelegates.TryGetValue("get_" + name, out res))
            {
                return () => res(Owner);
            }

            return null;
        }

        public virtual Func<bool> GetBoolFunction(string name)
        {
            Func<TOwner, bool> res;
            if (BoolFunctionDelegates.TryGetValue(name, out res) || BoolFunctionDelegates.TryGetValue("get_" + name, out res))
            {
                return () => res(Owner);
            }

            return null;
        }

        public static Dictionary<string, Func<TOwner, TResult>> CreateFunctionDelegates<TResult>()
        {
            Dictionary<string, Func<TOwner, TResult>> functions = new Dictionary<string, Func<TOwner, TResult>>();

            List<MethodInfo> methods =
                typeof(TOwner)
                    .GetMethods()
                    .Where(m => m.ReturnType == typeof(TResult) && m.GetParameters().Length == 0)
                    .ToList();

            foreach (MethodInfo method in methods)
            {
                Func<TOwner, TResult> dlg = (Func<TOwner, TResult>)Delegate.CreateDelegate(typeof(Func<TOwner, TResult>), method);
                functions.Add(method.Name, dlg);
            }

            return functions;
        }
    }
}