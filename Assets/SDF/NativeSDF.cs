using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace SDF {
    using Internal;

    /// <summary>
    /// Represents a signed distance field.  Given a position, it can tell you the minimum distance to the 
    /// nearest surface.  If the given position is inside a shape, the returned distance will be negative.
    /// 
    /// This representation can be used inside of the Unity Job System and be efficiently Burst compiled.
    /// 
    /// A NativeSDF instance should be created by first creating a SDFNode hierarchy.
    /// </summary>
    public struct NativeSDF : IDisposable {
        [NativeDisableUnsafePtrRestriction]
        private IntPtr _instructions, _stacks;
        private Allocator _allocator;

        private int _instructionCount;
        private int _stackSize;

        [NativeSetThreadIndex]
        private int _threadIndex;

        /// <summary>
        /// Constructs a new NativeSDF instance.  SDFNode is the recommended way to construct NativeSDF instances.
        /// </summary>
        /// <param name="instructions">
        /// The instructions argument is a pointer to a buffer of instructions to execute.  Each instruction should
        /// be preceded by a UINT representing that instructions opCode.  When calling Sample, each instruction will
        /// be executed sequentially in the order they appear in this buffer.
        /// 
        /// This NativeSDF instance assumes ownership of this buffer once constructed and will be responsible for
        /// freeing it.
        /// </param>
        /// <param name="instructionCount">
        /// The argument instructionCount specifies how many instructions are stored in the buffer.
        /// </param>
        /// <param name="stackSize">
        /// The stackSize argument specifies maximum depth in bytes of the provided program.
        /// </param>
        /// <param name="allocator">
        /// The allocator argument specifies the allocation label used to allocate the instruction buffer, and
        /// which allocation label to use to allocate the stack memory.
        /// </param>
        public NativeSDF(IntPtr instructions, int instructionCount, int stackSize, Allocator allocator) {
            _instructions = instructions;
            _allocator = allocator;

            _instructionCount = instructionCount;
            _threadIndex = JobsUtility.MaxJobThreadCount - 1;

            unsafe {
                _stackSize = stackSize;
                _stackSize = (_stackSize / JobsUtility.CacheLineSize + 1) * JobsUtility.CacheLineSize;

                _stacks = (IntPtr)UnsafeUtility.Malloc(_stackSize * JobsUtility.MaxJobThreadCount,
                                                       JobsUtility.CacheLineSize,
                                                       allocator);
            }
        }

        /// <summary>
        /// Disposes this structure and frees all connected resources.  No method should be 
        /// called on this instance once Dispose has been called.
        /// </summary>
        public void Dispose() {
            unsafe {
                UnsafeUtility.Free((void*)_instructions, _allocator);
                UnsafeUtility.Free((void*)_stacks, _allocator);
            }
        }

        /// <summary>
        /// Evaluates and returns the signed distance at a given position.
        /// </summary>
        public float Sample(float3 pos) {
            unsafe {
                NativeExecutor1x ex;
                ex.pc = (void*)_instructions;
                ex.stack = (float*)((byte*)_stacks + _threadIndex * _stackSize);
                ex.pos = pos;

                for (int i = 0; i < _instructionCount; i++) {
                    byte opCode = *(byte*)ex.pc;
                    Instruction.Execute(ref ex, opCode);
                }

                return *(ex.stack - 1);
            }
        }

        /// <summary>
        /// Evaluates and returns the signed distances at the given four positions.
        /// </summary>
        public float4 Sample(float3 pos0, float3 pos1, float3 pos2, float3 pos3) {
            unsafe {
                NativeExecutor4x ex;
                ex.pc = (void*)_instructions;
                ex.stack = (float4*)((byte*)_stacks + _threadIndex * _stackSize);
                ex.pos0 = pos0;
                ex.pos1 = pos1;
                ex.pos2 = pos2;
                ex.pos3 = pos3;

                for (int i = 0; i < _instructionCount; i++) {
                    byte opCode = *(byte*)ex.pc;
                    Instruction.Execute(ref ex, opCode);
                }

                return *(ex.stack - 1);
            }
        }
    }
}
