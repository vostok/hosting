namespace Vostok.Hosting.Components
{
    internal interface IBuilder<out T>
    {
        T Build(Context context);
    }
}