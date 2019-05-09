using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace SDF.Internal {

    /// <summary>
    /// This interface can be implemented in order to create new instruction types.  If you are inheriting
    /// from SDFNodeShape, SDFNodeUnary, or SDFNodeBinary, you do not need to worry about implementing this 
    /// interface directly, as those classes take care of implementing it for you.
    /// 
    /// IMPORTANT: Any time you create a new instruction, either by inheriting this interface directly, or
    /// by inheriting from SDFNodeShape, SDFNodeUnary, or SDFNodeBinary, you will need to register the new
    /// instruction within the Execute method in Instruction.cs
    /// 
    /// Instructions in the sdf tree are executed in a depth-first order.  An instruction can be executed 
    /// either pre-order or post-order.  The default for almost all instructions is post order, to allow 
    /// the instructions to operate on distances provided by child instructions.
    /// 
    /// The stack pointer always points to the first byte of empty space after the top of the stack.  Any
    /// instruction can read and write to the stack, but should always make sure to place the stack pointer
    /// to point at the first byte of empty space once the instruction is complete.
    /// </summary>
    public unsafe interface IInstruction {

        /// <summary>
        /// The number of bytes that will be added to the stack when this instruction is executed.  Can
        /// be negative if this instruction removes bytes.
        /// </summary>
        int StackOffset { get; }

        /// <summary>
        /// The number of bytes that will be added to the stack when this instruction is executed in 4x 
        /// mode.  Can be negative if this instruction removes bytes.
        /// </summary>
        int StackOffset4x { get; }

        /// <summary>
        /// Execute this instruction with the given stack pointer and world-space position.
        /// </summary>
        void Exec(ref float* stack, ref float3 pos);

        /// <summary>
        /// Execute this instruction in 4x mode with the given stack pointer and world-space positions.
        /// In 4x mode, all instructions work on 4 distances and 4 positions at a time, instead of just
        /// 1 distance and 1 position.
        /// </summary>
        void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3);
    }
}
