using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalEquipment
    {
        private string Owner;
        private string EquipNo;
        private int Qty;
        private string PanelId;
        private int Voltage;
        private float Amp;
        private float Va;
        private bool Is3Ph;
        private string SpecSheetId;

        public ElectricalEquipment(
            string owner,
            string equipNo,
            int qty,
            string panelId,
            int voltage,
            float amp,
            float va,
            bool is3Ph,
            string specSheetId
        )
        {
            Owner = owner;
            EquipNo = equipNo;
            Qty = qty;
            PanelId = panelId;
            Voltage = voltage;
            Amp = amp;
            Va = va;
            Is3Ph = is3Ph;
            SpecSheetId = specSheetId;
        }

        public bool Verify()
        {
            if (!Utils.IsOwnerName(Owner))
            {
                return false;
            }
            if (!Utils.IsEquipName(EquipNo))
            {
                return false;
            }
            if (!Utils.IsUuid(PanelId))
            {
                return false;
            }
            if (!Utils.IsUuid(SpecSheetId))
            {
                return false;
            }
            return true;
        }
    }
}
