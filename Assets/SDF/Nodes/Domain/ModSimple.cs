using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

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
                float3 cellPos = pos / CellSize;
                cellPos = cellPos - math.floor(cellPos);
                float3 modPos = cellPos * CellSize;
                pos = math.select(pos, modPos, Mask);
            }
        }

        public static float Mod1(ref float pos, float size) {
            float halfsize = size * 0.5f;
            float c = floor((pos + halfsize) / size);
            pos = mod(pos + halfsize, size) - halfsize;
            return c;
        }
    }
}
