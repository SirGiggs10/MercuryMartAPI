﻿namespace MercuryMartAPI.Dtos.General
{
    public class ReturnResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public object ObjectValue { get; set; }
    }
}
