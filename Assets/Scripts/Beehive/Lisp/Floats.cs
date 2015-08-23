using System;
using System.Linq;

namespace Beehive.Lisp
{
    public interface ICanEvaluateToFloat : ICanEvaluateToString
    {
        float EvaluateToFloat();
    }

    public interface ICanEvaluateToInt : ICanEvaluateToFloat
    {
        int EvaluateToInt();
    }

    public class FloatFunction : Function<float>, ICanEvaluateToFloat
    {
        public FloatFunction(string name, Func<float> func)
            : base(name, func)
        {
        }

        public float EvaluateToFloat()
        {
            return Func();
        }

        public string EvaluateToString()
        {
            return EvaluateToFloat().ToString();
        }
    }

    public abstract class FloatOperand<TBb> : LispOperator<TBb>, ICanEvaluateToFloat where TBb : IBlackboard
    {
        public abstract float EvaluateToFloat();

        public string EvaluateToString()
        {
            return EvaluateToFloat().ToString();
        }
    }

    public class IntConstant<TBb> : FloatOperand<TBb>, ICanEvaluateToInt where TBb : IBlackboard
    {
        public IntConstant(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        public override float EvaluateToFloat()
        {
            return Value;
        }

        public int EvaluateToInt()
        {
            return Value;
        }

        public override LispParser.Node BuildParseTree()
        {
            return new LispParser.IntegerNode(Value);
        }

        public override LispOperator<TBb> CreateOperator(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.IntegerNode integerNode = node as LispParser.IntegerNode;
            return integerNode != null ? new IntConstant<TBb>(integerNode.Value) : null;
        }
    }

    public class BlackboardFloatFunction<TBb> : FloatOperand<TBb> where TBb : IBlackboard
    {
        private readonly Func<float> _func;

        public BlackboardFloatFunction(string name, Func<float> func)
        {
            Name = name;
            _func = func;
        }

        public string Name { get; set; }

        public override float EvaluateToFloat()
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

            Func<float> func = blackboard.GetFloatFunction(identifierNode.Value);
            if (func == null)
            {
                return null;
            }

            return new BlackboardFloatFunction<TBb>(identifierNode.Value, func);
        }
    }

    public class FloatConstant<TBb> : FloatOperand<TBb> where TBb : IBlackboard
    {
        public FloatConstant(float value)
        {
            Value = value;
        }

        public float Value { get; set; }

        public override float EvaluateToFloat()
        {
            return Value;
        }

        public override LispParser.Node BuildParseTree()
        {
            return new LispParser.FloatNode(Value);
        }

        public override LispOperator<TBb> CreateOperator(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.FloatNode floatNode = node as LispParser.FloatNode;
            return floatNode != null ? new FloatConstant<TBb>(floatNode.Value) : null;
        }
    }

    public abstract class FloatMethod<TBb> : LispMethod<TBb>, ICanEvaluateToFloat where TBb : IBlackboard
    {
        protected FloatMethod(
            string name, params LispOperator<TBb>[] children)
            : base(name, children)
        {
        }

        public abstract float EvaluateToFloat();

        public string EvaluateToString()
        {
            return EvaluateToFloat().ToString();
        }

        protected override void ValidateChildren(LispParser.MethodNode node)
        {
            base.ValidateChildren(node);
            foreach (LispOperator<TBb> child in Children)
            {
                ICanEvaluateToFloat floater = child as ICanEvaluateToFloat;
                if (floater == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Arguments for {0} must be evaluatable to float. {1} isn't!",
                            Name,
                            child.GetType().Name));
                }
            }
        }
    }

    public abstract class FloatMethod2OrMoreChildren<TBb> : FloatMethod<TBb> where TBb : IBlackboard
    {
        protected FloatMethod2OrMoreChildren(
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

    public class Add<TBb> : FloatMethod2OrMoreChildren<TBb> where TBb : IBlackboard
    {
        public Add(params LispOperator<TBb>[] children)
            : base("+", children)
        {
        }

        public override float EvaluateToFloat()
        {
            return
                Children
                    .Cast<ICanEvaluateToFloat>()
                    .Sum(floater => floater.EvaluateToFloat());
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new Add<TBb>();
        }
    }

    public class Sub<TBb> : FloatMethod2OrMoreChildren<TBb> where TBb : IBlackboard
    {
        public Sub(params LispOperator<TBb>[] children)
            : base("-", children)
        {
        }

        public override float EvaluateToFloat()
        {
            float sum = 0;
            for (int index = 0; index < Children.Count; index++)
            {
                ICanEvaluateToFloat floater = (ICanEvaluateToFloat)Children[index];
                if (index == 0)
                {
                    sum = floater.EvaluateToFloat();
                }
                else
                {
                    sum -= floater.EvaluateToFloat();
                }
            }
            return sum;
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new Sub<TBb>();
        }
    }

    public class Mul<TBb> : FloatMethod2OrMoreChildren<TBb> where TBb : IBlackboard
    {
        public Mul(params LispOperator<TBb>[] children)
            : base("*", children)
        {
        }

        public override float EvaluateToFloat()
        {
            float mul = 1;
            for (int index = 0; index < Children.Count; index++)
            {
                ICanEvaluateToFloat floater = (ICanEvaluateToFloat)Children[index];
                mul *= floater.EvaluateToFloat();
            }
            return mul;
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new Mul<TBb>();
        }
    }

    public class Div<TBb> : FloatMethod2OrMoreChildren<TBb> where TBb : IBlackboard
    {
        public Div(params LispOperator<TBb>[] children)
            : base("/", children)
        {
        }

        public override float EvaluateToFloat()
        {
            float div = 0;
            for (int index = 0; index < Children.Count; index++)
            {
                ICanEvaluateToFloat floater = (ICanEvaluateToFloat)Children[index];
                if (index == 0)
                {
                    div = floater.EvaluateToFloat();
                }
                else
                {
                    div /= floater.EvaluateToFloat();
                }
            }
            return div;
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new Div<TBb>();
        }
    }
}