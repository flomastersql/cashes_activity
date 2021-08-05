using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace cashes_activity
{
    class Rkeeper_DB_Methods
    {
        public static DataTable getSuspicionCashes(string[] RestCredData, string CashesExclude)
        {
            Dictionary<string, string[]> Dict_rest = new Dictionary<string, string[]>();
            List<string> rests = new List<string>();
            foreach (string s in RestCredData)
            {
                string[] param = s.Split(new char[] { ' ' });

                Dict_rest[param[0]] = new string[2];
                Dict_rest[param[0]][0] = param[2]; //pwd sa
                Dict_rest[param[0]][1] = param[1]; //ip server

                rests.Add(param[0]);
            }

            DataTable DT = new DataTable();
            for (int i = 0; i < rests.Count; i++)
            {
                try
                {
                    SqlDataAdapter sda = new SqlDataAdapter(
                    //отбираем назавние реторана из текстового конфига, название кассового места (клик подставляем в ручную, т.к. в cashes он указан как касса
                    //показываем среднее кол-во транзакций по кассовому месте и реально за прошедший час и также показываем время последней транзакции
                    //за прошедший час
                    "	select '" + rests[i] + " ' + case when (c.NAME like '%kio%' or c.NAME like '%кио%') then c.NAME else 'Касса CLC' end cash,  xx.avg_tr,  isnull(yy.cur_cnt, 0) cur_cnt	 " +
                    "	, isnull((convert(varchar, yy.max_t_check, 102) + ' ' + convert(varchar, yy.max_t_check, 108)), 'pzdc как давно уже') t from (	 " +
                    //получение среднего кол-ва транзакций по каждому кассовому месту за выбранный час в разрезе 30 дней
                    //учитывая те дни когда они в этот час не торговали (забито нулями, см **) - НАЧАЛО                                                        ***
                    "	select z.sifr, SUBSTRING(z.t, 1, 2) h, avg(z.cnt) avg_tr from(	 " +
                    //присоединение реального кол-во по кассовым местам за выбранный час к безусловному перемножению. Таким образом - получени
                    //нулевых значений в том месте, где кассовое место за конкретный час конеретного дня - вобще не торгует - НАЧАЛО                           **
                    "	select y1.t, y1.SIFR, isnull(y2.cnt, 0) cnt from(	 " +
                    //участок перемножения кассовых мест на все 30 (пока принято такое решение) дней по выбранному часу (в продакашене прошлый час) - НАЧАЛО   *
                    "	select x1.t, x2.sifr from(	 " +
                    "	select distinct SUBSTRING(CONVERT(varchar, CLOSEDATETIME, 108), 1, 2) + ' ' + CONVERT(varchar, CLOSEDATETIME, 101) t	 " +
                    "	from PRINTCHECKS where	 " +
                    "	CLOSEDATETIME > GETDATE() - 30 and	 " +
                    "	SUBSTRING(CONVERT(varchar, CLOSEDATETIME, 108), 1, 2) = SUBSTRING(CONVERT(varchar, DATEADD(hour, -1, GETDATE()), 108), 1, 2)	 " +
                    "	)x1, (	 " +
                    "	select C.SIFR from CASHES C join CASHGROUPS CG on C.CASHGROUP = CG.SIFR where	 " +
                    //здесь определяется какие именно кассовые места нужны, в данном случае тольк киоски и касса клика (презентор)
                    "	(c.NAME like '%kios%' or cg.NAME like '%clc%'))x2	 " +
                    //участок перемножения кассовых мест на все 30 (пока принято такое решение) дней по выбранному часу (в продакашене прошлый час) - КОНЕЦ    *
                    "	)y1 left join (	 " +
                    "	select ICLOSESTATION,	 " +
                    "	SUBSTRING(CONVERT(varchar, CLOSEDATETIME, 108), 1, 2) +' ' + CONVERT(varchar, CLOSEDATETIME, 101) t,	 " +
                    "	count(*) cnt	 " +
                    "	from PRINTCHECKS where CLOSEDATETIME > GETDATE() -30 and	 " +
                    "	   SUBSTRING(CONVERT(varchar, CLOSEDATETIME, 108), 1, 2) = SUBSTRING(CONVERT(varchar, DATEADD(hour, -1, GETDATE()), 108), 1, 2)	 " +
                    "	    group by ICLOSESTATION,	 " +
                    "	SUBSTRING(CONVERT(varchar, CLOSEDATETIME, 108), 1, 2) + ' ' + CONVERT(varchar, CLOSEDATETIME, 101)	 " +
                    "	)y2 on y1.t = y2.t and y1.SIFR = y2.ICLOSESTATION )z	 " +
                    //присоединение реального кол-во по кассовым местам за выбранный час к безусловному перемножению. Таким образом - получени
                    //нулевых значений в том месте, где кассовое место за конкретный час конеретного дня - вобще не торгует - КОНЕЦ                            **
                    "   group by z.sifr, SUBSTRING(z.t, 1, 2) )xx 	 " +
                    //получение среднего кол-ва транзакций по каждому кассовому месту за выбранный час в разрезе 30 дней
                    //учитывая те дни когда они в этот час не торговали (забито нулями, см **) - КОНЕЦ                                                        ***
                    "	left join (	 " +
                    //Присоединяем по ID касс кол-во транзакций по кассовым местам за прошедший час - НАЧАЛО
                    "	 select ICLOSESTATION, count(*) cur_cnt, max(CLOSEDATETIME) max_t_check from PRINTCHECKS where	 " +
                    "	    SUBSTRING(CONVERT(varchar, CLOSEDATETIME, 108), 1, 2) +' ' + CONVERT(varchar, CLOSEDATETIME, 101) =	 " +
                    "	    SUBSTRING(CONVERT(varchar, DATEADD(hour, -1, GETDATE()), 108), 1, 2) + ' ' + CONVERT(varchar, DATEADD(hour, -1, GETDATE()), 101)	 " +
                    "	group by ICLOSESTATION	 " +
                    "	)yy on xx.SIFR = yy.ICLOSESTATION	 " +
                    //Присоединяем по ID касс кол-во транзакций по кассовым местам за прошедший час - КОНЕЦ
                    //присоединяем таблицу касс, чтобы вывести их название в отчете вверху запроса
                    "	join CASHES c on c.SIFR = xx.SIFR  left join (" + CashesExclude + ")xxx on c.NAME = xxx.name and xxx.t > GETDATE() " +
                    //отбираем для отчета только те кассовые месте у которых транзакций больше одной (предполагается что если даже кассовое место завалится в этот
                    //час когда мы его не смотрим, то потери от этого будут не велики, потому что в этот период оно почти не торгует

                    //и отбираем те строки, где за прошедший час где среднее колл-во транзакций по кассе в 2 и более раз больше чем за прошедший час
                    "	where   xx.avg_tr > 1 and  xx.avg_tr/2 > isnull(yy.cur_cnt, 0) and xxx.name is null 	 "

                , string.Format("uid=sa;pwd={0};Initial Catalog=rk7;Data Source={1}"
                , Dict_rest[rests[i]][0]
                , Dict_rest[rests[i]][1]
            ));


                    sda.SelectCommand.CommandTimeout = 30000;
                    sda.Fill(DT);

                    Console.WriteLine(rests[i] + " - ok");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    //если по сети рест отъехал или проблем sql то отдельный вызов
                    //WebReq.call_fast_answer_error_sql(Dict_rest[rests[i]][2], e.Message);
                }
            }
            return DT;
        }
    }
}
