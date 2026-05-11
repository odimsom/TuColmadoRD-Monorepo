export class OperationResult<T, E> {
  private readonly _ok: boolean;
  private readonly _value: T | undefined;
  private readonly _error: E | undefined;

  private constructor(ok: boolean, value?: T, error?: E) {
    this._ok = ok;
    this._value = value;
    this._error = error;
  }

  static ok<T, E>(value: T): OperationResult<T, E> {
    return new OperationResult<T, E>(true, value, undefined);
  }

  static fail<T, E>(error: E): OperationResult<T, E> {
    return new OperationResult<T, E>(false, undefined, error);
  }

  get isOk(): boolean {
    return this._ok;
  }

  get value(): T {
    if (!this._ok) throw new TypeError('Cannot read value of a failed OperationResult.');
    return this._value as T;
  }

  get error(): E {
    if (this._ok) throw new TypeError('Cannot read error of a successful OperationResult.');
    return this._error as E;
  }
}
