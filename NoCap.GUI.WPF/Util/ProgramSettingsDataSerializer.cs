using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using Bindable.Linq;
using System.Linq;
using NoCap.GUI.WPF.Runtime;
using NoCap.GUI.WPF.Settings;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF.Util {
    class ProgramSettingsDataSerializer {
        private readonly ExtensionManager extensionManager;

        public ProgramSettingsDataSerializer(ExtensionManager extensionManager) {
            this.extensionManager = extensionManager;
        }

        public static XmlDocument WriteToDocument(string data) {
            var document = new XmlDocument();
            document.LoadXml(data);

            return document;
        }

        private XmlObjectSerializer GetSettingsSerializer() {
            return new DataContractSerializer(
                typeof(ProgramSettingsData),
                Enumerable.Empty<Type>(),
                int.MaxValue,
                false,
                true,
                new MySurrogate(),
                new MyDataContractResolver(this.extensionManager)
            );
        }

        public string SerializeSettings(ProgramSettingsData settings) {
            var serializer = GetSettingsSerializer();

            return Serialize(settings, serializer);
        }

        public ProgramSettingsData DeserializeSettings(string configData) {
            var deserializer = GetSettingsSerializer();

            return (ProgramSettingsData) Deserialize(configData, deserializer);
        }

        private string Serialize(object obj, XmlObjectSerializer serializer) {
            var output = new StringBuilder();

            var xmlSettings = new XmlWriterSettings {
                Indent = true
            };

            using (var writer = XmlWriter.Create(output, xmlSettings)) {
                try {
                    serializer.WriteObject(writer, obj);
                } catch (SerializationException e) {
                    // TODO Error handling
                    throw;
                }
            }

            return output.ToString();
        }

        private object Deserialize(string data, XmlObjectSerializer serializer) {
            using (var stringReader = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            using (var xmlReader = XmlDictionaryReader.CreateTextReader(stringReader, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null)) {
                try {
                    return serializer.ReadObject(xmlReader);
                } catch (SerializationException e) {
                    // TODO Error handling
                    throw;
                }
            }
        }

        public static T Clone<T>(T obj) {
            var cloner = new BinaryFormatter {
                Context = new StreamingContext(StreamingContextStates.Clone)
            };

            using (var stream = new MemoryStream()) {
                cloner.Serialize(stream, obj);

                stream.Position = 0;

                return (T) cloner.Deserialize(stream);
            }
        }
    }

    class MyDataContractResolver : DataContractResolver {
        private readonly ExtensionManager extensionManager;

        public MyDataContractResolver(ExtensionManager extensionManager) {
            this.extensionManager = extensionManager;
        }

        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace) {
            var dictionary = new XmlDictionary();

            var dataContractAttributes = type.GetCustomAttributes(typeof(DataContractAttribute), true).OfType<DataContractAttribute>();
            var nameAttr = dataContractAttributes.FirstOrDefault((attr) => attr.Name != null);

            if (nameAttr == null) {
                goto defaultLookup;
            }

            typeName = dictionary.Add(nameAttr.Name);

            var namespaceAttr = dataContractAttributes.FirstOrDefault((attr) => attr.Namespace != null);

            if (namespaceAttr == null) {
                var namespaceAttributes = type.Assembly.GetCustomAttributes(typeof(ContractNamespaceAttribute), false).OfType<ContractNamespaceAttribute>();

                var namespaceAttr2 = namespaceAttributes.FirstOrDefault((attr) => attr.ClrNamespace == type.Namespace);

                if (namespaceAttr2 == null) {
                    goto defaultLookup;;
                }

                typeNamespace = dictionary.Add(namespaceAttr2.ContractNamespace);
            } else {
                typeNamespace = dictionary.Add(namespaceAttr.Namespace);
            }

            return true;

            defaultLookup:

            return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
        }

        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver) {
            var assemblies = GetNamespaceAssemblies(typeNamespace);

            if (assemblies == null) {
                goto defaultLookup;
            }

            foreach (var assembly in assemblies) {
                var type = assembly.GetTypes().Where((t) => t.Name == typeName).SingleOrDefault();

                if (type != null) {
                    return type;
                }
            }

            defaultLookup:

            return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
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

    class MySurrogate : IDataContractSurrogate {
        public Type GetDataContractType(Type type) {
            return type;
        }

        public object GetObjectToSerialize(object obj, Type targetType) {
            // TODO Round-trip dummy

            if (obj is Type) {
                var type = (Type) obj;

                return type.AssemblyQualifiedName;
            }

            return obj;
        }

        public object GetDeserializedObject(object obj, Type targetType) {
            // TODO Round-trip dummy

            return obj;
        }

        public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType) {
            throw new NotSupportedException();
        }

        public object GetCustomDataToExport(Type clrType, Type dataContractType) {
            throw new NotSupportedException();
        }

        public void GetKnownCustomDataTypes(Collection<Type> customDataTypes) {
            // TODO?
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData) {
            return null;
        }

        public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit) {
            return typeDeclaration;
        }
    }

    class MySurrogateSelector : ISurrogateSelector {
        private ISurrogateSelector nextSelector;

        public void ChainSelector(ISurrogateSelector selector) {
            this.nextSelector = selector;
        }

        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector) {
            if (type.IsGenericType && type.GetInterfaces().Contains(typeof(IBindableCollection))) {
                selector = this;

                return new BindableCollectionSurrogate();
            }

            selector = null;

            return this.nextSelector == null
                ? null
                : this.nextSelector.GetSurrogate(type, context, out selector);
        }

        public ISurrogateSelector GetNextSelector() {
            return this.nextSelector;
        }
    }

    class BindableCollectionSurrogate : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            var bindableCollection = (IBindableCollection) obj;

            var items = new ArrayList(bindableCollection.Count);

            foreach (var item in bindableCollection) {
                items.Add(item);
            }

            info.AddValue("Items", items);
            info.AddValue("ItemType", obj.GetType());
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            var items = info.GetValue<IEnumerable>("Items");
            var type = info.GetValue<Type>("ItemType");

            dynamic bindableCollection = Activator.CreateInstance(type, new object[] { });

            foreach (var item in items) {
                bindableCollection.Add(item);
            }

            return bindableCollection;
        }
    }
}