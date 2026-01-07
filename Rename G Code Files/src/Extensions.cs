namespace Rename_G_Code_Files.src;

public static class Extensions
{
    extension<T,R>(T self)
    {
        /// <summary>
        /// Pipe operator. Forward the value on the left, to the function on the right.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static R operator >>>(T value, Func<T,R> pipe) => pipe(value);
    }

    /// <summary>
    /// Tuple extensions.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <param name=""></param>
    extension<T1,T2,R>((T1,T2) self)
    {
        /// <summary>
        /// Pipe operator. Forward the value on the left, to the function on the right.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static R operator >>>((T1,T2) value, Func<(T1,T2),R> pipe) => pipe(value);
    }
}
