using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using KsWare.Presentation.StaticWrapper;

namespace KsWare.PhotoManager.Screens.About
{
	[Export]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class AboutViewModel : Screen, IPartImportsSatisfiedNotification
	{
		[Import] private IApplication _application;

		private string _applicationTitle;
		private string _applicationProduct;
		private string _applicationCompany;
		private string _applicationCopyright;
		private string _applicationDescription;
		private string _applicationVersion;
		private string _applicationFileVersion;
		private string _applicationInformationalVersion;

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
			var assembly = Assembly.GetEntryAssembly();
			var customAttributes = assembly.GetCustomAttributes();

			_applicationTitle = customAttributes.OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title;
			_applicationProduct = customAttributes.OfType<AssemblyProductAttribute>().FirstOrDefault()?.Product;
			_applicationCompany = customAttributes.OfType<AssemblyCompanyAttribute>().FirstOrDefault()?.Company;
			_applicationCopyright = customAttributes.OfType<AssemblyCopyrightAttribute>().FirstOrDefault()?.Copyright;
			_applicationDescription = customAttributes.OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description;
			_applicationVersion = customAttributes.OfType<AssemblyVersionAttribute>().FirstOrDefault()?.Version;
			_applicationFileVersion = customAttributes.OfType<AssemblyFileVersionAttribute>().FirstOrDefault()?.Version;
			_applicationInformationalVersion = customAttributes.OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion;

			InformationItems.Clear();
			InformationItems.Add(new InformationItem("License", LoadEmbeddedResource(Assembly.GetExecutingAssembly(), "Resources/LICENSE")));
			InformationItems.Add(new InformationItem("Components", LoadEmbeddedResource(Assembly.GetExecutingAssembly(), "Resources/Components.txt")));
		}

		private string LoadEmbeddedResource(Assembly assembly, string name)
		{
			var customAttributes = assembly.GetCustomAttributes();
			var company = customAttributes.OfType<AssemblyCompanyAttribute>().FirstOrDefault()?.Company;
			var poduct = customAttributes.OfType<AssemblyProductAttribute>().FirstOrDefault()?.Product;
			name= name.Replace('\\', '.').Replace('/', '.');
			var resourceName = $"{company}.{poduct}.{name}";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		public string ApplicationTitle { get => _applicationTitle; set => Set(ref _applicationTitle, value);}
		public string ApplicationProduct { get => _applicationProduct; set => Set(ref _applicationProduct, value); }
		public string ApplicationCompany { get => _applicationCompany; set => Set(ref _applicationCompany, value);}
		public string ApplicationCopyright { get => _applicationCopyright; set => Set(ref _applicationCopyright, value);}
		public string ApplicationDescription { get => _applicationDescription; set => Set(ref _applicationDescription, value);}
		public string ApplicationVersion { get => _applicationVersion; set => Set(ref _applicationVersion, value);}
		public string ApplicationFileVersion { get => _applicationFileVersion; set => Set(ref _applicationFileVersion, value);}
		public string ApplicationInformationalVersion { get => _applicationInformationalVersion; set => Set(ref _applicationInformationalVersion, value);}
		public BindableCollection<InformationItem> InformationItems { get; } = new BindableCollection<InformationItem>();
	}

	public class InformationItem
	{
		public InformationItem()
		{
		}

		public InformationItem(string name, string details)
		{
			Name = name;
			Details = details;
		}

		public string Name { get; set; }
		public string Details { get; set; }
	}
}
