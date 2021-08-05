using System.IO;

namespace cashes_activity
{
    class ReadConfig
    {
        public static string[] ReadRestrauntsData()
        {
            return File.ReadAllLines(@"C:\scripts\cashes_activity\keeper_rests2.ini");
        }

        public static string ReadCashesRetroExclude()
        {
            return File.ReadAllText(@"C:\scripts\cashes_activity\cashes_retro_exclude.txt");
        }
    }
}
