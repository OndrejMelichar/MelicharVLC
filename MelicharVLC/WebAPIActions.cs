using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MelicharVLC
{
    public class WebAPIActions
    {
        public async Task<string> GET_HTML(string uri)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(uri);
            string html = await response.Content.ReadAsStringAsync();
            return html;
        }
    }
}
