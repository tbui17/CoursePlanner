export function decorate<T extends (...args: any) => any>(
  fn: T,
  decorator: (fn: T, ...args: Parameters<T>) => ReturnType<T>
) {
  return decorator.bind(null, fn)
}

export function withTryCatch<T extends (...args: any) => any, R>(
  fn: T,
  onError: (error: unknown) => R
) {
  return decorate(fn, (fn, ...args) => {
    try {
      return fn(...args)
    } catch (error) {
      return onError(error)
    }
  })
}
