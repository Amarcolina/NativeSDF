using UnityEngine;

namespace SDF.Hierarchy {

  public class ShapeBehaviour : SDFBehaviour {

    public ShapeType Type = ShapeType.Sphere;

    private SDFNode _cachedNode;

    protected override SDFNode GenerateNode() {
      var sphere = _cachedNode as Sphere;
      var box = _cachedNode as Box;

      switch (Type) {
        case ShapeType.Sphere:
          if (sphere == null) _cachedNode = sphere = new Sphere();
          sphere.Center = transform.position;
          sphere.Radius = transform.lossyScale.x;
          break;
        case ShapeType.Box:
          if (box == null) _cachedNode = box = new Box();
          box.ToLocalSpace = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
          box.Extents = transform.lossyScale;
          break;
      }

      return _cachedNode;
    }

    public enum ShapeType {
      Sphere,
      Box
    }
  }
}
