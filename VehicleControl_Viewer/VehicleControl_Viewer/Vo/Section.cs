using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace VehicleControl_Viewer.Vo
{
    public class Section
    {
        public string ID;
        public string StartArd, EndArd;

        public Address StartAddress;
        public Address EndAddress;

        public Section(List<Address> adrs, string id, string startAdr, string endAdr)
        {
            ID = id;
            StartArd = startAdr;
            EndArd = endAdr;
            StartAddress = adrs.Where(adr => adr.ID.Trim() == startAdr.Trim()).FirstOrDefault();
            if(StartAddress == null)
                Console.WriteLine($"adr id:{startAdr} not exist");

            EndAddress = adrs.Where(adr => adr.ID.Trim() == endAdr.Trim()).FirstOrDefault();
            if (EndAddress == null)
                Console.WriteLine($"adr id:{endAdr} not exist");
        }
        public Section(string id, Address startAdr, Address endAdr)
        {
            ID = id;
            StartAddress = startAdr;
            EndAddress = endAdr;

        }
    }
}