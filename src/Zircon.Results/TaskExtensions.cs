namespace Zircon.Results;

public static class TaskExtensions
{
    public static async Task<Result<T>> ToResultAsync<T>(this Task<T?> task) where T : class
    {
        try
        {
            var result = await task;
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }

    public static async Task<Result> ToResultAsync(this Task apiTask)
    {
        try
        {
            await apiTask;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }
    }
}
