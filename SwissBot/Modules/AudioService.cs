using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

public class AudioService
{
    private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        IAudioClient client;
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            return;
        }
        if (target.Guild.Id != guild.Id)
        {
            return;
        }

        var audioClient = await target.ConnectAsync();

        if (ConnectedChannels.TryAdd(guild.Id, audioClient))
        {
            // If you add a method to log happenings from this service,
            // you can uncomment these commented lines to make use of that.
            //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        IAudioClient client;
        if (ConnectedChannels.TryRemove(guild.Id, out client))
        {
            await client.StopAsync();
            //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
        }
    }

    public async Task SendAudioAsync(IGuild guild, string path)
    {
        // Your task: Get a full path to the file if the value of 'path' is only a filename.
        
        IAudioClient client;
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
            int blockSize = 3840;
            
            var stream = client.CreateDirectPCMStream(AudioApplication.Mixed);
            var f = File.Open(@"C:\Users\plynch\Downloads\Ilyushin IL-96 VS Airbus a340.wav", FileMode.Open);
            byte[] buffer = new byte[blockSize];
            while (f.Read(buffer, 0, buffer.Length) > 0)
            {
                try { stream.Write(buffer, 0, buffer.Length); }
                catch (Exception ex)
                {

                }
            }
        }
    }

    private Process CreateProcess(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }
}