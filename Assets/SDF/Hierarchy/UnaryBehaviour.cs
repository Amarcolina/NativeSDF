
namespace SDF.Hierarchy {

  public class UnaryBehaviour : SDFBehaviour {
    public UnaryType Type;
    public float Offset;

    private SDFNode _cachedNode;

    protected override SDFNode GenerateNode() {
      var inverse = _cachedNode as Inverse;
      var offset = _cachedNode as Offset;

      switch (Type) {
        case UnaryType.Inverse:
          if (inverse == null) _cachedNode = inverse = new Inverse();
          break;
        case UnaryType.Offset:
          if (offset == null) _cachedNode = offset = new Offset();
          offset.PostInstruction.Op.Offset = Offset;
          break;
      }

      return _cachedNode;
    }

    public enum UnaryType {
      Inverse,
      Offset
    }
  }
}
