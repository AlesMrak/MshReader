using System;
using System.Collections.Generic;
using System.Text;

using Library3d.Math;
using Library3d.Mesh;

namespace Library3d.Nodes
{
    public enum eNodeObjectType
    {
        eNone,
        e3dMesh,
        e3dMeshLod,
        eMshFile,
        eHimFile,
        e3dMeshCollision,
        eMshHook,
        e3dMshShadow,
        eNode,
    }
    public class NodeObject
    {
        public string Name = string.Empty;
        public Node ParentNode = null;
        public eNodeObjectType Type = eNodeObjectType.eNone;
        public bool Enabled = true;
        public bool Export = true;

    }

    public class Node : NodeObject
    {
        public JMatrix Global;
        public JMatrix Local;

        public NodeManager Scene = null;

        public List<Node> Children;
        public List<NodeObject> Objects;

        public Node()
        {
            Init();
        }

        protected void Init()
        {
            Global = new JMatrix();
            Local = new JMatrix();
            Children = new List<Node>();
            Objects = new List<NodeObject>();
        }

        public void AddObject(NodeObject o)
        {
            o.ParentNode = this;
            Objects.Add(o);
        }

        public void RemoveObject(NodeObject o)
        {
            o.ParentNode = null;
            Objects.Remove(o);

        }
        public void AddNode(Node n)
        {
            Children.Add(n);
            n.ParentNode = this;
            n.Scene = this.Scene;
            if (this.Scene != null)
            {
                this.Scene.AddNode(n);
            }
        }
        public void RemoveNode(Node n)
        {
            Children.Remove(n);
            n.ParentNode = null;
            if (this.Scene != null)
            {
                this.Scene.RemoveNode(n);
            }
        }

        public void Update_Position()
        {
            _CalculateGlobalPosition(this);
        }

        public void _CalculateGlobalPosition(Node parent)
        {

            foreach (Node child in Children)
            {
                child.Global = child.Local * parent.Global;
                _CalculateGlobalPosition(parent);
            }
        }
        public MeshData ToEmptyMesh()
        {
            MeshData m = new MeshData();
            m.Name = "["+this.Name+"]";
            m.OverrideLocalMatrix = this.Local;

            Vertex v = new Vertex();
            v.pos = new JVector(0);
            m.Vertexs.Add(v);

            m.Type = eNodeObjectType.eNode;
            return m;
        }

    }

    public class NodeManager
    {
        public Node Root = null;
        public List<Node> Nodes = new List<Node>();
        public void AddNode(Node n)
        {
            Nodes.Add(n);
        }
        public void RemoveNode(Node n)
        {
            Nodes.Remove(n);
        }

    }
}
