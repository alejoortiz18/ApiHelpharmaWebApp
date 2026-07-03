using Business.DocSoporteBusiness;
using Data.DocSoporte;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Interface
{
    public static class BusinessDependencyInjectionAccess
    {
        public static IServiceCollection BusDependencyInjectionAccess(this IServiceCollection services)
        {
            #region [General]
            services.AddScoped<IDocSoportBusiness, DocSuportBusiness>();
            #endregion

            return services;
        }
    }
}
