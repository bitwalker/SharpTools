using System;

namespace SharpTools.Configuration.Attributes
{
    /// <summary>
    /// Mark a configuration class or property with this attribute to
    /// encrypt it's value when the configuration is persisted. When
    /// a class is decorated with this attribute, all properties will have
    /// their values encrypted. IMPORTANT: This attribute is only valid on
    /// string-typed properties. Other types will result in an exception!
    ///
    /// NOTE: Ensure GetEncryptionKey is overridden, or attempting to read or
    /// write the configuration will fail!
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EncryptAttribute : Attribute
    {
    }
}
