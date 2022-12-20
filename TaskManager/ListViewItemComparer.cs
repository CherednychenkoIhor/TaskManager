using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskManager
{
     class ListViewItemComparer : IComparer
    { 
        private int _columnIndex;

        public int ColumIndex {
            get {
                return _columnIndex;
            }
            set {
                _columnIndex = value;
            }
        }

        private SortOrder _sortDirection;
        public SortOrder SortDirection {
            get {
                return _sortDirection;
                }
            set {
                _sortDirection = value;
                }
        }

        public ListViewItemComparer() {
            _sortDirection = SortOrder.None;
        }
      
        int IComparer.Compare(object? first, object? second)
        {
            ListViewItem listViewItemFirst = first as ListViewItem;
            ListViewItem listViewItemSecond = second as ListViewItem;
            int result;
            switch (_columnIndex) {
                case 0:
                    result = string.Compare(listViewItemFirst.SubItems[_columnIndex].Text, listViewItemSecond.SubItems[_columnIndex].Text, false);
                    break;
                case 1:
                    double valueFirst = double.Parse(listViewItemFirst.SubItems[_columnIndex].Text);
                    double valueSecond = double.Parse(listViewItemSecond.SubItems[_columnIndex].Text);
                    result = valueFirst.CompareTo(valueSecond);
                    break;
                case 2:
                    valueFirst = double.Parse(listViewItemFirst.SubItems[_columnIndex].Text);
                    valueSecond = double.Parse(listViewItemSecond.SubItems[_columnIndex].Text);
                    result = valueFirst.CompareTo(valueSecond);
                    break;
                case 3:
                    valueFirst = double.Parse(listViewItemFirst.SubItems[_columnIndex].Text);
                    valueSecond = double.Parse(listViewItemSecond.SubItems[_columnIndex].Text);
                    result = valueFirst.CompareTo(valueSecond);
                    break;
                default:
                    result = string.Compare(listViewItemFirst.SubItems[_columnIndex].Text, listViewItemSecond.SubItems[_columnIndex].Text, false);
                    break;

            }
            return _sortDirection == SortOrder.Descending ? -result : result;
        }
    }
}
