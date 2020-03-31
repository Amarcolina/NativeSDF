
namespace SDF {

    public enum NodeType {
        /// <summary>
        /// Represents a node that takes in a position and outputs a distance.
        /// Cannot have children.
        /// </summary>
        Shape = 0,
        /// <summary>
        /// Represents a node that takes in a distance and outputs a new distance.
        /// Can only have a single child.
        /// </summary>
        Unary = 1,
        /// <summary>
        /// Represents a node that takes in two distances and outputs a new distance.
        /// Can only have two children.
        /// </summary>
        Binary = 2,
        /// <summary>
        /// Represents a node that takes in two or more distances and outputs a new distance.
        /// Can have two or more children.  More than two children are folded one by one using
        /// the pair operation.
        /// </summary>
        BinaryCommutative = 3,
        /// <summary>
        /// Represents a custom node that does not have any restrictions on inputs or outputs,
        /// or number of children.
        /// </summary>
        Custom = 4
    }
}
