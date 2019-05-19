using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDF.Hierarchy {

  public class SDFBehaviour : MonoBehaviour {
    public const string NODE_TYPE_PROPERTY = nameof(Type);

    public NodeType Type;

    [DisplayFor(NodeType.SmoothUnion)]
    public float K;

    [DisplayFor(NodeType.Offset)]
    public float Offset;

    [DisplayFor(NodeType.ModSimple)]
    public float CellSize;
    [DisplayFor(NodeType.ModSimple)]
    public bool X = true, Y = true, Z = true;

    private SDFNode _cachedNode;

    public SDFNode GetNode() {

      SDFNode node = GenerateNode();

      node.ClearChildren();
      linkUpChildren(node, transform);

      return node;
    }

    private static List<SDFBehaviour> _tmpList = new List<SDFBehaviour>();
    private void linkUpChildren(SDFNode node, Transform t) {
      for (int i = 0; i < t.childCount; i++) {
        t.GetChild(i).GetComponents(_tmpList);
        if (_tmpList.Count > 0) {
          node.Add(_tmpList[0].GetNode());
        } else {
          linkUpChildren(node, t.GetChild(i));
        }
      }
    }

    private SDFNode GenerateNode() {
      //Shapes
      var sphere = _cachedNode as Sphere;
      var box = _cachedNode as Box;

      //Binaries
      var intersection = _cachedNode as Intersection;
      var union = _cachedNode as Union;
      var smoothUnion = _cachedNode as UnionSmooth;

      //Unaries
      var inverse = _cachedNode as Inverse;
      var offset = _cachedNode as Offset;

      //Domains
      var modSimple = _cachedNode as ModSimple;

      switch (Type) {
        //Shapes
        case NodeType.Sphere:
          if (sphere == null) _cachedNode = sphere = new Sphere();
          sphere.Center = transform.position;
          sphere.Radius = transform.lossyScale.x;
          break;
        case NodeType.Box:
          if (box == null) _cachedNode = box = new Box();
          box.ToLocalSpace = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
          box.Extents = transform.lossyScale;
          break;

        //Binaries
        case NodeType.Intersection:
          if (intersection == null) _cachedNode = intersection = new Intersection();
          break;
        case NodeType.Union:
          if (union == null) _cachedNode = union = new Union();
          break;
        case NodeType.SmoothUnion:
          if (smoothUnion == null) _cachedNode = smoothUnion = new UnionSmooth();
          smoothUnion.K = K;
          break;

        //Unaries
        case NodeType.Inverse:
          if (inverse == null) _cachedNode = inverse = new Inverse();
          break;
        case NodeType.Offset:
          if (offset == null) _cachedNode = offset = new Offset();
          offset.Value = Offset;
          break;

        //Domains
        case NodeType.ModSimple:
          if (modSimple == null) _cachedNode = modSimple = new ModSimple();
          modSimple.CellSize = CellSize;
          modSimple.ModX = X;
          modSimple.ModY = Y;
          modSimple.ModZ = Z;
          break;
      }

      return _cachedNode;
    }

    public enum NodeType {
      [Category("Shape")]
      Sphere,
      [Category("Shape")]
      Box,

      [Category("Binary")]
      Intersection,
      [Category("Binary")]
      Union,
      [Category("Binary")]
      SmoothUnion,

      [Category("Unary")]
      Inverse,
      [Category("Unary")]
      Offset,

      [Category("Domain")]
      ModSimple
    }

    public class DisplayForAttribute : Attribute {
      public readonly NodeType[] Types;

      public DisplayForAttribute(params NodeType[] Types) {
        this.Types = Types;
      }
    }

    public class CategoryAttribute : Attribute {
      public readonly string Category;

      public CategoryAttribute(string Category) {
        this.Category = Category;
      }
    }
  }
}
