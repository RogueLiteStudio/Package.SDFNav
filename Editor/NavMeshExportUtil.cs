using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SDFNav.Editor
{
    public static class NavMeshExportUtil
    {
        private static void ReplaceIndice(int[] indices, int value, int newIdx)
        {
            for (int i=0; i<indices.Length; ++i)
            {
                if (indices[i] == value)
                    indices[i] = newIdx;
            }
        }
        private static Vector3[] cornersCaches = new Vector3[8];
        public static Bounds TransformBounds(Bounds localBounds, Matrix4x4 localToWorldMatrix)
        {
            // 计算Bounds的八个角点在世界坐标中的位置
            Vector3 center = localToWorldMatrix.MultiplyPoint3x4(localBounds.center);
            Vector3 extents = localBounds.extents;
            cornersCaches[0] = center + localToWorldMatrix.MultiplyVector(new Vector3(extents.x, extents.y, extents.z));
            cornersCaches[1] = center + localToWorldMatrix.MultiplyVector(new Vector3(-extents.x, extents.y, extents.z));
            cornersCaches[2] = center + localToWorldMatrix.MultiplyVector(new Vector3(extents.x, -extents.y, extents.z));
            cornersCaches[3] = center + localToWorldMatrix.MultiplyVector(new Vector3(-extents.x, -extents.y, extents.z));
            cornersCaches[4] = center + localToWorldMatrix.MultiplyVector(new Vector3(extents.x, extents.y, -extents.z));
            cornersCaches[5] = center + localToWorldMatrix.MultiplyVector(new Vector3(-extents.x, extents.y, -extents.z));
            cornersCaches[6] = center + localToWorldMatrix.MultiplyVector(new Vector3(extents.x, -extents.y, -extents.z));
            cornersCaches[7] = center + localToWorldMatrix.MultiplyVector(new Vector3(-extents.x, -extents.y, -extents.z));

            // 初始化新的Bounds
            Bounds worldBounds = new Bounds(cornersCaches[0], Vector3.zero);
            foreach (Vector3 corner in cornersCaches)
            {
                worldBounds.Encapsulate(corner);
            }

            return worldBounds;
        }

        public static Bounds CalculateBounds(List<NavMeshBuildSource> sources)
        {
            Bounds bounds = new Bounds();
            foreach (var source in sources)
            {
                switch (source.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                        if (source.sourceObject is Mesh mesh)
                        {
                            var b = TransformBounds(mesh.bounds, source.transform);
                            if (bounds.size == Vector3.zero)
                                bounds = b;
                            else
                                bounds.Encapsulate(b);
                        }
                	    break;
                }
            }
            return bounds;
        }

        public static List<NavMeshBuildSource> BuildSources(GameObject root, System.Func<GameObject, bool> checkFunc)
        {
            var meshFilters = root.GetComponentsInChildren<MeshFilter>();
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            foreach (var filter in meshFilters)
            {
                if (checkFunc ==null || checkFunc(filter.gameObject))
                {
                    var source = new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Mesh,
                        sourceObject = filter.sharedMesh,
                        transform = filter.transform.localToWorldMatrix,
                        component = filter,
                        area = 0,
                    };
                    sources.Add(source);
                }
            }
            return sources;
        }

        public static NavMeshTriangulation BuildNavMeshData(List<NavMeshBuildSource> sources, Bounds bounds, NavMeshBuildSettings settings)
        {
            NavMeshData navmesh = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
            NavMesh.RemoveAllNavMeshData();
            NavMesh.AddNavMeshData(navmesh);
            NavMeshBuilder.UpdateNavMeshData(navmesh, settings, sources, bounds);
            var triangulation = NavMesh.CalculateTriangulation();
            NavMesh.RemoveAllNavMeshData();
            return triangulation;
        }

        //重新对navmesh的数据进行梳理：1 过滤掉距离过小的三角形，把顶点合并成一个 2：过滤掉平均高度超过指定数值的三角形
        public static MeshData NavToMesh(NavMeshTriangulation triangulation,  float reachHeight = 0.5f)
        {
            triangulation = DeDuplicate(triangulation);
            int triangleCount = triangulation.indices.Length / 3;
            List<TriangleIndice> triangles = new List<TriangleIndice>(triangleCount);
            for (int i=0; i<triangleCount; ++i)
            {
                int startIndex = i * 3;
                var triangle = new TriangleIndice
                {
                    A = triangulation.indices[startIndex],
                    B = triangulation.indices[startIndex + 1],
                    C = triangulation.indices[startIndex + 2],
                };
                Vector3 a = triangulation.vertices[triangle.A];
                Vector3 b = triangulation.vertices[triangle.B];
                Vector3 c = triangulation.vertices[triangle.C];
                Vector3 center = SDFNavEditorUtil.GetTriangleCenter(a, b, c);
                //过滤掉空中的三角形
                if (center.y > reachHeight)
                    continue;
                triangles.Add(triangle);
            }
            var mesh = new MeshData
            {
                Vertices = triangulation.vertices,
                Triangles = triangles.ToArray(),
            };
            return mesh;
        }
        //去除重复的点，重新构建Mesh。重复点的定义：距离过小（0.001以内，就会被认为是重复节点，最前面的节点会替换掉后面的节点）
        public static NavMeshTriangulation DeDuplicate(NavMeshTriangulation triangulation)
        {
            HashSet<int> duplicate = new HashSet<int>();
            int[] indices = (int[])triangulation.indices.Clone();
            for (int i = 0; i < triangulation.vertices.Length - 1; ++i)
            {
                if (duplicate.Contains(i))
                    continue;
                Vector3 p = triangulation.vertices[i];
                for (int j = 1; j < triangulation.vertices.Length; ++j)
                {
                    if (i == j || duplicate.Contains(j))
                        continue;
                    Vector3 diff = p - triangulation.vertices[j];
                    if (diff.sqrMagnitude < 0.001)
                    {
                        duplicate.Add(j);
                        //将重复的点索引替换掉
                        ReplaceIndice(indices, j, i);
                    }
                }
            }
            NavMeshTriangulation newmesh = new NavMeshTriangulation();
            //去除重复的点
            List<Vector3> vertices = new List<Vector3>(triangulation.vertices);
            for (int i=vertices.Count-1; i>=0; --i)
            {
                if (duplicate.Contains(i))
                {
                    for (int j=0;j< indices.Length; ++j)
                    {
                        int idx = indices[j];
                        if (idx > i)
                            indices[j] = idx - 1;
                    }
                    vertices.RemoveAt(i);
                }
            }
            newmesh.vertices = vertices.ToArray();
            newmesh.indices = indices;
            newmesh.areas = triangulation.areas;
            return newmesh;
        }
        //获取子网格（navmesh有可能会生成多个网格，他们互相是不相连的，这里通过submesh的概念，把这些网格分开并保存在不同的bubmeshdata中)
        public static List<SubMeshData> SplitSubMesh(MeshData mesh)
        {
            List<SubMeshData> subMeshs = new List<SubMeshData>();
            //过滤的三角形索引
            HashSet<int> filterTriangles = new HashSet<int>();
            for (int i=0; i< mesh.Triangles.Length; ++i)
            {
                if (filterTriangles.Contains(i))
                    continue;
                filterTriangles.Add(i);
                var subMesh = new SubMeshData { Mesh = mesh };
                subMeshs.Add(subMesh);
                subMesh.TriangleIndices.Add(i);
                BuildSubMesh(subMesh, filterTriangles);
            }
            return subMeshs;
        }
        //这里在做一个搜索，把所有相连的顶点划分到同一个子网格中
        public static void BuildSubMesh(SubMeshData subMesh, HashSet<int> filterTriangles)
        {
            int index = 0;
            var mesh = subMesh.Mesh;
            while(index < subMesh.TriangleIndices.Count)
            {
                var triangle = mesh.Triangles[subMesh.TriangleIndices[index]];
                for (int j = 0; j < mesh.Triangles.Length; ++j)
                {
                    if (filterTriangles.Contains(j))
                        continue;
                    var testTriangle = mesh.Triangles[j];
                    if (IsTriangleConnect(triangle, testTriangle))
                    {
                        filterTriangles.Add(j);
                        //添加到三角形列表中，在上层循环中继续判断和这个三角形相邻的三角形
                        subMesh.TriangleIndices.Add(j);
                    }
                }
                ++index;
            }
        }

        public static SubMeshData SelectMaxAreaSubMesh(List<SubMeshData> subMeshs)
        {
            float maxArea = 0;
            SubMeshData subMesh = null;
            foreach (var s in subMeshs)
            {
                float area = CalcSubMeshArea(s);
                if (area > maxArea)
                {
                    maxArea = area;
                    subMesh = s;
                }
            }
            return subMesh;
        }

        public static SubMeshData SelectSubMeshByPoint(List<SubMeshData> subMeshs, Vector2 point)
        {
            foreach (var s in subMeshs)
            {
                if (SDFNavEditorUtil.IsPointInSubMesh(point, s))
                    return s;
            }
            return null;
        }

        public static float CalcSubMeshArea(SubMeshData subMesh)
        {
            var mesh = subMesh.Mesh;
            float area = 0;
            foreach (var idx in subMesh.TriangleIndices)
            {
                var t = mesh.Triangles[idx];
                
                float a = Vector3.Distance(mesh.Vertices[t.A], mesh.Vertices[t.B]);
                float b = Vector3.Distance(mesh.Vertices[t.C], mesh.Vertices[t.B]);
                float c = Vector3.Distance(mesh.Vertices[t.A], mesh.Vertices[t.C]);

                float p = (a + b + c) / 2;//半周长
                area += Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));//海伦公式计算三角形面积
            }
            return area;
        }


        public static bool IsTriangleConnect(TriangleIndice a, TriangleIndice b)
        {
            int count = 0;
            if (b.IsVertice(a.A))
                ++count;
            if (b.IsVertice(a.B))
                ++count;
            if (b.IsVertice(a.C))
                ++count;
            return count >= 2;
        }

    }
}