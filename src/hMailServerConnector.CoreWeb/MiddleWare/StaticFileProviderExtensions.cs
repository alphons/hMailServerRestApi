
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace hMailServerConnector.CoreWeb.MiddleWare
{
	/// <summary>
	/// Wrapper for UseStaticFiles to easily use the StaticFileOptions registered in the IStaticFileProvider
	/// </summary>
	public static class StaticFileProviderExtensions
	{
		/// <summary>
		/// UseStaticFileProvider does map every virtual directory by UseStaticFiles and does endpointmapping for controllers
		/// and mapping VirtualDirectoryController for every virtual directory
		/// </summary>
		/// <param name="application"></param>
		/// <param name="staticFileProvider"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseStaticFileProvider(this IApplicationBuilder application, IStaticFileProvider staticFileProvider)
        {
            foreach (var option in staticFileProvider.StaticFileOptionsCollection)
            {
                application.UseStaticFiles(option);
            }

			application.UseRouting();

			application.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();

				if (staticFileProvider.VirtualDirs != null)
				{
					foreach (var VirtualDir in staticFileProvider.VirtualDirs)
					{
						if (string.IsNullOrWhiteSpace(VirtualDir.DirectoryController))
							continue;
						if (VirtualDir.RequestPath == null)
							continue;
						endpoints.MapControllerRoute(
						name: VirtualDir.RequestPath,
						pattern: "~" + VirtualDir.RequestPath + "/{*path}",
							defaults: new { controller = VirtualDir.DirectoryController, action = "Index", DirectoryPath = VirtualDir.Directory, VirtualDir.RequestPath, VirtualDir.ShowDirsWhenContains });
					}
				}
			});

			return application;
        }
    }

	public class VirtualDir
	{
		public string? Directory { get; set; }
		public string? RequestPath { get; set; }
		public string? DirectoryController { get; set; }
		public string? ShowDirsWhenContains { get; set; }
	}

	public interface IStaticFileProvider
	{
		/// <summary>
		/// Contains a list of FileServer options, a combination of virtual + physical paths we can access at any time
		/// MapPath translates a virtual path to a physical path
		/// </summary>
		IList<StaticFileOptions> StaticFileOptionsCollection { get; }

		IList<VirtualDir>? VirtualDirs { get; }

		string? MapPath(string virtualPath);
	}

	/// <summary>
	/// Implements IFileServerProvider including mime mappings
	/// </summary>
	public class StaticFileProvider : IStaticFileProvider
	{
		private readonly string WebRootPath;

		public IList<StaticFileOptions> StaticFileOptionsCollection { get; }

		public IList<VirtualDir>? VirtualDirs { get; }

		public StaticFileProvider(string WebRootPath, IConfigurationSection? AppSettingsVirtualDirs = null, IConfigurationSection? AppSettingsContentTypes = null)
		{
			this.WebRootPath = WebRootPath;

			this.VirtualDirs = null;

			var staticFileOptions = new List<StaticFileOptions>();

			var contentTypeProvider = new FileExtensionContentTypeProvider();
			if (AppSettingsContentTypes != null)
			{
				var contenttypes = AppSettingsContentTypes.Get<List<ContentTypes>>();
				foreach (var ct in contenttypes)
				{
					if(ct.Extension != null)
						contentTypeProvider.Mappings.Remove(ct.Extension);
					if (ct.Extension != null && ct.ContentType != null)
						contentTypeProvider.Mappings.Add(ct.Extension, ct.ContentType);
				}
			}

			if (AppSettingsVirtualDirs != null)
			{
				VirtualDirs = AppSettingsVirtualDirs.Get<List<VirtualDir>>();
				foreach (var vir in VirtualDirs)
				{
					if (!Directory.Exists(vir.Directory))
						continue;

					var option = new StaticFileOptions()
					{
						OnPrepareResponse = ctx =>
						{
							// DOS> curl -H "Access-Control-Request-Method: GET" -H "Origin: http://localhost" --head https://localhost:44346
							if (ctx.Context.Request.Headers.ContainsKey(CorsConstants.Origin))
								ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = CorsConstants.AnyOrigin;
						},
						FileProvider = new PhysicalFileProvider(vir.Directory),
						RequestPath = new PathString(vir.RequestPath)
					};
					if (AppSettingsContentTypes != null)
						option.ContentTypeProvider = contentTypeProvider;
					staticFileOptions.Add(option);
				}
			}

			staticFileOptions.Add(new StaticFileOptions()
			{
				OnPrepareResponse = ctx =>
				{
					if (ctx.Context.Request.Headers.ContainsKey(CorsConstants.Origin))
						ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = CorsConstants.AnyOrigin;
				},
				ServeUnknownFileTypes = true,
				DefaultContentType = "text/plain"
			});

			StaticFileOptionsCollection = staticFileOptions;
		}

		public string? MapPath(string virtualPath)
		{
			var option = StaticFileOptionsCollection.FirstOrDefault(e => virtualPath.StartsWith(e.RequestPath.ToString(), StringComparison.InvariantCultureIgnoreCase));
			if (option == null || option.FileProvider == null)
				return this.WebRootPath + virtualPath.Replace('/', '\\');

			if (option.RequestPath == null || option.RequestPath.Value == null)
				return null;

			if (virtualPath.Length == option.RequestPath.Value.Length) // shortcut
				return option.FileProvider.GetFileInfo("/").PhysicalPath.TrimEnd('\\');
			else
				return option.FileProvider.GetFileInfo(virtualPath[option.RequestPath.Value.Length..]).PhysicalPath;
		}
	}
}
