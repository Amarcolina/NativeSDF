
namespace SDF {

  public class Inverse : SDFNodeUnary<Inverse.Op> {

    public struct Op : IUnaryOp {
      public void Modify(ref float dist) {
        dist = -dist;
      }
    }
  }
}
