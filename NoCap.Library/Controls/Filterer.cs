using System;
using System.Windows.Data;

namespace NoCap.Library.Controls {
    class Filterer {
        private object source;

        public object Source {
            get {
                return this.source;
            }

            set {
                this.source = value;

                this.viewSource.Source = this.source;
            }
        }

        public BindingBase SourceBinding {
            get {
                return new Binding {
                    Source = this.viewSource
                };
            }
        }

        private readonly CollectionViewSource viewSource;

        public Filterer() {
            this.viewSource = new CollectionViewSource();
            this.viewSource.Filter += FilterItem;
        }

        private Predicate<object> filter;

        public Predicate<object> Filter {
            get {
                return this.filter;
            }

            set {
                this.filter = value;

                Refresh();
            }
        }

        public void Refresh() {
            if (this.viewSource.View != null) {
                this.viewSource.View.Refresh();
            }
        }

        private void FilterItem(object sender, FilterEventArgs e) {
            if (Filter == null) {
                e.Accepted = true;

                return;
            }

            e.Accepted = Filter(e.Item);
        }
    }
}
