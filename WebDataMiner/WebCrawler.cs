using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WebDataMiner
{
    class WebCrawler
    {
        static Timer _chatTimer;
        static Timer _videoTimer;
        static bool _switchedToChatFrame;

        static void Main(string[] args)
        {
            // browser open
            IWebDriver redeemPageDriver = new ChromeDriver();
            redeemPageDriver.Url = "https://redeem.microsoft.com/enter?ref=xboxcom&wa=wsignin1.0";

            IWebDriver youtubeDriver = new ChromeDriver();
            youtubeDriver.Url = "https://www.youtube.com/watch?v=598ADtzT4oM";

            try
            {
                // ms login
                IWebElement msEmail = redeemPageDriver.FindElement(By.Id("i0116"));
                msEmail.SendKeys("miklos.molnar88@hotmail.com");
                IWebElement msNext = redeemPageDriver.FindElement(By.Id("idSIButton9"));
                msNext.Click();

                IWebElement msPwd = redeemPageDriver.FindElement(By.Id("i0118"));
                msPwd.SendKeys("pwd");
                Thread.Sleep(1000);

                IWebElement msSignIn = redeemPageDriver.FindElement(By.Id("idSIButton9"));
                msSignIn.Click();

                Thread.Sleep(16000);
                redeemPageDriver.SwitchTo().Frame("wb_auto_blend_container");


                // parse code
                int chatSeconds = 10000;

                _chatTimer =
                    new Timer(
                        (object o) => {
                            try
                            {
                                if (!_switchedToChatFrame)
                                {
                                    youtubeDriver.SwitchTo().Frame("chatframe");
                                    _switchedToChatFrame = true;
                                }
                                Console.WriteLine("Running: " + DateTime.Now);
                                var key = string.Empty;

                                // parse input
                                IWebElement msChat = youtubeDriver.FindElement(By.XPath("(//yt-live-chat-text-message-renderer//*[@id=\"message\"])[last()]"));
                                Console.WriteLine("Message: " + msChat.Text);

                                var match = Regex.Match(msChat.Text, ".....-.....-.....-.....-.....");
                                if (match.Success)
                                {
                                    key = match.Captures[0].Value;
                                }

                                if (!string.IsNullOrEmpty(key))
                                {
                                    _chatTimer.Change(Timeout.Infinite, chatSeconds);

                                    // enter key
                                    IWebElement msKey = redeemPageDriver.FindElement(By.Id("tokenString"));
                                    msKey.SendKeys(key);

                                    Thread.Sleep(400);

                                    IWebElement msNextBtn = redeemPageDriver.FindElement(By.Id("nextButton"));
                                    msNextBtn.SendKeys(Keys.Enter);

                                    Console.ReadKey();
                                    _chatTimer.Change(0, chatSeconds);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        , null, 5000, chatSeconds) ;

                int videoSeconds = 2000;

                _videoTimer =
                    new Timer(
                        (object o) => {
                            try
                            {
                                //player-container
                                Console.WriteLine("Running: " + DateTime.Now);
                                var key = string.Empty;

                                // parse input
                                //IWebElement ytVideo = youtubeDriver.FindElement(By.Id("player-container"));
                                Screenshot ss = ((ITakesScreenshot)youtubeDriver).GetScreenshot();
                                ss.SaveAsFile("./Image.tiff", ScreenshotImageFormat.Tiff);

                                WebCrawler.ProcessImage();
                                var textResult = File.ReadAllText("result.txt");
                                
                                Console.WriteLine("Message: " + textResult);

                                var match = Regex.Match(textResult, ".....-.....-.....-.....-.....");
                                if (match.Success)
                                {
                                    key = match.Captures[0].Value;
                                }

                                if (!string.IsNullOrEmpty(key))
                                {
                                    System.Media.SystemSounds.Asterisk.Play();
                                    _videoTimer.Change(Timeout.Infinite, videoSeconds);

                                    // enter key
                                    IWebElement msKey = redeemPageDriver.FindElement(By.Id("tokenString"));
                                    msKey.SendKeys(key);

                                    Thread.Sleep(400);

                                    try
                                    {
                                        IWebElement msNextBtn = redeemPageDriver.FindElement(By.Id("nextButton"));
                                        msNextBtn.SendKeys(Keys.Enter);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Cannot press next button!");
                                    }

                                    _videoTimer.Change(10000, videoSeconds);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        , null, 2500, videoSeconds);

                Console.ReadKey();
            }
            finally
            {
                // release drivers
                redeemPageDriver.Close();
                redeemPageDriver.Quit();
                youtubeDriver.Close();
                youtubeDriver.Quit();
            }
        }

        private static void ProcessImage()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Users\mimolnar\AppData\Local\Tesseract-OCR\tesseract.exe",
                    //Arguments = Path.Combine(Environment.CurrentDirectory, "Image.tiff"),
                    Arguments = "Image.tiff result",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.CurrentDirectory
                }
            };

            proc.Start();
            proc.WaitForExit();
            proc.Close();
        }
    }
}
