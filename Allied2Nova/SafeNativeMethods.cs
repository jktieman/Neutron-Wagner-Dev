using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace Allied2Nova
{
    [SuppressUnmanagedCodeSecurityAttribute]
    public static class SafeNativeMethods
    {

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.I4)]
        unsafe internal static extern int WRITEALLNOVARECS();

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.I4)]
        unsafe public static extern int GETNOVARECS(
            [MarshalAs(UnmanagedType.AnsiBStr)] ref string filename);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.I4)]
        unsafe internal static extern int CREATECSV(
            [MarshalAs(UnmanagedType.AnsiBStr)] ref string novaname,
            [MarshalAs(UnmanagedType.AnsiBStr)] ref string filename);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe public static extern string GETNAME();

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe public static extern string GETHISTORYBYORDER(
        [MarshalAs(UnmanagedType.AnsiBStr)] ref string order);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETORDER(
        [MarshalAs(UnmanagedType.AnsiBStr)] ref string order);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETORDERS();

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string GETFIRSTORDER();

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe public static extern string GETNEXTORDER();

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETDETAILRECS();

        //[DllImport("NOVAINTERFACE.DLL")]
        //[return: MarshalAs(UnmanagedType.AnsiBStr)]
        //unsafe public static extern string GETNOVARECS();

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETCDEFRECS();


        // HA Sku's in Order Control Records includes Replenishments
        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETHADETAILRECS();


        // Carousel DEfinition Records
        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETHACDEFRECS();

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETCDEFRECORD(
            [MarshalAs(UnmanagedType.AnsiBStr)]ref string SKU);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.I1)]
        unsafe internal static extern bool SAVECDEFRECORD(
            [MarshalAs(UnmanagedType.AnsiBStr)]ref string cdefRec);


        // OFF Carousel Defintion Records
        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETHAOCDEFRECS();

        // OFF Carousel Defintion Records
        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETOCDEFRECS();

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETOCDEFRECORD(
        [MarshalAs(UnmanagedType.AnsiBStr)]ref string SKU);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.I1)]
        unsafe internal static extern bool SAVEOCDEFRECORD(
        [MarshalAs(UnmanagedType.AnsiBStr)]ref string ocdefRec);


        // Carousel NOVA SKU Records  Sku, Location, Qty
        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETNOVARECORD(
        [MarshalAs(UnmanagedType.AnsiBStr)]ref string SKU);

        // Carousel NOVA SKU Records  Sku, Location, Qty
        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe public static extern string GETNOVARECSBYSKU(
         [MarshalAs(UnmanagedType.AnsiBStr)] ref string filename,
         [MarshalAs(UnmanagedType.AnsiBStr)]ref string sku);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.I1)]
        unsafe internal static extern bool SAVENOVARECORD(
        [MarshalAs(UnmanagedType.AnsiBStr)]ref string nRec);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETHANOVARECS();

        // OFF Carousel Reserve Records  Sku, Location, Qty
        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETRESERVERECORD(
        [MarshalAs(UnmanagedType.AnsiBStr)]ref string SKU);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.I1)]
        unsafe internal static extern bool SAVERESERVERECORD(
        [MarshalAs(UnmanagedType.AnsiBStr)]ref string nRec);

        [DllImport("NOVAINTERFACE.DLL")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETHARESERVERECS();

        [DllImport("NOVAINTERFACE.DLL", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        unsafe internal static extern string GETRESERVERECS();

        [DllImport("NOVAINTERFACE.DLL", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]

        public static extern string INITOCONLY();
    }
}
