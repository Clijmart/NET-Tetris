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

        /// <summary>
        /// Plays the music.
        /// </summary>
        public void PlayMusic()
        {
            MusicPlayer.Open(MusicPath);
            MusicPlayer.Volume = Volume;
            MusicPlayer.SpeedRatio = Speed;
            MusicPlayer.MediaEnded += new EventHandler(RestartMusic);
            MusicPlayer.Play();

            // Restarts the music and plays it again.
            void RestartMusic(object sender, EventArgs e)
            {
                MusicPlayer.Position = TimeSpan.Zero;
                MusicPlayer.Play();
            }
        }

        /// <summary>
        /// Increases the music speed.
        /// </summary>
        public void IncreaseSpeed()
        {
            if (Speed <= 1.15)
            {
                Speed += 0.03;
                MusicPlayer.SpeedRatio = Speed;
            }
        }

        /// <summary>
        /// Stops the music.
        /// </summary>
        public void StopMusic()
        {
            MusicPlayer.Stop();
        }
    }
}
