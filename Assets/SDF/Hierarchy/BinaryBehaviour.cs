
namespace SDF.Hierarchy {

  public class BinaryBehaviour : SDFBehaviour {

    public BinaryType Type;
    public float K;

    private SDFNode _cachedNode;

    protected override SDFNode GenerateNode() {
      var intersection = _cachedNode as Intersection;
      var union = _cachedNode as Union;
      var smoothUnion = _cachedNode as UnionSmooth;

      switch (Type) {
        case BinaryType.Intersection:
          if (intersection == null) _cachedNode = intersection = new Intersection();
          break;
        case BinaryType.Union:
          if (union == null) _cachedNode = union = new Union();
          break;
        case BinaryType.SmoothUnion:
          if (smoothUnion == null) _cachedNode = smoothUnion = new UnionSmooth();
          smoothUnion.Operation.K = K;
          break;
      }

      return _cachedNode;
    }

    public enum BinaryType {
      Intersection,
      Union,
      SmoothUnion
    }
  }
}
