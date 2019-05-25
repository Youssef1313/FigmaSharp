﻿/* 
 * FigmaViewContent.cs 
 * 
 * Author:
 *   Jose Medrano <josmed@microsoft.com>
 *
 * Copyright (C) 2018 Microsoft, Corp
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit
 * persons to whom the Software is furnished to do so, subject to the
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
 * NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

namespace FigmaSharp.Services
{
    public class PlatformCustomViewConverter
    {
        public PlatformCustomViewConverter (string platform, CustomViewConverter converter)
        {
            Platform = platform;
            Converter = converter;
        }

        public string Platform { get; private set; }
        public CustomViewConverter Converter { get; private set; }
    }

    public static class ModuleService
    {
        public static class Platform
        {
            public static string MAC = "mac";
            public static string iOS = "ios";
            public static string WinForms = "winforms";
        }

        public static List<PlatformCustomViewConverter> Converters = new List<PlatformCustomViewConverter>();

        public static void LoadModules (string directory, string platform)
        {
            Console.WriteLine("Loading all directory modules from {0}", directory);
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("[{0}] Error. Directory not found.", directory);
                return;
            }

            foreach (var dir in System.IO.Directory.EnumerateDirectories(directory))
            {
                LoadModuleDirectory(dir, platform);
            }
        }

        public static void LoadModuleDirectory (string directory, string platform)
        {
            Console.WriteLine("Loading module directory: {0}", directory);

            if (!Directory.Exists(directory))
            {
                Console.WriteLine("[{0}] Error. Directory not found.", directory);
                return;
            }

            var manifestFilePath = Path.Combine(directory, "figma.manifest");
            if (!File.Exists(manifestFilePath))
            {
                Console.WriteLine("Error figma.manifest not found in directory '{0}'", directory);
                return;
            }

            Console.WriteLine("Loading figma.manifest in {0} ...", manifestFilePath);

            var file = File.ReadAllText (manifestFilePath);
            var manifest = JsonConvert.DeserializeObject<FigmaAssemblyManifest>(file);

            if (manifest.platform != platform)
            {
                Console.WriteLine("Error platform in {0} is '{1}' but should be '{2}'", file, manifest.platform, platform);
                return;
            }
           
            Console.WriteLine("Version: {0}", manifest.version);
            Console.WriteLine("Platform: {0}", manifest.platform);

            var enumeratedFiles = Directory.EnumerateFiles(directory, "*.dll").ToArray();
            LoadModule(platform, enumeratedFiles);
        }

        public static void LoadModule(string platform, params string[] filePaths)
        {
            Dictionary<Assembly, string> instanciableTypes = new Dictionary<Assembly, string>();

            Console.WriteLine("Loading {0}...", string.Join(",", filePaths));

            foreach (var file in filePaths)
            {
                if (!File.Exists (file))
                {
                    Console.WriteLine("[{0}] Error. File not found.", file);
                    continue;
                }

                var fileName = Path.GetFileName(file);
                Console.WriteLine("[{0}] Found.", fileName);
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    instanciableTypes.Add(assembly, file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[{0}] Error loading.", fileName);
                }
            }

            foreach (var assemblyTypes in instanciableTypes)
            {
                try
                {
                    //we get all the type converters from the selected assembly
                    var interfaceType = typeof(CustomViewConverter);
                    var types = assemblyTypes.Key.GetTypes()
                        .Where(interfaceType.IsAssignableFrom);

                    foreach (var type in types)
                    {
                        if (type.GetTypeInfo().IsAbstract)
                        {
                            Console.WriteLine("[{0}] Skipping {1} (abstract class).", assemblyTypes.Key, type);
                            continue;
                        }
                        Console.WriteLine("[{0}] Creating instance {1}...", assemblyTypes.Key, type);
                        try
                        {
                            if (Activator.CreateInstance(type) is CustomViewConverter element)
                                Converters.Add(new PlatformCustomViewConverter (platform, element));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        Console.WriteLine("[{0}] Loaded.", type);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            Console.WriteLine("[{0}] Finished.");
        }
    }
}