/**
 * Export Result class
 * It manage the Ok, failed results.
 */
export class Result<TReturn = void, TError = any> {
    public isSuccess: boolean;
    public isFailure: boolean;
    public error: TError;
    private _value: TReturn;

    private constructor(isSuccess: boolean, error?: TError, value?: TReturn) {
        if (isSuccess && error) {
            throw new Error(`InvalidOperation: A result cannot be ` +
        `successful and contain an error`);
        }
        if (!isSuccess && !error) {
            throw new Error(`InvalidOperation: A failing result ` +
        `needs to contain an error message`);
        }

        this.isSuccess = isSuccess;
        this.isFailure = !isSuccess;
        this.error = error;
        this._value = value;

        Object.freeze(this);
    }

    public static ok<TReturn = void, TError = any>(value?: TReturn): Result<TReturn, TError> {
        return new Result<TReturn, TError>(true, null, value);
    }

    public static fail<TReturn = void, TError = any>(error: TError): Result<TReturn, TError> {
        return new Result<TReturn, TError>(false, error);
    }

    public static combine(results: Array<Result<any>>): Result<any> {
        for (let result of results) {
            if (result.isFailure) return result;
        }
        return Result.ok<any>();
    }

    public getValue(): TReturn {
        if (!this.isSuccess) {
            throw new Error(`Cant retrieve the value from a failed result.`);
        }

        return this._value;
    }
}
