using UnityEngine;
namespace SDFNav
{
    public class SDFDebugUtil
    {
        public static Texture2D ToTexture(SDFData sdf)
        {
            Texture2D texture = new Texture2D(sdf.Width, sdf.Height);
            for (int i = 0; i < sdf.Width; ++i)
            {
                for (int j = 0; j < sdf.Height; ++j)
                {
                    short val = sdf[i, j];
                    if (val <= 0)
                    {
                        texture.SetPixel(i, j, new Color(1, 0, 0, 0.1f));
                    }
                    else
                    {
                        texture.SetPixel(i, j, new Color(0, 1, 0, 0.2f));
                    }
                }
            }
            texture.Apply();
            return texture;
        }

        public static Mesh ToMesh(SDFData data)
        {
            Vector3 origin = new Vector3(data.Origin.x, 0, data.Origin.y);
            float width = data.Width * data.Grain;
            float height = data.Height * data.Grain;
            var mesh = new Mesh
            {
                hideFlags = HideFlags.HideAndDontSave,
                vertices = new Vector3[]
                {
                    origin,
                    origin + new Vector3(0, 0, height),
                    origin + new Vector3(width, 0, height),
                    origin + new Vector3(width, 0, 0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                },
                triangles = new int[]
                {
                    0, 1, 2,
                    2, 3, 0,
                }
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        public static Material CreateSDFMaterial(Texture2D texture)
        {
            var shader = Shader.Find("SDFPreview");
            if (shader)
            {
                var mat = new Material(shader);
                mat.SetTexture("_MainTex", texture);
                mat.doubleSidedGI = true;
                mat.renderQueue = 3000;
                return mat;
            }
            return null;
        }

        public static GameObject CreateVisableObj(SDFData data)
        {
            GameObject go = new GameObject("SDF_Visable");
            Mesh mesh = ToMesh(data);
            Texture2D texture = ToTexture(data);
            var mat = CreateSDFMaterial(texture);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var render = go.AddComponent<MeshRenderer>();
            render.material = mat;
            return go;
        }
    }
}