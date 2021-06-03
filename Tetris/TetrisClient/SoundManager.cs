using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TetrisClient
{
    public class SoundManager
    {
        public double Speed { get; set; }
        public double Volume { get; set; }
        private Uri MusicPath { get; set; }
        private MediaPlayer MusicPlayer { get; set; }

        public SoundManager()
        {
            Speed = 1;
            Volume = 0.1;
            MusicPath = new(Environment.CurrentDirectory + "/resources/Tetris_theme.wav", UriKind.Relative);
            MusicPlayer = new MediaPlayer();

        }
        public async void PlayMusic()
        {
            await Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MusicPlayer.Open(MusicPath);
                    MusicPlayer.Volume = Volume;
                    MusicPlayer.SpeedRatio = Speed;
                    MusicPlayer.MediaEnded += new EventHandler(RestartMusic);
                    MusicPlayer.Play();

                    void RestartMusic(object sender, EventArgs e)
                    {
                        MusicPlayer.Position = TimeSpan.Zero;
                        MusicPlayer.Play();
                    }

                }));
            });

        }
        public void StopMusic()
        {
            MusicPlayer.Stop();
        }
    }
}
