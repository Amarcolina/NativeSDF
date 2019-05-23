using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDF.Internal {

  /// <summary>
  /// This class provides information and utilities revolving around signed distance field instructions.
  /// </summary>
  public static class Instruction {

    /// <summary>
    /// Gets the opCode for the given instruction.  The opCode will be a number between 0 and 255 inclusive,
    /// and uniquely identifies the instruction type.
    /// 
    /// IMPORTANT: When creating new instructions, you will need to MODIFY the Execute method in Instruction.cs
    /// to include an entry for your new instruction so that you can specify its opCode.
    /// </summary>
    public static byte GetOpCode<T>() where T : IInstruction {
      return GetOpCode(typeof(T));
    }

    /// <summary>
    /// Gets the opCode for the given instruction.  The opCode will be a number between 0 and 255 inclusive,
    /// and uniquely identifies the instruction type.
    /// 
    /// IMPORTANT: When creating new instructions, you will need to MODIFY the Execute method in Instruction.cs
    /// to include an entry for your new instruction so that you can specify its opCode.
    /// </summary>
    public static byte GetOpCode(Type type) {
      if (_typeToOpCode == null) {
        initTypeToOpCodeMap();
      }

      byte opCode;
      if (!_typeToOpCode.TryGetValue(type, out opCode)) {
        string typeName = type.FullName;

        if (typeof(SDFNodeShape<>).IsAssignableFrom(type.DeclaringType) ||
            typeof(SDFNodeUnary<>).IsAssignableFrom(type.DeclaringType) ||
            typeof(SDFNodeBinary<>).IsAssignableFrom(type.DeclaringType)) {
          typeName = type.GenericTypeArguments[0].DeclaringType.FullName + "." + type.Name;
        }

        throw new InvalidProgramException("Could not determine the opCode of instruction " + typeName + "\n" +
                                          "If you have created a new instruction, make sure to go to Instruction.cs and add your " +
                                          "new instruction to the execution switch table inside of the Execute() method.");
      }

      return opCode;
    }



    /// <summary>
    /// Executes the instruction with the given opCode using the provided Executor.
    /// 
    /// IMPORTANT: When creating new instructions, you will need to MODIFY this method  to include 
    /// an entry for your new instruction so that you can specify its opCode.
    /// </summary>
    public static void Execute<ExecutorType>(ref ExecutorType ex, byte opcode) where ExecutorType : IExecutor {
      switch (opcode) {
        case 0:
          ex.Exec<Inverse.Instruction>();
          break;
        case 1:
          ex.Exec<Union.Instruction>();
          break;
        case 2:
          ex.Exec<UnionSmooth.Instruction>();
          break;
        case 3:
          ex.Exec<Intersection.Instruction>();
          break;
        case 4:
          ex.Exec<Sphere.Instruction>();
          break;
        case 5:
          ex.Exec<Box.Instruction>();
          break;
        case 6:
          ex.Exec<Offset.Instruction>();
          break;
        case 7:
          ex.Exec<SDFNodeDomainBase.PopPositionsInstruction>();
          break;
        case 8:
          ex.Exec<ModSimple.Instruction>();
          break;
          // Add new instructions here!
      }
    }

    private static Dictionary<Type, byte> _typeToOpCode = null;
    private static void initTypeToOpCodeMap() {
      var opCodeMap = new Dictionary<Type, byte>();
      TypeReceiver receiver = new TypeReceiver();

      for (int i = 0; i < 256; i++) {
        byte opCode = (byte)i;

        receiver.InstructionType = null;
        Execute(ref receiver, opCode);

        if (receiver.InstructionType != null) {
          if (opCodeMap.ContainsKey(receiver.InstructionType)) {
            throw new InvalidProgramException("A second instance of Instruction " + receiver.InstructionType + " was found with an op code of " + i);
          }

          opCodeMap[receiver.InstructionType] = opCode;
        }
      }

      _typeToOpCode = opCodeMap;
    }

    private class TypeReceiver : IExecutor {
      public Type InstructionType;

      public void Exec<InstructionType>() where InstructionType : struct, IInstruction {
        this.InstructionType = typeof(InstructionType);
      }
    }
  }
}
