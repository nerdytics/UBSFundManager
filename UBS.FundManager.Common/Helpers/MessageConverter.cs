using Newtonsoft.Json;
using System.Text;

namespace UBS.FundManager.Common.Helpers
{
    /// <summary>
    /// Extension utility for serialising / deserialising objects and setting non-disposable objects to null
    /// </summary>
    public static class MessageConverter
    {
        public static readonly JsonSerializerSettings JsonSerializationSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None
        };

        /// <summary>
        /// Serialises an object to string
        /// </summary>
        /// <param name="obj">Object to serialise</param>
        /// <returns>Serialised string</returns>
        public static string Serialise(this object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerializationSettings);
        }

        /// <summary>
        /// Deserialises a serialised string to its original type
        /// </summary>
        /// <typeparam name="T">Type to deserialise to</typeparam>
        /// <param name="obj">serialised message</param>
        /// <returns>Deserialised message</returns>
        public static T Deserialise<T>(this string obj)
        {
            T result = default(T);

            try
            {
                result = JsonConvert.DeserializeObject<T>(obj, JsonSerializationSettings);
            }
            catch (JsonReaderException)
            {

            }

            return result;
        }

        /// <summary>
        /// Deserialises an encoded AMQP message
        /// </summary>
        /// <typeparam name="T">Type to deserialise to</typeparam>
        /// <param name="data">Encoded amqp message</param>
        /// <returns></returns>
        public static T DecodeTransferedObject<T>(this byte[] data)
        {
            string strValue = Encoding.UTF8.GetString(data);

            if (typeof(T) == typeof(string))
            {
                return (T)(object)strValue;
            }

            return Deserialise<T>(strValue);
        }

        /// <summary>
        /// Encodes a payload for transfer via AMQP
        /// </summary>
        /// <typeparam name="T">Type to encode</typeparam>
        /// <param name="o">Object to encode</param>
        /// <returns>Encoded message</returns>
        public static byte[] EncodeForTransfer<T>(this T o)
        {
            string serialised = Serialise(o);
            return Encoding.UTF8.GetBytes(serialised);
        }

        /// <summary>
        /// Sets non-disposable objects to null
        /// </summary>
        /// <typeparam name="T">type to nullify</typeparam>
        /// <param name="resource">Object to nullify</param>
        public static void CleanUp<T>(this T resource) where T : class
        {
            if(resource != null)
            {
                resource = default(T);
            }
        }
    }
}
