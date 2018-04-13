using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;

namespace WorkingTools.Img
{
    public static class ImgLoader
    {
        private static bool IsUrlOrEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;

            //относительный путь
            //if (value.IndexOf("/", StringComparison.InvariantCultureIgnoreCase) == 0) return true;
            //полнуй путь http
            if (value.IndexOf("http://", StringComparison.InvariantCultureIgnoreCase) == 0) return true;
            //полнуй путь https
            if (value.IndexOf("https://", StringComparison.InvariantCultureIgnoreCase) == 0) return true;

            //что то другое
            return false;
        }

        /// <summary>
        /// Загрузка изображения
        /// </summary>
        /// <returns></returns>
        public static Image Load(string url)
        {
            if (url == null) return null;

            Image image = null;
            try
            {
                if (IsUrlOrEmpty(url))
                {
                    using (var handler = new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        Credentials = CredentialCache.DefaultCredentials
                    })
                    using (var client = new HttpClient(handler)
                    {
                        Timeout = TimeSpan.FromSeconds(3 * 60)
                    })
                    {

                        client.DefaultRequestHeaders.Add("User-Agent",
                            "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36 Yandex/1.01.001 notissimus.com");
                        client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "notissimus.com");

                        using (var response = client.GetAsync(new Uri(url)))
                        {
                            response.Result.EnsureSuccessStatusCode();

                            using (var inputStream = response.Result.Content.ReadAsStreamAsync())
                            {
                                image = Image.FromStream(inputStream.Result);
                            }
                        }
                    }
                }
                else
                {
                    //вырезаем начало строчки
                    const string base64SearchText = "base64,";
                    var indexOf = url.IndexOf(base64SearchText, StringComparison.InvariantCultureIgnoreCase);
                    if (indexOf != -1) url = url.Remove(0, indexOf + base64SearchText.Length);


                    if (string.IsNullOrWhiteSpace(url)) throw new Exception("не удалось получить картинку в формате base64");

                    byte[] bytes = Convert.FromBase64String(url);

                    using (var ms = new MemoryStream(bytes))
                        image = Image.FromStream(ms);
                }
            }
            catch (Exception) { /*ignore*/ }
            return image;
        }
    }
}
