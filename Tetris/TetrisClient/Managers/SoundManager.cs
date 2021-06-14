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
            MusicPath = new(Environment.CurrentDirectory + "/Resources/Tetris_theme.wav", UriKind.Relative);
            MusicPlayer = new MediaPlayer();
        }
        public void PlayMusic()
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
        }
        public void IncreaseSpeed()
        {
            if (Speed <= 1.15)
            {
                Speed += 0.03;
                MusicPlayer.SpeedRatio = Speed;
            }
        }
        public void StopMusic()
        {
            MusicPlayer.Stop();
        }
    }

    public class GameSounds
    {
        public static void PlaySound()
        {
            MediaPlayer mediaPlayer = new();
            mediaPlayer.Open(new(Environment.CurrentDirectory + "/Resources/RotateBlock.wav", UriKind.Relative));
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }
    }
}
