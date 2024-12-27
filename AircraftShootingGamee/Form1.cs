using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AircraftShootingGamee
{
    public partial class Form1 : Form
    {
        private Timer GameTimer;
        private Timer EnemyTimer;
        private Label LblScore;
        private Label LblGameOver;
        private Button BtnRestart;
        private List<PictureBox> enemyList = new List<PictureBox>();
        private List<PictureBox> projectileList = new List<PictureBox>();
        private Random random = new Random();
        private int score = 0;
        private int highScore = 0;
        private int enemiesHitGroundCount = 0; // Counter for enemies that reach the ground

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Initialize Timers
            GameTimer = new Timer();
            GameTimer.Interval = 20;  // Set interval for game timer
            GameTimer.Tick += GameTimer_Tick;

            EnemyTimer = new Timer();
            EnemyTimer.Interval = 1000;  // Set interval for enemy spawn
            EnemyTimer.Tick += EnemyTimer_Tick;

            // Load high score
            LoadHighScore();

            // Start the game and enemy timers
            GameTimer.Start();
            EnemyTimer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize additional settings if needed
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle player movement and shooting
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A:
                    if (playerAircraft1.Left > 0) playerAircraft1.Left -= 10; // Prevent moving out of bounds
                    break;
                case Keys.Right:
                case Keys.D:
                    if (playerAircraft1.Left + playerAircraft1.Width < this.ClientSize.Width)
                        playerAircraft1.Left += 10; // Prevent moving out of bounds
                    break;
                case Keys.Up:
                case Keys.W:
                    if (playerAircraft1.Top > 0) playerAircraft1.Top -= 10; // Prevent moving out of bounds
                    break;
                case Keys.Down:
                case Keys.S:
                    if (playerAircraft1.Top + playerAircraft1.Height < this.ClientSize.Height)
                        playerAircraft1.Top += 10; // Prevent moving out of bounds
                    break;
                case Keys.Space:
                    ShootProjectile(); // Shoot projectile when space is pressed
                    break;
                case Keys.R:
                    RestartGame(); // Restart the game when R is pressed
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
            // Spawn a new enemy aircraft at a random X position at the top of the form
            PictureBox enemy = new PictureBox
            {
                Image = Properties.Resources.enemyAircraft,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(50, 50),
                Location = new Point(random.Next(this.ClientSize.Width - 50), 0)
            };
            this.Controls.Add(enemy);
            enemyList.Add(enemy);
        }

        private void ShootProjectile()
        {
            // Create and shoot a bullet (red rectangle) from the player's aircraft position
            PictureBox projectile = new PictureBox
            {
                Size = new Size(5, 20),
                Location = new Point(playerAircraft1.Left + playerAircraft1.Width / 2 - 2, playerAircraft1.Top - 20),
                BackColor = Color.Red
            };
            this.Controls.Add(projectile);
            projectileList.Add(projectile);
        }

        private void MoveProjectiles()
        {
            // Move all projectiles upwards
            foreach (var projectile in projectileList.ToList())
            {
                projectile.Top -= 10;
                if (projectile.Top < 0) // Remove projectile if it goes off-screen
                {
                    this.Controls.Remove(projectile);
                    projectileList.Remove(projectile);
                }
            }

            // Move enemies downwards
            foreach (var enemy in enemyList.ToList())
            {
                enemy.Top += 5;
                if (enemy.Top >= this.ClientSize.Height) // If an enemy goes off-screen
                {
                    // Increment the counter for enemies that hit the ground
                    enemiesHitGroundCount++;

                    // Remove the enemy that hit the ground
                    this.Controls.Remove(enemy);
                    enemyList.Remove(enemy);
                }
            }
        }

        private void CheckCollisions()
        {
            // Check for projectile collisions with enemies
            foreach (var projectile in projectileList.ToList())
            {
                foreach (var enemy in enemyList.ToList())
                {
                    if (projectile.Bounds.IntersectsWith(enemy.Bounds))
                    {
                        // Remove enemy and projectile on collision
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
            // Check if 15 enemies have hit the ground
            if (enemiesHitGroundCount >= 15)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
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
            // Reset all necessary variables and objects
            score = 0;
            enemiesHitGroundCount = 0;
            lblScore.Text = "Score: 0\nHigh Score: " + highScore;

            // Clear projectiles and enemies
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

            // Restart the game and enemy timers
            GameTimer.Start();
            EnemyTimer.Start();
            lblGameOver.Visible = false;
            btnRestart.Visible = false;
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

        // You can leave these click event handlers empty if they are not used:
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void enemyAircraft1_Click(object sender, EventArgs e) { }
    }
}
