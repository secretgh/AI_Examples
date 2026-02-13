using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class BTDebugUI : MonoBehaviour
{
    public TextMeshProUGUI output;
    private BehaviorTree tree;
    float timer;
    private struct DepthLayer
    {
        public Node node;
        public int depth;
        public DepthLayer(Node node, int depth)
        {
            this.node = node;
            this.depth = depth;
        }
    }

    public void SetTree(BehaviorTree tree)
    {
        this.tree = tree;
    }

    void Update()
    {
        if (tree == null) return;
        output.text = Print(tree.root);
    }
    public string Print(Node root)
    {
        if (root == null)
            return "<Empty Tree>";

        var sb = new StringBuilder(1024);
        var stack = new Stack<DepthLayer>();

        stack.Push(new DepthLayer(root, 0));

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            var node = current.node;
            int depth = current.depth;

            // Indentation
            sb.Append(' ', depth * 2);

            // Node header
            sb.Append(node.name).Append("  ");

            // Selector-specific info
            if (node is SelectorNode selector && selector.currentChildIndex >= 0)
            {
                sb.Append(" -> ");
                sb.Append(selector.children[selector.currentChildIndex].name);
            }
            else
            {
                switch (node.status)
                {
                    case Node.NodeStatus.Running:
                    case Node.NodeStatus.Success:
                        sb.Append($"[{node.status.ToString().ToUpper()}]");
                        break;

                }
            }

            sb.AppendLine();

            // Push children in reverse so they print in correct order
            if (current.node.children != null && current.node.children.Count > 0)
            {
                for (int i = node.children.Count - 1; i >= 0; i--)
                {
                    stack.Push(new DepthLayer(node.children[i], depth + 1));
                }
            }
        }

        return sb.ToString();
    }
}
