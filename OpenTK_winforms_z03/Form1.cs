using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK_winforms_z03;

namespace OpenTK_winforms_z02 {
    public partial class Form1 : Form {

        //Stări de control cameră.
        private int eyePosX, eyePosY, eyePosZ;

        private Point mousePos;
        private float camDepth;

        //Stări de control mouse.
        private bool statusControlMouse2D, statusControlMouse3D, statusMouseDown;

        //Stări de control axe de coordonate.
        private bool statusControlAxe;

        //Stări de control iluminare.
        private bool lightON;
        private bool lightON_0;

        //Stări de control obiecte 3D.
        private string statusCube;

        //Texturare.
        private int[] textures = new int[2];
        private bool brick;
        private int colorTex;

        //Stări de control obiecte 3D.
        private bool statusCubeT;
        private bool statusCubeTex1;
        private bool statusCubeTex2;
        private bool statusCubeTex3;
        private bool statusCubeTex4;

        //Structuri de stocare a vertexurilor și a listelor de vertexuri.
        private int[,] arrVertex = new int[50, 3];         //Stocam matricea de vertexuri; 3 coloane vor reține informația pentru X, Y, Z. Nr. de linii definește nr. de vertexuri.
        private int nVertex;

        private int[] arrQuadsList = new int[100];        //Lista de vertexuri pentru construirea cubului prin intermediul quadurilor. Ne bazăm pe lista de vertexuri.
        private int nQuadsList;

        private int[] arrTrianglesList = new int[100];    //Lista de vertexuri pentru construirea cubului prin intermediul triunghiurilor. Ne bazăm pe lista de vertexuri.
        private int nTrianglesList;

        //Fișiere de in/out pentru manipularea vertexurilor.
        private string fileVertex = "vertexList.txt";
        private string fileQList = "quadsVertexList.txt";
        private string fileTList = "trianglesVertexList.txt";
        private bool statusFiles;

        //Dim valuesAmbientTemplate0() As Single = {255, 0, 0, 1.0}      //Valori alternative ambientale(lumină colorată)
        //# SET 1
        private float[] valuesAmbientTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesDiffuseTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float[] valuesSpecularTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesPositionTemplate0 = new float[] { 0.0f, 0.0f, 5.0f, 1.0f };
        //# SET 2
        //private float[] valuesAmbientTemplate0 = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
        //private float[] valuesDiffuseTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        //private float[] valuesSpecularTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        //private float[] valuesPositionTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 0.0f };

        private float[] valuesAmbient0 = new float[4];
        private float[] valuesDiffuse0 = new float[4];
        private float[] valuesSpecular0 = new float[4];
        private float[] valuesPosition0 = new float[4];

        // VBOs
        int VBOobject;
        int nVert;
        bool VBOon = false;
        System.Collections.Generic.List<Vector3[]> vboObjects = new System.Collections.Generic.List<Vector3[]>();
        System.Collections.Generic.List<float[]> vboTexUList = new System.Collections.Generic.List<float[]>();
        System.Collections.Generic.List<float[]> vboTexVList = new System.Collections.Generic.List<float[]>();
        bool vboLoaded = false;
        TextureFromBMP specialTextureField;


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   ON_LOAD
        public Form1() {
            InitializeComponent();

            /// TODO:
            /// În fișierul <Form1.Designer.cs>, la linia 26 înlocuiți conțînutul original cu linia de mai jos:
            ///         this.GlControl1 = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            /// Acest mod de inițializare va activa antialiasing-ul (multisampling MSAA la 8x).
            /// ATENTȚIE!
            /// Veți pierde designerul grafic. Aplicația funcționează dar pentru a putea accesa designerul grafic va trebui să reveniți la constructorul
            /// implicit al controlului OpenTK!
        }

        private void Form1_Load(object sender, EventArgs e) {

            SetupValues();
            SetupWindowGUI();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   SETARI INIȚIALE
        private void SetupValues() {
            eyePosX = 100;
            eyePosY = 100;
            eyePosZ = 50;

            camDepth = 1.04f;

            setLight0Values();

            numericXeye.Value = eyePosX;
            numericYeye.Value = eyePosY;
            numericZeye.Value = eyePosZ;

            statusCubeTex1 = false;
            statusCubeTex2 = false;
            statusCubeTex3 = false;
            statusCubeTex4 = false;
            brick = true;
            colorTex = 0;
        }

        private void SetupWindowGUI() {

            setControlMouse2D(false);
            setControlMouse3D(false);

            numericCameraDepth.Value = (int)camDepth;

            setControlAxe(true);

            setCubeStatus("OFF");
            setIlluminationStatus(false);
            setSource0Status(false);

            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   MANIPULARE VERTEXURI ȘI LISTE DE COORDONATE.
        //Încărcarea coordonatelor vertexurilor și lista de compunere a obiectelor 3D.
        private void loadVertex() {

            //Testăm dacă fișierul există
            try {
                StreamReader fileReader = new StreamReader((fileVertex));
                nVertex = Convert.ToInt32(fileReader.ReadLine().Trim());
                Console.WriteLine("Vertexuri citite: " + nVertex.ToString());

                string tmpStr = "";
                string[] str = new string[3];
                for (int i = 0; i < nVertex; i++) {
                    tmpStr = fileReader.ReadLine();
                    str = tmpStr.Trim().Split(' ');
                    arrVertex[i, 0] = Convert.ToInt32(str[0].Trim());
                    arrVertex[i, 1] = Convert.ToInt32(str[1].Trim());
                    arrVertex[i, 2] = Convert.ToInt32(str[2].Trim());
                }
                fileReader.Close();

            } catch (Exception e) {
                statusFiles = false;
                Console.WriteLine("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
                MessageBox.Show("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
            }
        }

        private int[,] loadVertexVbo()
        {

            //Testăm dacă fișierul există
            try
            {
                StreamReader fileReader = new StreamReader(("vertexDrawVbo.txt"));
                int nVertexer = Convert.ToInt32(fileReader.ReadLine().Trim());
                int[,] arrVertexer = new int[nVertexer, 3];
                Console.WriteLine("Vertexuri citite: " + nVertexer.ToString());

                string tmpStr = "";
                string[] str = new string[3];
                for (int i = 0; i < nVertexer; i++)
                {
                    tmpStr = fileReader.ReadLine();
                    str = tmpStr.Trim().Split(' ');
                    arrVertexer[i, 0] = Convert.ToInt32(str[0].Trim());
                    arrVertexer[i, 1] = Convert.ToInt32(str[1].Trim());
                    arrVertexer[i, 2] = Convert.ToInt32(str[2].Trim());
                }
                fileReader.Close();

                return arrVertexer;

            }
            catch (Exception)
            {
                statusFiles = false;
                Console.WriteLine("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
                MessageBox.Show("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
                return null;
            }
        }

        private void loadQList() {

            //Testăm dacă fișierul există
            try {
                StreamReader fileReader = new StreamReader(fileQList);

                int tmp;
                string line;
                nQuadsList = 0;

                while ((line = fileReader.ReadLine()) != null) {
                    tmp = Convert.ToInt32(line.Trim());
                    arrQuadsList[nQuadsList] = tmp;
                    nQuadsList++;
                }

                fileReader.Close();
            } catch (Exception e) {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileQList + "> nu exista!");
            }

        }

        private void loadTList() {

            //Testăm dacă fișierul există
            try {
                StreamReader fileReader = new StreamReader(fileTList);

                int tmp;
                string line;
                nTrianglesList = 0;

                while ((line = fileReader.ReadLine()) != null) {
                    tmp = Convert.ToInt32(line.Trim());
                    arrTrianglesList[nTrianglesList] = tmp;
                    nTrianglesList++;
                }

                fileReader.Close();
            } catch (Exception e) {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileTList + "> nu exista!");
            }

        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL CAMERĂ

        //Controlul camerei după axa X cu spinner numeric (un cadran).
        private void numericXeye_ValueChanged(object sender, EventArgs e) {
            eyePosX = (int)numericXeye.Value;
            GlControl1.Invalidate(); //Forțează redesenarea întregului control OpenGL. Modificările vor fi luate în considerare (actualizare).
        }
        //Controlul camerei după axa Y cu spinner numeric (un cadran).
        private void numericYeye_ValueChanged(object sender, EventArgs e) {
            eyePosY = (int)numericYeye.Value;
            GlControl1.Invalidate(); //Forțează redesenarea întregului control OpenGL. Modificările vor fi luate în considerare (actualizare).
        }
        //Controlul camerei după axa Z cu spinner numeric (un cadran).
        private void numericZeye_ValueChanged(object sender, EventArgs e) {
            eyePosZ = (int)numericZeye.Value;
            GlControl1.Invalidate(); //Forțează redesenarea întregului control OpenGL. Modificările vor fi luate în considerare (actualizare).
        }
        //Controlul adâncimii camerei față de (0,0,0).
        private void numericCameraDepth_ValueChanged(object sender, EventArgs e) {
            camDepth = 1 + ((float)numericCameraDepth.Value) * 0.1f;
            GlControl1.Invalidate();
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL MOUSE
        //Setăm variabila de stare pentru rotația în 2D a mouseului.
        private void setControlMouse2D(bool status) {
            if (status == false) {
                statusControlMouse2D = false;
                btnMouseControl2D.Text = "2D mouse control OFF";
            } else {
                statusControlMouse2D = true;
                btnMouseControl2D.Text = "2D mouse control ON";
            }
        }
        //Setăm variabila de stare pentru rotația în 3D a mouseului.
        private void setControlMouse3D(bool status) {
            if (status == false) {
                statusControlMouse3D = false;
                btnMouseControl3D.Text = "3D mouse control OFF";
            } else {
                statusControlMouse3D = true;
                btnMouseControl3D.Text = "3D mouse control ON";
            }
        }

        //Controlul mișcării setului de coordonate cu ajutorul mouseului (în plan 2D/3D)
        private void btnMouseControl2D_Click(object sender, EventArgs e) {
            if (statusControlMouse2D == true) {
                setControlMouse2D(false);
            } else {
                setControlMouse3D(false);
                setControlMouse2D(true);
            }
        }
        private void btnMouseControl3D_Click(object sender, EventArgs e) {
            if (statusControlMouse3D == true) {
                setControlMouse3D(false);
            } else {
                setControlMouse2D(false);
                setControlMouse3D(true);
            }
        }

        //Mișcarea lumii 3D cu ajutorul mouselui (click'n'drag de mouse).
        private void GlControl1_MouseMove(object sender, MouseEventArgs e) {
            if (statusMouseDown == true) {
                mousePos = new Point(e.X, e.Y);
                GlControl1.Invalidate();     //Forțează redesenarea întregului control OpenGL. Modificările vor fi luate în considerare (actualizare).
            }
        }
        private void GlControl1_MouseDown(object sender, MouseEventArgs e) {
            statusMouseDown = true;
        }
        private void GlControl1_MouseUp(object sender, MouseEventArgs e) {
            statusMouseDown = false;
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL ILUMINARE
        //Setăm variabila de stare pentru iluminare.
        private void setIlluminationStatus(bool status) {
            if (status == false) {
                lightON = false;
                btnLights.Text = "Iluminare OFF";
            } else {
                lightON = true;
                btnLights.Text = "Iluminare ON";
            }
        }

        //Activăm/dezactivăm iluminarea.
        private void btnLights_Click(object sender, EventArgs e) {
            if (lightON == false) {
                setIlluminationStatus(true);
            } else {
                setIlluminationStatus(false);
            }
            GlControl1.Invalidate();
        }

        //Identifică numărul maxim de lumini pentru implementarea curentă a OpenGL.
        private void btnLightsNo_Click(object sender, EventArgs e) {
            int nr = GL.GetInteger(GetPName.MaxLights);
            MessageBox.Show("Nr. maxim de lumini pentru aceasta implementare OpenGL este <" + nr.ToString() + ">.");
        }

        //Setăm variabila de stare pentru sursa de lumină 0.
        private void setSource0Status(bool status) {
            if (status == false) {
                lightON_0 = false;
                btnLight0.Text = "Sursa 0 OFF";
            } else {
                lightON_0 = true;
                btnLight0.Text = "Sursa 0 ON";
            }
        }

        //Activăm/dezactivăm sursa 0 de iluminare (doar dacă iluminarea este activă).
        private void btnLight0_Click(object sender, EventArgs e) {
            if (lightON == true) {
                if (lightON_0 == false) {
                    setSource0Status(true);
                } else {
                    setSource0Status(false);
                }
                GlControl1.Invalidate();
            }
        }

        //Schimbăm poziția sursei 0 de iluminare după axele XYZ.
        private void setTrackLigh0Default() {
            trackLight0PositionX.Value = (int)valuesPosition0[0];
            trackLight0PositionY.Value = (int)valuesPosition0[1];
            trackLight0PositionZ.Value = (int)valuesPosition0[2];
        }
        private void trackLight0PositionX_Scroll(object sender, EventArgs e) {
            valuesPosition0[0] = trackLight0PositionX.Value;
            GlControl1.Invalidate();
        }
        private void trackLight0PositionY_Scroll(object sender, EventArgs e) {
            valuesPosition0[1] = trackLight0PositionY.Value;
            GlControl1.Invalidate();
        }
        private void trackLight0PositionZ_Scroll(object sender, EventArgs e) {
            valuesPosition0[2] = trackLight0PositionZ.Value;
            GlControl1.Invalidate();
        }

        //Schimbăm culoarea sursei de lumină 0 (ambiental) în domeniul RGB.
        private void setColorAmbientLigh0Default() {
            numericLight0Ambient_Red.Value = (decimal)valuesAmbient0[0];
            numericLight0Ambient_Green.Value = (decimal)valuesAmbient0[1];
            numericLight0Ambient_Blue.Value = (decimal)valuesAmbient0[2];
        }
        private void numericLight0Ambient_Red_ValueChanged(object sender, EventArgs e) {
            valuesAmbient0[0] = (float)numericLight0Ambient_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Ambient_Green_ValueChanged(object sender, EventArgs e) {
            valuesAmbient0[1] = (float)numericLight0Ambient_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Ambient_Blue_ValueChanged(object sender, EventArgs e) {
            valuesAmbient0[2] = (float)numericLight0Ambient_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Schimbăm culoarea sursei de lumină 0 (difuză) în domeniul RGB.
        private void setColorDifuseLigh0Default() {
            numericLight0Difuse_Red.Value = (decimal)valuesDiffuse0[0];
            numericLight0Difuse_Green.Value = (decimal)valuesDiffuse0[1];
            numericLight0Difuse_Blue.Value = (decimal)valuesDiffuse0[2];
        }
        private void numericLight0Difuse_Red_ValueChanged(object sender, EventArgs e) {
            valuesDiffuse0[0] = (float)numericLight0Difuse_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Difuse_Green_ValueChanged(object sender, EventArgs e) {
            valuesDiffuse0[1] = (float)numericLight0Difuse_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Difuse_Blue_ValueChanged(object sender, EventArgs e) {
            valuesDiffuse0[2] = (float)numericLight0Difuse_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Schimbăm culoarea sursei de lumină 0 (specular) în domeniul RGB.
        private void setColorSpecularLigh0Default() {
            numericLight0Specular_Red.Value = (decimal)valuesSpecular0[0];
            numericLight0Specular_Green.Value = (decimal)valuesSpecular0[1];
            numericLight0Specular_Blue.Value = (decimal)valuesSpecular0[2];
        }
        private void numericLight0Specular_Red_ValueChanged(object sender, EventArgs e) {
            valuesSpecular0[0] = (float)numericLight0Specular_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Specular_Green_ValueChanged(object sender, EventArgs e) {
            valuesSpecular0[1] = (float)numericLight0Specular_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Specular_Blue_ValueChanged(object sender, EventArgs e) {
            valuesSpecular0[2] = (float)numericLight0Specular_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Resetare stare sursă de lumină nr. 0.
        private void setLight0Values() {
            for (int i = 0; i < valuesAmbientTemplate0.Length; i++) {
                valuesAmbient0[i] = valuesAmbientTemplate0[i];
            }
            for (int i = 0; i < valuesDiffuseTemplate0.Length; i++) {
                valuesDiffuse0[i] = valuesDiffuseTemplate0[i];
            }
            for (int i = 0; i < valuesPositionTemplate0.Length; i++) {
                valuesPosition0[i] = valuesPositionTemplate0[i];
            }
        }
        private void btnLight0Reset_Click(object sender, EventArgs e) {
            setLight0Values();
            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
            GlControl1.Invalidate();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL OBIECTE 3D
        //Setăm variabila de stare pentru afișarea/scunderea sistemului de coordonate.
        private void setControlAxe(bool status) {
            if (status == false) {
                statusControlAxe = false;
                btnShowAxes.Text = "Axe Oxyz OFF";
            } else {
                statusControlAxe = true;
                btnShowAxes.Text = "Axe Oxyz ON";
            }
        }

        //Controlul axelor de coordonate (ON/OFF).
        private void btnShowAxes_Click(object sender, EventArgs e) {
            if (statusControlAxe == true) {
                setControlAxe(false);
            } else {
                setControlAxe(true);
            }
            GlControl1.Invalidate();
        }

        //Setăm variabila de stare pentru desenarea cubului. Valorile acceptabile sunt:
        //TRIANGLES = cubul este desenat, prin triunghiuri.
        //QUADS = cubul este desenat, prin quaduri.
        //OFF (sau orice altceva) = cubul nu este desenat.
        private void setCubeStatus(string status) {
            if (status.Trim().ToUpper().Equals("TRIANGLES")) {
                statusCube = "TRIANGLES";
            } else if (status.Trim().ToUpper().Equals("QUADS")) {
                statusCube = "QUADS";
            } else {
                statusCube = "OFF";
            }
        }
        private void btnCubeQ_Click(object sender, EventArgs e) {
            statusFiles = true;
            loadVertex();
            loadQList();
            setCubeStatus("QUADS");
            GlControl1.Invalidate();
        }
        private void btnCubeT_Click(object sender, EventArgs e) {
            statusFiles = true;
            loadVertex();
            loadTList();
            setCubeStatus("TRIANGLES");
            GlControl1.Invalidate();
        }
        private void btnResetObjects_Click(object sender, EventArgs e) {
            setCubeStatus("OFF");
            GlControl1.Invalidate();
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   ADMINISTRARE MOD 3D (METODA PRINCIPALĂ)
        private void GlControl1_Paint(object sender, PaintEventArgs e) {
            //Resetează buffer-ele la valori default.
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            //Culoarea default a mediului.
            GL.ClearColor(Color.Black);

            //Setări preliminară pentru mediul 3D.
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView((float)camDepth, 4 / 3, 1, 10000);    //Declararea perspectivei spatiale.
            Matrix4 lookat = Matrix4.LookAt(eyePosX, eyePosY, eyePosZ, 0, 0, 0, 0, 1, 0);                   //Declararea camerei (stare inițială).
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);                                                             //Încărcarea modelului camerei.
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);
            GL.Viewport(0, 0, GlControl1.Width, GlControl1.Height);                                         //Mărimea suprafeței randate (scena 3D este proiectată pe aceasta).
            GL.Enable(EnableCap.DepthTest);                                                                 //Corecții de adâncime.
            GL.DepthFunc(DepthFunction.Less);                                                               //Corecții de adâncime.

            //Pornim iluminarea (daca avem permisiunea să o facem).
            if (lightON == true) {
                //Permite utilizarea iluminării. Fara aceasta corecție iluminarea nu functioneaza.
                GL.Enable(EnableCap.Lighting);
            } else {
                //Dezactivează utilizarea iluminării.
                GL.Disable(EnableCap.Lighting);
            }

            //Se creeaza sursa de iluminare. In acest caz folosim o singura sursa.
            //Numarul de surse de lumini depinde de implementarea OpenGL, dar de obicei cel putin 8 surse sunt posibile simultan.
            GL.Light(LightName.Light0, LightParameter.Ambient, valuesAmbient0);
            GL.Light(LightName.Light0, LightParameter.Diffuse, valuesDiffuse0);
            GL.Light(LightName.Light0, LightParameter.Specular, valuesSpecular0);
            GL.Light(LightName.Light0, LightParameter.Position, valuesPosition0);

            if ((lightON == true) && (lightON_0 == true)) {
                //Activam sursa 0 de lumina. Fara aceasta actiune nu avem iluminare.
                GL.Enable(EnableCap.Light0);
            } else {
                //Dezactivam sursa 0 de lumina.
                GL.Disable(EnableCap.Light0);
            }

            //Controlul rotației cu mouse-ul (2D).
            if (statusControlMouse2D == true) {
                GL.Rotate(mousePos.X, 0, 1, 0);
            }

            //Controlul rotației cu mouse-ul (3D).
            if (statusControlMouse3D == true) {
                GL.Rotate(mousePos.X, 0, 1, 1);
            }

            //---------------------------
            //---------------------------
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            LoadTextures();

            //---------------------------
            //---------------------------
            //Descrierea obiectelor 3D!!! Axe de coordonate.
            if (statusControlAxe == true) {
                DeseneazaAxe();
            }

            //Desenăm obiectele 3D (cub format din quads sau triunghiuri).
            if (statusCube.ToUpper().Equals("QUADS")) {
                DeseneazaCubQ();
            } else if (statusCube.ToUpper().Equals("TRIANGLES")) {
                DeseneazaCubT();
            }

            if (statusCubeTex1 == true) {
                DeseneazaCubQ_Tex1();
            }
            if (statusCubeTex2 == true) {
                DeseneazaCubQ_Tex2();
            }
            if (statusCubeTex3 == true) {
                DeseneazaCubT_Tex3();
            }
            if (statusCubeTex4 == true) {
                DeseneazaCubT_Tex4();
            }

            if (vboLoaded == true && vboObjects.Count > 0)
            {
                if (specialTextureField != null)
                {
                    GL.BindTexture(TextureTarget.Texture2D, specialTextureField.id);
                }
                GL.Color3(Color.White);
                for (int objIndex = 0; objIndex < vboObjects.Count; objIndex++)
                {
                    Vector3[] verts = vboObjects[objIndex];
                    float[] us = vboTexUList[objIndex];
                    float[] vs = vboTexVList[objIndex];
                    int count = verts.Length;
                    if ((count % 4) == 0)
                    {
                        GL.Begin(PrimitiveType.Quads);
                        for (int i = 0; i < count; i++)
                        {
                            GL.TexCoord2(us[i], vs[i]);
                            GL.Vertex3(verts[i]);
                        }
                        GL.End();
                    }
                    else
                    {
                        GL.Begin(PrimitiveType.Triangles);
                        for (int i = 0; i < count; i++)
                        {
                            GL.TexCoord2(us[i], vs[i]);
                            GL.Vertex3(verts[i]);
                        }
                        GL.End();
                    }
                }
            }

            //Limitează viteza de randare pentru a nu supraîncarca unitatea GPU (în caz contrar randarea se face cât de rapid este posibil, pe 100% din resurse). 
            //Dezavantajul este că o viteză prea mică poate afecta negativ cursivitatea animației!
            //GraphicsContext.CurrentContext.SwapInterval = 1;                                         //Testati cu valori din 10 in 10!!!
            //GraphicsContext.CurrentContext.VSync = True

            GlControl1.SwapBuffers();
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   DESENARE OBIECTE 3D
        //Desenează axe XYZ.
        private void DeseneazaAxe() {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(75, 0, 0);
            GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Yellow);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 75, 0);
            GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 75);
            GL.End();
        }

        //Desenează cubul - quads.
        private void DeseneazaCubQ() {
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i++) {
                switch (i % 4) {
                    case 0:
                        GL.Color3(Color.Blue);
                        break;
                    case 1:
                        GL.Color3(Color.Red);
                        break;
                    case 2:
                        GL.Color3(Color.Green);
                        break;
                    case 3:
                        GL.Color3(Color.Yellow);
                        break;
                }
                int x = arrQuadsList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }

        //Desenează cubul - triunghuri.
        private void DeseneazaCubT() {
            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i++) {
                switch (i % 3) {
                    case 0:
                        GL.Color3(Color.Blue);
                        break;
                    case 1:
                        GL.Color3(Color.Red);
                        break;
                    case 2:
                        GL.Color3(Color.Green);
                        break;
                }
                int x = arrTrianglesList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   TEXTURARE
        private void LoadTextures() {
            if (textures[0] != 0) return;
            GL.GenTextures(textures.Length, textures);
            LoadTexture(textures[0], "brickTexture.jpg");
            LoadTexture(textures[1], "OpenGLTexture.png");
        }

        private void LoadTexture(int textureId, string filename) {
            Bitmap bmp = new Bitmap(filename);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                          PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
        }

        private TextureFromBMP LoadTextureDynamically(string filename)
        {
            Bitmap bmp = new Bitmap(filename);

            int textureId = GL.GenTexture();

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                          PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            return new TextureFromBMP(textureId, bmp.Width, bmp.Height);
        }

        //Selecția imaginii texturii.
        private void rbTexture0_CheckedChanged(object sender, EventArgs e) {
            brick = true;
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            brick = false;
        }
        //Selecția culorii texturii.
        private void rbColorWhite_CheckedChanged(object sender, EventArgs e) {
            colorTex = 0;
        }
        private void rbColorRed_CheckedChanged(object sender, EventArgs e) {
            colorTex = 1;
        }
        private void rbColorBlue_CheckedChanged(object sender, EventArgs e) {
            colorTex = 2;
        }

        //Încărcare cub triunghiuri.
        private void btnTexT_1_Click(object sender, EventArgs e) {
            resetTexCubes();
            loadVertex();
            loadTList();
            statusCubeTex3 = true;
            GlControl1.Invalidate();
        }

        private void btnTexT_2_Click(object sender, EventArgs e) {
            resetTexCubes();
            loadVertex();
            loadTList();
            statusCubeTex4 = true;
            GlControl1.Invalidate();
        }

        private void btnReset_Tex2_Click(object sender, EventArgs e) {
            resetTexCubes();
        }

        //Încărcare cub quad.
        private void btnTexQ_1_Click(object sender, EventArgs e) {
            resetTexCubes();
            loadVertex();
            loadQList();
            statusCubeTex1 = true;
            GlControl1.Invalidate();
        }

        private void btnTexQ_2_Click(object sender, EventArgs e) {
            resetTexCubes();
            loadVertex();
            loadQList();
            statusCubeTex2 = true;
            GlControl1.Invalidate();
        }

        private void btnReset_Tex1_Click(object sender, EventArgs e) {
            resetTexCubes();
        }

        private void resetTexCubes() {
            setCubeStatus("OFF");
            statusCubeTex1 = false;
            statusCubeTex2 = false;
            statusCubeTex3 = false;
            statusCubeTex4 = false;
            GlControl1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // MODUL AMÂNAT - suport pentru mai multe obiecte în fisier
            vboObjects.Clear();
            vboTexUList.Clear();
            vboTexVList.Clear();
            try
            {
                string[] allLines = File.ReadAllLines("vertexDrawVbo.txt");
                int idx = 0;
                while (idx < allLines.Length)
                {
                    string line = allLines[idx].Trim();
                    if (line == "") { idx++; continue; }
                    int count = Convert.ToInt32(line);
                    idx++;
                    if (idx + count > allLines.Length) break;
                    Vector3[] verts = new Vector3[count];
                    for (int i = 0; i < count; i++)
                    {
                        string[] parts = allLines[idx + i].Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        int x = Convert.ToInt32(parts[0]);
                        int y = Convert.ToInt32(parts[1]);
                        int z = Convert.ToInt32(parts[2]);
                        verts[i] = new Vector3(x, y, z);
                    }
                    idx += count;

                    float[] us = new float[count];
                    float[] vs = new float[count];
                    float minX = float.MaxValue, maxX = float.MinValue;
                    float minY = float.MaxValue, maxY = float.MinValue;
                    for (int i = 0; i < count; i++)
                    {
                        if (verts[i].X < minX) minX = verts[i].X;
                        if (verts[i].X > maxX) maxX = verts[i].X;
                        if (verts[i].Y < minY) minY = verts[i].Y;
                        if (verts[i].Y > maxY) maxY = verts[i].Y;
                    }
                    float rangeX = maxX - minX; if (rangeX == 0) rangeX = 1;
                    float rangeY = maxY - minY; if (rangeY == 0) rangeY = 1;
                    for (int i = 0; i < count; i++)
                    {
                        us[i] = (verts[i].X - minX) / rangeX;
                        vs[i] = (verts[i].Y - minY) / rangeY;
                    }

                    vboObjects.Add(verts);
                    vboTexUList.Add(us);
                    vboTexVList.Add(vs);
                    nVert = count;
                }
            }
            catch (Exception)
            {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <vertexDrawVbo.txt> nu exista sau are format invalid!");
            }

            specialTextureField = LoadTextureDynamically("brickTexture.jpg");
            vboLoaded = (vboObjects.Count > 0);
            VBOon = false;
            GlControl1.Invalidate();

            int lat = 16; int lon = 24; double radius = 6.0;
            System.Collections.Generic.List<Vector3> sph = new System.Collections.Generic.List<Vector3>();
            System.Collections.Generic.List<float> sus = new System.Collections.Generic.List<float>();
            System.Collections.Generic.List<float> svs = new System.Collections.Generic.List<float>();
            for (int i = 0; i < lat; i++){
                double theta1 = Math.PI * i / lat;
                double theta2 = Math.PI * (i + 1) / lat;
                for (int j = 0; j < lon; j++){
                    double phi1 = 2.0 * Math.PI * j / lon;
                    double phi2 = 2.0 * Math.PI * (j + 1) / lon;
                    double x1 = radius * Math.Sin(theta1) * Math.Cos(phi1) + 30;
                    double y1 = radius * Math.Cos(theta1);
                    double z1 = radius * Math.Sin(theta1) * Math.Sin(phi1);
                    double x2 = radius * Math.Sin(theta1) * Math.Cos(phi2) + 30;
                    double y2 = radius * Math.Cos(theta1);
                    double z2 = radius * Math.Sin(theta1) * Math.Sin(phi2);
                    double x3 = radius * Math.Sin(theta2) * Math.Cos(phi1) + 30;
                    double y3 = radius * Math.Cos(theta2);
                    double z3 = radius * Math.Sin(theta2) * Math.Sin(phi1);
                    double x4 = radius * Math.Sin(theta2) * Math.Cos(phi2) + 30;
                    double y4 = radius * Math.Cos(theta2);
                    double z4 = radius * Math.Sin(theta2) * Math.Sin(phi2);
                    sph.Add(new Vector3((float)x1,(float)y1,(float)z1)); sus.Add((float)(phi1/(2*Math.PI))); svs.Add((float)(theta1/Math.PI));
                    sph.Add(new Vector3((float)x3,(float)y3,(float)z3)); sus.Add((float)(phi1/(2*Math.PI))); svs.Add((float)(theta2/Math.PI));
                    sph.Add(new Vector3((float)x2,(float)y2,(float)z2)); sus.Add((float)(phi2/(2*Math.PI))); svs.Add((float)(theta1/Math.PI));
                    sph.Add(new Vector3((float)x2,(float)y2,(float)z2)); sus.Add((float)(phi2/(2*Math.PI))); svs.Add((float)(theta1/Math.PI));
                    sph.Add(new Vector3((float)x3,(float)y3,(float)z3)); sus.Add((float)(phi1/(2*Math.PI))); svs.Add((float)(theta2/Math.PI));
                    sph.Add(new Vector3((float)x4,(float)y4,(float)z4)); sus.Add((float)(phi2/(2*Math.PI))); svs.Add((float)(theta2/Math.PI));
                }
            }
            vboObjects.Add(sph.ToArray()); vboTexUList.Add(sus.ToArray()); vboTexVList.Add(svs.ToArray());

            int slices = 24; float height = 8f; float cylR = 4f;
            System.Collections.Generic.List<Vector3> cyl = new System.Collections.Generic.List<Vector3>();
            System.Collections.Generic.List<float> cus = new System.Collections.Generic.List<float>();
            System.Collections.Generic.List<float> cvs = new System.Collections.Generic.List<float>();
            for (int i = 0; i < slices; i++){
                double a1 = 2*Math.PI*i/slices; double a2 = 2*Math.PI*(i+1)/slices;
                float x1 = (float)(cylR*Math.Cos(a1)) - 30; float z1 = (float)(cylR*Math.Sin(a1)); float y1 = -height/2;
                float x2 = (float)(cylR*Math.Cos(a2)) - 30; float z2 = (float)(cylR*Math.Sin(a2)); float y2 = -height/2;
                float x3 = x2; float z3 = z2; float y3 = height/2;
                float x4 = x1; float z4 = z1; float y4 = height/2;
                cyl.Add(new Vector3(x1,y1,z1)); cus.Add((float)(i/(double)slices)); cvs.Add(0f);
                cyl.Add(new Vector3(x2,y2,z2)); cus.Add((float)((i+1)/(double)slices)); cvs.Add(0f);
                cyl.Add(new Vector3(x3,y3,z3)); cus.Add((float)((i+1)/(double)slices)); cvs.Add(1f);
                cyl.Add(new Vector3(x4,y4,z4)); cus.Add((float)(i/(double)slices)); cvs.Add(1f);
            }
            vboObjects.Add(cyl.ToArray()); vboTexUList.Add(cus.ToArray()); vboTexVList.Add(cvs.ToArray());

            int torusU = 32; int torusV = 16; double R = 10.0; double r = 2.5;
            System.Collections.Generic.List<Vector3> tor = new System.Collections.Generic.List<Vector3>();
            System.Collections.Generic.List<float> tus = new System.Collections.Generic.List<float>();
            System.Collections.Generic.List<float> tvs = new System.Collections.Generic.List<float>();
            for (int i = 0; i < torusU; i++){
                double u1 = 2*Math.PI*i/torusU; double u2 = 2*Math.PI*(i+1)/torusU;
                for (int j = 0; j < torusV; j++){
                    double v1 = 2*Math.PI*j/torusV; double v2 = 2*Math.PI*(j+1)/torusV;
                    double x11 = (R + r*Math.Cos(v1))*Math.Cos(u1); double z11 = (R + r*Math.Cos(v1))*Math.Sin(u1); double y11 = r*Math.Sin(v1);
                    double x12 = (R + r*Math.Cos(v2))*Math.Cos(u1); double z12 = (R + r*Math.Cos(v2))*Math.Sin(u1); double y12 = r*Math.Sin(v2);
                    double x21 = (R + r*Math.Cos(v1))*Math.Cos(u2); double z21 = (R + r*Math.Cos(v1))*Math.Sin(u2); double y21 = r*Math.Sin(v1);
                    double x22 = (R + r*Math.Cos(v2))*Math.Cos(u2); double z22 = (R + r*Math.Cos(v2))*Math.Sin(u2); double y22 = r*Math.Sin(v2);
                    tor.Add(new Vector3((float)(x11),(float)(y11),(float)(z11))); tus.Add((float)(i/(double)torusU)); tvs.Add((float)(j/(double)torusV));
                    tor.Add(new Vector3((float)(x21),(float)(y21),(float)(z21))); tus.Add((float)((i+1)/(double)torusU)); tvs.Add((float)(j/(double)torusV));
                    tor.Add(new Vector3((float)(x12),(float)(y12),(float)(z12))); tus.Add((float)(i/(double)torusU)); tvs.Add((float)((j+1)/(double)torusV));
                    tor.Add(new Vector3((float)(x12),(float)(y12),(float)(z12))); tus.Add((float)(i/(double)torusU)); tvs.Add((float)((j+1)/(double)torusV));
                    tor.Add(new Vector3((float)(x21),(float)(y21),(float)(z21))); tus.Add((float)((i+1)/(double)torusU)); tvs.Add((float)(j/(double)torusV));
                    tor.Add(new Vector3((float)(x22),(float)(y22),(float)(z22))); tus.Add((float)((i+1)/(double)torusU)); tvs.Add((float)((j+1)/(double)torusV));
                }
            }
            vboObjects.Add(tor.ToArray()); vboTexUList.Add(tus.ToArray()); vboTexVList.Add(tvs.ToArray());

        }

        private void GlControl1_Load(object sender, EventArgs e)
        {

        }

        //Desenează cubul - quads - textura 1 - 100%.
        private void DeseneazaCubQ_Tex1() {
            if (brick == true) {
                GL.BindTexture(TextureTarget.Texture2D, textures[0]);
            } else {
                GL.BindTexture(TextureTarget.Texture2D, textures[1]);
            }

            switch (colorTex) {
                case 0:
                    GL.Color3(Color.White);      //Culoarea albă permite maparea texturii fără alterarea culorii originale.
                    break;
                case 1:
                    GL.Color3(Color.FromArgb(0, 255, 0, 0));
                    break;
                case 2:
                    GL.Color3(Color.FromArgb(0, 0, 0, 255));
                    break;
            }

            //GL.Normal3(0.0, 0.0, 1.0);
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i = i + 4) {
                //Atenție la modul de specificare a coordonatelor texturilor vis-a-vis de lista de vertexuri de intrare!
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(arrVertex[arrQuadsList[i], 0], arrVertex[arrQuadsList[i], 1], arrVertex[arrQuadsList[i], 2]);
                GL.TexCoord2(1.0, 1.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 1], 0], arrVertex[arrQuadsList[i + 1], 1], arrVertex[arrQuadsList[i + 1], 2]);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 2], 0], arrVertex[arrQuadsList[i + 2], 1], arrVertex[arrQuadsList[i + 2], 2]);
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 3], 0], arrVertex[arrQuadsList[i + 3], 1], arrVertex[arrQuadsList[i + 3], 2]);
            }
            GL.End();
        }

        //Desenează cubul - quads - textura 1 - 50%.
        private void DeseneazaCubQ_Tex2() {
            if (brick == true) {
                GL.BindTexture(TextureTarget.Texture2D, textures[0]);
            } else {
                GL.BindTexture(TextureTarget.Texture2D, textures[1]);
            }

            switch (colorTex) {
                case 0:
                    GL.Color3(Color.White);      //Culoarea albă permite maparea texturii fără alterarea culorii originale.
                    break;
                case 1:
                    GL.Color3(Color.FromArgb(0, 255, 0, 0));
                    break;
                case 2:
                    GL.Color3(Color.FromArgb(0, 0, 0, 255));
                    break;
            }

            //GL.Normal3(0.0, 0.0, 1.0);
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i = i + 4) {
                GL.TexCoord2(0.0, 0.5);
                GL.Vertex3(arrVertex[arrQuadsList[i], 0], arrVertex[arrQuadsList[i], 1], arrVertex[arrQuadsList[i], 2]);
                GL.TexCoord2(0.5, 0.5);
                GL.Vertex3(arrVertex[arrQuadsList[i + 1], 0], arrVertex[arrQuadsList[i + 1], 1], arrVertex[arrQuadsList[i + 1], 2]);
                GL.TexCoord2(0.5, 0.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 2], 0], arrVertex[arrQuadsList[i + 2], 1], arrVertex[arrQuadsList[i + 2], 2]);
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 3], 0], arrVertex[arrQuadsList[i + 3], 1], arrVertex[arrQuadsList[i + 3], 2]);
            }
            GL.End();
        }

        //Desenează cubul - triangles - textura 1 - 100%.
        private void DeseneazaCubT_Tex3() {
            if (brick == true) {
                GL.BindTexture(TextureTarget.Texture2D, textures[0]);
            } else {
                GL.BindTexture(TextureTarget.Texture2D, textures[1]);
            }

            switch (colorTex) {
                case 0:
                    GL.Color3(Color.White);      //Culoarea albă permite maparea texturii fără alterarea culorii originale.
                    break;
                case 1:
                    GL.Color3(Color.FromArgb(0, 255, 0, 0));
                    break;
                case 2:
                    GL.Color3(Color.FromArgb(0, 0, 0, 255));
                    break;
            }

            //GL.Normal3(0.0, 0.0, 1.0);
            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i = i + 6) {
                //Atenție la modul de specificare a coordonatelor texturilor vis-a-vis de lista de vertexuri de intrare!
                //La maparea texturii pe triughi pot apare deformări in imaginea texturată, porțiuni lipsă, etc. Pentru o eficiență crescută se pot folosi 2 triughiuri
                //ce formează un pătrat pentru procesare.

                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i], 0], arrVertex[arrTrianglesList[i], 1], arrVertex[arrTrianglesList[i], 2]);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 1], 0], arrVertex[arrTrianglesList[i + 1], 1], arrVertex[arrTrianglesList[i + 1], 2]);
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 2], 0], arrVertex[arrTrianglesList[i + 2], 1], arrVertex[arrTrianglesList[i + 2], 2]);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 3], 0], arrVertex[arrTrianglesList[i + 3], 1], arrVertex[arrTrianglesList[i + 3], 2]);
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 4], 0], arrVertex[arrTrianglesList[i + 4], 1], arrVertex[arrTrianglesList[i + 4], 2]);
                GL.TexCoord2(1.0, 1.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 5], 0], arrVertex[arrTrianglesList[i + 5], 1], arrVertex[arrTrianglesList[i + 5], 2]);
            }
            GL.End();
        }

        //Desenează cubul - triunghuri.
        private void DeseneazaCubT_Tex4() {
            if (brick == true) {
                GL.BindTexture(TextureTarget.Texture2D, textures[0]);
            } else {
                GL.BindTexture(TextureTarget.Texture2D, textures[1]);
            }

            switch (colorTex) {
                case 0:
                    GL.Color3(Color.White);      //Culoarea albă permite maparea texturii fără alterarea culorii originale.
                    break;
                case 1:
                    GL.Color3(Color.FromArgb(0, 255, 0, 0));
                    break;
                case 2:
                    GL.Color3(Color.FromArgb(0, 0, 0, 255));
                    break;
            }

            //GL.Normal3(0.0, 0.0, 1.0);
            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i = i + 6) {
                //Atenție la modul de specificare a coordonatelor texturilor vis-a-vis de lista de vertexuri de intrare!
                //La maparea texturii pe triughi pot apare deformări in imaginea texturată, porțiuni lipsă, etc. Pentru o eficiență crescută se pot folosi 2 triughiuri
                //ce formează un pătrat pentru procesare.

                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i], 0], arrVertex[arrTrianglesList[i], 1], arrVertex[arrTrianglesList[i], 2]);
                GL.TexCoord2(0.5, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 1], 0], arrVertex[arrTrianglesList[i + 1], 1], arrVertex[arrTrianglesList[i + 1], 2]);
                GL.TexCoord2(0.0, 0.5);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 2], 0], arrVertex[arrTrianglesList[i + 2], 1], arrVertex[arrTrianglesList[i + 2], 2]);
                GL.TexCoord2(0.5, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 3], 0], arrVertex[arrTrianglesList[i + 3], 1], arrVertex[arrTrianglesList[i + 3], 2]);
                GL.TexCoord2(0.0, 0.5);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 4], 0], arrVertex[arrTrianglesList[i + 4], 1], arrVertex[arrTrianglesList[i + 4], 2]);
                GL.TexCoord2(0.5, 0.5);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 5], 0], arrVertex[arrTrianglesList[i + 5], 1], arrVertex[arrTrianglesList[i + 5], 2]);
            }
            GL.End();
        }
    }
}
