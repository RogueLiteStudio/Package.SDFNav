using UnityEngine;
namespace SDFNav
{
    public interface ISDF
    {
        int Width { get; }
        int Height { get; }
        float Grain { get; }
        float Scale { get; }
        Vector2 Origin { get; }
        float Sample(Vector2 pos);
        float this[int idx] { get; }
    }
}