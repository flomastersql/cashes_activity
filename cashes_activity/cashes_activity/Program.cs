using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;


namespace cashes_activity
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] restCredensials = ReadConfig.ReadRestrauntsData();

            string expl = ReadConfig.ReadCashesRetroExclude();

            DataTable dt = Rkeeper_DB_Methods.getSuspicionCashes(restCredensials, expl);

            string alarm_data = "";

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                alarm_data += string.Format("\r\n{0}\r\nтранз должно: {1}\r\nтранз факт: {2}\r\nвермя последней: {3}\r\n"
                    , dt.Rows[i]["cash"]
                    , dt.Rows[i]["avg_tr"]
                    , dt.Rows[i]["cur_cnt"]
                    , dt.Rows[i]["t"]);
            }

            if (alarm_data != "")
            {
                WebReq.call_fast_answer(alarm_data);
            }

            Console.WriteLine(alarm_data);

            //Console.ReadLine();
        }
    }
}

