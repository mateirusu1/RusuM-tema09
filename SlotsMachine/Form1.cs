using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SlotsMachine
{
    public partial class Form1: Form
    {
        private GLControl glControl;
        private NumericUpDown numericCycles;
        private Button btnPull;
        private Label lblStatus;

        private int[] textures = new int[4];
        private Slot[] slots = new Slot[3];

        private bool isRolling = false;
        private int cyclesRemaining = 0;
        private double cycleTime = 0.5;
        private double accumulated = 0.0;

        private Stopwatch stopwatch = new Stopwatch();
        private Timer uiTimer = new Timer();

        private readonly string[] assetFiles = new string[]
        {
            "C:\\Users\\rsebi\\OneDrive\\Desktop\\RusuM_tema09\\SlotsMachine\\Assets\\cherry.png",
            "C:\\Users\\rsebi\\OneDrive\\Desktop\\RusuM_tema09\\SlotsMachine\\Assets\\lemon.png",
            "C:\\Users\\rsebi\\OneDrive\\Desktop\\RusuM_tema09\\SlotsMachine\\Assets\\bell.png",
            "C:\\Users\\rsebi\\OneDrive\\Desktop\\RusuM_tema09\\SlotsMachine\\Assets\\seven.png"
        };

        public Form1()
        {
            InitializeComponent();
            InitUI(); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void InitUI()
        {
            this.Text = "Slot Machine - .NET 4.8.1 + OpenTK";
            this.ClientSize = new Size(900, 500);

            // GLControl
            glControl = new GLControl();
            glControl.Location = new Point(10, 10);
            glControl.Size = new Size(600, 480);
            glControl.BackColor = Color.Black;
            glControl.Paint += GlControl_Paint;
            glControl.Load += GlControl_Load;
            glControl.Resize += GlControl_Resize;
            this.Controls.Add(glControl);

            // NumericUpDown
            numericCycles = new NumericUpDown();
            numericCycles.Minimum = 1;
            numericCycles.Maximum = 50;
            numericCycles.Value = 6;
            numericCycles.Location = new Point(650, 40);
            numericCycles.Width = 150;
            this.Controls.Add(numericCycles);

            Label lbl = new Label();
            lbl.Text = "Cicluri (0.5s fiecare):";
            lbl.Location = new Point(650, 15);
            lbl.AutoSize = true;
            this.Controls.Add(lbl);

            // Button Trage
            btnPull = new Button();
            btnPull.Text = "Trage!";
            btnPull.Location = new Point(650, 90);
            btnPull.Size = new Size(150, 40);
            btnPull.Click += BtnPull_Click;
            this.Controls.Add(btnPull);

            // Status label
            lblStatus = new Label();
            lblStatus.Text = "Status: Pregătit";
            lblStatus.Location = new Point(650, 150);
            lblStatus.AutoSize = true;
            this.Controls.Add(lblStatus);

            // Timer pentru update
            uiTimer.Interval = 16; // ~60 FPS
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();

            stopwatch.Start();
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.DarkGreen);

            // Inițializare sloturi
            for (int i = 0; i < 3; i++)
            {
                slots[i] = new Slot();
                slots[i].currentIndex = 0;
                slots[i].textures = textures;
            }

            // Load textures
            for (int i = 0; i < assetFiles.Length; i++)
            {
                string path = Path.Combine(Application.StartupPath, assetFiles[i]);
                if (!File.Exists(path))
                {
                    MessageBox.Show("Lipsesc imaginile din Assets!");
                    return;
                }
                textures[i] = LoadTexture(path);
            }

            SetupOrtho();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            SetupOrtho();
        }

        private void SetupOrtho()
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, glControl.Width, glControl.Height, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private int LoadTexture(string path)
        {
            Bitmap bmp = new Bitmap(path);


            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return tex;
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            double dt = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            UpdateLogic(dt);
            glControl.Invalidate();
        }

        private void UpdateLogic(double dt)
        {
            if (!isRolling) return;

            accumulated += dt;
            if (accumulated >= cycleTime)
            {
                accumulated -= cycleTime;

                Random rnd = new Random();
                foreach (var slot in slots)
                {
                    slot.currentIndex = rnd.Next(0, textures.Length);
                }

                cyclesRemaining--;
                lblStatus.Text = $"Rotim... {cyclesRemaining} rămase";

                if (cyclesRemaining <= 0)
                {
                    isRolling = false;
                    lblStatus.Text = "Finalizat";
                    CheckWin();
                }
            }
        }

        private void CheckWin()
        {
            int a = slots[0].currentIndex;
            int b = slots[1].currentIndex;
            int c = slots[2].currentIndex;

            if (a == b && b == c)
                MessageBox.Show("Ai câștigat! 🎉");
            else
                MessageBox.Show("Ai pierdut. 😢");
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            int slotW = glControl.Width / 4;
            int slotH = glControl.Height / 2;
            int y = glControl.Height / 4;

            int x0 = slotW / 2;
            int x1 = x0 + slotW + 20;
            int x2 = x1 + slotW + 20;

            DrawQuad(slots[0].textures[slots[0].currentIndex], x0, y, slotW, slotH);
            DrawQuad(slots[1].textures[slots[1].currentIndex], x1, y, slotW, slotH);
            DrawQuad(slots[2].textures[slots[2].currentIndex], x2, y, slotW, slotH);

            glControl.SwapBuffers();
        }

        private void DrawQuad(int texture, int x, int y, int w, int h)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(x, y);
            GL.TexCoord2(1, 0); GL.Vertex2(x + w, y);
            GL.TexCoord2(1, 1); GL.Vertex2(x + w, y + h);
            GL.TexCoord2(0, 1); GL.Vertex2(x, y + h);
            GL.End();

            GL.Disable(EnableCap.Texture2D);
        }

        private void BtnPull_Click(object sender, EventArgs e)
        {
            if (isRolling) return;

            cyclesRemaining = (int)numericCycles.Value;
            accumulated = 0;
            isRolling = true;
            lblStatus.Text = "Rotire începută";
        }
    }

    public class Slot
    {
        public int[] textures;
        public int currentIndex;
    }
}

