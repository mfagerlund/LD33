using System;
using System.Collections.Generic;
using Beehive.Utilities;

namespace Beehive.Lisp
{
    // TBb=TBlackboard, but it's too long!
    public class LispCompiler<TBb> : LispParser.ICompiler<TBb> where TBb : IBlackboard
    {
        public LispCompiler()
        {
            Operators = new List<LispOperator<TBb>>();
            // Float
            Operators.Add(new Add<TBb>());
            Operators.Add(new Mul<TBb>());
            Operators.Add(new Sub<TBb>());
            Operators.Add(new Div<TBb>());
            Operators.Add(new FloatConstant<TBb>(0));
            Operators.Add(new IntConstant<TBb>(0));
            
            // Bool
            Operators.Add(new And<TBb>());
            Operators.Add(new Or<TBb>());
            Operators.Add(new Not<TBb>());
            Operators.Add(new Xor<TBb>());
            Operators.Add(new TrueConstant<TBb>());
            Operators.Add(new FalseConstant<TBb>());

            // Comparators
            Operators.Add(new EqualTo<TBb>());
            Operators.Add(new MoreThan<TBb>());
            Operators.Add(new MoreThanOrEqualTo<TBb>());
            Operators.Add(new LessThan<TBb>());
            Operators.Add(new LessThanOrEqualTo<TBb>());

            // Strings
            Operators.Add(new StringConstant<TBb>());
            Operators.Add(new Format<TBb>());

            // Blackboard
            Operators.Add(new BlackboardFloatFunction<TBb>(null, null));
            Operators.Add(new BlackboardBoolFunction<TBb>(null, null));
        }

        public List<LispOperator<TBb>> Operators { get; set; }

        public LispOperator<TBb> Compile(TBb blackboard, string code)
        {
            LispParser lispParser = new LispParser();
            LispParser.Node parseTree = lispParser.Parse(code);
            return Compile(blackboard, parseTree);
        }

        public LispOperator<TBb> Compile(TBb blackboard, LispParser.Node parseTree)
        {
            foreach (LispOperator<TBb> lispOperator in Operators)
            {
                LispOperator<TBb> result = lispOperator.CreateOperator(blackboard, parseTree, this);
                if (result != null)
                {
                    return result;
                }
            }

            throw new InvalidOperationException(
                string.Format("Unrecognized token: {0}.", parseTree));
        }

        LispParser.ICanCreateFromLispParseTree<TBb> LispParser.ICompiler<TBb>.Compile(TBb blackboard, LispParser.Node parseTree)
        {
            return Compile(blackboard, parseTree);
        }
    }

    public class Function<T> : LispParser.ICanBuildLispParseTree
    {
        public Function(string name, Func<T> func)
        {
            Name = name;
            Func = func;
        }

        public string Name { get; set; }
        public Func<T> Func { get; set; }

        public T Evaluate()
        {
            return Func();
        }

        public LispParser.Node BuildParseTree()
        {
            return new LispParser.IdentifierNode(Name);
        }
    }

    public abstract class LispOperator<TBb> : LispParser.ICanBuildLispParseTree, LispParser.ICanCreateFromLispParseTree<TBb> where TBb : IBlackboard
    {
        public abstract LispParser.Node BuildParseTree();

        public abstract LispOperator<TBb> CreateOperator(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler);

        LispParser.ICanCreateFromLispParseTree<TBb> LispParser.ICanCreateFromLispParseTree<TBb>.CreateFromParseTree(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            return CreateOperator(blackboard, node, compiler);
        }

        public float TryEvaluateToFloat()
        {
            ICanEvaluateToFloat floater = this as ICanEvaluateToFloat;
            if (floater == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                    "{0} can't be evaluated to float!",
                    GetType().Name));
            }
            return floater.EvaluateToFloat();
        }

        public bool TryEvaluateToBool()
        {
            ICanEvaluateToBool floater = this as ICanEvaluateToBool;
            if (floater == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                    "{0} can't be evaluated to bool!",
                    GetType().Name));
            }
            return floater.EvaluateToBool();
        }

        public string TryEvaluateToString()
        {
            ICanEvaluateToString floater = this as ICanEvaluateToString;
            if (floater == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                    "{0} can't be evaluated to string!",
                    GetType().Name));
            }
            return floater.EvaluateToString();
        }
    }

    public abstract class LispMethod<TBb> : LispOperator<TBb> where TBb : IBlackboard
    {
        protected LispMethod(string name, params LispOperator<TBb>[] children)
        {
            Name = name;
            Children = new List<LispOperator<TBb>>(children);
        }

        public string Name { get; set; }
        public List<LispOperator<TBb>> Children { get; set; }

        public override LispParser.Node BuildParseTree()
        {
            LispParser.MethodNode node = new LispParser.MethodNode(Name);
            DecorateParseTree(node);
            return node;
        }

        public override LispOperator<TBb> CreateOperator(TBb blackboard, LispParser.Node node, LispParser.ICompiler<TBb> compiler)
        {
            LispParser.MethodNode methodNode = node as LispParser.MethodNode;
            if (methodNode == null || methodNode.Identifier.Value != Name)
            {
                return null;
            }

            LispMethod<TBb> method = CreateMethod(blackboard, methodNode, compiler);
            if (method != null)
            {
                if (method.GetType() != GetType())
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Expected {0} but compiled to {1}!",
                            TypeHelper.GetFriendlyTypeName(GetType()),
                            TypeHelper.GetFriendlyTypeName(method.GetType())));
                }

                foreach (LispParser.Node child in methodNode.Children)
                {
                    method.Children.Add((LispOperator<TBb>)compiler.Compile(blackboard, child));
                }
            }
            return method;
        }

        protected virtual void DecorateParseTree(LispParser.MethodNode node)
        {
            foreach (LispOperator<TBb> lispNode in Children)
            {
                node.Children.Add(lispNode.BuildParseTree());
            }
            ValidateChildren(node);
        }

        protected virtual void ValidateChildren(LispParser.MethodNode node)
        {
            //if (Children.Count == 0)
            //{
            //    throw new InvalidOperationException("{0} must have 1 or more arguments!");
            //}
        }

        protected abstract LispMethod<TBb> CreateMethod(TBb blackboard, LispParser.MethodNode methodNode, LispParser.ICompiler<TBb> compiler);
    }
}