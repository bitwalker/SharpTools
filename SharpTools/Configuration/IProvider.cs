using SharpTools.Configuration.Errors;

namespace SharpTools.Configuration
{
    using SharpTools.Functional;
    /// <summary>
    /// This interface describes the API for classes which serialize/deserialize
    /// instances of IConfiguration. This could be to a app/web.config file, a json
    /// file, a database, and more.
    /// </summary>
    public interface IProvider<T> where T : class, IConfig<T>, new()
    {
        /// <summary>
        /// Determines if the configuration source has been initialized.
        /// </summary>
        /// <returns>Boolean</returns>
        bool IsInitialized();

        /// <summary>
        /// Initializes the configuration source.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Reads configuration from the default source for this provider.
        /// For example, this would load via the current app's web.config/app.config
        /// if using the AppSettingsConfigProvider, or from app.json in the project
        /// root if using the JsonConfigProvider.
        /// </summary>
        /// <returns>A populated configuration instance.</returns>
        T Read();

        /// <summary>
        /// Reads configuration from a specific source represented by the provided
        /// string value. This may be a file path, or a database connection string, etc.
        /// </summary>
        /// <param name="source">A string which represents the configuration source to use.</param>
        /// <returns>A populated configuration instance.</returns>
        T Read(string source);

        /// <summary>
        /// Reads configuration by parsing a string representation of the config, the format
        /// of which is dependent upon the provider. For example, the JsonConfigProvider would
        /// expect a JSON string, but an XmlConfigProvider would expect an XML string.
        /// </summary>
        /// <param name="config">The stringified configuration.</param>
        /// <returns>Either a ParseConfigError, or a populated configuration instance.</returns>
        Either<ParseConfigError<T>, T>  Parse(string config);

        /// <summary>
        /// Saves the configuration to the default source.
        /// </summary>
        /// <param name="config">The configuration to save</param>
        void Save(T config);
        /// <summary>
        /// Saves the configuration to the specified source represented by the provided
        /// string value. This may be a file path, or a database connection string, etc. It
        /// depends upon the provider used.
        /// </summary>
        /// <param name="config">The configuration to save</param>
        /// <param name="source">A string which represents the configuration source to save to</param>
        void Save(T config, string source);

        /// <summary>
        /// Saves the configuration by serializing it as a string, and returning it rather
        /// than persisting it to the backing store. The string representation used is dependent
        /// upon the provider, but some examples of choices you have as an implementor: JSON, XML,
        /// init-style plain text, and CSV (likely the most natural if the backing store is a RDBMS).
        /// </summary>
        /// <param name="config">The configuration to save</param>
        /// <returns>The stringified configuration</returns>
        string Serialize(T config);
    }
}
