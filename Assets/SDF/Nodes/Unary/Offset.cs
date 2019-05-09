
namespace SDF {

  public class Offset : SDFNodeUnary<Offset.Op> {

    public Offset() : base() { }
    public Offset(float offset) : base(new Op() { Offset = offset }) { }

    public struct Op : IUnaryOp {
      public float Offset;

      public void Modify(ref float dist) {
        dist += Offset;
      }
    }
  }
}
