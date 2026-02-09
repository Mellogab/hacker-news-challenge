using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Hacker News Core services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="config">Optional configuration action for core services</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null</exception>
    public static IServiceCollection AddHackerNewsDomainServices(
        this IServiceCollection services, 
        Action<dynamic>? config = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (config != null)
            services.Configure(config);

        var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;

        services.AddMediator(options =>
        {
            options.AddPipelineForLogging(loggingOptions =>
            {
                loggingOptions.Level = LogLevel.Debug;
                loggingOptions.LogCommand = true;
                loggingOptions.LogCommandResult = true;
                loggingOptions.LogEvent = true;
                loggingOptions.LogQuery = true;
                loggingOptions.LogQueryResult = true;
            });
            options.AddPipelineForValidation(validationOptions =>
            {
                validationOptions.ValidateCommand = true;
                validationOptions.ValidateEvent = true;
                validationOptions.ValidateQuery = true;
                validationOptions.FailIfValidatorNotFound = false;

                validationOptions.OnFailedValidation = (instance, errors, ct) =>
                {
                    var sb = new StringBuilder("Invalid information has been submitted");
                    foreach (var e in errors.Errors)
                        sb.Append("  ").Append(e.ErrorMessage).AppendLine();

                    throw new InvalidOperationException(sb.ToString());
                };
            });
            //TO DO: Finish configuration transaction pipelines
            options.AddValidatorsFromAssembly(assembly);
            options.AddHandlersFromAssembly(assembly);
        });

        return services;
    }
}
