using System.IO;

namespace cashes_activity
{
    class ReadConfig
    {
        public static string[] ReadRestrauntsData()
        {
            return File.ReadAllLines(@"C:\scripts\cashes_activity\keeper_rests2.ini");
        }
    }
}
