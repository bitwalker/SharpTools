using System;

namespace SharpTools.Testing.EntityFramework.Internal
{
    /// <summary>
    /// The interface of an entity identity type.
    /// </summary>
    internal interface IIdentityType
    {
        /// <summary>
        /// The type of which the id property is.
        /// </summary>
        Type UnderlyingClrType { get; }

        /// <summary>
        /// A method used to convert to string an identity value of <see cref="UnderlyingClrType"/> type.
        /// </summary>
        /// <param name="o">The identity value.</param>
        /// <returns>A string representation of <paramref name="o"/>.</returns>
        string ToString(object o);

        /// <summary>
        /// A method conveting a string representation of an id into the id.
        /// </summary>
        /// <param name="id">The string representation of an id.</param>
        /// <returns>The object of <see cref="UnderlyingClrType"/> type.</returns>
        object FromString(string id);

        /// <summary>
        /// Determine if the value for this id is equivalent to an uninitialized value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsUninitializedValue(object id);
    }
}
