// See https://aka.ms/new-console-template for more information
using Uwp_Cs;

Console.WriteLine("Hello, World!");

var mediaManager = new MediaManager();
while (true)
{
    await Task.Delay(2000);
    mediaManager.PlayMedia();
    await Task.Delay(2000);
    mediaManager.PauseMedia();
}
