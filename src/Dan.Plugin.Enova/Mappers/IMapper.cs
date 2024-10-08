namespace Dan.Plugin.Enova.Mappers;

public interface IMapper<in T, out TU>
    where T : class
    where TU : class
{
    TU Map(T input);
}
