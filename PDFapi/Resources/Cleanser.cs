﻿
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;


namespace PDFapi.Resources
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Cleanser
    {
        /// <summary>
        /// This function runs a number of regex replace operations on the Json string of the parameters.
        /// The operations are:
        /// Remove possible HTML characters
        /// Remove double spaces from anywhere in the string
        /// Left trim anything between double quotes
        /// Right trim anything between double quotes
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JObject Cleanse(dynamic parameters)
        {
            return JObject.Parse(Cleanse(parameters.ToString()));
        }

        /// <summary>
        /// Run Cleanse operations
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static string Cleanse(string aValue)
        {
            string pstring = "";

            if (aValue != null)
            {
                //Remove HTML parameters
                if (!((aValue.Contains("HeaderTemplate") || (aValue.Contains("FooterTemplate"))
                    || ((aValue.Contains("headerTemplate")) || ((aValue.Contains("footerTemplate")))))))
                {
                    pstring = Regex.Replace(aValue, @"<.*?>", "");
                }
                else
                {
                    pstring = aValue;
                }

                //Remove double spaces
                pstring = Regex.Replace(pstring, @" {2,}", " ");

                // left trim anything between quotes
                pstring = Regex.Replace(pstring, "\"\\s+", "\"");

                //Right trim anything between quotes
                pstring = Regex.Replace(pstring, "\\s+\"", "\"");
            }

            return pstring;
        }

    }
}
