using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GMEPDesignTool
{
    public class ElectricalPanelMiniBreakerViewModel : ViewModelBase
    {
        public ObservableCollection<ElectricalComponent> UnassignedEquipment { get; set; }
        public ObservableCollection<ElectricalComponent> EquipmentOptions { get; set; }
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

        private bool _InterlockAToNextB = false;
        public bool InterlockAToNextB
        {
            get => _InterlockAToNextB;
            set
            {
                _InterlockAToNextB = value;
                Circuit.MiniBreakerInterlockA = value;
            }
        }

        private bool _InterlockBToNextA = false;
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

        private Circuit? NextCircuit;

        public string Id { get; set; }

        public ElectricalPanelMiniBreakerViewModel(
            Database.Database database,
            ObservableCollection<ElectricalComponent> unassignedEquipment,
            string panelId,
            string panelName,
            Circuit circuit,
            Circuit? nextCircuit
        )
        {
            GmepDb = database;
            UnassignedEquipment = unassignedEquipment;
            EquipmentOptions = new ObservableCollection<ElectricalComponent>();
            foreach (ElectricalComponent component in unassignedEquipment)
            {
                if (component.GetType() == typeof(ElectricalEquipment))
                {
                    ElectricalEquipment eq = (ElectricalEquipment)component;
                    if (eq.Pole == 1 || eq.Pole == 2)
                    {
                        EquipmentOptions.Add(component);
                    }
                }
            }
            PanelId = panelId;
            PanelName = panelName;
            Circuit = circuit;
            NextCircuit = nextCircuit;
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
            string descA = string.Empty;
            string descB = string.Empty;
            int vaA = 0;
            int vaB = 0;
            int voltA = 0;
            int voltB = 0;
            (descA, descB, vaA, vaB, voltA, voltB) = GmepDb.UpdateElectricalPanelMiniBreaker(
                Id,
                EquipAId,
                EquipBId,
                BreakerSizeA,
                BreakerSizeB,
                InterlockAToNextB,
                InterlockBToNextA,
                CircuitNo
            );
            if (NextCircuit != null)
            {
                if (InterlockBToNextA || InterlockAToNextB)
                {
                    string nextId = GmepDb.CreateElectricalPanelMiniBreaker(
                        PanelId,
                        NextCircuit.Number
                    );
                    string nextEquipAId = "";
                    string nextEquipBId = "";

                    if (voltA == 3 || voltA == 4 || voltA == 5)
                    {
                        nextEquipBId = EquipAId;
                        vaA = vaA / 2;
                    }
                    if (voltB == 3 || voltB == 4 || voltB == 5)
                    {
                        nextEquipAId = EquipBId;
                        vaB = vaB / 2;
                    }
                    GmepDb.UpdateElectricalPanelMiniBreaker(
                        nextId,
                        nextEquipAId,
                        nextEquipBId,
                        BreakerSizeB,
                        BreakerSizeA,
                        false,
                        false,
                        NextCircuit.Number
                    );
                    NextCircuit.Description = $"{descB};{descA}";
                    NextCircuit.Va = vaA + vaB;
                    NextCircuit.BreakerSize = 2020;
                }
            }

            Circuit.Description = $"{descA};{descB}";
            Circuit.Va = vaA + vaB;
            Circuit.BreakerSize = 2020;
            int i = 0;
            while (i < UnassignedEquipment.Count())
            {
                if (UnassignedEquipment[i].Id == EquipAId)
                {
                    UnassignedEquipment[i].CircuitNo = CircuitNo;
                    UnassignedEquipment[i].circuitHalf = 1;
                    UnassignedEquipment.RemoveAt(i);
                }
                else if (UnassignedEquipment[i].Id == EquipBId)
                {
                    UnassignedEquipment[i].CircuitNo = CircuitNo;
                    UnassignedEquipment[i].circuitHalf = 2;
                    UnassignedEquipment.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        public void Delete()
        {
            GmepDb.DeleteElectricalPanelMiniBreaker(PanelId, CircuitNo);
            Id = string.Empty;
            Circuit.Description = "Space";
            Circuit.Va = 0;
            Circuit.BreakerSize = 0;
        }
    }
}
