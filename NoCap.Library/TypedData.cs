using System;
using System.Drawing;
using System.IO;

namespace NoCap.Library {
    /// <summary>
    /// The type of a <see cref="TypedData"/> instance.
    /// </summary>
    public enum TypedDataType {
        /// <summary>Null data.</summary>
        None = 0,

        /// <summary><see cref="TypedData.Data"/> is an instance of <see cref="System.Drawing.Image"/>.</summary>
        Image = 1,

        /// <summary><see cref="TypedData.Data"/> is an instance of <see cref="System.String"/>.</summary>
        Text = 2,

        /// <summary><see cref="TypedData.Data"/> is an instance of <see cref="System.Uri"/>.</summary>
        Uri = 3,

        /// <summary><see cref="TypedData.Data"/> is an instance of <see cref="System.IO.Stream"/>.</summary>
        Stream = 4,

        // Add new types here

        /// <summary>
        /// A user-defined type.  Implementors which wish to define more data
        /// types must use enum values no less than <see cref="User"/>.
        /// </summary>
        User = 9001
    }

    public sealed class TypedData : IDisposable {
        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        public TypedDataType DataType {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the data stored in the reference.
        /// </summary>
        /// <value>The data with the type described by <see cref="DataType"/>.</value>
        public object Data {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the name of the data.
        /// </summary>
        /// <remarks>
        /// The name has no true semantic value.  It is purely intended for
        /// user convenience.
        /// </remarks>
        /// <value>The name of the data.</value>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedData"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="data">The data with the type described by <paramref name="dataType"/>.</param>
        /// <param name="name">The name of the data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        public TypedData(TypedDataType dataType, object data, string name) {
            if (data == null && dataType != TypedDataType.None) {
                throw new ArgumentNullException("data");
            }

            if (name == null) {
                throw new ArgumentNullException("name");
            }

            DataType = dataType;
            Data = data;
            Name = name;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TypedData"/>
        /// class representing the given URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="name">The name.</param>
        /// <returns>A new TypedData instance.</returns>
        public static TypedData FromUri(string uri, string name) {
            return FromUri(new Uri(uri), name);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="TypedData"/>
        /// class representing the given URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="name">The name.</param>
        /// <returns>A new TypedData instance.</returns>
        public static TypedData FromUri(Uri uri, string name) {
            return new TypedData(TypedDataType.Uri, uri, name);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="TypedData"/>
        /// class representing the given image.
        /// </summary>
        /// <param name="image">The Image instance.</param>
        /// <param name="name">The name.</param>
        /// <returns>A new TypedData instance.</returns>
        public static TypedData FromImage(Image image, string name) {
            return new TypedData(TypedDataType.Image, image, name);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="TypedData"/>
        /// class representing the given string.
        /// </summary>
        /// <param name="text">The string data.</param>
        /// <param name="name">The name.</param>
        /// <returns>A new TypedData instance.</returns>
        public static TypedData FromText(string text, string name) {
            return new TypedData(TypedDataType.Text, text, name);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="TypedData"/>
        /// class representing the given raw data.
        /// </summary>
        /// <remarks>
        /// <paramref name="stream"/> will be disposed when this instance is disposed.
        /// </remarks>
        /// <param name="stream">The stream.</param>
        /// <param name="name">The name.</param>
        /// <returns>A new TypedData instance.</returns>
        public static TypedData FromStream(Stream stream, string name) {
            return new TypedData(TypedDataType.Stream, stream, name);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() {
            return string.Format("{0} ({1}: {2})", Name, DataType, Data);
        }

        /// <summary>
        /// Disposes of <see cref="Data"/> if possible and marks this instance as disposed.
        /// </summary>
        public void Dispose() {
            // TODO Mark this instance as disposed.  =X
            var dataAsDisposable = Data as IDisposable;

            if (dataAsDisposable != null) {
                dataAsDisposable.Dispose();
            }
        }

        [Obsolete]
        public object CloneData() {
            // TODO Allow smart cloning or something (e.g. register a cloner?)
            switch (DataType) {
                case TypedDataType.Image:
                    return ((Image) Data).Clone();

                case TypedDataType.Stream:
                    throw new InvalidOperationException("Don't know how to clone a stream");

                default:
                    return Data;
            }
        }
    }
}
