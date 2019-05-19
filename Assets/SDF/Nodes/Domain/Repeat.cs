using Unity.Mathematics;

namespace SDF {

  public class Repeat : SDFNodeDomainSimple<Repeat.Op> {

    public float CellSize;

    public Repeat() : this(1) { }
    public Repeat(float cellSize) {
      CellSize = cellSize;
    }

    protected override Op GetOp() {
      return new Op() {
        CellSize = CellSize
      };
    }

    public struct Op : IDomainOp {
      public float CellSize;

      public void Modify(ref float3 pos) {
        pos = math.frac(pos / CellSize) * CellSize;
      }
    }
  }
}
