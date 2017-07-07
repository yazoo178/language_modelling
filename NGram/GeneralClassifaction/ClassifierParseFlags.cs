using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    [Flags]
    enum ClassifierParseFlags
    {
        XmlClassiferParse = 0x0000001,
        ForceFlat = 0x0000002,
        RawStringData = 0x0000002
    }
}
