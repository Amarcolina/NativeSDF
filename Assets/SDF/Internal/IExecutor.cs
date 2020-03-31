using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDF.Internal {

    /// <summary>
    /// Instances that implement this interface can be used to execute instructions via Instruction.Execute.
    /// 
    /// Note that even if you are creating new instructions, it is very likely that you do no need to implement
    /// this interface.  This interface only becomes relevant if you are creating new ways to execute entire
    /// signed distance hierarchies.
    /// </summary>
    public interface IExecutor {
        void Exec<T>() where T : struct, IInstruction;
    }
}
