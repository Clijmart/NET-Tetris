using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TetrisClient
{
    public class SoundManager
    {
        public double speed { get; set; }
        public double volume { get;  set; }
        private Uri musicPath { get; set; }
        private MediaPlayer musicPlayer { get; set; }

        public SoundManager()
        {
            speed = 1;
            volume = 0.1;
            musicPath = new(Environment.CurrentDirectory + "/resources/Tetris_theme.wav", UriKind.Relative);
            musicPlayer = new MediaPlayer();

        }
        public async void PlayMusic()
        {
            await Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    musicPlayer.Open(musicPath);
                    musicPlayer.Volume = volume;
                    musicPlayer.SpeedRatio = speed;
                    musicPlayer.MediaEnded += new EventHandler(RestartMusic);
                    musicPlayer.Play();

                    void RestartMusic(object sender, EventArgs e)
                    {
                        musicPlayer.Position = TimeSpan.Zero;
                        musicPlayer.Play();
                    }

                }));
            });

        }
        public void StopMusic()
        {
            musicPlayer.Stop();
        }
    }
}
