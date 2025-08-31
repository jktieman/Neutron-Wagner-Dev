using System.Collections.Generic;

namespace Hanel_DC.HanelStatics
{
    public class HanelDcStatics
    {
        public static IReadOnlyCollection<string> Valid_Controller_Types() => new List<string>
        {
            Controller_Type_Hanel_Mp12D(),
            Controller_Type_Hanel_Mp12N(),

        }.AsReadOnly();

        public static string Controller_Type_Hanel_Mp12D() => "Hanel MP12D";

        public static string Controller_Type_Hanel_Mp12N() => "Hanel MP12N";

    }
}
