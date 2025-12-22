namespace CsProj.Core;

internal class Either<TSuccess, TFailure>
{
    private readonly TSuccess? _success;
    private readonly TFailure? _failure;

    private readonly bool _isSuccess;

    public Either(TSuccess success)
    {
        _success = success;
        _failure = default;
        _isSuccess = true;
    }

    public Either(TFailure failure)
    {
        _failure = failure;
        _success = default;
        _isSuccess = false;
    }

    public static implicit operator Either<TSuccess, TFailure>(TSuccess success)
        => new Either<TSuccess, TFailure>(success);

    public static implicit operator Either<TSuccess, TFailure>(TFailure failure)
        => new Either<TSuccess, TFailure>(failure);

    public void OnSuccess(Action<TSuccess> action)
    {
        if (_isSuccess && _success != null)
        {
            action(_success);
        }
    }

    public void OnFailure(Action<TFailure> action)
    {
        if (!_isSuccess && _failure != null)
        {
            action(_failure);
        }
    }
}