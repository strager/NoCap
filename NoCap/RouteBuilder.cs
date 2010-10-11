using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NoCap.Library;
using NoCap.Library.Destinations;
using NoCap.Library.Sources;
using NoCap.Plugins;

namespace NoCap
{
    public partial class RouteBuilder : Form {
#pragma warning disable 649
        [ImportMany]
        private IEnumerable<ISource> sources;

        [ImportMany]
        private IEnumerable<IDestination> destinations;
#pragma warning restore 649

        private object dummyValue = new object();
        private TreeNode rootNode;

        public RouteBuilder() {
            InitializeComponent();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ClipboardSource).Assembly));

            var container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);

            this.rootNode = MakeNode("root");
            this.routes.Nodes.Add(this.rootNode);
        }

        private void RouteExpanded(object sender, TreeViewEventArgs e) {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Tag == dummyValue) {
                e.Node.Nodes.Clear();

                var parent = e.Node.Parent;

                if (parent == null) {
                    e.Node.Nodes.AddRange(sources.Select(MakeNode).ToArray());
                } else {
                    e.Node.Nodes.AddRange(
                        GetNodeTypes(e.Node).SelectMany(
                            (type) => this.destinations.Where(
                                (dest) => dest.GetInputDataTypes().Contains(type)
                            )
                        ).Unique().Select(MakeNode).ToArray()
                    );
                }
            }
        }

        private TreeNode MakeNode(object data) {
            var node = new TreeNode(data.ToString()) {
                Tag = data
            };

            var dummy = new TreeNode("Loading ...") {
                Tag = dummyValue
            };

            node.Nodes.Add(dummy);

            return node;
        }

        private IEnumerable<TypedDataType> GetNodeTypes(TreeNode node) {
            var source = node.Tag as ISource;

            if (source != null) {
                return source.GetOutputDataTypes();
            }

            var destination = node.Tag as IDestination;

            if (destination == null) {
                return new TypedDataType[] { };
            }

            return GetNodeTypes(node.Parent).SelectMany(destination.GetOutputDataTypes).Unique();
        }
    }
}
