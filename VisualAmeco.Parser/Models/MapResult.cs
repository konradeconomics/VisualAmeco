namespace VisualAmeco.Parser.Models;

public class MapResult<T>
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public T? Value { get; }

    private MapResult(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private MapResult(string error)
    {
        IsSuccess = false;
        ErrorMessage = error;
    }

    public static MapResult<T> Success(T value) => new(value);
    public static MapResult<T> Fail(string message) => new(message);
}