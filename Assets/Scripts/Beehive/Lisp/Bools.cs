using System;
using System.Linq;

namespace Beehive.Lisp
{
    public interface ICanEvaluateToBool
    {
        bool EvaluateToBool();
    }

    public class BoolFunction : Function<bool>, ICanEvaluateToBool
    {
        public BoolFunction(string name, Func<bool> func)
            : base(name, func)
        {
        }

        public bool EvaluateToBool()
        {
            return Func();
        }
    }

    public abstract class BoolOperand<TBb> : LispOperator<TBb>, ICanEvaluateToBool where TBb : IBlackboard
    {
        public abstract bool EvaluateToBool();
    }

    public class TrueConstant<TBb> : BoolOperand<TBb> where TBb : IBlackboard
    {
        public override bool EvaluateToBool()
        {
            return true;
        }

        public override LispParser.Node BuildParseTree()
        {
            return new LispParser.IdentifierNode("True");
        }

        public override LispOperator<TBb> CreateOperator(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.IdentifierNode identifierNode = node as LispParser.IdentifierNode;

            return identifierNode != null && identifierNode.Value == "True" ? new TrueConstant<TBb>() : null;
        }
    }

    public class FalseConstant<TBb> : BoolOperand<TBb> where TBb : IBlackboard
    {
        public override bool EvaluateToBool()
        {
            return false;
        }

        public override LispParser.Node BuildParseTree()
        {
            return new LispParser.IdentifierNode("False");
        }

        public override LispOperator<TBb> CreateOperator(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.IdentifierNode identifierNode = node as LispParser.IdentifierNode;

            return identifierNode != null && identifierNode.Value == "False" ? new FalseConstant<TBb>() : null;
        }
    }

    public abstract class BoolMethod<TBb> : LispMethod<TBb>, ICanEvaluateToBool where TBb : IBlackboard
    {
        protected BoolMethod(
            string name, params LispOperator<TBb>[] children)
            : base(name, children)
        {
        }

        public abstract bool EvaluateToBool();

        protected override void ValidateChildren(LispParser.MethodNode node)
        {
            base.ValidateChildren(node);
            foreach (LispOperator<TBb> child in Children)
            {
                ICanEvaluateToBool floater = child as ICanEvaluateToBool;
                if (floater == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Arguments for {0} must be evaluatable to bool. {1} isn't!",
                            Name,
                            child.GetType().Name));
                }
            }
        }
    }

    public abstract class BoolMethodUnary<TBb> : BoolMethod<TBb> where TBb : IBlackboard
    {
        protected BoolMethodUnary(
            string name, params LispOperator<TBb>[] children)
            : base(name, children)
        {
        }

        protected override void ValidateChildren(LispParser.MethodNode node)
        {
            base.ValidateChildren(node);
            if (Children.Count != 1)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} must have 1 argument!",
                        Name));
            }
        }
    }

    public abstract class BoolMethod2OrMoreChildren<TBb> : BoolMethod<TBb> where TBb : IBlackboard
    {
        protected BoolMethod2OrMoreChildren(
            string name, params LispOperator<TBb>[] children)
            : base(name, children)
        {
        }

        protected override void ValidateChildren(LispParser.MethodNode node)
        {
            base.ValidateChildren(node);
            if (Children.Count < 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} must have 2 or more arguments!",
                        Name));
            }
        }
    }

    public class And<TBb> : BoolMethod2OrMoreChildren<TBb> where TBb : IBlackboard
    {
        public And(params LispOperator<TBb>[] children)
            : base("And", children)
        {
        }

        public override bool EvaluateToBool()
        {
            return
                Children
                    .Cast<ICanEvaluateToBool>()
                    .All(floater => floater.EvaluateToBool());
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new And<TBb>();
        }
    }

    public class Or<TBb> : BoolMethod2OrMoreChildren<TBb> where TBb : IBlackboard
    {
        public Or(params LispOperator<TBb>[] children)
            : base("Or", children)
        {
        }

        public override bool EvaluateToBool()
        {
            return
                Children
                    .Cast<ICanEvaluateToBool>()
                    .Any(floater => floater.EvaluateToBool());
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new Or<TBb>();
        }
    }

    public class Xor<TBb> : BoolMethod2OrMoreChildren<TBb> where TBb : IBlackboard
    {
        public Xor(params LispOperator<TBb>[] children)
            : base("Xor", children)
        {
        }

        public override bool EvaluateToBool()
        {
            int hits =
                Children
                    .Cast<ICanEvaluateToBool>()
                    .Count(floater => floater.EvaluateToBool());

            return hits == 1;
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new Xor<TBb>();
        }
    }

    public class Not<TBb> : BoolMethodUnary<TBb> where TBb : IBlackboard
    {
        public Not(params LispOperator<TBb>[] children)
            : base("Not", children)
        {
        }

        public override bool EvaluateToBool()
        {
            return !((ICanEvaluateToBool)Children[0]).EvaluateToBool();
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new Not<TBb>();
        }
    }

    public class BlackboardBoolFunction<TBb> : BoolOperand<TBb> where TBb : IBlackboard
    {
        private readonly Func<bool> _func;

        public BlackboardBoolFunction(string name, Func<bool> func)
        {
            Name = name;
            _func = func;
        }

        public string Name { get; set; }

        public override bool EvaluateToBool()
        {
            return _func();
        }

        public override LispParser.Node BuildParseTree()
        {
            return new LispParser.IdentifierNode(Name);
        }

        public override LispOperator<TBb> CreateOperator(
            TBb blackboard,
            LispParser.Node node,
            LispParser.ICompiler<TBb> compiler)
        {
            LispParser.IdentifierNode identifierNode = node as LispParser.IdentifierNode;
            if (identifierNode == null)
            {
                return null;
            }

            Func<bool> func = blackboard.GetBoolFunction(identifierNode.Value);
            if (func == null)
            {
                return null;
            }

            return new BlackboardBoolFunction<TBb>(identifierNode.Value, func);
        }
    }
}
