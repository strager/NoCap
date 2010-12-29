using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using NoCap.GUI.WPF.Runtime;

namespace NoCap.GUI.WPF.Util {
    class ExtensionDataContractResolver : DataContractResolver {
        private readonly ExtensionManager extensionManager;

        public ExtensionDataContractResolver(ExtensionManager extensionManager) {
            this.extensionManager = extensionManager;
        }

        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace) {
            var dictionary = new XmlDictionary();

            string typeNameString = GetDataContractName(type);
            string typeNamespaceString = GetDataContractNamespace(type);

            if (typeNameString == null || typeNamespaceString == null) {
                return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
            }

            typeName = dictionary.Add(typeNameString);
            typeNamespace = dictionary.Add(typeNamespaceString);

            return true;
        }

        private static string GetDataContractNamespace(Type type) {
            var dataContractAttributes = type.GetCustomAttributes(typeof(DataContractAttribute), true).OfType<DataContractAttribute>();
            var namespaceAttr = dataContractAttributes.FirstOrDefault((attr) => attr.Namespace != null);

            if (namespaceAttr != null) {
                return namespaceAttr.Namespace;
            }

            var namespaceAttributes = type.Assembly.GetCustomAttributes(typeof(ContractNamespaceAttribute), false).OfType<ContractNamespaceAttribute>();
            var namespaceAttr2 = namespaceAttributes.FirstOrDefault((attr) => attr.ClrNamespace == type.Namespace);

            return namespaceAttr2 == null ? null : namespaceAttr2.ContractNamespace;
        }

        private static string GetDataContractName(Type type) {
            var dataContractAttributes = type.GetCustomAttributes(typeof(DataContractAttribute), true).OfType<DataContractAttribute>();
            var nameAttr = dataContractAttributes.FirstOrDefault((attr) => attr.Name != null);

            return nameAttr == null ? null : nameAttr.Name;
        }

        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver) {
            var assemblies = GetNamespaceAssemblies(typeNamespace);

            foreach (var assembly in assemblies) {
                var type = assembly.GetTypes().Where((t) => IsTypeContractMatch(t, typeName, typeNamespace)).SingleOrDefault();

                if (type != null) {
                    return type;
                }
            }

            return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
        }

        private static bool IsTypeContractMatch(Type type, string typeName, string typeNamespace) {
            return GetDataContractName(type) == typeName && GetDataContractNamespace(type) == typeNamespace;
        }

        private IEnumerable<Assembly> GetNamespaceAssemblies(string typeNamespace) {
            // TODO Clean up

            var extension = this.extensionManager.Extensions.Where((ext) => ext.Namespace == typeNamespace).SingleOrDefault();

            if (extension == null) {
                // Not a type defined in an extension; test NoCap.Library.
                if (typeNamespace == "http://strager.net/nocap/lib") {
                    return new[] { typeof(NoCap.Library.ICommand).Assembly };
                }

                if (typeNamespace == "http://strager.net/nocap/gui") {
                    return new[] { typeof(App).Assembly };
                }

                return Enumerable.Empty<Assembly>();
            }

            return extension.Assemblies;
        }
    }
}