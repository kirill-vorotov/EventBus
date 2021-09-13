#if UNITY_2019_1_OR_NEWER
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace KV.Events.Editor
{
    public static class EventBusEditor
    {
        private const string generatedFileDirectory = "Plugins/EventBus/Generated";
        private const string generatedFileName = "EventDispatchers.generated.cs";
        private const string @namespace = "Generated";
        private const string className = "EventDispatchersGenerated";
        private const string methodName = "Stub";
        private const string menuItem = "EventBus/Generate il2cpp stub";
        
        public static string GenerateIl2CppStubCode()
        {
            var writer = new CodeGenWriter(new StringBuilder());
            var namespaces = new HashSet<string>();
            var eventTypes = EventBus.GetAllEventTypes().ToList();
            foreach (var type in eventTypes)
            {
                namespaces.Add(type.Namespace);
            }

            writer.WriteLine($"namespace {@namespace}");
            writer.BeginBlock();
            
            writer.WriteLine($"public static class @{className}");
            writer.BeginBlock();
            
            writer.WriteLine($"private static void {methodName}()");
            
            writer.BeginBlock();
            foreach (var type in eventTypes)
            {
                if (type.FullName == null)
                {
                    continue;
                }

                var typename = type.FullName.Replace('+', '.');
                writer.WriteLine($"new KV.EventBus.EventDispatcher<{typename}>();");
            }
            writer.EndBlock();
            
            writer.EndBlock();
            
            writer.EndBlock();
            
            return writer.StringBuilder.ToString();
        }

        public static void CreateIl2CppStubFile(string relativePath)
        {
            Assert.IsTrue(!string.IsNullOrEmpty(relativePath));
            var dirPath = Path.Combine(Application.dataPath, relativePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var filePath = Path.Combine(dirPath, generatedFileName);
            var code = GenerateIl2CppStubCode();
            File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
        }

        [MenuItem(menuItem)]
        public static void CreateIl2CppStubFile()
        {
            CreateIl2CppStubFile(generatedFileDirectory);
        }
    }
}
#endif