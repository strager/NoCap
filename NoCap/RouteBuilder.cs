using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows.Forms;
using NoCap.Library;
using NoCap.Library.Util;
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

        private readonly object dummyValue = new object();
        private readonly TreeNode rootNode;

        public RouteBuilder() {
            InitializeComponent();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ImageBinUploader).Assembly));

            var container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);

            this.rootNode = MakeNode("root");
            this.routes.Nodes.Add(this.rootNode);
        }

        private void RouteExpanded(object sender, TreeViewEventArgs e) {
            if (e.Node.Nodes.Count != 1 || e.Node.Nodes[0].Tag != this.dummyValue) {
                return;
            }

            e.Node.Nodes.Clear();

            var parent = e.Node.Parent;

            if (parent == null) {
                e.Node.Nodes.AddRange(this.sources.Select(MakeNode).ToArray());
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
