using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SDFNav
{
    public static class VoxelSampleUtil
    {
        public struct BoundInt
        {
            public Vector3Int Min;
            public Vector3Int Size;
        }

        public struct MeshInfo
        {
            public Mesh Mesh;
            public Matrix4x4 Matrix;
        }

        public static bool IsPointInTriangle(Vector3 point, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float angleSum = 0f;

            angleSum += Vector3.Angle(v2 - v1, point - v1);
            angleSum += Vector3.Angle(v3 - v2, point - v2);
            angleSum += Vector3.Angle(v1 - v3, point - v3);

            return Mathf.Approximately(angleSum, 360f);
        }

        public static float DistanceToSegment(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector3 segment = end - start;
            float segmentLength = segment.magnitude;
            Vector3 segmentDirection = segment / segmentLength;

            float t = Vector3.Dot(point - start, segmentDirection);

            if (t < 0)
            {
                return Vector3.Distance(point, start);
            }
            else if (t > segmentLength)
            {
                return Vector3.Distance(point, end);
            }
            else
            {
                Vector3 projection = start + t * segmentDirection;
                return Vector3.Distance(point, projection);
            }
        }

        public static float DistanceToTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
        {
            Plane trianglePlane = new Plane(a, b, c);
            float distanceToPlane = trianglePlane.GetDistanceToPoint(point);
            Vector3 projectedPoint = point - distanceToPlane * trianglePlane.normal;

            if (IsPointInTriangle(projectedPoint, a, b, c))
            {
                return Mathf.Abs(distanceToPlane);
            }
            else
            {
                // 如果投影点不在三角形内部，则计算点到三个三角形边的最小距离
                float distance1 = DistanceToSegment(point, a, b);
                float distance2 = DistanceToSegment(point, b, c);
                float distance3 = DistanceToSegment(point, c, a);

                // 返回最小距离
                return Mathf.Min(distance1, Mathf.Min(distance2, distance3));
            }
        }

        public static BoundInt CalcBoundInt(VoxelBox box, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 min = Vector3.Min(Vector3.Min(a, b), c);
            Vector3 max = Vector3.Max(Vector3.Max(a, b), c);
            if (box.ValidBounds != null)
            {
                var bounds = new Bounds();
                bounds.SetMinMax(min, max);
                bool overlap = false;
                for (int i = 0; i < box.ValidBounds.Length; i++)
                {
                    if (box.ValidBounds[i].Intersects(bounds))
                    {
                        overlap = true;
                        break;
                    }
                }
                if (!overlap)
                {
                    return new BoundInt();
                }
            }
            Vector3 size = max - min;
            BoundInt bound = new BoundInt();
            min -= box.OriginPoint;
            bound.Min.x = Mathf.FloorToInt(min.x / box.VoxelSize);
            bound.Min.y = Mathf.FloorToInt(min.y / box.VoxelSize);
            bound.Min.z = Mathf.FloorToInt(min.z / box.VoxelSize);
            bound.Size.x = Mathf.CeilToInt(size.x / box.VoxelSize);
            bound.Size.y = Mathf.CeilToInt(size.y / box.VoxelSize);
            bound.Size.z = Mathf.CeilToInt(size.z / box.VoxelSize);
            if (bound.Min.x >= box.Size.x || bound.Min.y >= box.Size.y || bound.Min.z >= box.Size.z)
            {
                bound.Size = Vector3Int.zero;
                return bound;
            }
            if (bound.Min.x < 0)
            {
                bound.Size.x += bound.Min.x;
                bound.Min.x = 0;
            }
            if (bound.Min.y < 0)
            {
                bound.Size.y += bound.Min.y;
                bound.Min.y = 0;
            }
            if (bound.Min.z < 0)
            {
                bound.Size.z += bound.Min.z;
                bound.Min.z = 0;
            }
            if (bound.Size.x < 0 || bound.Size.y < 0 || bound.Size.z < 0)
            {
                bound.Size = Vector3Int.zero;
                return bound;
            }
            if (bound.Min.x + bound.Size.x > box.Size.x)
            {
                bound.Size.x = box.Size.x - bound.Min.x;
            }
            if (bound.Min.y + bound.Size.y > box.Size.y)
            {
                bound.Size.y = box.Size.y - bound.Min.y;
            }
            if (bound.Min.z + bound.Size.z > box.Size.z)
            {
                bound.Size.z = box.Size.z - bound.Min.z;
            }
            return bound;
        }

        public static void SampleTriangle(VoxelBox box, Vector3 a, Vector3 b, Vector3 c)
        {
            var bound = CalcBoundInt(box, a, b, c);
            if (bound.Size == Vector3Int.zero)
                return;
            float halfVoxelSize = box.VoxelSize * 0.5f;
            float minDistance = box.VoxelSize * 0.7f;
            for (int x = bound.Min.x; x < bound.Min.x + bound.Size.x; x++)
            {
                for (int y = bound.Min.y; y < bound.Min.y + bound.Size.y; y++)
                {
                    BitArray voxel = null;
                    for (int z = bound.Min.z; z < bound.Min.z + bound.Size.z; z++)
                    {
                        Vector3 point = box.OriginPoint + new Vector3(x, y, z) * halfVoxelSize;
                        float distance = DistanceToTriangle(point, a, b, c);
                        if (distance < minDistance)
                        {
                            voxel ??= box.Get(x, y);
                            voxel[z] = true;
                        }
                    }
                }
            }
        }

        public static void SampleMesh(VoxelBox box, Mesh mesh, Matrix4x4 matrix)
        {
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = matrix.MultiplyPoint(vertices[triangles[i]]);
                Vector3 b = matrix.MultiplyPoint(vertices[triangles[i + 1]]);
                Vector3 c = matrix.MultiplyPoint(vertices[triangles[i + 2]]);
                SampleTriangle(box, a, b, c);
            }
        }

        private static List<MeshInfo> ToMeshInfo(IEnumerable<GameObject> roots)
        {
            List<MeshFilter> cache = new List<MeshFilter>();
            List<MeshInfo> results = new List<MeshInfo>();
            foreach (var go in roots)
            {
                cache.Clear();
                go.GetComponentsInChildren(cache);
                foreach (var f in cache)
                {
                    if (f.sharedMesh != null)
                    {
                        results.Add(new MeshInfo()
                        {
                            Mesh = f.sharedMesh,
                            Matrix = f.transform.localToWorldMatrix
                        });
                    }
                }
            }
            return results;
        }

        private static Bounds CalcBounds(Bounds bounds, Matrix4x4 Matrix, Vector3[] vertices, int[] triangles)
        {
            for (int i=0; i<triangles.Length; i+=3)
            {
                var a = Matrix.MultiplyPoint(vertices[triangles[i]]);
                var b = Matrix.MultiplyPoint(vertices[triangles[i + 1]]);
                var c = Matrix.MultiplyPoint(vertices[triangles[i + 2]]);
                var min = Vector3.Min(Vector3.Min(a, b), c);
                var max = Vector3.Max(Vector3.Max(a, b), c);
                var box = new Bounds();
                box.SetMinMax(min, max);
                bounds.Encapsulate(box);
            }
            return bounds;
        }

        public static Bounds CalcBounds(List<MeshInfo> meshs)
        {
            Bounds bounds = new Bounds();
            if (meshs.Count > 0)
            {
                var first = meshs[0];
                var vertices = first.Mesh.vertices;
                var tri = first.Mesh.triangles;

                var a = first.Matrix.MultiplyPoint(vertices[tri[0]]);
                var b = first.Matrix.MultiplyPoint(vertices[tri[1]]);
                var c = first.Matrix.MultiplyPoint(vertices[tri[2]]);
                var min = Vector3.Min(Vector3.Min(a, b), c);
                var max = Vector3.Max(Vector3.Max(a, b), c);
                bounds.SetMinMax(min, max);
                bounds = CalcBounds(bounds, first.Matrix, vertices, tri);
            }
            for (int i=1; i<meshs.Count; ++i)
            {
                var mesh = meshs[i];
                var vertices = mesh.Mesh.vertices;
                var tri = mesh.Mesh.triangles;
                bounds = CalcBounds(bounds, mesh.Matrix, vertices, tri);
            }
            return bounds;
        }

        public static VoxelBox SampleScene(IEnumerable<GameObject> roots, float voxelSize, Bounds[] validBounds)
        {
            var meshs = ToMeshInfo(roots);
            var bounds = CalcBounds(meshs);
            var box = new VoxelBox(bounds, voxelSize, validBounds);
            foreach (var mesh in meshs)
            {
                SampleMesh(box, mesh.Mesh, mesh.Matrix);
            }
            return box;
        }


        public static VoxelBox SampleScene(IEnumerable<GameObject> roots, float voxelSize, Bounds bounds, Bounds[] validBounds)
        {
            var meshs = ToMeshInfo(roots);
            var box = new VoxelBox(bounds, voxelSize, validBounds);
            foreach (var mesh in meshs)
            {
                SampleMesh(box, mesh.Mesh, mesh.Matrix);
            }
            return box;
        }
    }

}