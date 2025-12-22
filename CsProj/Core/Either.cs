using System.Diagnostics.CodeAnalysis;

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

    public bool IsSuccess([MaybeNullWhen(false)] out TSuccess? success)
    {
        if (_isSuccess)
        {
            success = _success;
            return true;
        }
        success = default;
        return false;
    }

    public bool IsFailure([MaybeNullWhen(false)] out TFailure? failure)
    {
        if (!_isSuccess)
        {
            failure = _failure;
            return true;
        }
        failure = default;
        return false;
    }
}