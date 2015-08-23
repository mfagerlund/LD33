using System;
using System.Collections.Generic;
using System.Linq;

namespace Beehive.Lisp
{
    public interface ICanEvaluateToString
    {
        string EvaluateToString();
    }

    public abstract class StringOperand<TBb> : LispOperator<TBb>, ICanEvaluateToString where TBb : IBlackboard
    {
        public abstract string EvaluateToString();
    }

    public class StringConstant<TBb> : StringOperand<TBb> where TBb : IBlackboard
    {
        public StringConstant(string value = null)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override string EvaluateToString()
        {
            return Value;
        }

        public override LispParser.Node BuildParseTree()
        {
            return new LispParser.StringNode(Value);
        }

        public override LispOperator<TBb> CreateOperator(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.StringNode stringNode = node as LispParser.StringNode;
            return stringNode != null ? new StringConstant<TBb>(stringNode.Value) : null;
        }
    }

    public abstract class StringMethod<TBb> : LispMethod<TBb>, ICanEvaluateToString where TBb : IBlackboard
    {
        protected StringMethod(
            string name, params LispOperator<TBb>[] children)
            : base(name, children)
        {
        }

        public abstract string EvaluateToString();

        protected override void ValidateChildren(LispParser.MethodNode node)
        {
            base.ValidateChildren(node);
            foreach (LispOperator<TBb> child in Children)
            {
                ICanEvaluateToString floater = child as ICanEvaluateToString;
                if (floater == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Arguments for {0} must be evaluatable to string. {1} isn't!",
                            Name,
                            child.GetType().Name));
                }
            }
        }
    }

    public class Format<TBb> : StringMethod<TBb> where TBb : IBlackboard
    {
        public Format(
            params LispOperator<TBb>[] children)
            : base("Format", children)
        {
        }

        public override string EvaluateToString()
        {
            List<object> arguments = new List<object>();

            foreach (LispOperator<TBb> @op in Children.Skip(1))
            {
                ICanEvaluateToBool canEvaluateToBool = @op as ICanEvaluateToBool;
                if (canEvaluateToBool != null)
                {
                    arguments.Add(canEvaluateToBool.EvaluateToBool());
                    continue;
                }

                ICanEvaluateToFloat canEvaluateToFloat = @op as ICanEvaluateToFloat;
                if (canEvaluateToFloat != null)
                {
                    arguments.Add(canEvaluateToFloat.EvaluateToFloat());
                    continue;
                }

                ICanEvaluateToString canEvaluateToString = @op as ICanEvaluateToString;
                if (canEvaluateToString != null)
                {
                    arguments.Add(canEvaluateToString.EvaluateToString());
                    continue;
                }

                throw new InvalidOperationException(string.Format("Argument can't be evaluated to string: {0}", @op));
            }

            string formatString = Children.First().TryEvaluateToString();
            try
            {
                return string.Format(formatString, arguments.ToArray());
            }
            catch
            {
                return "BAD FORMAT: " + formatString;
            }
        }

        protected override LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler)
        {
            return new Format<TBb>();
        }
    }
}