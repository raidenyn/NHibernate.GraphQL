using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NHibernate.GraphQL
{
    /// <summary>
    /// Cursor formatter as json string
    /// </summary>
    public class CursorJsonFormatter : ICursorFormatter
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Format passed object to string representation.
        /// <para>Should be parsable in <see cref="ParseAs"/></para>
        /// </summary>
        /// <typeparam name="TOrder">Object instance type</typeparam>
        /// <param name="order">Object instance</param>
        /// <returns>Cursor string representation</returns>
        public Cursor Format<TOrder>(TOrder order)
        {
            string json = JsonConvert.SerializeObject(order, Settings);

            return Convert.ToBase64String(DefaultEncoding.GetBytes(json));
        }

        /// <summary>
        /// Parse passed string into a object instance.
        /// <para>Should be formattable in <see cref="Format"/></para>
        /// </summary>
        /// <typeparam name="TOrder">Object instance type</typeparam>
        /// <param name="cursor">Cursor string representation</param>
        /// <returns>Parsed object instance</returns>
        public TOrder ParseAs<TOrder>(Cursor cursor)
        {
            try
            {
                string json = DefaultEncoding.GetString(Convert.FromBase64String(cursor));

                return JsonConvert.DeserializeObject<TOrder>(json, Settings);
            } catch (Exception exception)
            {
                throw new CursorParsingException(cursor, typeof(TOrder), exception);
            }
        }

        /// <summary>
        /// Check that the cursor has any value
        /// </summary>
        /// <param name="cursor">Cursor string representation</param>
        /// <returns>True if the cursor has a value</returns>
        public bool HasValue(Cursor cursor)
        {
            return !String.IsNullOrWhiteSpace(cursor);
        }
    }
}
