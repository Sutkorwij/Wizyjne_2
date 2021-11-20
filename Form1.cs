using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;

namespace Filtracja
{
    public partial class Form1 : Form
    {
        //Zmienna typu Size moze byc przez nas dowolnie 
        //powołana i wykorzystana w kodzie. Dzieki temu
        //mozna zdefiniowac zadany rozmiar obrazku z wewnatrz kodu.
        //Właściwość StrechImage będzie odpowiedzialna za dopasowanie
        //rozmiarow
        private Size desired_image_size;
        Image<Bgr, byte> image_PB1, image_PB2;
        Image<Bgr, byte>[] image_buffers;
        double[] filter_coeff;
        VideoCapture camera;

        //Konstruktor klasy Form1. Odpowiada za inicjalizację wszystkich
        //komponentów (kontrolki i ich rozmieszczenie i właściwości)
        //na oknie aplikacji
        public Form1()
        {
            InitializeComponent();
            desired_image_size = new Size(320, 240);
            image_PB1 = new Image<Bgr, byte>(desired_image_size);
            image_PB2 = new Image<Bgr, byte>(desired_image_size);

            filter_coeff = new double[9];

            image_buffers = new Image<Bgr, byte>[3];
            for (int i = 0; i < image_buffers.Length; i++)
            {
                image_buffers[i] = new Image<Bgr, byte>(desired_image_size);
            }

            //Blok try catch aby przechwycić ewentualne niepowodzenie
            //tworzenia nowej instancji obiektu VideoCapture. Potrzebny, gdyz
            //w chwili tworzenia następuje próba połączenia się z kamerą która
            //może zakończyć się niepowodzeniem.
            try
            {
                camera = new VideoCapture();
                camera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, desired_image_size.Width);
                camera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, desired_image_size.Height);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void button_From_CvInvoke_PB1_Click(object sender, EventArgs e)
        {
            image_PB1 = new Image<Bgr, byte>(pictureBox1.Size);
            CvInvoke.Rectangle(image_PB1, new Rectangle(30, 30, 120, 120), new MCvScalar(0, 0, 255), -1);
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_From_CvInvoke_PB2_Click(object sender, EventArgs e)
        {
            image_PB2 = new Image<Bgr, byte>(pictureBox2.Size);
            CvInvoke.Rectangle(image_PB2, new Rectangle(30, 30, 120, 120), new MCvScalar(0, 255, 255), -1);
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void button_Clr_PB1_Click(object sender, EventArgs e)
        {
            //Możliwe jest przekazanie obiektów jakie chcemy zmodyfikować
            //jako argumenty metody.
            //Obiekty w C# są domyślnie przekazywane jako referencje. Są to tzw. typy referencyjne
            //Oznacza to, że zmiany dokonane na tak przekazanych obiektach "przeniosą się"
            //poza metodę, w której te modyfikacje były dokonane
            //PS: zmienne typów int, double itd to tzw typy wartościowe, a nie referencyjne.
            //Oznacza to, że te typy są kopiowane do metody.
            clear_image(pictureBox1, image_PB1);
        }

        private void button_Clr_PB2_Click(object sender, EventArgs e)
        {
            clear_image(pictureBox2, image_PB2);
        }

        private void clear_image(PictureBox PB, Image<Bgr, byte> Image)
        {
            //Zmienne typu PictureBox i Image<Bgr, byte> to instancje klas.
            //Zostały zatem "pod maską" przekazane do metody jako referencje.
            Image.SetZero();
            PB.Image = Image.Bitmap;
        }

        private void button_Browse_Files_PB1_Click(object sender, EventArgs e)
        {
            textBox_Image_Path_PB1.Text = get_image_path();
        }

        private void button_Browse_Files_PB2_Click(object sender, EventArgs e)
        {
            textBox_Image_Path_PB2.Text = get_image_path();
        }

        private string get_image_path()
        {
            string ret = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Obrazy|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog1.Title = "Wybierz obrazek.";
            //Jeśli wszystko przebiegło ok to pobiera nazwę pliku
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            { 
                ret = openFileDialog1.FileName;
            }

            return ret;
        }

        private void button_From_File_PB1_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = get_image_bitmap_from_file(textBox_Image_Path_PB1.Text, ref image_PB1);
            }
            catch (Exception)
            {
                MessageBox.Show("Brak ścieżki");
            }
        }

        private void button_From_File_PB2_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox2.Image = get_image_bitmap_from_file(textBox_Image_Path_PB2.Text, ref image_PB2);
            }
            catch (Exception)
            {
                MessageBox.Show("Brak ścieżki");
                throw;
            }
        }

        private Bitmap get_image_bitmap_from_file(string path, ref Image<Bgr, byte> Data)
        {
            Mat temp = CvInvoke.Imread(path);
            CvInvoke.Resize(temp, temp, desired_image_size);
            Data = temp.ToImage<Bgr, byte>();
            return Data.Bitmap;
        }

        private void button_From_Camera_PB1_Click(object sender, EventArgs e)
        {
            Mat temp = new Mat();
            camera.Read(temp);
            CvInvoke.Resize(temp, temp, image_PB1.Size);
            image_PB1 = temp.ToImage<Bgr, byte>();
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_From_Camera_PB2_Click(object sender, EventArgs e)
        {
            Mat temp = new Mat();
            camera.Read(temp);
            CvInvoke.Resize(temp, temp, image_PB2.Size);
            image_PB2 = temp.ToImage<Bgr, byte>();
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        //Bufory
        private void button_Buf1_To_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[0], image_PB1);
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_Buf1_From_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB1, image_buffers[0]);
            pictureBox_BUF1.Image = image_buffers[0].Bitmap;

        }

        private void button_Buf2_To_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[1], image_PB1);
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_Buf2_From_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB1, image_buffers[1]);
            pictureBox_BUF2.Image = image_buffers[1].Bitmap;
        }

        private void button_Buf3_To_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[2], image_PB1);
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_Buf3_From_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB1, image_buffers[2]);
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void button_Buf1_To_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[0], image_PB2);
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void button_Buf1_From_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB2, image_buffers[0]);
            pictureBox_BUF1.Image = image_buffers[0].Bitmap;
        }

        private void button_Buf2_To_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[1], image_PB2);
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void button_Buf2_From_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB2, image_buffers[1]);
            pictureBox_BUF2.Image = image_buffers[1].Bitmap;
        }

        private void button_Buf3_To_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[2], image_PB2);
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void button_Buf3_From_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB2, image_buffers[2]);
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void copy_image_data(Image<Bgr, byte> src, Image<Bgr, byte> dest)
        {
            src.CopyTo(dest);
        }

        private void button_Logical_Operation_Click(object sender, EventArgs e)
        {
            byte[, ,] b1, b2, b3;
            b1 = image_buffers[0].Data;
            b2 = image_buffers[1].Data;
            b3 = image_buffers[2].Data;
            for (int x = 0; x < desired_image_size.Width; x++)
            {
                for (int y = 0; y < desired_image_size.Height; y++)
                {
                    if (radioButton_buf_AND.Checked)
                    {
                        b3[y, x, 0] = (byte)(b1[y, x, 0] & b2[y, x, 0]);
                        b3[y, x, 1] = (byte)(b1[y, x, 1] & b2[y, x, 1]);
                        b3[y, x, 2] = (byte)(b1[y, x, 2] & b2[y, x, 2]);
                    }
                    else if (radioButton_buf_OR.Checked)
                    {
                        b3[y, x, 0] = (byte)(b1[y, x, 0] | b2[y, x, 0]);
                        b3[y, x, 1] = (byte)(b1[y, x, 1] | b2[y, x, 1]);
                        b3[y, x, 2] = (byte)(b1[y, x, 2] | b2[y, x, 2]);
                    }
                    else if (radioButton_buf_XOR.Checked)
                    {
                        b3[y, x, 0] = (byte)(b1[y, x, 0] ^ b2[y, x, 0]);
                        b3[y, x, 1] = (byte)(b1[y, x, 1] ^ b2[y, x, 1]);
                        b3[y, x, 2] = (byte)(b1[y, x, 2] ^ b2[y, x, 2]);
                    }
                }
            }
            image_buffers[2].Data = b3;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        //Filtry

        private void button_Low_Pass_Coeff_Click(object sender, EventArgs e)
        {
            numericUpDown_Filtr_Param11.Value = 1;
            numericUpDown_Filtr_Param12.Value = 8;
            numericUpDown_Filtr_Param13.Value = 1;

            numericUpDown_Filtr_Param21.Value = 4;
            numericUpDown_Filtr_Param22.Value = 6;
            numericUpDown_Filtr_Param23.Value = 4;

            numericUpDown_Filtr_Param31.Value = 1;
            numericUpDown_Filtr_Param32.Value = 8;
            numericUpDown_Filtr_Param33.Value = 1;


        }

        private void button_High_Pass_Coeff_Click(object sender, EventArgs e)
        {
            numericUpDown_Filtr_Param11.Value = 1;
            numericUpDown_Filtr_Param12.Value = -1;
            numericUpDown_Filtr_Param13.Value = -1;

            numericUpDown_Filtr_Param21.Value = -1;
            numericUpDown_Filtr_Param22.Value = 9;
            numericUpDown_Filtr_Param23.Value = -1;

            numericUpDown_Filtr_Param31.Value = -1;
            numericUpDown_Filtr_Param32.Value = -1;
            numericUpDown_Filtr_Param33.Value = -1;
        }

        private void button_Apply_Filter_Click(object sender, EventArgs e)
        {
            filter();
        }

        private void filter()
        {
            double[] wsp = new double[9];
            double suma_wsp = 0;
            wsp[0] = (double)numericUpDown_Filtr_Param11.Value;
            wsp[1] = (double)numericUpDown_Filtr_Param12.Value;
            wsp[2] = (double)numericUpDown_Filtr_Param13.Value;
            wsp[3] = (double)numericUpDown_Filtr_Param21.Value;
            wsp[4] = (double)numericUpDown_Filtr_Param22.Value;
            wsp[5] = (double)numericUpDown_Filtr_Param23.Value;
            wsp[6] = (double)numericUpDown_Filtr_Param31.Value;
            wsp[7] = (double)numericUpDown_Filtr_Param32.Value;
            wsp[8] = (double)numericUpDown_Filtr_Param33.Value;

            for (int i = 0; i < 9; i++)
            {
                suma_wsp += wsp[i];
            }

            byte[,,] temp1 = image_buffers[1].Data;
            byte[,,] temp2 = image_buffers[2].Data;

            for (int x = 1; x < desired_image_size.Width - 1; x++)
            {
                for (int y = 1; y < desired_image_size.Height - 1; y++)
                {
                    double R = 0, G = 0, B = 0;
                    //blue
                    B += wsp[0] * temp1[y - 1, x - 1, 0];
                    B += wsp[1] * temp1[y - 1, x, 0];
                    B += wsp[2] * temp1[y - 1, x + 1, 0];

                    B += wsp[3] * temp1[y, x - 1, 0];
                    B += wsp[4] * temp1[y, x, 0];
                    B += wsp[5] * temp1[y, x + 1, 0];

                    B += wsp[6] * temp1[y + 1, x - 1, 0];
                    B += wsp[7] * temp1[y + 1, x, 0];
                    B += wsp[8] * temp1[y + 1, x + 1, 0];

                    //green
                    G += wsp[0] * temp1[y - 1, x - 1, 1];
                    G += wsp[1] * temp1[y - 1, x, 1];
                    G += wsp[2] * temp1[y - 1, x + 1, 1];

                    G += wsp[3] * temp1[y, x - 1, 1];
                    G += wsp[4] * temp1[y, x, 1];
                    G += wsp[5] * temp1[y, x + 1, 1];

                    G += wsp[6] * temp1[y + 1, x - 1, 1];
                    G += wsp[7] * temp1[y + 1, x, 1];
                    G += wsp[8] * temp1[y + 1, x + 1, 1];

                    //red
                    R += wsp[0] * temp1[y - 1, x - 1, 2];
                    R += wsp[1] * temp1[y - 1, x, 2];
                    R += wsp[2] * temp1[y - 1, x + 1, 2];

                    R += wsp[3] * temp1[y, x - 1, 2];
                    R += wsp[4] * temp1[y, x, 2];
                    R += wsp[5] * temp1[y, x + 1, 2];

                    R += wsp[6] * temp1[y + 1, x - 1, 2];
                    R += wsp[7] * temp1[y + 1, x, 2];
                    R += wsp[8] * temp1[y + 1, x + 1, 2];

                    if((int)suma_wsp != 0)
                    {
                        B /= suma_wsp;
                        G /= suma_wsp;
                        R /= suma_wsp;
                    }

                    if (checkBox_Add_Half.Checked)
                    {
                        B /= 2;
                        G /= 2;
                        R /= 2;
                        B += 128;
                        G += 128;
                        R += 128;
                    }

                    if (B < 0) B = 0;
                    if (G < 0) G = 0;
                    if (R < 0) R = 0;
                    if (B > 255) B = 255;
                    if (G > 255) G = 255;
                    if (R > 255) R = 255;

                    temp2[y, x, 0] = (byte)B;
                    temp2[y, x, 1] = (byte)G;
                    temp2[y, x, 2] = (byte)R;
                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void button_Thresh_Click(object sender, EventArgs e)
        {
            Threshold();
        }

        private void Threshold()
        {
            int thresh = (int)numericUpDown_Thresh.Value;

            byte[,,] temp1 = image_buffers[1].Data;
            byte[,,] temp2 = image_buffers[2].Data;

            for (int x = 0; x < desired_image_size.Width; x++)
            {
                for (int y = 0; y < desired_image_size.Height; y++)
                {
                    if (!checkBox_Mono_Thresh.Checked)
                    {
                        if (temp1[y, x, 0] < thresh || temp1[y, x, 1] < thresh || temp1[y,x,2] < thresh)
                        {
                            temp2[y, x, 0] = 0;
                            temp2[y, x, 1] = 0;
                            temp2[y, x, 2] = 0;
                        }
                        else
                        {
                            temp2[y, x, 0] = 255;
                            temp2[y, x, 1] = 255;
                            temp2[y, x, 2] = 255;
                        }
                    }
                    else
                    {
                        int mono;
                        mono = temp1[y, x, 0] + temp1[y, x, 1] + temp1[y, x, 2];
                        mono /= 3;
                        if (mono < thresh)
                        {
                            temp2[y, x, 0] = 0;
                            temp2[y, x, 1] = 0;
                            temp2[y, x, 2] = 0;
                        }
                        else
                        {
                            temp2[y, x, 0] = 255;
                            temp2[y, x, 1] = 255;
                            temp2[y, x, 2] = 255;
                        }
                    }
                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void button_Dilate_Click(object sender, EventArgs e)
        {
            Dilate();
        }

        private void button_Erode_Click(object sender, EventArgs e)
        {
            Erode();
        }

        private void Dilate()
        {
            double R, G, B;

            byte[, ,] temp1 = image_buffers[1].Data;
            byte[, ,] temp2 = image_buffers[2].Data;

            for (int x = 1; x < desired_image_size.Width - 1; x++)
            {
                for (int y = 1; y < desired_image_size.Height - 1; y++)
                {
                    R = G = B = 0;

                    //BLUE
                    B = temp1[y - 1, x - 1, 0];
                    B = Math.Max(temp1[y - 1, x, 0], B);
                    B = Math.Max(temp1[y - 1, x + 1, 0], B);
                    B = Math.Max(temp1[y, x -1 , 0], B);
                    B = Math.Max(temp1[y, x, 0], B);
                    B = Math.Max(temp1[y, x + 1, 0], B);
                    B = Math.Max(temp1[y + 1, x -1, 0], B);
                    B = Math.Max(temp1[y + 1, x, 0], B);
                    B = Math.Max(temp1[y + 1, x + 1, 0], B);

                    //GREEN
                    G = temp1[y - 1, x - 1, 1];
                    G = Math.Max(temp1[y - 1, x, 1], G);
                    G = Math.Max(temp1[y - 1, x + 1, 1], G);
                    G = Math.Max(temp1[y, x - 1, 1], G);
                    G = Math.Max(temp1[y, x, 1], G);
                    G = Math.Max(temp1[y, x + 1, 1], G);
                    G = Math.Max(temp1[y + 1, x - 1, 1], G);
                    G = Math.Max(temp1[y + 1, x, 1], G);
                    G = Math.Max(temp1[y + 1, x + 1, 1], G);

                    //RED
                    R = temp1[y - 1, x - 1, 2];
                    R = Math.Max(temp1[y - 1, x, 2], R);
                    R = Math.Max(temp1[y - 1, x + 1, 2], R);

                    R = Math.Max(temp1[y, x - 1, 2], R);
                    R = Math.Max(temp1[y, x, 2], R);
                    R = Math.Max(temp1[y, x + 1, 2], R);

                    R = Math.Max(temp1[y + 1, x - 1, 2], R);
                    R = Math.Max(temp1[y + 1, x, 2], R);
                    R = Math.Max(temp1[y + 1, x + 1, 2], R);


                    temp2[y, x, 0] = (byte)B;
                    temp2[y, x, 1] = (byte)G;
                    temp2[y, x, 2] = (byte)R;
                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void textBox_Image_Path_PB1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox_BUF1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thresh();
        }

        private void Thresh()
        {
            byte[,,] temp1 = image_buffers[1].Data;
            byte[,,] temp2 = image_buffers[2].Data;

            double BH, BL, GH, GL, RH, RL;
            BH = (double)numericUpDown_BH.Value;
            BL = (double)numericUpDown_BL.Value;

            GH = (double)numericUpDown_GH.Value;
            GL = (double)numericUpDown_GL.Value;

            RH = (double)numericUpDown_RH.Value;
            RL = (double)numericUpDown_RL.Value;

            for (int x = 1; x < desired_image_size.Width; x++)
            {
                for (int y = 1; y < desired_image_size.Height; y++)
                {
                    if (temp1[y,x,0] >= BL && temp1[y,x,0] <= BH &&
                        temp1[y,x,1] >= GL && temp1[y,x,1] <= GH &&
                        temp1[y,x,2] >= RL && temp1[y,x,2] <= RH)
                    {
                        temp2[y, x, 0] = 255;
                        temp2[y, x, 1] = 255;
                        temp2[y, x, 2] = 255;
                    }
                    else
                    {
                        temp2[y, x, 0] = 0;
                        temp2[y, x, 1] = 0;
                        temp2[y, x, 2] = 0;
                    }
                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void numericUpDown_RL_ValueChanged(object sender, EventArgs e)
        {
            button_progowanie.PerformClick();
        }

        private void numericUpDown_GL_ValueChanged(object sender, EventArgs e)
        {
            button_progowanie.PerformClick();
        }

        private void numericUpDown_BL_ValueChanged(object sender, EventArgs e)
        {
            button_progowanie.PerformClick();
        }

        private void numericUpDown_RH_ValueChanged(object sender, EventArgs e)
        {
            button_progowanie.PerformClick();
        }

        private void numericUpDown_GH_ValueChanged(object sender, EventArgs e)
        {
            button_progowanie.PerformClick();
        }

        private void numericUpDown_BH_ValueChanged(object sender, EventArgs e)
        {
            button_progowanie.PerformClick();
        }

        private void Erode()
        {
            double R, G, B;

            byte[, ,] temp1 = image_buffers[1].Data;
            byte[, ,] temp2 = image_buffers[2].Data;

            for (int x = 1; x < desired_image_size.Width - 1; x++)
            {
                for (int y = 1; y < desired_image_size.Height - 1; y++)
                {
                    R = G = B = 0;

                    //BLUE
                    B = temp1[y - 1, x - 1, 0];
                    B = Math.Min(temp1[y - 1, x, 0], B);
                    B = Math.Min(temp1[y - 1, x + 1, 0], B);
                    B = Math.Min(temp1[y, x - 1, 0], B);
                    B = Math.Min(temp1[y, x, 0], B);
                    B = Math.Min(temp1[y, x + 1, 0], B);
                    B = Math.Min(temp1[y + 1, x - 1, 0], B);
                    B = Math.Min(temp1[y + 1, x, 0], B);
                    B = Math.Min(temp1[y + 1, x + 1, 0], B);

                    //GREEN
                    G = temp1[y - 1, x - 1, 1];
                    G = Math.Min(temp1[y - 1, x, 1], G);
                    G = Math.Min(temp1[y - 1, x + 1, 1], G);
                    G = Math.Min(temp1[y, x - 1, 1], G);
                    G = Math.Min(temp1[y, x, 1], G);
                    G = Math.Min(temp1[y, x + 1, 1], G);
                    G = Math.Min(temp1[y + 1, x - 1, 1], G);
                    G = Math.Min(temp1[y + 1, x, 1], G);
                    G = Math.Min(temp1[y + 1, x + 1, 1], G);

                    //RED
                    R = temp1[y - 1, x - 1, 2];
                    R = Math.Min(temp1[y - 1, x, 2], R);
                    R = Math.Min(temp1[y - 1, x + 1, 2], R);
                    R = Math.Min(temp1[y, x - 1, 2], R);
                    R = Math.Min(temp1[y, x, 2], R);
                    R = Math.Min(temp1[y, x + 1, 2], R);
                    R = Math.Min(temp1[y + 1, x - 1, 2], R);
                    R = Math.Min(temp1[y + 1, x, 2], R);
                    R = Math.Min(temp1[y + 1, x + 1, 2], R);


                    temp2[y, x, 0] = (byte)B;
                    temp2[y, x, 1] = (byte)G;
                    temp2[y, x, 2] = (byte)R;
                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }
    }
}
