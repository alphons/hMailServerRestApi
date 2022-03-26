
using System.Text.Json;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace hMailServerConnector.CoreWeb.MiddleWare
{
	public class JsonParametersValueProvider : IValueProvider
	{
		private readonly JsonElement jElement;
		private readonly CultureInfo CultureInfo;
		private readonly Dictionary<string, JsonElement> dict;

		private string CurrentPrefix = string.Empty;
		private int CurrentIndex = 0; // Array index

		public JsonParametersValueProvider(JsonElement jElement, CultureInfo CultureInfo)
		{
			this.CultureInfo = CultureInfo;
			this.jElement = jElement;
			dict = new Dictionary<string, JsonElement>();
		}

		public bool ContainsPrefix(string prefix)
		{
			if (CurrentPrefix == prefix)
			{
				CurrentIndex++;
				Debug.WriteLine($"Prefix: {prefix} Index:{CurrentIndex}");
				return true;
			}
			else
			{
				CurrentIndex = 0;
				CurrentPrefix = prefix;
				Debug.WriteLine($"Prefix: New:{prefix}");
			}

			if (dict.ContainsKey(prefix))
				return true;

			var keys = prefix.Split('.');

			var je = jElement;

			var key = "";

			for (int intI=0;intI<keys.Length;intI++)
			{
				if (intI > 0)
					key += ".";
				key += keys[intI];
				if (dict.ContainsKey(key))
				{
					je = dict[key];
					continue;
				}

				if (je.ValueKind == JsonValueKind.Array)
				{
					if (CurrentIndex < je.GetArrayLength())
					{
						var jes = je[CurrentIndex];
						dict.Add(key, jes);
						je = jes;
					}
					else
					{
						return false;
					}
				}
				else
				{

					if (je.TryGetProperty(keys[intI], out JsonElement jes))
					{
						dict.Add(key, jes);
						je = jes;
					}
					else
					{
						return false;
					}
				}
			}
			return true;
		}

		public ValueProviderResult GetValue(string key)
		{
			//Debug.WriteLine($"GetValue Key:{key}");

			var parentkey = key;
			var intI = key.LastIndexOf('.');

			if (intI > 0)
			{
				parentkey = key.Substring(0, intI);
				key = key.Substring(intI + 1);
			}

			var je = jElement;

			if (dict.ContainsKey(parentkey))
				je = dict[parentkey];
			else
				return ValueProviderResult.None;
			JsonElement jes;
			if (je.ValueKind == JsonValueKind.Array)
			{
				Debug.WriteLine($"GetValue (Array) {parentkey}[{CurrentIndex}].{key}");
				if (CurrentIndex < je.GetArrayLength())
				{
					if (!je[CurrentIndex].TryGetProperty(key, out jes))
						return ValueProviderResult.None;
				}
				else
				{
					return ValueProviderResult.None;
				}
			}
			else if(je.ValueKind == JsonValueKind.Object)
			{
				Debug.WriteLine($"GetValue (object) {parentkey}.{key}");
				if (!je.TryGetProperty(key, out jes))
					return ValueProviderResult.None;
			}
			else
			{
				Debug.WriteLine($"GetValue (value) {parentkey}.{key}");
				jes = je;
			}

			ValueProviderResult result = jes.ValueKind switch
			{
				JsonValueKind.Array => new ValueProviderResult(jes.EnumerateArray()
										.Select(x => x.ValueKind == JsonValueKind.Null ? null : x.ValueKind == JsonValueKind.String ? x.GetString() : x.GetRawText())
										.ToArray(), this.CultureInfo),
				JsonValueKind.String => new ValueProviderResult(jes.GetString(), this.CultureInfo),
				JsonValueKind.Null => new ValueProviderResult(default, this.CultureInfo),
				_ => new ValueProviderResult(je.GetRawText(), this.CultureInfo),
			};

			return result;
		}
	}

	public class JsonParametersValueProviderFactory : IValueProviderFactory
	{
		private readonly CultureInfo CultureInfo;

		public JsonParametersValueProviderFactory(string CultureName) : base()
		{
			this.CultureInfo = new CultureInfo(CultureName);
		}

		public JsonParametersValueProviderFactory() : base()
		{
			this.CultureInfo = CultureInfo.InvariantCulture;
		}

		private static async Task AddValueProviderAsync(ValueProviderFactoryContext context, CultureInfo CultureInfo)
		{
			try
			{
				var jElement = await JsonSerializer.DeserializeAsync<JsonElement>(context.ActionContext.HttpContext.Request.Body);
				context.ValueProviders.Add(new JsonParametersValueProvider(jElement, CultureInfo));
			}
			catch(Exception eee)
			{
				// Not valid json, dont bother
				Debug.WriteLine(eee.Message);
			}
		}

		public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			var Request = context.ActionContext.HttpContext.Request;
			if (Request.Method == "POST")
			{
				if (Request.ContentType == null || Request.ContentType.StartsWith("application/json"))
				{
					if (Request.ContentLength == null || // Chunked encoding
						Request.ContentLength >= 2) // Normal encoding, using content length minimum '{}'
						return AddValueProviderAsync(context, this.CultureInfo);
				}
			}
			return Task.CompletedTask;
		}
	}



}

