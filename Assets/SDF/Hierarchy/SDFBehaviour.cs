using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace SDF.Hierarchy {

    public class SDFBehaviour : MonoBehaviour {
        public const string NODE_TYPE_PROPERTY = nameof(Type);

        public NodeType Type;

        [DisplayFor(NodeType.UnionChamfer, NodeType.UnionColumns, NodeType.UnionRound, NodeType.UnionStairs,
                    NodeType.IntersectionChamfer, NodeType.IntersectionColumns, NodeType.IntersectionRound, NodeType.IntersectionStairs,
                    NodeType.DifferenceChamfer, NodeType.DifferenceColumns, NodeType.DifferenceRound, NodeType.DifferenceStairs,
                    NodeType.Engrave, NodeType.Groove, NodeType.Pipe, NodeType.Tongue,
                    NodeType.Capsule, NodeType.Torus)]
        public float RadiusA;

        [DisplayFor(NodeType.Groove, NodeType.Tongue, NodeType.Torus)]
        public float RadiusB;

        [DisplayFor(NodeType.UnionColumns, NodeType.UnionStairs,
                    NodeType.IntersectionColumns, NodeType.IntersectionStairs,
                     NodeType.DifferenceColumns, NodeType.DifferenceStairs)]
        public int N = 3;

        [DisplayFor(NodeType.Offset)]
        public float Offset;

        [DisplayFor(NodeType.ModSimple)]
        public float CellSize;
        [DisplayFor(NodeType.ModSimple)]
        public bool X = true, Y = true, Z = true;

        [DisplayFor(NodeType.Rotate45)]
        [Range(0, 2)]
        public int Axis;

        private SDFNode _cachedNode;

        public SDFNode GetNode() {

            SDFNode node = GenerateNode();

            node.ClearChildren();
            linkUpChildren(node, transform);

            return node;
        }

        private static List<SDFBehaviour> _tmpList = new List<SDFBehaviour>();
        private void linkUpChildren(SDFNode node, Transform t) {
            for (int i = 0; i < t.childCount; i++) {
                var child = t.GetChild(i);
                if (!child.gameObject.activeInHierarchy) {
                    continue;
                }

                t.GetChild(i).GetComponents(_tmpList);

                bool foundChild = false;
                for (int j = 0; j < _tmpList.Count; j++) {
                    if (_tmpList[j].isActiveAndEnabled) {
                        node.Add(_tmpList[j].GetNode());
                        foundChild = true;
                        break;
                    }
                }

                if (foundChild) {
                    continue;
                }

                linkUpChildren(node, t.GetChild(i));
            }
        }

        private SDFNode GenerateNode() {
            _cachedNode = GetNodeFromEnum();

            float4x4 toLocal = math.inverse(float4x4.TRS(transform.position, transform.rotation, Vector3.one));
            float xLength = transform.TransformVector(Vector3.right).magnitude;
            float yLength = transform.TransformVector(Vector3.up).magnitude;
            float zLength = transform.TransformVector(Vector3.forward).magnitude;
            float3 size = new float3(xLength, yLength, zLength);
            float3 extent = size / 2;

            if (_cachedNode is Box box) {
                box.ToLocalSpace = toLocal;
                box.Extents = extent;
            } else if (_cachedNode is Capsule capsule) {
                capsule.PointA = transform.TransformPoint(Vector3.up);
                capsule.PointB = transform.TransformPoint(Vector3.down);
                capsule.Radius = RadiusA;
            } else if (_cachedNode is Cone cone) {
                cone.ToLocalSpace = toLocal;
                cone.Height = extent.y;
                cone.Radius = math.length(extent.xz);
            } else if (_cachedNode is Cylinder cylinder) {
                cylinder.ToLocalSpace = toLocal;
                cylinder.Height = extent.y;
                cylinder.Radius = math.length(extent.xz);
            } else if (_cachedNode is Plane plane) {
                UnityEngine.Plane p = new UnityEngine.Plane(transform.up, transform.position);
                plane.Normal = p.normal;
                plane.DistFromOrigin = p.distance;
            } else if (_cachedNode is Sphere sphere) {
                sphere.Center = transform.position;
                sphere.Radius = math.length(extent.xyz);
            } else if (_cachedNode is Torus torus) {
                torus.ToLocalSpace = toLocal;
                torus.SmallRadius = RadiusA * math.length(extent);
                torus.LargeRadius = RadiusB * math.length(extent);
            } else if (_cachedNode is IChamferOp chamfer) {
                chamfer.Radius = RadiusA;
            } else if (_cachedNode is IColumnsOp columns) {
                columns.Radius = RadiusA;
                columns.N = N;
            } else if (_cachedNode is IRoundOp round) {
                round.Radius = RadiusA;
            } else if (_cachedNode is IStairsOp stairs) {
                stairs.Radius = RadiusA;
                stairs.N = N;
            } else if (_cachedNode is Engrave engrave) {
                engrave.Radius = RadiusA;
            } else if (_cachedNode is Groove groove) {
                groove.RadiusA = RadiusA;
                groove.RadiusB = RadiusB;
            } else if (_cachedNode is Pipe pipe) {
                pipe.Radius = RadiusA;
            } else if (_cachedNode is Tongue tongue) {
                tongue.RadiusA = RadiusA;
                tongue.RadiusB = RadiusB;
            } else if (_cachedNode is Offset offset) {
                offset.Value = Offset;
            } else if (_cachedNode is ModSimple modSimple) {
                modSimple.CellSize = CellSize;
                modSimple.ModX = X;
                modSimple.ModY = Y;
                modSimple.ModZ = Z;
            } else if (_cachedNode is Rotate45 rot45) {
                rot45.Axis = Axis;
            }

            return _cachedNode;
        }

        private SDFNode GetNodeFromEnum() {
            switch (Type) {
                case NodeType.Box: return As<Box>();
                case NodeType.Capsule: return As<Capsule>();
                case NodeType.Cone: return As<Cone>();
                case NodeType.Cylinder: return As<Cylinder>();
                case NodeType.Plane: return As<Plane>();
                case NodeType.Sphere: return As<Sphere>();
                case NodeType.Torus: return As<Torus>();

                case NodeType.Union: return As<Union>();
                case NodeType.UnionChamfer: return As<UnionChamfer>();
                case NodeType.UnionColumns: return As<UnionColumns>();
                case NodeType.UnionRound: return As<UnionRound>();
                case NodeType.UnionStairs: return As<UnionStairs>();

                case NodeType.Intersection: return As<Intersection>();
                case NodeType.IntersectionChamfer: return As<IntersectionChamfer>();
                case NodeType.IntersectionColumns: return As<IntersectionColumns>();
                case NodeType.IntersectionRound: return As<IntersectionRound>();
                case NodeType.IntersectionStairs: return As<IntersectionStairs>();

                case NodeType.Difference: return As<Difference>();
                case NodeType.DifferenceChamfer: return As<DifferenceChamfer>();
                case NodeType.DifferenceColumns: return As<DifferenceColumns>();
                case NodeType.DifferenceRound: return As<DifferenceRound>();
                case NodeType.DifferenceStairs: return As<DifferenceStairs>();

                case NodeType.Engrave: return As<Engrave>();
                case NodeType.Groove: return As<Groove>();
                case NodeType.Pipe: return As<Pipe>();
                case NodeType.Tongue: return As<Tongue>();

                case NodeType.Inverse: return As<Inverse>();
                case NodeType.Offset: return As<Offset>();

                case NodeType.ModSimple: return As<ModSimple>();
                case NodeType.Rotate45: return As<Rotate45>();
                default:
                    throw new Exception();
            }
        }

        private T As<T>() where T : class, new() {
            if (_cachedNode is T) {
                return _cachedNode as T;
            } else {
                return new T();
            }
        }

        public enum NodeType {
            [Category("Shape")] Box,
            [Category("Shape")] Capsule,
            [Category("Shape")] Cone,
            [Category("Shape")] Cylinder,
            [Category("Shape")] Plane,
            [Category("Shape")] Sphere,
            [Category("Shape")] Torus,

            [Category("Binary")] Union,
            [Category("Binary")] UnionChamfer,
            [Category("Binary")] UnionColumns,
            [Category("Binary")] UnionRound,
            [Category("Binary")] UnionStairs,

            [Category("Binary")] Intersection,
            [Category("Binary")] IntersectionChamfer,
            [Category("Binary")] IntersectionColumns,
            [Category("Binary")] IntersectionRound,
            [Category("Binary")] IntersectionStairs,

            [Category("Binary")] Difference,
            [Category("Binary")] DifferenceChamfer,
            [Category("Binary")] DifferenceColumns,
            [Category("Binary")] DifferenceRound,
            [Category("Binary")] DifferenceStairs,

            [Category("Binary")] Engrave,
            [Category("Binary")] Groove,
            [Category("Binary")] Pipe,
            [Category("Binary")] Tongue,

            [Category("Unary")] Inverse,
            [Category("Unary")] Offset,

            [Category("Domain")] ModSimple,
            [Category("Domain")] Rotate45
        }

        public class DisplayForAttribute : Attribute {
            public readonly NodeType[] Types;

            public DisplayForAttribute(params NodeType[] Types) {
                this.Types = Types;
            }
        }

        public class CategoryAttribute : Attribute {
            public readonly string Category;

            public CategoryAttribute(string Category) {
                this.Category = Category;
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.white;
            switch (Type) {
                case NodeType.Sphere:
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
                    break;
                case NodeType.Box:
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                    break;
            }
        }
    }
}
