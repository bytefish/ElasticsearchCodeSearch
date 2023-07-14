﻿using System.Net;
using System.Runtime.Serialization;

namespace ElasticsearchCodeSearch.Shared.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException()
        {
        }

        public ApiException(string? message) : base(message)
        {
        }

        public ApiException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Http status code.
        /// </summary>
        public required HttpStatusCode StatusCode { get; set; }

    }
}