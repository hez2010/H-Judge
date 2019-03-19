using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using SpanJson;
using SpanJson.Resolvers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SpanJson.AspNetCore.Formatter
{
    public class SpanJsonOutputFormatter<TResolver> : TextOutputFormatter where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
    {
        public SpanJsonOutputFormatter()
        {
            SupportedMediaTypes.Add("application/json");
            SupportedMediaTypes.Add("text/json");
            SupportedMediaTypes.Add("application/*+json");
            SupportedEncodings.Add(Encoding.UTF8);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
        {
            if (context.Object != null)
            {
                var valueTask = JsonSerializer.NonGeneric.Utf8.SerializeAsync<TResolver>(context.Object, context.HttpContext.Response.Body);
                return valueTask.IsCompletedSuccessfully ? Task.CompletedTask : valueTask.AsTask();
            }

            return Task.CompletedTask;
        }
    }
    public class SpanJsonInputFormatter<TResolver> : TextInputFormatter where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
    {
        public SpanJsonInputFormatter()
        {
            SupportedMediaTypes.Add("application/json");
            SupportedMediaTypes.Add("text/json");
            SupportedMediaTypes.Add("application/*+json");
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            try
            {
                var model = await JsonSerializer.NonGeneric.Utf8.DeserializeAsync<TResolver>(context.HttpContext.Request.Body, context.ModelType)
                    .ConfigureAwait(false);
                if (model == null && !context.TreatEmptyInputAsDefaultValue)
                {
                    return InputFormatterResult.NoValue();
                }

                return InputFormatterResult.Success(model);
            }
            catch (Exception ex)
            {
                context.ModelState.AddModelError("JSON", ex.Message);
                return InputFormatterResult.Failure();
            }
        }
    }
    public class AspNetCoreDefaultResolver<TSymbol> : ResolverBase<TSymbol, AspNetCoreDefaultResolver<TSymbol>> where TSymbol : struct
    {
        public AspNetCoreDefaultResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.IncludeNulls,
            NamingConvention = NamingConventions.CamelCase,
            EnumOption = EnumOptions.Integer
        })
        {
        }
    }
    /// <summary>
    /// Extensions to use SpanJson in UTF8 mode as the default serializer
    /// </summary>
    public static class SpanJsonMvcExtensions
    {/// <summary>
     /// Uses SpanJson in UTF8 mode with custom formatter resolver
     /// </summary>
        public static IMvcCoreBuilder AddSpanJsonCustom<TResolver>(this IMvcCoreBuilder mvcCoreBuilder)
            where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
        {
            Configure<TResolver>(mvcCoreBuilder.Services);
            return mvcCoreBuilder;
        }

        /// <summary>
        /// Uses SpanJson in UTF8 mode with custom formatter resolver
        /// </summary>
        public static IMvcBuilder AddSpanJsonCustom<TResolver>(this IMvcBuilder mvcBuilder) where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
        {
            Configure<TResolver>(mvcBuilder.Services);
            return mvcBuilder;
        }

        /// <summary>
        /// Uses SpanJson in UTF8 mode with ASP.NET Core 2.1 defaults (IncludeNull, CamelCase, Enum as Ints)
        /// </summary>
        public static IMvcCoreBuilder AddSpanJson(this IMvcCoreBuilder mvcBuilder)
        {
            Configure<AspNetCoreDefaultResolver<byte>>(mvcBuilder.Services);
            return mvcBuilder;
        }

        /// <summary>
        /// Uses SpanJson in UTF8 mode with ASP.NET Core 2.1 defaults (IncludeNull, CamelCase, Enum as Ints)
        /// </summary>
        public static IMvcBuilder AddSpanJson(this IMvcBuilder mvcBuilder)
        {
            Configure<AspNetCoreDefaultResolver<byte>>(mvcBuilder.Services);
            return mvcBuilder;
        }


        private static void Configure<TResolver>(IServiceCollection serviceCollection) where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
        {
            serviceCollection.Configure<MvcOptions>(config =>
            {
                config.InputFormatters.Clear();
                config.OutputFormatters.Clear();
                config.InputFormatters.Add(new SpanJsonInputFormatter<TResolver>());
                config.OutputFormatters.Add(new SpanJsonOutputFormatter<TResolver>());
            });
        }
    }
}