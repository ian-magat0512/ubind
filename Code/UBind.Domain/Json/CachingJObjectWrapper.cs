// <copyright file="CachingJObjectWrapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Wrapper for JObject that holds a cached version of the json or JObject when converting to/from a string.
    /// This is needed to ensure we don't continually serialize, then deserialize. It's done once and cached.
    /// </summary>
    public class CachingJObjectWrapper
    {
        /// <summary>
        /// The internally stored string json.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected string internalJson;

        /// <summary>
        /// The internally stored JObject.
        /// </summary>
        protected JObject internalJObject;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingJObjectWrapper"/> class from a JObject.
        /// </summary>
        /// <param name="jObject">The jObject to wrap.</param>
        public CachingJObjectWrapper(JObject jObject)
        {
            this.internalJObject = jObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingJObjectWrapper"/> class from a string json.
        /// </summary>
        /// <param name="json">The json string to use.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        public CachingJObjectWrapper(string json)
        {
            this.internalJson = json;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingJObjectWrapper"/> class.
        /// </summary>
        /// <param name="content">A array of objects.</param>
        public CachingJObjectWrapper(params object[] content)
        {
            this.internalJObject = new JObject(content);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingJObjectWrapper"/> class.
        /// </summary>
        /// <param name="content">An object to represent as a JObject.</param>
        public CachingJObjectWrapper(object content)
        {
            this.internalJObject = new JObject(content);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingJObjectWrapper"/> class.
        /// Used for JSON deserialisation.
        /// </summary>
        protected CachingJObjectWrapper()
        {
        }

        /// <summary>
        /// Gets or sets a string representation of the json object, and stores it for future invokations.
        /// </summary>
        [JsonProperty]

        // System.Text.Json: Exclude this property from serialization
        [System.Text.Json.Serialization.JsonIgnore]
        public string Json
        {
            get
            {
                if (this.internalJson == null)
                {
                    this.internalJson = this.internalJObject?.ToString();
                }

                return this.internalJson;
            }

            set
            {
                this.internalJObject = null;
                this.internalJson = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner JObject, and lazily parses json if necessary.
        /// </summary>
        // System.Text.Json: Exclude this property from serialization
        [System.Text.Json.Serialization.JsonIgnore]
        public JObject JObject
        {
            get
            {
                if (this.internalJObject == null)
                {
                    this.internalJObject = JObject.Parse(this.internalJson);
                }

                return this.internalJObject;
            }

            set
            {
                this.internalJObject = value;
                this.internalJson = null;
            }
        }

        /// <summary>
        /// Invalidates the internal json string by setting it to null, so that if someone wants
        /// to access the json string again, it would be regenerated from the JObject.
        /// </summary>
        public void InvalidateJson()
        {
            this.internalJson = null;
        }

        /// <summary>
        /// Invalidates the internal JObject by setting it to null, so that if someone wants
        /// to access the JObject again, it would be regenerated from the json string.
        /// </summary>
        public void InvalidateJObject()
        {
            this.internalJObject = null;
        }
    }
}
