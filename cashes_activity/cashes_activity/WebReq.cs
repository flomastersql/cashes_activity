
using System.Net;

namespace cashes_activity
{
    class WebReq
    {
        public static void call_fast_answer(string mess)
        {
            // Адрес ресурса, к которому выполняется запрос
            string url =
                string.Format(
                "https://dilun.ru/api/geter_data.php?token=2D83B46B23D1BF3BAEED53544E928&param_type=1&dev_id=218&prop_id=10&value_varchar={0}",
                mess);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Создаём объект WebClient
            using (var webClient = new WebClient())
            {
                // Выполняем запрос по адресу и получаем ответ в виде строки
                webClient.DownloadString(url);
            }
        }
    }
}
