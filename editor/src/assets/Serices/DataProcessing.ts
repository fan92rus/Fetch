import { Node } from "../models/tree";

export class Convertor {
  CheckNode(node: Node, Text: string): boolean {
    if (!node) return node;
    if (
      (node.Attributes && node.Attributes.find(a => a.Value.includes(Text))) ||
      (node.Classes && node.Classes.find(c => c.includes(Text))) ||
      (node.Text && node?.Text?.includes(Text))
    ) {
      return true;
    }
    return false;
  }

  FindText(node: Node, Text: string): Node[] {
    console.log(node);
    const nodes: Node[] = [];

    if (Text === "" || Text === undefined) {
      nodes.push(node);
      return nodes;
    }
    const checked = this.CheckNode(node, Text);

    if (checked) {
      nodes.push(node);
    }
    if (node?.Nodes) {
      node.Nodes.forEach(n => {
        n.Parent = node;
        
        const subNodes = this.FindText(n, Text);

        if (subNodes) {
          subNodes.forEach(x => nodes.push(x));
        }
      });
    }

    return nodes;
  }
}
