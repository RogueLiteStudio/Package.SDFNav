using TrueSync;
namespace TSDFNav
{
    public interface ISDF
    {
        int Width { get; }
        int Height { get; }
        TFloat Grain { get; }
        TFloat Scale { get; }
        TVector2 Origin { get; }
        TFloat Sample(TVector2 pos);
        TFloat this[int idx] { get; }
    }
}