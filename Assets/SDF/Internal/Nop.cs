using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace SDF.Internal {

    /// <summary>
    /// This instruction does nothing.  It is compiled out when a signed distance hierarchy is compiled and represents 
    /// a zero cost instruction.
    /// </summary>
    public struct Nop : IInstruction {
        public int StackOffset => 0;
        public int StackOffset4x => 0;

        public unsafe void Exec(ref float* stack, ref float3 pos) { }
        public unsafe void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3) { }
    }
}
