using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TreeViewVirtualizingDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// ItemsSource for the TreeView
        /// </summary>
        public BindingList<MenuItem> TreeViewData { get; set; } = new BindingList<MenuItem>();

        public MainWindow()
        {
            string[] words = { "Alpha", "Bravo", "Charlie", "Its", "Over", "Juan", "Also", "Carla", "After", "Skate", "Two" };
            Random random = new Random();
            DataContext = this;
            InitializeComponent();
            MenuItem root = new MenuItem() { Title = @"C:\Users\Stack\File.txt" };
            for (int i = 0; i < 50; i++)
            {
                //Make a random piece of text
                string header = words.OrderBy(x => random.Next()).Take(3).Aggregate((x, y) => x + y);
                MenuItem childItem = new MenuItem() { Title = i + " " + header, Parent = root };
                int randSubs = random.Next(8);
                for (int j = 0; j < randSubs; j++)
                {
                    header = words.OrderBy(x => random.Next()).Take(4).Aggregate((x, y) => x + "_" + y);
                    childItem.Items.Add(new MenuItem() { Title = j + " " + header, Parent = childItem });
                }
                root.Items.Add(childItem);
            }
            TreeViewData.Add(root);
        }

        /// <summary>
        /// Search Depth-First for items in the tree that match the text in the searchbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search(object sender, RoutedEventArgs e)
        {
            string searchTerm = searchBox.Text.ToLower();
            MenuItem selectedNode = (MenuItem)tView.SelectedItem;
            var items = TreeViewData[0].FlattenTree();
            int pos = selectedNode == null ? -1 : items.IndexOf(selectedNode);
            pos += 1; //search from 1 forward
            for (int i = 0; i < items.Count; i++)
            {
                int curIndex = (i + pos) % items.Count;
                MenuItem node = items[(i + pos) % items.Count];
                if (node.Parent == null)
                {
                    //skip the root node
                    continue;
                }
                if (node.Title.ToLower().Contains(searchTerm))
                {
                    node.IsSelected = true;
                    FocusTreeViewNode(node);
                    break;
                }
            }
        }

        /// <summary>
        /// Focus and bring into view the selected node - this is where the error occurs
        /// </summary>
        /// <param name="node">Node to focus and select</param>
        private void FocusTreeViewNode(MenuItem node)
        {
            if (node == null) return;
            var nodes = (IEnumerable<MenuItem>)tView.ItemsSource;
            if (nodes == null) return;

            var stack = new Stack<MenuItem>();
            stack.Push(node);
            var parent = node.Parent;
            while (parent != null)
            {
                stack.Push(parent);
                parent = parent.Parent;
            }

            var generator = tView.ItemContainerGenerator;
            while (stack.Count > 0)
            {
                var dequeue = stack.Pop();
                tView.UpdateLayout();

                var treeViewItem = (TreeViewItem)generator.ContainerFromItem(dequeue);
                if (stack.Count > 0)
                {
                    treeViewItem.IsExpanded = true;
                }
                else
                {
                    if (treeViewItem == null)
                    {
                        //This is being triggered when it shouldn't be
                        Debugger.Break();
                    }
                    treeViewItem.IsSelected = true;
                }
                treeViewItem.BringIntoView();
                generator = treeViewItem.ItemContainerGenerator;
            }
        }
    }

    /// <summary>
    /// Backing datatype for binding
    /// </summary>
    [DebuggerDisplay("MenuItem | {Title}")]
    public class MenuItem : INotifyPropertyChanged
    {
        public MenuItem Parent;
        public string Title { get; set; }

        public MenuItem()
        {
            this.Items = new ObservableCollection<MenuItem>();
        }

        /// <summary>
        /// Flattens the tree into depth first order. Use this method for searching the list.
        /// </summary>
        /// <returns></returns>
        public List<MenuItem> FlattenTree()
        {
            List<MenuItem> nodes = new List<MenuItem>();
            nodes.Add(this);
            foreach (MenuItem tve in Items)
            {
                nodes.AddRange(tve.FlattenTree());
            }
            return nodes;
        }


        public ObservableCollection<MenuItem> Items { get; set; }
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }
}
