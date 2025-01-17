using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPDesignTool
{
    class ExampleViewModel : IDropTarget
    {
        public ObservableCollection<ElectricalEquipment> ElectricalEquipments;

        /*void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            ExampleItemViewModel sourceItem = dropInfo.Data as ExampleItemViewModel;
            ExampleItemViewModel targetItem = dropInfo.TargetItem as ExampleItemViewModel;

            if (sourceItem != null && targetItem != null && targetItem.CanAcceptChildren)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            ExampleItemViewModel sourceItem = dropInfo.Data as ExampleItemViewModel;
            ExampleItemViewModel targetItem = dropInfo.TargetItem as ExampleItemViewModel;
            targetItem.Children.Add(sourceItem);
        }*/
    }
}
