using System.Linq;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using SDF;
using SDF.Hierarchy;

public class ExampleMesher : MonoBehaviour {
  
  [Range(4, 64)]
  public int Resolution = 16;
  public float GridSize = 0.1f;
  public int Chunks = 2;
  public Material previewMat;

  private float _resultDist;
  private Mesh _mesh;

  private void OnValidate() {
    Resolution = (Resolution / 4) * 4;
    _mesh = new Mesh();
  }

  private void Update() {
    List<Transform> children = new List<Transform>();
    foreach (var child in transform) {
      children.Add(child as Transform);
    }

    var sdf = GetComponent<SDFBehaviour>().GetNode().Compile();

    var pairArrays = new NativeArray<KeyValuePair<int3, float3>>[Chunks * Chunks];
    for (int i = 0; i < pairArrays.Length; i++) {
      pairArrays[i] = new NativeArray<KeyValuePair<int3, float3>>(Resolution * Resolution * Resolution, Allocator.TempJob);
    }

    NativeArray<JobHandle> handles = new NativeArray<JobHandle>(Chunks * Chunks, Allocator.TempJob);

    var counts = new NativeArray<int>(Chunks * Chunks, Allocator.TempJob);
    var offsets = new NativeArray<int>(Chunks * Chunks, Allocator.TempJob);

    float size = (Resolution - 1) * GridSize;

    //STEP 1
    {
      int pairIndex = 0;
      for (int dx = 0; dx < Chunks; dx++) {
        for (int dz = 0; dz < Chunks; dz++) {
          handles[pairIndex] = new BuildVertices() {
            Resolution = Resolution,
            GridSize = GridSize,
            GridCorner = transform.position + new Vector3(dx, 0, dz) * size,
            SDF = sdf,
            Vertices = pairArrays[pairIndex],
            Counts = counts,
            CountIndex = pairIndex,
            CellOffset = new int3(dx * (Resolution - 1), 0, dz * (Resolution - 1))
          }.Schedule();

          pairIndex++;
        }
      }

      JobHandle.CompleteAll(handles);
    }

    //STEP 2
    {
      new CalculateOffsets() {
        Counts = counts,
        Offsets = offsets
      }.Run();
    }

    int totalVerts = 0;
    for (int i = 0; i < counts.Length; i++) {
      totalVerts += counts[i];
    }

    var verts = new NativeArray<Vector3>(totalVerts, Allocator.TempJob);
    var map = new NativeHashMap<int3, int>(totalVerts, Allocator.TempJob);

    //STEP 3
    {
      int pairIndex = 0;
      for (int dx = 0; dx < Chunks; dx++) {
        for (int dz = 0; dz < Chunks; dz++) {

          new BuildVertexArrayAndMap() {
            InVertices = pairArrays[pairIndex],
            Counts = counts,
            Offsets = offsets,
            CountIndex = pairIndex,
            Vertices = verts,
            VertexMap = map.ToConcurrent()
          }.Schedule().Complete();

          pairIndex++;
        }
      }
    }

    var triQueue = new NativeQueue<int3>(Allocator.TempJob);

    //STEP 4
    {
      int pairIndex = 0;
      for (int dx = 0; dx < Chunks; dx++) {
        for (int dz = 0; dz < Chunks; dz++) {
          new BuildTriQueue() {
            InVertices = pairArrays[pairIndex],
            Counts = counts,
            CountIndex = pairIndex,
            TriQueue = triQueue.ToConcurrent(),
            VertexMap = map,

            SDF = sdf,
            GridCorner = transform.position,
            GridSize = GridSize
          }.Schedule().Complete();

          pairIndex++;
        }
      }
    }

    var triArray = new NativeArray<int>(triQueue.Count * 3, Allocator.TempJob);

    //STEP 5
    new BuildTriArray() {
      TriQueue = triQueue,
      Triangles = triArray
    }.Schedule().Complete();

    _mesh.Clear();
    _mesh.vertices = verts.ToArray();
    _mesh.triangles = triArray.ToArray();
    _mesh.RecalculateNormals();
    Graphics.DrawMesh(_mesh, Matrix4x4.identity, previewMat, 0);

    foreach (var pairArray in pairArrays) {
      pairArray.Dispose();
    }
    counts.Dispose();
    offsets.Dispose();
    sdf.Dispose();
    verts.Dispose();
    map.Dispose();
    triQueue.Dispose();
    triArray.Dispose();
    handles.Dispose();
  }

  private void OnDrawGizmos() {
    Gizmos.color = Color.gray;

    for (int dx = 0; dx < Chunks; dx++) {
      for (int dz = 0; dz < Chunks; dz++) {
        Vector3 size = Vector3.one * (Resolution - 1) * GridSize;
        Gizmos.DrawWireCube(transform.position + size * 0.5f + new Vector3(dx, 0, dz) * size.x, size);
      }
    }
  }

  [BurstCompile]
  public struct BuildVertices : IJob {
    public int Resolution;

    public float GridSize;
    public float3 GridCorner;
    public int3 CellOffset;

    public NativeSDF SDF;

    public NativeArray<KeyValuePair<int3, float3>> Vertices;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<int> Counts;
    public int CountIndex;

    public unsafe void Execute() {
      float* slice0 = allocSlice();
      float* slice1 = allocSlice();

      populateSlice(slice1, 0);

      for (int z = 0; z < Resolution - 1; z++) {
        //Swap rows
        {
          float* tmp = slice0;
          slice0 = slice1;
          slice1 = tmp;
        }

        //Slice1 is now stale, lets refresh it
        populateSlice(slice1, z + 1);

        //Now calculate all verts for this slice
        for (int y = 0; y < Resolution - 1; y++) {
          for (int x = 0; x < Resolution - 1; x++) {
            int3 cell = new int3(x, y, z);

            float3 vertCenter = float3.zero;
            int totalSamples = 0;
            checkEdge(ref vertCenter, ref totalSamples, slice0, cell + new int3(0, 0, 0), cell + new int3(0, 1, 0));
            checkEdge(ref vertCenter, ref totalSamples, slice0, cell + new int3(0, 1, 0), cell + new int3(1, 1, 0));
            checkEdge(ref vertCenter, ref totalSamples, slice0, cell + new int3(1, 1, 0), cell + new int3(1, 0, 0));
            checkEdge(ref vertCenter, ref totalSamples, slice0, cell + new int3(1, 0, 0), cell + new int3(0, 0, 0));

            checkEdge(ref vertCenter, ref totalSamples, slice1, cell + new int3(0, 0, 1), cell + new int3(0, 1, 1));
            checkEdge(ref vertCenter, ref totalSamples, slice1, cell + new int3(0, 1, 1), cell + new int3(1, 1, 1));
            checkEdge(ref vertCenter, ref totalSamples, slice1, cell + new int3(1, 1, 1), cell + new int3(1, 0, 1));
            checkEdge(ref vertCenter, ref totalSamples, slice1, cell + new int3(1, 0, 1), cell + new int3(0, 0, 1));

            checkEdge(ref vertCenter, ref totalSamples, slice0, slice1, cell + new int3(0, 0, 0));
            checkEdge(ref vertCenter, ref totalSamples, slice0, slice1, cell + new int3(0, 1, 0));
            checkEdge(ref vertCenter, ref totalSamples, slice0, slice1, cell + new int3(1, 1, 0));
            checkEdge(ref vertCenter, ref totalSamples, slice0, slice1, cell + new int3(1, 0, 0));

            //Cell is either fully outside or fully inside, don't generate a vertex
            if (totalSamples == 0) {
              continue;
            }

            int count = Counts[CountIndex];
            Vertices[count] = new KeyValuePair<int3, float3>(CellOffset + new int3(x, y, z), (vertCenter / totalSamples) * GridSize + GridCorner);
            Counts[CountIndex] = count + 1;
          }
        }
      }

      UnsafeUtility.Free(slice0, Allocator.Temp);
      UnsafeUtility.Free(slice1, Allocator.Temp);
    }

    private unsafe void checkEdge(ref float3 vertCenter, ref int totalSamples, float* slice, int3 offset0, int3 offset1) {
      int index0 = offset0.x + offset0.y * Resolution;
      int index1 = offset1.x + offset1.y * Resolution;

      float dist0 = slice[index0];
      float dist1 = slice[index1];

      float t = dist0 / (dist0 - dist1);

      if (t >= 0 && t <= 1) {
        vertCenter += math.lerp(offset0, offset1, t);
        totalSamples += 1;
      }
    }

    private unsafe void checkEdge(ref float3 vertCenter, ref int totalSamples, float* slice0, float* slice1, int3 offset0) {
      int index = offset0.x + offset0.y * Resolution;

      float dist0 = slice0[index];
      float dist1 = slice1[index];

      float t = dist0 / (dist0 - dist1);
      if (t >= 0 && t <= 1) {
        vertCenter += math.lerp(offset0, new float3(offset0.x, offset0.y, offset0.z + 1), t);
        totalSamples += 1;
      }
    }

    private unsafe float* allocSlice() {
      return (float*)UnsafeUtility.Malloc(sizeof(float) * Resolution * Resolution,
                                          UnsafeUtility.AlignOf<float>(),
                                          Allocator.Temp);
    }

    private unsafe void populateSlice(float* slice, int sliceIndex) {
      int index = 0;
      float grid4x = GridSize * 4;

      for (int dy = 0; dy < Resolution; dy++) {
        float3 position0 = new float3(0, dy * GridSize, sliceIndex * GridSize) + GridCorner;
        float3 position1 = position0 + new float3(GridSize, 0, 0);
        float3 position2 = position1 + new float3(GridSize, 0, 0);
        float3 position3 = position2 + new float3(GridSize, 0, 0);

        for (int dx = 0; dx < Resolution; dx += 4) {
          float4 distances = SDF.Sample(position0, position1, position2, position3);
          slice[index++] = distances.x;
          slice[index++] = distances.y;
          slice[index++] = distances.z;
          slice[index++] = distances.w;

          position0.x += grid4x;
          position1.x += grid4x;
          position2.x += grid4x;
          position3.x += grid4x;
        }
      }
    }
  }

  [BurstCompile]
  public struct CalculateOffsets : IJob {
    [ReadOnly]
    public NativeArray<int> Counts;
    public NativeArray<int> Offsets;

    public void Execute() {
      int prefixSum = 0;
      for (int i = 0; i < Counts.Length; i++) {
        Offsets[i] = prefixSum;
        prefixSum += Counts[i];
      }
    }
  }

  [BurstCompile]
  public struct BuildVertexArrayAndMap : IJob {
    [ReadOnly]
    public NativeArray<KeyValuePair<int3, float3>> InVertices;
    [ReadOnly]
    public NativeArray<int> Counts;
    [ReadOnly]
    public NativeArray<int> Offsets;
    public int CountIndex;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<Vector3> Vertices;
    [WriteOnly]
    public NativeHashMap<int3, int>.Concurrent VertexMap;

    public void Execute() {
      int count = Counts[CountIndex];
      int offset = Offsets[CountIndex];

      for (int i = 0; i < count; i++) {
        var pair = InVertices[i];

        Vertices[i + offset] = pair.Value;
        VertexMap.TryAdd(pair.Key, i + offset);
      }
    }
  }

  [BurstCompile]
  public struct BuildTriQueue : IJob {
    [ReadOnly]
    public NativeArray<KeyValuePair<int3, float3>> InVertices;
    [ReadOnly]
    public NativeArray<int> Counts;
    public int CountIndex;

    public NativeQueue<int3>.Concurrent TriQueue;

    public NativeSDF SDF;
    public float3 GridCorner;
    public float GridSize;

    [ReadOnly]
    public NativeHashMap<int3, int> VertexMap;

    public void Execute() {
      int count = Counts[CountIndex];
      for (int i = 0; i < count; i++) {
        var key = InVertices[i].Key;

        tryBuildInDirection(key, new int3(1, 0, 0), new int3(0, 1, 0), new int3(0, 0, 1));
        tryBuildInDirection(key, new int3(0, 0, 1), new int3(1, 0, 0), new int3(0, 1, 0));
        tryBuildInDirection(key, new int3(0, 1, 0), new int3(0, 0, 1), new int3(1, 0, 0));
      }
    }

    private void tryBuildInDirection(int3 cell00, int3 dirA, int3 dirB, int3 normal) {
      int3 cell01 = cell00 + dirA;
      int3 cell10 = cell00 + dirB;
      int3 cell11 = cell01 + dirB;

      int index00, index01, index10, index11;

      if (!VertexMap.TryGetValue(cell00, out index00)) return;
      if (!VertexMap.TryGetValue(cell01, out index01)) return;
      if (!VertexMap.TryGetValue(cell10, out index10)) return;
      if (!VertexMap.TryGetValue(cell11, out index11)) return;

      float3 centerPos0 = (float3)cell11 * GridSize + GridCorner;

      float dist0 = SDF.Sample(centerPos0);

      if (dist0 < 0) {
        TriQueue.Enqueue(new int3(index00, index01, index11));
        TriQueue.Enqueue(new int3(index00, index11, index10));
      } else {
        TriQueue.Enqueue(new int3(index01, index00, index11));
        TriQueue.Enqueue(new int3(index11, index00, index10));
      }
    }
  }

  [BurstCompile]
  public struct BuildTriArray : IJob {
    public NativeQueue<int3> TriQueue;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<int> Triangles;

    public void Execute() {
      int count = TriQueue.Count;
      int index = 0;
      for (int i = 0; i < count; i++) {
        int3 tri = TriQueue.Dequeue();

        Triangles[index++] = tri.x;
        Triangles[index++] = tri.y;
        Triangles[index++] = tri.z;
      }
    }
  }
}
