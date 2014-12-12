using System;

namespace SharpTools.Functional
{
    /// <summary>
    /// This class provides helpers for composing and transforming functions.
    /// </summary>
    public static class Function
    {
        /// <summary>
        /// Applies an argument to a function of arity 1, producing a function
        /// that takes no arguments.
        /// </summary>
        /// <typeparam name="T">The type of the argument to apply</typeparam>
        /// <typeparam name="U">The type of the result produced by the applied function</typeparam>
        /// <param name="f">The function to apply to</param>
        /// <param name="arg">The argument to apply</param>
        /// <returns>A function with it's argument pre-applied</returns>
        public static Func<U> Apply<T, U>(this Func<T, U> f, T arg)
        {
            return () => f(arg);
        }

        /// <summary>
        /// Partially applies an argument to a function which takes two arguments.
        /// Example:
        /// 
        ///     var multiply      = (x, factor) => x * factor
        ///     var square        = (y) => y * y
        ///     var squaredDouble = Compose(Apply(multiply, 2), square);
        ///     var result        = squaredDouble(4); # 256, or (4*4)^2
        /// </summary>
        /// <typeparam name="T">The type of the input argument to the produced function</typeparam>
        /// <typeparam name="U">The type of the argument to apply to the input function</typeparam>
        /// <typeparam name="V">The type of the produced function's result</typeparam>
        /// <param name="f">The function to partially apply an argument to</param>
        /// <param name="arg">The argument to apply</param>
        /// <returns>A new function with the argument provided pre-applied.</returns>
        public static Func<T, V> Apply<T, U, V>(this Func<T, U, V> f, U arg)
        {
            return (t) => f(t, arg);
        }

        /// <summary>
        /// Partially applies an argument to a function which takes three arguments.
        /// 
        /// Given the function `f(w, x, y) -> z`, calling `apply(f, y)` results in a new
        /// function `g(w, x) -> z`, where `y` is pre-applied, such that calling `g(w, x)`
        /// is equivalent to calling `f(w, x, y)`.
        /// </summary>
        /// <typeparam name="T">The type of the first argument</typeparam>
        /// <typeparam name="U">The type of the second argument</typeparam>
        /// <typeparam name="V">The type of the applied argument</typeparam>
        /// <typeparam name="W">The type of the result</typeparam>
        /// <param name="f">The function to apply the argument to</param>
        /// <param name="arg">The argument to apply</param>
        /// <returns>A new function with the argument provided pre-applied</returns>
        public static Func<T, U, W> Apply<T, U, V, W>(this Func<T, U, V, W> f, V arg)
        {
            return (t, u) => f(t, u, arg);
        }

        /// <summary>
        /// Applies the result of this function to a void-returning function or lambda.
        /// </summary>
        /// <typeparam name="T">The type of the value being applied.</typeparam>
        /// <param name="value">The function which will produce the value to apply</param>
        /// <param name="receiver">The function to apply to</param>
        public static void Then<T>(this Func<T> value, Action<T> target)
        {
            target(value());
        }

        /// <summary>
        /// Given a function `f(x) -> y`, the identity function will return that same function.
        /// This has it's uses in functional programming, particularly in regards to monads.
        /// </summary>
        /// <typeparam name="T">The type of the input argument</typeparam>
        /// <typeparam name="U">The type of the result</typeparam>
        /// <param name="f">The function to get the identity of</param>
        /// <returns>The identity of this function</returns>
        public static Func<T, U> Identity<T, U>(Func<T, U> f)
        {
            return f;
        }

        /// <summary>
        /// This function will take two functions `f(x) -> y`, and `g(y) -> z`, and
        /// return the function `h(x) -> z`, which is equivalent to `g(f(x)) -> z`.
        /// An example:
        /// 
        ///     var double        = (x) => x * 2
        ///     var square        = (y) => y * y
        ///     var squaredDouble = Compose(double, square);
        ///     var result        = squaredDouble(2); # 16, or (2*2)^2
        ///
        /// This is very useful when you have functions which are good building blocks,
        /// and you wish to compose them together to form more complex behavior.
        /// </summary>
        /// <typeparam name="T">The type of the input argument of the first function</typeparam>
        /// <typeparam name="U">The type of the input argument of the second function</typeparam>
        /// <typeparam name="V">The type of the result of the second function</typeparam>
        /// <param name="f">The function being composed on</param>
        /// <param name="g">The function which will transform the result of `f(x)`</param>
        /// <returns>The composed function, `h(x) -> z`</returns>
        public static Func<T, V> Compose<T, U, V>(this Func<T, U> f, Func<U, V> g)
        {
            return (x) => g(f(x));
        }
    }
}
