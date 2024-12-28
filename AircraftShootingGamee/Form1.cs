using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using NAudio.Wave; // Add this for NAudio

namespace AircraftShootingGamee
{
    public partial class Form1 : Form
    {
        // Define NAudio objects for background music
        private WaveOutEvent waveOutEvent;
        private AudioFileReader backgroundMusicReader;

        // Define SoundPlayer objects for sound effects
        private SoundPlayer shootSound;
        private SoundPlayer gameOverSound;
        private SoundPlayer enemyHitSound;

        // Define game elements
        private Timer GameTimer;
        private Timer EnemyTimer;
        private List<PictureBox> enemyList = new List<PictureBox>();
        private List<PictureBox> projectileList = new List<PictureBox>();
        private Random random = new Random();
        private int score = 0;
        private int highScore = 0;
        private int enemiesHitGroundCount = 0;

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Initialize game timers
            GameTimer = new Timer();
            GameTimer.Interval = 20;
            GameTimer.Tick += GameTimer_Tick;

            EnemyTimer = new Timer();
            EnemyTimer.Interval = 1000;
            EnemyTimer.Tick += EnemyTimer_Tick;

            // Load high score
            LoadHighScore();

            // Initialize and play background music using NAudio
            try
            {
                backgroundMusicReader = new AudioFileReader(@"C:\Users\youse\Downloads\415804__sunsai__mushroom-background-music.wav");
                waveOutEvent = new WaveOutEvent();
                waveOutEvent.Init(backgroundMusicReader);
                waveOutEvent.Volume = 0.2f; // Lower volume of background music (0.0 to 1.0 range)
                waveOutEvent.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading background music: " + ex.Message);
            }

            // Initialize sound effects using SoundPlayer
            shootSound = new SoundPlayer(@"C:\Users\youse\Downloads\shootSound.wav");
            gameOverSound = new SoundPlayer(@"C:\Users\youse\Downloads\gameOverSound.wav");
            enemyHitSound = new SoundPlayer(@"C:\Users\youse\Downloads\enemyHitSound.wav");

            // Start game timers
            GameTimer.Start();
            EnemyTimer.Start();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int moveSpeed = 15; // Increase this value to make the player aircraft move faster

            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A:
                    if (playerAircraft1.Left > 0) playerAircraft1.Left -= moveSpeed;
                    break;
                case Keys.Right:
                case Keys.D:
                    if (playerAircraft1.Left + playerAircraft1.Width < this.ClientSize.Width)
                        playerAircraft1.Left += moveSpeed;
                    break;
                case Keys.Up:
                case Keys.W:
                    if (playerAircraft1.Top > 0) playerAircraft1.Top -= moveSpeed;
                    break;
                case Keys.Down:
                case Keys.S:
                    if (playerAircraft1.Top + playerAircraft1.Height < this.ClientSize.Height)
                        playerAircraft1.Top += moveSpeed;
                    break;
                case Keys.Space:
                    ShootProjectile();
                    break;
                case Keys.R:
                    RestartGame();
                    break;
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            MoveProjectiles();
            CheckCollisions();
            CheckGameOver();
        }

        private void EnemyTimer_Tick(object sender, EventArgs e)
        {
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            PictureBox enemy = new PictureBox
            {
                Image = Properties.Resources.enemyAircraft,  // Replace with your own image
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(50, 50),
                Location = new Point(random.Next(this.ClientSize.Width - 50), 0)
            };
            this.Controls.Add(enemy);
            enemyList.Add(enemy);
        }

        private void ShootProjectile()
        {
            PictureBox projectile = new PictureBox
            {
                Size = new Size(5, 20),
                Location = new Point(playerAircraft1.Left + playerAircraft1.Width / 2 - 2, playerAircraft1.Top - 20),
                BackColor = Color.Red
            };
            this.Controls.Add(projectile);
            projectileList.Add(projectile);

            // Play shooting sound
            shootSound.Play();
        }

        private void MoveProjectiles()
        {
            foreach (var projectile in projectileList.ToList())
            {
                projectile.Top -= 10;
                if (projectile.Top < 0)
                {
                    this.Controls.Remove(projectile);
                    projectileList.Remove(projectile);
                }
            }

            foreach (var enemy in enemyList.ToList())
            {
                enemy.Top += 5;
                if (enemy.Top >= this.ClientSize.Height)
                {
                    enemiesHitGroundCount++;
                    this.Controls.Remove(enemy);
                    enemyList.Remove(enemy);
                }
            }
        }

        private void CheckCollisions()
        {
            foreach (var projectile in projectileList.ToList())
            {
                foreach (var enemy in enemyList.ToList())
                {
                    if (projectile.Bounds.IntersectsWith(enemy.Bounds))
                    {
                        enemyHitSound.Play();
                        this.Controls.Remove(enemy);
                        this.Controls.Remove(projectile);
                        enemyList.Remove(enemy);
                        projectileList.Remove(projectile);
                        score++;
                        lblScore.Text = "Score: " + score;
                        break;
                    }
                }
            }
        }

        private void CheckGameOver()
        {
            if (enemiesHitGroundCount >= 5)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            // Stop the background music
            waveOutEvent.Stop();

            // Play the game over sound
            gameOverSound.Play();

            GameTimer.Stop();
            EnemyTimer.Stop();
            lblGameOver.Visible = true;
            btnRestart.Visible = true;
            SaveHighScore();
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            RestartGame();
        }

        private void RestartGame()
        {
            score = 0;
            enemiesHitGroundCount = 0;
            lblScore.Text = "Score: 0\nHigh Score: " + highScore;

            foreach (var enemy in enemyList.ToList())
            {
                this.Controls.Remove(enemy);
            }
            enemyList.Clear();

            foreach (var projectile in projectileList.ToList())
            {
                this.Controls.Remove(projectile);
            }
            projectileList.Clear();

            GameTimer.Start();
            EnemyTimer.Start();
            lblGameOver.Visible = false;
            btnRestart.Visible = false;

            // Restart the background music
            waveOutEvent.Play();
        }

        private void SaveHighScore()
        {
            if (score > highScore)
            {
                highScore = score;
                File.WriteAllText("highscore.txt", highScore.ToString());
            }
        }

        private void LoadHighScore()
        {
            if (File.Exists("highscore.txt"))
            {
                highScore = int.Parse(File.ReadAllText("highscore.txt"));
            }
            lblScore.Text = "Score: 0\nHigh Score: " + highScore;
        }
    }
}
