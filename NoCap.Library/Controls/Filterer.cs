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

                // HACK We delay because blah blah blah
                this.viewSource.Dispatcher.BeginInvoke(new Action(() => {
                    this.viewSource.Source = this.source;
                }));
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

                if (this.viewSource.View != null) {
                    this.viewSource.View.Refresh();
                }
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
