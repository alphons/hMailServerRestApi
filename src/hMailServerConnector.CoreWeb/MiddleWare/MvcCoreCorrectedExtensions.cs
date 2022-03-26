
using System.Text.Json;
using System.Text.Json.Serialization;

namespace hMailServerConnector.CoreWeb.MiddleWare
{
	public static class MvcCoreCorrectedExtensions
	{
		/// <summary>
		/// Adding JsonParametersValueProviderFactory to the MVC layer
		/// </summary>
		/// <param name="services">IServiceCollection</param>
		/// <returns>IMvcCoreBuilder</returns>
		public static IMvcCoreBuilder AddMvcCoreCorrected(this IServiceCollection services, bool CorrectDateTime = true)
		{
			return services.AddMvcCore()
				.AddMvcOptions(options =>
				{
					options.EnableEndpointRouting = false;
					options.ModelMetadataDetailsProviders.Add(new NoNullMetadataProvider());
					options.ValueProviderFactories.Insert(0, new JsonParametersValueProviderFactory());
				})
				.AddFormatterMappings()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.DictionaryKeyPolicy = null;
					options.JsonSerializerOptions.PropertyNamingPolicy = null;
					if(CorrectDateTime)
						options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
				});
		}

		private class DateTimeConverter : JsonConverter<DateTime>
		{
			public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				_ = DateTime.TryParse(reader.GetString(), out DateTime dtm);
				return dtm;
			}

			public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(value.ToLocalTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"));
			}
		}
	}
}
