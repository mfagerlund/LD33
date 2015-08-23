using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Beehive.Lisp
{
    // Simple parser for compact structured scripting
    public class LispParser
    {
        private Match _match;
        private List<Node> _tokens;
        private static readonly Regex Tokenizer =
            new Regex(
                @"((?<operator>[<>]=?|[+*/-]|=)\s
		        |(?<whitespace>\s+)
		        |(?<ope_paren>\()
		        |(?<clo_paren>\))
		        |(?<identifier>[A-Za-z_→?][A-Za-z_0-9.→?]*)
		        |(?<float>[+-]?([0-9]+(\.[0-9]+)))
                |(?<int>[+-]?[0-9]+(?!\.))
		        |(?<string>""[^""]*"")
                |(?<colon>:)
                |(?<equals>=)
                |(?<comment>\/\*.*?\*\/)
		        |(?<unmatched>.*))",
                RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

        public interface ICanBuildLispParseTree
        {
            Node BuildParseTree();
        }

        public interface ICompiler<TBb> where TBb : IBlackboard
        {
            ICanCreateFromLispParseTree<TBb> Compile(TBb blackboard, Node node);
        }

        public interface ICanCreateFromLispParseTree<TBb> where TBb : IBlackboard
        {
            ICanCreateFromLispParseTree<TBb> CreateFromParseTree(TBb blackboard, Node node, ICompiler<TBb> compiler);
        }

        public List<Node> Tokenize(string lispText)
        {
            _tokens = new List<Node>();
            _match = Tokenizer.Match(lispText);
            while (_match.Success && _match.Length > 0)
            {
                if (_match.Groups["unmatched"].Success)
                {
                    throw new InvalidOperationException(
                        string.Format("Unmatched token: {0} at {1}", _match.Value, _match.Index));
                }

                bool matched =
                    Trymatch("whitespace", null)
                    || Trymatch("comment", null)
                    || Trymatch("ope_paren", g => new MethodNode { Group = g })
                    || Trymatch("clo_paren", g => new CloseParenNode { Group = g })
                    || Trymatch("operator", g => new OperatorNode { Group = g })
                    || Trymatch("identifier", g => new IdentifierNode { Group = g })
                    || Trymatch("int", g => new IntegerNode { Group = g })
                    || Trymatch("float", g => new FloatNode { Group = g })
                    || Trymatch("colon", g => new NamedParameterNode { Group = g })
                    || Trymatch("equals", g => new NamedParameterValueNode { Group = g })
                    || Trymatch("string", g => new StringNode { Group = g });

                if (!matched)
                {
                    throw new InvalidOperationException("Unhandled token: " + _match.Value);
                }

                _match = _match.NextMatch();
            }

            return _tokens;
        }

        public Node Parse(string lispText)
        {
            List<Node> tokenList = Tokenize(lispText);
            Queue<Node> tokenQueue = new Queue<Node>(tokenList);
            Node node = tokenQueue.Dequeue();
            node.Populate(tokenQueue);
            if (tokenQueue.Any())
            {
                throw new InvalidOperationException(
                    string.Format(
                    "Parse failed, there are tokens [{0}] left in the token queue!",
                    tokenQueue.Peek()));
            }
            return node;
        }

        private bool Trymatch(string name, Func<Group, Node> func)
        {
            Group group = _match.Groups[name];
            if (!group.Success)
            {
                return false;
            }
            if (func != null)
            {
                _tokens.Add(func(group));
            }
            return true;
        }

        public abstract class Node
        {
            private Group _group;

            public Group Group
            {
                get
                {
                    return _group;
                }
                set
                {
                    _group = value;
                    UpdateValue();
                }
            }

            public abstract void Populate(Queue<Node> tokens);
            public abstract string ToCode(bool prettyPrint = true, string indent = "");
            public override string ToString()
            {
                return ToCode(false);
            }

            public bool ParamX<TParameterType>(
                Func<TParameterType, bool> predicate,
                Action<TParameterType> action) where TParameterType : Node
            {
                TParameterType parameter = this as TParameterType;
                if (parameter != null)
                {
                    if (predicate == null || predicate(parameter))
                    {
                        action(parameter);
                        return true;
                    }
                }
                return false;
            }

            public bool NamedParamX<TValueType>(
               string identifier,
               Action<TValueType> action)
               where TValueType : LispParser.Node
            {
                NamedParameterNode parameter = this as NamedParameterNode;
                if (parameter != null)
                {
                    if (parameter.Identifier.Value == identifier)
                    {
                        action((TValueType)parameter.Value);
                        return true;
                    }
                }
                return false;
            }

            protected abstract void UpdateValue();

            protected Node GetNextTokenMatching(Queue<Node> tokens, Node previousNode, Func<Node, bool> predicate, string format)
            {
                VerifyThatThereAreMoreTokens(tokens, previousNode);

                Node node = tokens.Dequeue();
                if (!predicate(node))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            format,
                            Group.Index + Group.Length,
                            node.GetType().Name));
                }
                return node;
            }

            protected void VerifyThatThereAreMoreTokens(Queue<Node> tokens, Node previousNode)
            {
                if (!tokens.Any())
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Expected identifier after [{0}] at {1}, but the queue is empty!",
                            previousNode.ToCode(false),
                            Group.Index + Group.Length));
                }
            }
        }

        public abstract class LeafNode : Node
        {
            public string GroupText { get { return Group.Value; } }

            public override void Populate(Queue<Node> tokens)
            {
                // No children for most leaves
            }
        }

        public class NamedParameterNode : LeafNode
        {
            public NamedParameterNode()
            {
            }

            public NamedParameterNode(string identifier, int value)
            {
                Identifier = new IdentifierNode(identifier);
                Value = new IntegerNode(value);
            }

            public NamedParameterNode(string identifier, string value)
            {
                Identifier = new IdentifierNode(identifier);
                Value = new StringNode(value);
            }

            public NamedParameterNode(string identifier, Node node)
            {
                Identifier = new IdentifierNode(identifier);
                Value = node;
            }

            public IdentifierNode Identifier { get; set; }
            public Node Value { get; set; }

            public override string ToCode(bool prettyPrint = true, string indent = "")
            {
                if (Identifier != null && Value != null)
                {
                    return string.Format(
                        ":{0}={1}",
                        Identifier.ToCode(prettyPrint, indent),
                        Value.ToCode(prettyPrint, indent));
                }
                else
                {
                    return ":";
                }
            }

            public override void Populate(Queue<Node> tokens)
            {
                Node node =
                    GetNextTokenMatching(
                        tokens,
                        this,
                        n => n is IdentifierNode,
                        "Expected identifier after [:] at {0}, but received {1}.");
                Identifier = (IdentifierNode)node;

                // This node is just skipped, it's here for parsing but isn't kept.
                node = GetNextTokenMatching(tokens, Identifier, n => n is NamedParameterValueNode, "Expected identifier after [:] at {0}, but received {1}.");

                VerifyThatThereAreMoreTokens(tokens, node);

                Value = tokens.Dequeue();
                Value.Populate(tokens);
            }

            protected override void UpdateValue()
            {
                // Don't have no value of our own!
            }
        }

        public abstract class NumberNode : LeafNode
        {
            public abstract float FloatValue { get; }
        }

        public class FloatNode : NumberNode
        {
            public FloatNode(float value = 0)
            {
                Value = value;
            }

            public float Value { get; set; }
            public override float FloatValue { get { return Value; } }

            public override string ToCode(bool prettyPrint = true, string indent = "") { return Value.ToString(CultureInfo.InvariantCulture); }

            protected override void UpdateValue()
            {
                Value = float.Parse(GroupText, CultureInfo.InvariantCulture);
            }
        }

        public class IntegerNode : NumberNode
        {
            public IntegerNode(int value = 0)
            {
                Value = value;
            }
            public int Value { get; set; }
            public override float FloatValue { get { return Value; } }

            public override string ToCode(bool prettyPrint = true, string indent = "") { return Value.ToString(CultureInfo.InvariantCulture); }

            protected override void UpdateValue()
            {
                Value = int.Parse(GroupText, CultureInfo.InvariantCulture);
            }
        }

        public class StringNode : LeafNode
        {
            public StringNode()
            {
            }

            public StringNode(string value)
            {
                Value = value;
            }

            public string Value { get; set; }

            public override string ToCode(bool prettyPrint = true, string indent = "") { return "\"" + Value + "\""; }

            protected override void UpdateValue()
            {
                Value = GroupText.Substring(1, GroupText.Length - 2);
            }
        }

        public class IdentifierNode : LeafNode
        {
            public IdentifierNode(string value = "")
            {
                Value = value;
            }

            public string Value { get; set; }
            public override string ToCode(bool prettyPrint = true, string indent = "") { return Value; }

            protected override void UpdateValue()
            {
                Value = GroupText;
            }
        }

        public class OperatorNode : IdentifierNode
        {
        }

        public class MethodNode : Node
        {
            public MethodNode()
            {
                Children = new List<Node>();
            }

            public MethodNode(string identifier)
                : this()
            {
                Identifier = new IdentifierNode(identifier);
            }

            public List<Node> Children { get; set; }
            public IdentifierNode Identifier { get; set; }
            public string MethodName { get { return Identifier.Value; } }

            public NamedParameterNode GetChild(string childIdentifier)
            {
                NamedParameterNode namedParameter =
                    Children
                        .OfType<NamedParameterNode>()
                        .SingleOrDefault(x => x.Identifier.Value == childIdentifier);
                return namedParameter;
            }

            public override string ToCode(
                bool prettyPrint = true,
                string indent = "")
            {
                string result = string.Empty;
                if (prettyPrint && indent != string.Empty)
                {
                    result = Environment.NewLine + indent;
                }

                string identifier = Identifier != null ? Identifier.ToCode(prettyPrint, indent) : "";

                result +=
                    "(" + identifier + (Children.Any() ? " " : string.Empty)
                    + string.Join(" ", Children.Select(x => x.ToCode(prettyPrint, indent + "  ")).ToArray())
                    + ")";

                return result;
            }

            public override void Populate(Queue<Node> tokens)
            {
                if (!tokens.Any())
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Expected identifier after [(] at {0}.",
                            Group.Index + Group.Length));
                }

                Node node = tokens.Dequeue();
                Identifier = node as IdentifierNode;
                if (Identifier == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Expected identifier or operator after [(] at {0}, but received {1}.",
                            Group.Index + Group.Length,
                            node.GetType().Name));
                }

                //Children.Add(node);

                while (true)
                {
                    if (!tokens.Any())
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "Opening parentheseis [(] at {0} not closed!",
                                Group.Index + Group.Length));
                    }

                    node = tokens.Dequeue();

                    if (node is CloseParenNode)
                    {
                        return;
                    }
                    node.Populate(tokens);
                    Children.Add(node);
                }
            }

            public void Add(Node node)
            {
                Children.Add(node);
            }

            public void Add(string name, string value, bool skipIfNull = false)
            {
                if (value == null && skipIfNull)
                {
                    return;
                }
                Add(new NamedParameterNode(name, value));
            }

            public void Add(string name, int value)
            {
                Add(new NamedParameterNode(name, value));
            }

            public void Add(string name, Node node)
            {
                Add(new NamedParameterNode(name, node));
            }

            protected override void UpdateValue()
            {
            }
        }

        internal class CloseParenNode : LeafNode
        {
            public override void Populate(Queue<Node> tokens)
            {
                throw new InvalidOperationException(
                    string.Format(
                    "Unexpected token [)] at {0}.",
                    Group.Index));
            }

            public override string ToCode(
                bool prettyPrint = true,
                string indent = "")
            {
                return ")";
            }

            protected override void UpdateValue()
            {
            }
        }

        internal class NamedParameterValueNode : LeafNode
        {
            public override string ToCode(bool prettyPrint = true, string indent = "")
            {
                return "=";
            }

            protected override void UpdateValue()
            {
                // Don't have no value of our own!
            }
        }
    }
}