namespace app.interfaces
{
    public interface IServicesManager<TService>
    {
        TService Service { get; }
    }
}
