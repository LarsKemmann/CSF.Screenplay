using System;
using CSF.Screenplay.Playwright.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSF.Screenplay.Playwright
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Activates the Playwright extension for Screenplay by adding its services to the dependency injection container.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This registers <see cref="IGetsBrowser"/> (one browser per Screenplay, launched on first use), the
        /// <see cref="PlaywrightOptions"/> (bind them from configuration with <c>services.Configure&lt;PlaywrightOptions&gt;(configuration.GetSection("Playwright"))</c>
        /// or configure them here), the report formatters for targets and URIs, and a handler which saves a screenshot — and the
        /// trace, when recorded — as report assets whenever a performable fails.
        /// </para>
        /// </remarks>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">An optional delegate to configure the Playwright options</param>
        /// <returns>The same service collection, so calls may be chained</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="services"/> is <see langword="null" />.</exception>
        public static IServiceCollection AddPlaywright(this IServiceCollection services, Action<PlaywrightOptions> configureOptions = null)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            services.AddOptions<PlaywrightOptions>();
            if (configureOptions != null) services.Configure(configureOptions);

            services.TryAddSingleton<PlaywrightBrowserProvider>();
            services.TryAddSingleton<IGetsBrowser>(s => s.GetRequiredService<PlaywrightBrowserProvider>());
            services.TryAddSingleton<ISavesAssetsWhenPerformableFails, PerformableFailureAssetSaver>();

            services.AddTransient<TargetFormatter>();
            services.AddTransient<NamedUriFormatter>();
            services.AddTransient<CookiesFormatter>();

            services.Configure<ScreenplayOptions>(o =>
            {
                o.ValueFormatters.Add(typeof(TargetFormatter));
                o.ValueFormatters.Add(typeof(NamedUriFormatter));
                o.ValueFormatters.Add(typeof(CookiesFormatter));

                o.PerformanceEventHandlers.Add((eventBus, s) =>
                {
                    var saver = s.GetRequiredService<ISavesAssetsWhenPerformableFails>();
                    eventBus.PerformableFailed -= saver.OnPerformableFailed;
                    eventBus.PerformableFailed += saver.OnPerformableFailed;
                });
            });

            return services;
        }
    }
}
