export interface Attribute {
  Key: string;
  Value: string;
}

export interface Node {
  Type: string;
  Selector: string;
  Attributes: Attribute[];
  Classes: string[];
  Nodes: Node[];
  Parent: Node;
  Text?: string;
  Selected: boolean;
}
