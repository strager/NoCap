using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using NoCap.GUI.WPF.Runtime;

namespace NoCap.GUI.WPF.Util {
    class ExtensionDataContractResolver : DataContractResolver {
        private const string ClrNamespacePrefix = @"http://schemas.datacontract.org/2004/07/";

        private readonly XmlDictionary dictionary = new XmlDictionary();
        private readonly ExtensionManager extensionManager;

        public ExtensionDataContractResolver(ExtensionManager extensionManager) {
            this.extensionManager = extensionManager;
        }

        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace) {
            return TryResolveDataContractType(type, declaredType, out typeName, out typeNamespace)
                || knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace)
                || TryResolveClrType(type, declaredType, out typeName, out typeNamespace);
        }

        private bool TryResolveClrType(Type type, Type declaredType, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace) {
            if (type.FullName == null) {
                typeName = null;
                typeNamespace = null;

                return false;
            }

            typeName = this.dictionary.Add(type.FullName);
            typeNamespace = this.dictionary.Add(ClrNamespacePrefix + type.Assembly.GetName().Name);

            return true;
        }

        private bool TryResolveDataContractType(Type type, Type declaredType, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace) {
            string typeNameString = GetDataContractName(type);
            string typeNamespaceString = GetDataContractNamespace(type);

            if (typeNameString == null || typeNamespaceString == null) {
                typeName = null;
                typeNamespace = null;

                return false;
            }

            typeName = this.dictionary.Add(typeNameString);
            typeNamespace = this.dictionary.Add(typeNamespaceString);

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
            return ResolveRegisteredNamespaceName(typeName, typeNamespace, declaredType)
                ?? knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver)
                ?? ResolveClrName(typeName, typeNamespace, declaredType);
        }

        private Type ResolveClrName(string typeName, string typeNamespace, Type declaredType) {
            // This is quite the piece of code...
            // Hopefully this won't break stuff.
            // TODO FIXME

            if (!typeNamespace.StartsWith(ClrNamespacePrefix)) {
                return null;
            }

            string assemblyName = typeNamespace.Substring(ClrNamespacePrefix.Length);

            var goodAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where((assembly) => assembly.GetName().Name == assemblyName);

            // HACK
            goodAssemblies = goodAssemblies.Union(this.extensionManager.Extensions.Select(
                (extension) => {
                    var assemblyFileName = Path.Combine(extension.RootDirectory, string.Format("{0}.dll", assemblyName));

                    return File.Exists(assemblyFileName) ? Assembly.LoadFile(assemblyFileName) : null;
                }
            ).Where((assembly) => assembly != null));

            var types = goodAssemblies.Select((assembly) => assembly.GetType(typeName, false)).Where((type) => type != null);

            if (types.Count() > 1) {
                throw new Exception(string.Format(
                    "Could not load deserialize object of type '{0}' from assembly '{1}' because multiple loaded assemblies have that name'",
                    typeName,
                    typeNamespace
                ));
            }

            return types.SingleOrDefault();
        }

        private Type ResolveRegisteredNamespaceName(string typeName, string typeNamespace, Type declaredType) {
            var assemblies = GetNamespaceAssemblies(typeNamespace);

            foreach (var assembly in assemblies) {
                var type = assembly.GetTypes().Where((t) => IsTypeContractMatch(t, typeName, typeNamespace)).SingleOrDefault();

                if (type != null) {
                    return type;
                }
            }

            return null;
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