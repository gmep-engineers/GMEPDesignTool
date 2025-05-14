using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class ElectricalPanelMiniBreakerViewModel : ViewModelBase
    {
        public IEnumerable<ElectricalComponent> UnassignedEquipment { get; set; }
        public string PanelId { get; set; }
        public string PanelName { get; set; }
        public int CircuitNo { get; set; }

        private string _EquipAId = string.Empty;
        public string EquipAId
        {
            get => _EquipAId;
            set
            {
                _EquipAId = value;
                Circuit.MiniBreakerEquipAId = value;
            }
        }

        private string _EquipBId = string.Empty;
        public string EquipBId
        {
            get => _EquipBId;
            set
            {
                _EquipBId = value;
                Circuit.MiniBreakerEquipBId = value;
            }
        }

        private int _BreakerSizeA;
        public int BreakerSizeA
        {
            get => _BreakerSizeA;
            set
            {
                _BreakerSizeA = value;
                Circuit.MiniBreakerSizeA = value;
            }
        }

        private int _BreakerSizeB;
        public int BreakerSizeB
        {
            get => _BreakerSizeB;
            set
            {
                _BreakerSizeB = value;
                Circuit.MiniBreakerSizeB = value;
            }
        }

        private bool _InterlockAToNextB;
        public bool InterlockAToNextB
        {
            get => _InterlockAToNextB;
            set
            {
                _InterlockAToNextB = value;
                Circuit.MiniBreakerInterlockA = value;
            }
        }

        private bool _InterlockBToNextA;
        public bool InterlockBToNextA
        {
            get => _InterlockAToNextB;
            set
            {
                _InterlockBToNextA = value;
                Circuit.MiniBreakerInterlockB = value;
            }
        }
        Database.Database GmepDb { get; set; }

        private Circuit Circuit;

        public string Id { get; set; }

        public ElectricalPanelMiniBreakerViewModel(
            Database.Database database,
            IEnumerable<ElectricalComponent> unassignedEquipment,
            string panelId,
            string panelName,
            Circuit circuit
        )
        {
            GmepDb = database;
            UnassignedEquipment = unassignedEquipment;
            PanelId = panelId;
            PanelName = panelName;
            Circuit = circuit;
            CircuitNo = circuit.Number;
            EquipAId = circuit.MiniBreakerEquipAId;
            EquipBId = circuit.MiniBreakerEquipBId;
            BreakerSizeA = circuit.MiniBreakerSizeA;
            BreakerSizeB = circuit.MiniBreakerSizeB;
            InterlockAToNextB = circuit.MiniBreakerInterlockA;
            InterlockBToNextA = circuit.MiniBreakerInterlockB;
            Id = GmepDb.GetElectricalPanelMiniBreakerId(PanelId, CircuitNo);
            if (string.IsNullOrEmpty(Id))
            {
                Id = GmepDb.CreateElectricalPanelMiniBreaker(PanelId, CircuitNo);
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return;
            }
            GmepDb.UpdateElectricalPanelMiniBreaker(
                Id,
                EquipAId,
                EquipBId,
                BreakerSizeA,
                BreakerSizeB,
                InterlockAToNextB,
                InterlockBToNextA
            );
        }

        public void Delete()
        {
            GmepDb.DeleteElectricalPanelMiniBreaker(Id);
            Id = string.Empty;
        }
    }
}
