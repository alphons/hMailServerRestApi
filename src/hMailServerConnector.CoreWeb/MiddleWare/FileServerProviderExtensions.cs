
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

/*
{
	"Conf": {
		"ContentTypes": [
			{
				"Extension": ".m3u8",
				"ContentType": "application/x-mpegURL"
			},
			{
				"Extension": ".ts",
				"ContentType": "video/MP2T"
			}
		],
		"VirtualDirs": [
			{
				"Directory": "R:\\",
				"RequestPath": "/Live"
			},
			{
				"Directory": "F:\\DJPodium\\Uploads",
				"RequestPath": "/Uploads",
				"DirectoryController": "VirtualDirectory"
			},
			{
				"Directory": "F:\\DJPodium\\Recordings",
				"RequestPath": "/Recordings",
				"DirectoryController": "VirtualDirectory",
				"ShowDirsWhenContains": "*.m3u8"
			}
		]
	}
}
 */



/// <summary>
/// Original:
/// https://stackoverflow.com/questions/43190824/get-physicalpath-after-setting-up-app-usefileserver-in-asp-net-core
/// Adapted: MapPath
/// </summary>

namespace hMailServerConnector.CoreWeb.MiddleWare
{
    /// <summary>
    /// Wrapper for UseFileServer to easily use the FileServerOptions registered in the IFileServerProvider
    /// </summary>
    public static class FileServerProviderExtensions
    {
        public static IApplicationBuilder UseFileServerProvider(this IApplicationBuilder application, IFileServerProvider fileServerprovider)
        {
            foreach (var option in fileServerprovider.FileServerOptionsCollection)
            {
                application.UseFileServer(option);
            }
			
            return application;
        }
    }

	public interface IFileServerProvider
	{
		/// <summary>
		/// Contains a list of FileServer options, a combination of virtual + physical paths we can access at any time
		/// MapPath translates a virtual path to a physical path
		/// </summary>
		IList<FileServerOptions> FileServerOptionsCollection { get; }

		string? MapPath(string virtualPath);
	}

	public class ContentTypes
	{
		public string? Extension { get; set; }
		public string? ContentType { get; set; }
	}

	/// <summary>
	/// Implements IFileServerProvider including mime mappings
	/// </summary>
	public class FileServerProvider : IFileServerProvider
	{
		private readonly string WebRootPath;

		public IList<FileServerOptions> FileServerOptionsCollection { get; }

		public FileServerProvider(string WebRootPath, IConfigurationSection? AppSettingsVirtualDirs = null, IConfigurationSection? AppSettingsContentTypes = null)
		{
			this.WebRootPath = WebRootPath;

			var fileServerOptions = new List<FileServerOptions>();

			var provider = new FileExtensionContentTypeProvider();
			if (AppSettingsContentTypes != null)
			{
				var contenttypes = AppSettingsContentTypes.Get<List<ContentTypes>>();
				foreach (var ct in contenttypes)
				{
					if(ct.Extension != null)
						provider.Mappings.Remove(ct.Extension);
					if(ct.Extension != null && ct.ContentType != null)
						provider.Mappings.Add(ct.Extension, ct.ContentType);
				}
			}

			if (AppSettingsVirtualDirs != null)
			{
				var virtualDirs = AppSettingsVirtualDirs.Get<List<VirtualDir>>();
				foreach (var vir in virtualDirs)
				{
					if (!Directory.Exists(vir.Directory))
						continue;
					var option = new FileServerOptions()
					{
						FileProvider = new PhysicalFileProvider(vir.Directory),
						RequestPath = new PathString(vir.RequestPath),
						EnableDirectoryBrowsing = !string.IsNullOrWhiteSpace(vir.DirectoryController)
					};
					if (AppSettingsContentTypes != null)
						option.StaticFileOptions.ContentTypeProvider = provider;
					fileServerOptions.Add(option);
				}
			}

			FileServerOptionsCollection = fileServerOptions;
		}

		public string? MapPath(string virtualPath)
		{
			var option = FileServerOptionsCollection.FirstOrDefault(e => virtualPath.StartsWith(e.RequestPath.ToString(), StringComparison.InvariantCultureIgnoreCase));
			if (option == null)
				return this.WebRootPath + virtualPath.Replace('/', '\\');
			else
			{
				if (option.RequestPath == null || option.RequestPath.Value == null || option.FileProvider == null)
					return null;
				if (virtualPath.Length == option.RequestPath.Value.Length)
					return option.FileProvider.GetFileInfo("/").PhysicalPath.TrimEnd('\\');
				else
					return option.FileProvider.GetFileInfo(virtualPath[option.RequestPath.Value.Length..]).PhysicalPath;
			}
		}
	}
}
