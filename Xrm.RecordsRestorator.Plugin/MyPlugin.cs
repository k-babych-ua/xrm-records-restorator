using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace Xrm.RecordsRestorator.Plugin
{
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Records Restorator"),
        ExportMetadata("Description", "The plug-in restores deleted records from audit log"),
        ExportMetadata("SmallImageBase64", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAcFJREFUWIXt179qVEEUBvDfikbFBLWxUIyVVlkVtRKCDxDRwtcxFj6CGiGPYCfYuf4haLAQrBXRJASxCQoiJE32WswsGZfdmxnvxm3ywTDDzHfP+e7h3DPnso8xo1XIv4g7uI42jmMb6/iGDp7i0wg1glksococL3FtFI6PYBHdaPgHFnALZzEROedwE48jp4rPPMKhEodLccAJvIvGNnEPxzJsTGI+PlPhFU7mCuiF8CiW4/orLuUaSDCDL4mIrEj0BCzEeUUI9TAs403N+WnhBSo8LBHQFUJ4OZNfh5loq4uruQIq3N+NnCkA7kZep0RAOl43FDCJjci9kB4c6CO+zTD2L/iNJ3F9e5SGcyNAqB0Vno9LwHTkfhyXgInI/ZVu9udAUyF1CdxKuEMF5Fw46RdRkrSn4vw93TxYYGAQZgu4vaK2VifgRiM59ZiL867FaC8wZeeaPj8OAfMyS3FpEuagjS2hdbvSfziqz3AYzuAZDgtX/Ic99vcX2li10yMWtWZNMCW0b1uJ8+yWrBQtoX2bFm65RfyMjrfxQGGtKUnC9zWcjgEJNwhNKuGG8Jabwo/JOl4IPyafG9jdx//FH+U/qMCQnQrmAAAAAElFTkSuQmCC"),
        ExportMetadata("BigImageBase64", "data:image/png;base64,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"),
        ExportMetadata("BackgroundColor", "Lavender"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "Gray")]
    public class MyPlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new MyPluginControl();
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public MyPlugin()
        {
            // If you have external assemblies that you need to load, uncomment the following to 
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        /// <summary>
        /// Event fired by CLR when an assembly reference fails to load
        /// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
        /// For example, a folder named Sample.XrmToolBox.MyPlugin 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            // base name of the assembly that failed to resolve
            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            // check to see if the failing assembly is one that we reference.
            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            // if the current unresolved assembly is referenced by our plugin, attempt to load
            if (refAssembly != null)
            {
                // load from the path to this plugin assembly, not host executable
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }
    }
}