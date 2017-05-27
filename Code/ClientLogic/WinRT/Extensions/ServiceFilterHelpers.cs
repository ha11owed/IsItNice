using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic.WinRT.Extensions
{
    public static class ServiceFilterHelpers
    {
        public static IMobileServiceTable<T> GetTable<T>(this MobileServiceClient mobileService, object parametersObject)
        {
            if (parametersObject != null)
            {
                // Extract parameters to a dictionary
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                foreach (var property in IntrospectionExtensions.GetTypeInfo(parametersObject.GetType()).DeclaredProperties)
                {
                    string name = property.Name;
                    object value = property.GetValue(parametersObject);

                    parameters[name] = value;
                }

                // Apply custom parameters filter and get the requested table
                return mobileService.WithFilter(new CustomParametersServiceFilter(parameters)).GetTable<T>();
            }

            return mobileService.GetTable<T>();
        }

        public static IMobileServiceTable<T> GetTable<T>(this MobileServiceClient mobileService, IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                // Apply custom parameters filter and get the requested table
                return mobileService.WithFilter(new CustomParametersServiceFilter(parameters)).GetTable<T>();
            }

            return mobileService.GetTable<T>();
        }
    }
}
