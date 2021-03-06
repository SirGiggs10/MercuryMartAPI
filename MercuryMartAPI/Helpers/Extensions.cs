﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Helpers
{
    public static class Extensions
    {
        public static void AddPagination(this HttpResponse response,
           int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public static string SingularizeWord(this string keyWord)
        {
            var isKeywordSingular = PluralizationService.CreateService(new System.Globalization.CultureInfo("en")).IsSingular(keyWord);
            if (!isKeywordSingular) return PluralizationService.CreateService(new System.Globalization.CultureInfo("en")).Singularize(keyWord);
            return keyWord;
        }
    }
}
