﻿using System;
using Jasper.Configuration;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

namespace Jasper.Http.Configuration
{
    internal class JasperRequestDelegateStartup : IStartup
    {
        private readonly Router _router;

        public JasperRequestDelegateStartup(Router router)
        {
            _router = router;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // TODO -- sounds goofy, but this never gets used
            return null;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use(_router.Apply);
        }
    }

    internal class HostBuilder : IWebHostBuilder
    {
        private readonly AspNetCoreFeature _parent;
        private readonly WebHostBuilder _inner;


        public HostBuilder(AspNetCoreFeature parent)
        {
            _parent = parent;
            _inner = new WebHostBuilder();
            _inner.ConfigureServices(_ =>
            {
                _.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            });
        }

        public IWebHost Build()
        {
            throw new NotSupportedException("Jasper needs to do the web host building within its bootstrapping");
        }

        public IWebHostBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            return _inner.UseLoggerFactory(loggerFactory);
        }

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return _inner.ConfigureServices(configureServices);
        }

        public IWebHostBuilder ConfigureLogging(Action<ILoggerFactory> configureLogging)
        {
            return _inner.ConfigureLogging(configureLogging);
        }

        public IWebHostBuilder UseSetting(string key, string value)
        {
            return _inner.UseSetting(key, value);
        }

        public string GetSetting(string key)
        {
            return _inner.GetSetting(key);
        }

        internal IWebHost Activate(IContainer container)
        {
            _inner.ConfigureServices(services => JasperStartup.Register(container, services));

            return _inner.Build();
        }
    }
}
