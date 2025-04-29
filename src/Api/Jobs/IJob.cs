namespace Api.Jobs;
public interface IJob
{
    Task ExecuteAsync(params object[] args);
}
