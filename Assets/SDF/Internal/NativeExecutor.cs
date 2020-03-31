using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace SDF.Internal {

    unsafe struct NativeExecutor1x : IExecutor {
        public void* pc;
        public float* stack;
        public float3 pos;

        public void Exec<T>() where T : struct, IInstruction {
            UnsafeUtility.CopyPtrToStructure((byte*)pc + sizeof(uint), out T instruction);
            instruction.Exec(ref stack, ref pos);
            pc = (byte*)pc + sizeof(uint) + UnsafeUtility.SizeOf<T>();
        }
    }

    unsafe struct NativeExecutor4x : IExecutor {
        public void* pc;
        public float4* stack;
        public float3 pos0, pos1, pos2, pos3;

        public void Exec<T>() where T : struct, IInstruction {
            UnsafeUtility.CopyPtrToStructure((byte*)pc + sizeof(uint), out T instruction);
            instruction.Exec(ref stack, ref pos0, ref pos1, ref pos2, ref pos3);
            pc = (byte*)pc + sizeof(uint) + UnsafeUtility.SizeOf<T>();
        }
    }
}
