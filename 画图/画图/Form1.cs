using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using 模拟鼠标操作;

namespace 画图
{
    public partial class Form1 : Form
    {
        #region 声明API
		
        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("USER32.DLL")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);  //导入模拟键盘的方法 
        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);  //导入寻找windows窗体的方法
	#endregion
        public string path = @"H:\work_today\14-6-27\test.jpg";
        bool[,] alreadyDraw;
        bool[,] alreadyRead;
        bool[,] ColorInt;

        private void MouseMoveTO(int X,int Y)
        {
            int SH = Screen.PrimaryScreen.Bounds.Height;
            int SW = Screen.PrimaryScreen.Bounds.Width;
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X * 65536 / SW, Y * 65536 / SH, 0, 0);
           
        }//画点
        private Color[,] CheckImageOFcolor(string path)
        {
            Image origiPic = Image.FromFile(path);
            Bitmap resizedPic = new Bitmap(origiPic, 400, 300);
            Color colorPX = new Color();
            Color[,] colorArr = new Color[resizedPic.Width, resizedPic.Height];

            for (int i = 0; i < resizedPic.Width; i++)
            {
                for (int j = 0; j < resizedPic.Height; j++)
                {
                    colorPX = resizedPic.GetPixel(i, j);
                    if (colorPX.R == 255 && colorPX.G == 255 && colorPX.B == 255)
                    //if (colorPX.R >=128)
                    {
                        colorArr[i, j] = Color.FromArgb(colorPX.R,colorPX.G,colorPX.B);//白
                    }
                    else
                    {
                        colorArr[i, j] = Color.FromArgb(colorPX.R, colorPX.G, colorPX.B);//有色
                    }
                }
            }
            return colorArr;
        }//得到数组包含像素信息及是否下笔
        private bool[,] CheckImageOFused(string path)
        {
            Image origiPic = Image.FromFile(path);
            Bitmap resizedPic = new Bitmap(origiPic, 400, 300);
            bool[,] colorArr = new bool[resizedPic.Width, resizedPic.Height];
            return colorArr;
        }
        private void CheckImageOFALL(string path)
        {
            //Image origiPic = Image.FromFile(path);
            //Bitmap resizedPic = new Bitmap(origiPic, 400, 300);

            ColorInt = CheckImageBlack(Image.FromFile(path));
            alreadyDraw = CheckImageOFused(path);
            alreadyRead = CheckImageOFused(path);
           
        } //得到判断性数组
        private int[] simRGB(Color oricl)
        {
            int[] sRGB =new int[3];
            sRGB[0]=oricl.R/9*9;
            sRGB[1]=oricl.G/9*9;
            sRGB[2]=oricl.B/9*9;
            return sRGB;

        } //得到近似颜色
        private bool[,] CheckImageBlack(Image image)
        {
            Bitmap srcBitmap = new Bitmap(image, new Size(400, 300));

            //Bitmap bmp=new Bitmap ()
            //srcBitmap = ChangeSize(srcBitmap, this.Width / DrawingWidth, this.Height / DrawingHeight);
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            bool[,] boolArr = new bool[wide, height];

            Rectangle rect = new Rectangle(0, 0, wide, height);
            //将Bitmap锁定到系统内存中,获得BitmapData
            BitmapData srcBmData = srcBitmap.LockBits(rect,
                      ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            //位图中第一个像素数据的地址。它也可以看成是位图中的第一个扫描行
            System.IntPtr srcPtr = srcBmData.Scan0;
            //将Bitmap对象的信息存放到byte数组中
            int src_bytes = srcBmData.Stride * height;
            byte[] srcValues = new byte[src_bytes];
            //复制GRB信息到byte数组
            System.Runtime.InteropServices.Marshal.Copy(srcPtr, srcValues, 0, src_bytes);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < wide; j++)
                {
                    //只处理每行中图像像素数据,舍弃未用空间
                    //注意位图结构中RGB按BGR的顺序存储
                    int k = 3 * j;

                    if (srcValues[i * srcBmData.Stride + k + 2] >= 230)
                    {
                        boolArr[j, i] = false;//白
                    }
                    else
                    {
                        boolArr[j, i] = true;
                    }
                }
            }
            //解锁位图
            srcBitmap.UnlockBits(srcBmData);
            return boolArr;
        }
        
        private void exiten (int x,int y)
        {
            //Color[,] pixel=CheckImageOFcolor(path);
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if (x + j >= 0 && x + j < 400 && y + i >= 0 && y + i < 300 && !alreadyRead[x + j, y + i])
                    {
                        alreadyRead[x + j, y + i] = true;
                        //int[] chgcl = simRGB(pixel[x + j, y + i]);
                        //if (chgcl[0]==nowcl[0] && chgcl[1]==nowcl[1] && chgcl[2]==nowcl[2] && !alreadyDraw[x + j, y + i])
                        //{
                        //    MouseMoveTO(x + j + 400, y + i + 300);
                        //    alreadyDraw[x + j, y + i] = true;
                        //    exiten(x+j,y+i,alreadyRead ,chgcl);
                        //}
                        if (!alreadyDraw[x + j, y + i] && ColorInt[x + j, y + i])
                        {
                            MouseMoveTO(x+j+400,y+i+300);
                            alreadyDraw[x + j, y + i] = true;
                            Thread.Sleep(5);
                            exiten(x + j, y + i);
                        }
                        

                    }
                }
            }
        }

        private void CheckPixel()
        {
            //Color[,] ColorInt = CheckImageOFcolor(path);
            CheckImageOFALL(path);
            
            for (int x = 0; x < ColorInt.GetLength(0); x++)
            {
                for (int y = 0; y < ColorInt.GetLength(1); y++)
                {
                    if (!alreadyDraw[x, y] && ColorInt[x, y])
                    {

                        exiten(x, y);
                        Thread.Sleep(10);
                    }
                    
                }
            }
        }//开始下笔
        

        public Form1()
        {
            InitializeComponent();
        }
        KeyboardHook kh;
        private void Form1_Load(object sender, EventArgs e)
        {
            kh = new KeyboardHook();
            kh.SetHook();
            kh.OnKeyDownEvent += kh_OnKeyDownEvent;
        }

        void kh_OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.B | Keys.Control | Keys.Alt))
            {
                Environment.Exit(0);
            }
        }//退出热键

        private void button1_Click(object sender, EventArgs e)
        {
            //MouseMoveTO(400, 300);
            //string path =@"H:\work_today\14-6-27\test.jpg";
            Thread first = new Thread(CheckPixel);
            first.Start();
            

            //CheckPixel(path);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MouseMoveTO(1010, 110); MouseMoveTO(1010, 110);

            for (IntPtr calculatorHandle = FindWindow(null, "无标题-画图"); calculatorHandle == IntPtr.Zero; )
            {
                
                
            }
            MessageBox.Show("得到");

        }
    }
}
