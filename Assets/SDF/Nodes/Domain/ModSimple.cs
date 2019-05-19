using System;
using Unity.Mathematics;

namespace SDF {

  [Serializable]
  public class ModSimple : SDFNodeDomainSimple<ModSimple.Op> {

    public float CellSize;
    public bool ModX, ModY, ModZ;

    public ModSimple() : this(1) { }
    public ModSimple(float cellSize) {
      CellSize = cellSize;
    }

    protected override Op GetOp() {
      return new Op() {
        CellSize = CellSize,
        Mask = new bool3(ModX, ModY, ModZ)
      };
    }

    public struct Op : IDomainOp {
      public float CellSize;
      public bool3 Mask;

      public void Modify(ref float3 pos) {
        float3 modPos = math.frac(pos / CellSize) * CellSize;
        pos = math.select(pos, modPos, Mask);
      }
    }
  }
}
