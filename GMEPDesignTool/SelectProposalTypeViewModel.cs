using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class SelectProposalTypeViewModel
    {
        private int typeId;
        public int TypeId
        {
            get { return typeId; }
            set
            {
                if (typeId != value)
                {
                    typeId = value;
                }
            }
        }

        public SelectProposalTypeViewModel()
        {
            typeId = 0;
        }
    }
}
