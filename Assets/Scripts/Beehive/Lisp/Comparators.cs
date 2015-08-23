using System;
using Beehive.Utilities;

namespace Beehive.Lisp
{
    public abstract class Comparator<TBb> : LispMethod<TBb>, ICanEvaluateToBool where TBb : IBlackboard
    {
        protected Comparator(
            string name, params LispOperator<TBb>[] children)
            : base(name, children)
        {
        }

        public bool EvaluateToBool()
        {
            return Compare(
                ((ICanEvaluateToFloat)Children[0]).EvaluateToFloat(),
                ((ICanEvaluateToFloat)Children[1]).EvaluateToFloat());
        }

        protected abstract bool Compare(float childValue1, float childValue2);

        protected override void ValidateChildren(LispParser.MethodNode node)
        {
            base.ValidateChildren(node);
            if (Children.Count != 2)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} must have 2 arguments!", Name));
            }
            foreach (LispOperator<TBb> child in Children)
            {
                ICanEvaluateToFloat floater = child as ICanEvaluateToFloat;
                if (floater == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Arguments for {0} must be evaluatable to float. {1} isn't!",
                            Name,
                            TypeHelper.GetFriendlyTypeName(child.GetType())));
                }
            }
        }
    }

    public class EqualTo<TBb> : Comparator<TBb> where TBb : IBlackboard
    {
        public EqualTo(params LispOperator<TBb>[] children)
            : base("=", children)
        {
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new EqualTo<TBb>();
        }

        protected override bool Compare(float childValue1, float childValue2)
        {
            return Math.Abs(childValue1 - childValue2) < float.Epsilon;
        }
    }

    public class LessThan<TBb> : Comparator<TBb> where TBb : IBlackboard
    {
        public LessThan(params LispOperator<TBb>[] children)
            : base("<", children)
        {
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new LessThan<TBb>();
        }

        protected override bool Compare(float childValue1, float childValue2)
        {
            return childValue1 < childValue2;
        }
    }

    public class LessThanOrEqualTo<TBb> : Comparator<TBb> where TBb : IBlackboard
    {
        public LessThanOrEqualTo(params LispOperator<TBb>[] children)
            : base("<=", children)
        {
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new LessThanOrEqualTo<TBb>();
        }

        protected override bool Compare(float childValue1, float childValue2)
        {
            return childValue1 <= childValue2;
        }
    }

    public class MoreThan<TBb> : Comparator<TBb> where TBb : IBlackboard
    {
        public MoreThan(params LispOperator<TBb>[] children)
            : base(">", children)
        {
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new MoreThan<TBb>();
        }

        protected override bool Compare(float childValue1, float childValue2)
        {
            return childValue1 > childValue2;
        }
    }

    public class MoreThanOrEqualTo<TBb> : Comparator<TBb> where TBb : IBlackboard
    {
        public MoreThanOrEqualTo(params LispOperator<TBb>[] children)
            : base(">=", children)
        {
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new MoreThanOrEqualTo<TBb>();
        }

        protected override bool Compare(float childValue1, float childValue2)
        {
            return childValue1 >= childValue2;
        }
    }
}