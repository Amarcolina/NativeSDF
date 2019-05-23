# NativeSDF
Evaluate signed-distance-fields with great efficiency using the power of the Unity Job System and the Burst Compiler.  This library allows you to construct signed distance fields by assembling a tree of signed distance operations.  Once assembled, this tree can be compiled into a NativeSDF representation, which can be evaluated extremely efficiently and is compatible with the Unity Job System and the Unity Burst Compiler.

Here is an example of what is possible using the power of NativeSDF.  This is a real-time meshing example which samples the distance field tens of thousands of times every frame, and is still able to run at 60fps.  

<img src="https://i.imgur.com/8EQN36G.gif"> 

Here is another example scene that is included in this project.  You can view slices of the distance field color coded, which can help with debugging.

<img src="https://i.imgur.com/7haVJRO.gif">

Note that these examples are not really the point of this library, there are going to be way more efficient ways to mesh distance fields, or visualize things effectively.  Hopefully as things progress, we can start including some of those techniques in this repository as well!

# Quick Start
Download or clone the repository, and copy the SDF folder into your Unity project.  Make sure you have unsafe code enabled, and are running 2019.1 or later.

Construct signed distance fields by instantiating classes that inherit from SDFNode, like Sphere, Box, Union, or Inverse.  Child these nodes to each other to form a tree, and then call Compile on the root node to receive a NativeSDF representation of that tree.  The NativeSDF can be used to evaluate the distance of the field at any position.

You can also create SDFNode hierarchies interactively by using the SDFBehaviour class.  You can add the SDFBehaviour class to a gameObject, and then select the type of node to use.  By adding child gameObjects and adding more SDFBehaviour classes, you can create a hierarchy of SDFNodes interactively.

# Creating New Types Of Nodes
If you would like to create new types of nodes, there are three base classes created that you can inherit from.
 * SDFNodeShape - Inherit from this to create a node that represents a shape in the signed distance field.  You receive a position as input and must return the signed distance to your shape as output.
 * SDFNodeUnary - Inherit from this to create a node that represents a unary operation on the child distance field.  You receive the childs distance as input and must return a new distance as output.
 * SDFNodeBinary - Inherit from this to create a node that represents a binary operation on the two child distance fields.  You receive the two childrens distances as input and must return a new combined distance as output.  In order to create nodes that accept more than two children, you can also optionally override the IsCommutative bool to return True, to signal that your binary operation can be successively applied to children distances to arrive at a final single result.
 * SDFNodeDomain - Inherit from this to create a node that represents a domain operation on the child distance field.  You can modify the sample position that is used to sample the childs field, which can be used to create a large number of powerful effects, like looping or mirroring.

You can also directly inherit from SDFNode if you want to create a custom node that does not fall into one of the above categories, but you will require deeper knowledge of how the system functions.

IMPORTANT: Once you have created a new node, before you can use it you will need to add it to the instruction execution table.  To do this simply visit SDF/Internal/Instruction.cs and locate the Execute method.  Inside the Execute method is a large switch table for all supported instructions.  If you are inheriting from one of the above classes, simply add a new case to the switch statement with an unused number, and for the body write `ex.Exec<MyNewNode.Instruction>();`.  This is unfortunately required in order to support the Unity Job system.  I hope that in the future this step can be skipped by using code generation to perform it automatically.
