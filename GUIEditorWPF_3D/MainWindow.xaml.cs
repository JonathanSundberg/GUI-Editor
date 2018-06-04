using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using System.IO;

// Github commit 
namespace GUIEditorWPF_3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        struct Btn
        {
            
            public string PressTexture;
            public string HoverTexture;
        }
        struct ElementInfo
        {
            public string rectName;
            public string textName;
            public int posX;
            public int posY;
            public bool Btn;
            public Btn buttonTextures;
            public int layerDepth;
        };
        struct brushStruct
        {
            public string name;
            public ImageBrush idle;
            public ImageBrush hovered;
            public ImageBrush pressed;
        };
        struct Header
        {
            public int NrOfRects;
            public int NrOfTexts;
        };
        struct rectangleExport
        {
            public int rectNameLength;
            public string rectName;
            public int textNameLength;
            public string textName;
            public int Width;
            public int Height;
            public int PositionX;
            public int PositionY;
            public int Opacity;

            public bool Btn;
            public int hoverTexLength;
            public string hoverTex;
            public int pressTexLength;
            public string PressTex;
            public int layerDepth;
        };
        struct TextExport
        {
            public string TextName;
            public string TextContent;
            public int Width;
            public int Height;
            public int PosX;
            public int PosY;
        };

        Window CanvasWindow = new Window();
        Canvas canvas = new Canvas();
        private bool _isRectDragInProg;
        List<Rectangle> rectList = new List<Rectangle>();
        List<ElementInfo> ElementInfoList = new List<ElementInfo>();
        List<brushStruct> brushList = new List<brushStruct>();
        List<Viewbox> viewBoxList = new List<Viewbox>();
        List<rectangleExport> Package = new List<rectangleExport>();
        List<TextExport> textPackage = new List<TextExport>(); 
        
        string fileName;
        Random rnd = new Random();
        int index = 0;
        Window nameWindow = new Window();

        public MainWindow()
        {
            InitializeComponent();
            initializeWindow();
            CanvasWindow.Show();
            DisableAttrEditors();
            
        }
        
        private void initializeWindow()
        {
            CanvasWindow.Width = 1280;
            CanvasWindow.Height = 720;
            CanvasWindow.ResizeMode = ResizeMode.NoResize;
            
            canvas.Width = 1280;
            canvas.Height = 720;
            canvas.Background = new SolidColorBrush(Colors.Gray);
            CanvasWindow.Content = canvas;
        }

        private void AddElement(object sender, RoutedEventArgs e)
        {
            Opacity_Slider.Value = 100;
            for (int i = 0; i < HUD_Listbox.Items.Count; i++)
            {
                if (HudPart_Txtbox.Text.ToString() == HUD_Listbox.Items[i].ToString())
                {
                    MessageBoxResult error = MessageBox.Show("Name already exist, please chose a different name","Name Error",MessageBoxButton.OK);
                    return;
                }
                
                
            }
            
            HUD_Listbox.Items.Add(HudPart_Txtbox.Text.ToString());
            ListBoxUpdated();
            Rectangle rect = new Rectangle();

            rect.Name = HudPart_Txtbox.Text;
            rect.Tag = index;
            index++;
            int color = rnd.Next(11);

            switch (color)
            {
                case 1:
                    rect.Fill = new SolidColorBrush(Colors.Gold);
                    break;
                case 2:
                    rect.Fill = new SolidColorBrush(Colors.Aqua);
                    break;
                case 3:
                    rect.Fill = new SolidColorBrush(Colors.Red);
                    break;
                case 4:
                    rect.Fill = new SolidColorBrush(Colors.Purple);
                    break;
                case 5:
                    rect.Fill = new SolidColorBrush(Colors.DeepPink);
                    break;
                case 6:
                    rect.Fill = new SolidColorBrush(Colors.Violet);
                    break;
                case 7:
                    rect.Fill = new SolidColorBrush(Colors.YellowGreen);
                    break;
                case 8:
                    rect.Fill = new SolidColorBrush(Colors.Black);
                    break;
                case 9:
                    rect.Fill = new SolidColorBrush(Colors.DarkOrange);
                    break;
                case 10:
                    rect.Fill = new SolidColorBrush(Colors.ForestGreen);
                    break;
                default:
                    rect.Fill = new SolidColorBrush(Colors.SteelBlue);
                    break;
            }

            ElementInfo elm = new ElementInfo();
            elm.rectName = rect.Name;
            elm.Btn = false;
            elm.layerDepth = rectList.Count;
            ElementInfoList.Add(elm);

            brushStruct brush = new brushStruct();

            brush.name = rect.Name;
            brushList.Add(brush);
            
            rect.Width = 200;
            rect.Height = 200;
            rect.MouseLeftButtonDown += rect_MouseLeftButtonDown;
            rect.MouseLeftButtonUp += rect_MouseLeftButtonUp;
            rect.MouseMove += rect_MouseMove;
            rect.MouseEnter += rect_MouseEnter;
            rect.MouseLeave += rect_MouseLeave;
            Canvas.SetLeft(rect, 540);
            Canvas.SetTop(rect, 260);
            
            canvas.Children.Add(rect);

            Canvas.SetZIndex(rect, rectList.Count);
            rectList.Add(rect);
            
        }

        private void rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            _isRectDragInProg = true;
            Rectangle rect = (Rectangle)sender;

            rect.CaptureMouse();



            HUD_Listbox.UnselectAll();
            for (int i = 0; i < HUD_Listbox.Items.Count; i++)
            {

                if (HUD_Listbox.Items[i].ToString() == rect.Name)
                {
                  HUD_Listbox.SelectedItem = HUD_Listbox.Items.GetItemAt(i);
                }
            }
            for (int j = 0; j < brushList.Count; j++)
            {
                if (brushList[j].name == rect.Name)
                {
                    if (brushList[j].pressed != null)
                    {

                        rect.Fill = brushList[j].pressed;
                    }
                }
            }
            ListBoxUpdated();
        }

        private void rect_MouseMove(object sender, MouseEventArgs e)
        {

            
            if (!_isRectDragInProg) return;
            Rectangle rect = (Rectangle)sender;

            var mousePos = e.GetPosition(canvas);

            double left = mousePos.X - (rect.ActualWidth / 2);
            double top = mousePos.Y - (rect.ActualHeight / 2);
            double right = mousePos.X + (rect.ActualWidth / 2);
            double down = mousePos.Y + (rect.ActualHeight / 2);

            double windowHeight = CanvasWindow.ActualHeight;
            double windowWidth = CanvasWindow.ActualWidth;


            stayInsideCanvas(rect, left, top, right, down,windowHeight,windowWidth);
            

        }

        private void rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isRectDragInProg = false;
            Rectangle rect = (Rectangle)sender;
            rect.ReleaseMouseCapture();



            
                
            for (int i = 0; i < brushList.Count; i++)
            {
                if (brushList[i].name == rect.Name)
                {
                    if (rect.IsMouseOver)
                    {
                        if (brushList[i].hovered != null)
                        {

                             rect.Fill = brushList[i].hovered;
                        }
                    }
                    else
                    {
                        if (brushList[i].idle != null)
                        {
                            rect.Fill = brushList[i].idle;
                        }
                    }
                }
            }
           


        }

        void stayInsideCanvas(Rectangle rect, double left, double top, double right, double down,double windowHeight,double windowWidth)
        {
            if (left <= 10)
            {
                Canvas.SetLeft(rect, 0);
                if (top <= 10)
                {
                    Canvas.SetTop(rect, 0);
                }
                else
                {

                    Canvas.SetTop(rect, top);
                }
            }

            if (top <= 10)
            {
                Canvas.SetTop(rect, 0);
                if (left <= 10)
                {
                    Canvas.SetLeft(rect, 0);
                }
                else
                {
                    Canvas.SetLeft(rect, left);

                }
            }
            if (right >= (windowWidth - 15))
            {
                Canvas.SetLeft(rect, (windowWidth - rect.ActualWidth));
                if (down >= (windowHeight-35))
                {
                    //
                    Canvas.SetTop(rect, (windowHeight - rect.ActualHeight));
                }
                else
                {

                    Canvas.SetTop(rect, down - rect.ActualHeight);
                }
            }
            //
            if (down >= (windowHeight))
            {
                //
                Canvas.SetTop(rect,  (windowHeight - rect.ActualHeight));
                if (right >= windowWidth - 15)
                {
                    Canvas.SetLeft(rect, (windowWidth - rect.ActualWidth));
                }
                else
                {
                    Canvas.SetLeft(rect, right);

                }
            }
            //
            if (down >= windowHeight)
            {
                //
                Canvas.SetTop(rect, (windowHeight - rect.ActualHeight));
                if (left <= 10)
                {
                    Canvas.SetLeft(rect, 0);
                }
                else
                {
                    Canvas.SetLeft(rect, left);

                }
            }

            if (right >= windowWidth - 15)
            {
                Canvas.SetLeft(rect, (windowWidth - rect.ActualWidth));
                if (top <= 10)
                {
                    Canvas.SetTop(rect, 0);
                }
                //
                else if (down > windowHeight)
                {
                    //
                    Canvas.SetTop(rect, windowHeight - rect.ActualHeight);

                }



            }
            //
            if (top > 10 && left > 10 && down < windowHeight && right < windowWidth-15)
            {
                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, top);
            }

            if (HUD_Listbox.SelectedItem.ToString() == rect.Name)
            {
                for (int i = 0; i < ElementInfoList.Count; i++)
                {
                    if (rect.Name==ElementInfoList[i].rectName)
                    {
                        ElementInfo myInfo = new ElementInfo();

                        myInfo = ElementInfoList[i];
                        if (Canvas.GetLeft(rect) <0)
                        {
                            myInfo.posX = 0;
                        }
                        else
                        {
                            myInfo.posX = (int)Canvas.GetLeft(rect);
                        }
                        if (Canvas.GetTop(rect) < 0)
                        {
                            myInfo.posY = 0;
                        }
                        else
                        {
                            myInfo.posY = (int)Canvas.GetTop(rect);
                        }
                        ElementInfoList.RemoveAt(i);
                        ElementInfoList.Add(myInfo);
                    }
                }
            }

        }
        void stayInsideCanvasText(Viewbox myViewBox, double left, double top, double right, double down, double windowHeight, double windowWidth)
        {
            if (left <= 10)
            {
                Canvas.SetLeft(myViewBox, 0);
                if (top <= 10)
                {
                    Canvas.SetTop(myViewBox, 0);
                }
                else
                {

                    Canvas.SetTop(myViewBox, top);
                }
            }

            if (top <= 10)
            {
                Canvas.SetTop(myViewBox, 0);
                if (left <= 10)
                {
                    Canvas.SetLeft(myViewBox, 0);
                }
                else
                {
                    Canvas.SetLeft(myViewBox, left);

                }
            }
            if (right >= (windowWidth - 35))
            {
                Canvas.SetLeft(myViewBox, (windowWidth - myViewBox.ActualWidth) - 15);
                if (down >= (windowHeight - 35))
                {
                    Canvas.SetTop(myViewBox, (windowHeight - myViewBox.ActualHeight - 35));
                }
                else
                {

                    Canvas.SetTop(myViewBox, down - myViewBox.ActualHeight);
                }
            }

            if (down >= (windowHeight - 35))
            {
                Canvas.SetTop(myViewBox, (windowHeight - myViewBox.ActualHeight - 35));
                if (right >= windowWidth - 35)
                {
                    Canvas.SetLeft(myViewBox, (windowWidth - myViewBox.ActualWidth) - 15);
                }
                else
                {
                    Canvas.SetLeft(myViewBox, right);

                }
            }

            if (down >= windowHeight - 35)
            {
                Canvas.SetTop(myViewBox, (windowHeight - myViewBox.ActualHeight - 35));
                if (left <= 10)
                {
                    Canvas.SetLeft(myViewBox, 0);
                }
                else
                {
                    Canvas.SetLeft(myViewBox, left);

                }
            }

            if (right >= windowWidth - 35)
            {
                Canvas.SetLeft(myViewBox, (windowWidth - myViewBox.ActualWidth)-15);
                if (top <= 10)
                {
                    Canvas.SetTop(myViewBox, 0);
                }
                else if (down > windowHeight - 35)
                {
                    Canvas.SetTop(myViewBox, windowHeight - myViewBox.ActualHeight - 35);

                }



            }

            if (top > 10 && left > 10 && down < windowHeight - 35 && right < windowWidth - 35)
            {
                Canvas.SetLeft(myViewBox, left);
                Canvas.SetTop(myViewBox, top);
            }
        }

        void DisableAttrEditors()
        {
            Width_Slider.IsEnabled = false;
            Width_Slider.Opacity = 0.9;
            Width_TextBox.IsEnabled = false;
            Width_TextBox.Opacity = 0.5;
            Width_Label.Opacity = 0.5;

            Height_Slider.IsEnabled = false;
            Height_Slider.Opacity = 0.9;
            Height_TextBox.IsEnabled = false;
            Height_TextBox.Opacity = 0.5;
            Height_Label.Opacity = 0.5;

            UniformScale_Slider.IsEnabled = false;
            UniformScale_Slider.Opacity = 0.9;
            UniformScale_TextBox.IsEnabled = false;
            UniformScale_TextBox.Opacity = 0.5;
            UniformScale_Label.Opacity = 0.5;

            Opacity_Slider.IsEnabled = false;
            Opacity_Slider.Opacity = 0.9;
            Opacity_TextBox.IsEnabled = false;
            Opacity_TextBox.Opacity = 0.5;
            Opacity_Label.Opacity = 0.5;
        }

        void EnableAttrEditors()
        {
            Width_Slider.IsEnabled = true;
            Width_Slider.Opacity = 1;
            Width_TextBox.IsEnabled = true;
            Width_TextBox.Opacity = 1;
            Width_Label.Opacity = 1;

            Height_Slider.IsEnabled = true;
            Height_Slider.Opacity = 1;
            Height_TextBox.IsEnabled = true;
            Height_TextBox.Opacity = 1;
            Height_Label.Opacity = 1;

            UniformScale_Slider.IsEnabled = true;
            UniformScale_Slider.Opacity = 1;
            UniformScale_TextBox.IsEnabled = true;
            UniformScale_TextBox.Opacity = 1;
            UniformScale_Label.Opacity = 1;

            Opacity_Slider.IsEnabled = true;
            Opacity_Slider.Opacity = 1;
            Opacity_TextBox.IsEnabled = true;
            Opacity_TextBox.Opacity = 1;
            Opacity_Label.Opacity = 1;
        }

        private void ListBoxUpdated()
        {
            if (HUD_Listbox.SelectedItems.Count == 0)
            {
                DisableAttrEditors();
            }
            else
            {
                EnableAttrEditors();
            }
        }

        private void HUD_Listbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxUpdated();

        }

        void removeElement()
        {
            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItems.Count > 0)
                {

                    

                    if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                    {

                        for (int j = 0; j < ElementInfoList.Count; j++)
                        {
                            if (HUD_Listbox.SelectedItem.ToString() == ElementInfoList[j].rectName)
                            {
                                ElementInfoList.RemoveAt(j);
                            }
                        }

                        for (int k = 0; k < brushList.Count; k++)
                        {
                            if (HUD_Listbox.SelectedItem.ToString() == brushList[k].name)
                            {
                                brushList.RemoveAt(k);
                            }
                        }

                        HUD_Listbox.Items.Remove(HUD_Listbox.SelectedItems[0]);
                        canvas.Children.Remove(rectList[i]);
                        rectList.RemoveAt(i);


                    }

                }
            }
        }

       
        private void HudPart_Txtbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Add_Element.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }

        }

        private void Width_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Width_TextBox.Text = Width_Slider.Value.ToString();

            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                {
                    Rectangle tempRect = rectList[i];
                    
                    tempRect.Width = Width_Slider.Value;
                }
            }
        }

        private void Height_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Height_TextBox.Text = Height_Slider.Value.ToString();

            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                {
                    Rectangle tempRect = rectList[i];
                    tempRect.Height = Height_Slider.Value;
                }
            }
        }

        private void UniformScale_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UniformScale_TextBox.Text = UniformScale_Slider.Value.ToString();

            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                {
                    Rectangle tempRect = rectList[i];
                    tempRect.Height = UniformScale_Slider.Value;
                    tempRect.Width = UniformScale_Slider.Value;
                }
            }
        }

        private void Opacity_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity_TextBox.Text = Opacity_Slider.Value.ToString();

            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                {
                    Rectangle tempRect = rectList[i];
                    tempRect.Opacity = (Opacity_Slider.Value / 100);

                   
                }
            }
        }

        private void Add_Texture_Btn_Click(object sender, RoutedEventArgs e)
        {

            
            ImageBrush brush = new ImageBrush();
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "Select an Image";
            open.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
            "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
            "Portable Network Graphic (*.png)|*.png";

            if (open.ShowDialog() == true)
            {
                brush.ImageSource = new BitmapImage(new Uri(open.FileName));
            }

            bool found = false;
            int index = 0;
            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItems.Count > 0) 
                {

                    if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                    {

                        for (int j   = 0; j < ElementInfoList.Count; j++)
                        {
                            if (ElementInfoList[j].rectName == rectList[i].Name)
                            {
                                found = true;
                                index = j;
                            }
                        }
                        if (found)
                        {
                            for (int k = 0; k < brushList.Count; k++)
                            {
                                if (brushList[k].name == rectList[i].Name)
                                {
                                    if (open.SafeFileName != null)
                                    {

                                        brushStruct myStruct = new brushStruct();
                                        myStruct.name = brushList[k].name;
                                        myStruct.idle = brush;
                                        myStruct.hovered = brushList[k].hovered;
                                        myStruct.pressed = brushList[k].pressed;
                                        brushList.RemoveAt(k);
                                        brushList.Add(myStruct);
                                    }
                                }
                            }
                            ElementInfo tx = new ElementInfo();
                            if (open.SafeFileName != null)
                            {

                                Rectangle tempRect = rectList[i];
                                tempRect.Fill = brush;
                                tx = ElementInfoList[index];
                                tx.textName = open.SafeFileName;
                                
                                if (tx.Btn)
                                {
                                    Btn myBtn;
                                    myBtn.HoverTexture = ElementInfoList[index].buttonTextures.HoverTexture;
                                    myBtn.PressTexture = ElementInfoList[index].buttonTextures.PressTexture;

                                    tx.buttonTextures = myBtn;
                                }
                            }


                           

                            ElementInfoList.RemoveAt(index);
                            ElementInfoList.Add(tx);

                        }
                        else
                        {
                            
                            Rectangle tempRect = rectList[i];
                            tempRect.Fill = brush;

                            ElementInfo tx = new ElementInfo();
                           

                            tx.rectName = rectList[i].Name;
                            tx.textName = open.SafeFileName;
                            tx.Btn = false;

                            ElementInfoList.RemoveAt(index);
                            ElementInfoList.Add(tx);
                        }
                    }
                }
            }
        }

        private void Remove_Element_Btn_Click(object sender, RoutedEventArgs e)
        {
            removeElement();
        }

        private void Zorder_up_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                {
                    if (HUD_Listbox.SelectedIndex < HUD_Listbox.Items.Count - 1)
                    {
                        int location = HUD_Listbox.SelectedIndex;
                        int tradeIndex = HUD_Listbox.SelectedIndex + 1;
                        object temp = HUD_Listbox.SelectedItem;
                        object trader = HUD_Listbox.Items[tradeIndex];

                        HUD_Listbox.Items.RemoveAt(location);
                        HUD_Listbox.Items.RemoveAt(location);

                        HUD_Listbox.Items.Insert(location, trader);
                        HUD_Listbox.Items.Insert(tradeIndex, temp);
                        HUD_Listbox.SelectedIndex = tradeIndex;
                        Canvas.SetZIndex(rectList[i], HUD_Listbox.SelectedIndex);
                        for (int p = 0; p < ElementInfoList.Count; p++)
                        {
                            if (rectList[i].Name == ElementInfoList[p].rectName)
                            {
                                ElementInfo tempInfo = new ElementInfo();
                                tempInfo = ElementInfoList[p];
                                tempInfo.layerDepth = HUD_Listbox.SelectedIndex;
                                ElementInfoList.RemoveAt(p);
                                ElementInfoList.Add(tempInfo);
                            }
                        }

                        for (int j = 0; j < rectList.Count; j++)
                        {

                            if (rectList[j].Name == HUD_Listbox.Items[location].ToString())
                            {
                                Canvas.SetZIndex(rectList[j], location);
                                for (int p = 0; p < ElementInfoList.Count; p++)
                                {
                                    if (rectList[j].Name == ElementInfoList[p].rectName)
                                    {
                                        ElementInfo tempInfo = new ElementInfo();
                                        tempInfo = ElementInfoList[p];
                                        tempInfo.layerDepth = location;
                                        ElementInfoList.RemoveAt(p);
                                        ElementInfoList.Add(tempInfo);
                                    }
                                }
                            }

                        }

                    }


                }
            }
            
            
        }

        private void Zorder_down_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                {
                    if (HUD_Listbox.SelectedIndex > 0)
                    {
                        int location = HUD_Listbox.SelectedIndex;
                        int tradeIndex = HUD_Listbox.SelectedIndex - 1;
                        object temp = HUD_Listbox.SelectedItem;
                        object trader = HUD_Listbox.Items[tradeIndex];

                        HUD_Listbox.Items.RemoveAt(location);
                        HUD_Listbox.Items.RemoveAt(tradeIndex);

                        HUD_Listbox.Items.Insert(location - 1, temp);
                        HUD_Listbox.Items.Insert(location, trader);
                        HUD_Listbox.SelectedIndex = location - 1;
                        Canvas.SetZIndex(rectList[i], HUD_Listbox.SelectedIndex);
                        for (int p = 0; p < ElementInfoList.Count; p++)
                        {
                            if (rectList[i].Name == ElementInfoList[p].rectName)
                            {
                                ElementInfo tempInfo = new ElementInfo();
                                tempInfo = ElementInfoList[p];
                                tempInfo.layerDepth = HUD_Listbox.SelectedIndex;
                                ElementInfoList.RemoveAt(p);
                                ElementInfoList.Add(tempInfo);
                            }
                        }

                        for (int j = 0; j < rectList.Count; j++)
                        {

                            if (rectList[j].Name == HUD_Listbox.Items[location].ToString())
                            {
                                Canvas.SetZIndex(rectList[j], location);
                                for (int p = 0; p < ElementInfoList.Count; p++)
                                {
                                    if (rectList[j].Name == ElementInfoList[p].rectName)
                                    {
                                        ElementInfo tempInfo = new ElementInfo();
                                        tempInfo = ElementInfoList[p];
                                        tempInfo.layerDepth = location;
                                        ElementInfoList.RemoveAt(p);
                                        ElementInfoList.Add(tempInfo);
                                    }
                                }
                            }

                        }

                    }


                }
            }
        }

        private void HudPart_Txtbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void Width_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            
            
        }

        private void Width_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
           
        }

       

        private void Opacity_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Opacity_TextBox.Text.ToString();
            decimal test = 0;

            bool succesfull = Decimal.TryParse(text, out test);
            Opacity_Slider.Value = (double)test;
        }

        private void UniformScale_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = UniformScale_TextBox.Text.ToString();
            decimal test = 0;

            bool succesfull = Decimal.TryParse(text, out test);
            UniformScale_Slider.Value = (double)test;
        }

        private void Height_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Height_TextBox.Text.ToString();
            decimal test = 0;

            bool succesfull = Decimal.TryParse(text, out test);
            Height_Slider.Value = (double)test;
        }

        private void Width_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Width_TextBox.Text.ToString();
            decimal test = 0;

            bool succesfull = Decimal.TryParse(text, out test);
            Width_Slider.Value = (double)test;
        }

        private void Make_Btn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < rectList.Count; i++)
            {
                if (HUD_Listbox.SelectedItems.Count > 0)
                {

                    if (HUD_Listbox.SelectedItem.ToString() == rectList[i].Name)
                    {
                        for (int j = 0; j < ElementInfoList.Count; j++)
                        {
                            if (ElementInfoList[j].rectName == rectList[i].Name)
                            {
                                ElementInfo temp = new ElementInfo();
                                temp = ElementInfoList[j];
                                temp.Btn = true;

                                ImageBrush Idle = new ImageBrush();
                                ImageBrush hovered = new ImageBrush();
                                ImageBrush pressed = new ImageBrush();
                                OpenFileDialog open = new OpenFileDialog();
                                open.Title = "Select an idle image";
                                open.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                "Portable Network Graphic (*.png)|*.png";

                                if (open.ShowDialog() == true)
                                {
                                    Idle.ImageSource = new BitmapImage(new Uri(open.FileName));
                                }
                                if (open.SafeFileName != "")
                                {

                                    temp.textName = open.SafeFileName;
                                    rectList[i].Fill = Idle;
                                }
                                else
                                {
                                    MessageBoxResult error = MessageBox.Show("You must chose an idle texture", "Texture Error", MessageBoxButton.OK);
                                    return;
                                }
                                
                                open.Title = "Select a hover image";
                                if (open.ShowDialog() == true)
                                {
                                    hovered.ImageSource = new BitmapImage(new Uri(open.FileName));
                                }

                                if (open.SafeFileName != "")
                                {

                                    temp.buttonTextures.HoverTexture = open.SafeFileName;
                                }
                                else
                                {
                                    MessageBoxResult error = MessageBox.Show("You must chose a hover image", "Texture Error", MessageBoxButton.OK);
                                    return;
                                }
                                open.Title = "Select a pressed image";
                                if (open.ShowDialog() == true)
                                {
                                    pressed.ImageSource = new BitmapImage(new Uri(open.FileName));
                                }
                                if (open.SafeFileName != "")
                                {

                                    temp.buttonTextures.PressTexture = open.SafeFileName;
                                }
                                else
                                {
                                    MessageBoxResult error = MessageBox.Show("You must chose a pressed image", "Texture Error", MessageBoxButton.OK);
                                    return;
                                }

                                ElementInfoList.RemoveAt(j);
                                ElementInfoList.Add(temp);

                                for (int k = 0; k < brushList.Count; k++)
                                {
                                    if (brushList[k].name == rectList[i].Name)
                                    {
                                        brushStruct myStruct = new brushStruct();
                                        myStruct.name = brushList[k].name;
                                        myStruct.idle = Idle;
                                        myStruct.hovered = hovered;
                                        myStruct.pressed = pressed;
                                        brushList.RemoveAt(k);
                                        brushList.Add(myStruct);
                                    }
                                }

                            }
                        }

                    }
                }
            }
        }

        private void rect_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_isRectDragInProg)
            {
                Rectangle rect = (Rectangle)sender;
                for (int i = 0; i < brushList.Count; i++)
                {
                    if (brushList[i].name == rect.Name)
                    {
                        if (brushList[i].hovered != null)
                        {
                            rect.Fill = brushList[i].hovered;
                        }
                    }
                }
            }
        }

        private void rect_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isRectDragInProg)
            {
                Rectangle rect = (Rectangle)sender;
                for (int i = 0; i < brushList.Count; i++)
                {
                    if (brushList[i].name == rect.Name)
                    {
                        if (brushList[i].idle != null)
                        {
                            rect.Fill = brushList[i].idle;
                        }
                    }
                }
            }
        }

        private void Exit_Btn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void Text_width_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            for (int i = 0; i < viewBoxList.Count; i++)
            {
                if (HUD_Listbox.SelectedItem.ToString() == viewBoxList[i].Name)
                {
                    Viewbox tempViewBox = viewBoxList[i];

                    tempViewBox.Width = Text_width_slider.Value;
                }
            }
            Text_width_Box.Text = Text_width_slider.Value.ToString();
                      
        }

        private void Text_width_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Text_width_Box.Text.ToString();
            decimal test = 0;

            bool succesfull = Decimal.TryParse(text, out test);
            Text_width_slider.Value = (double)test;
        }

        private void Text_height_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            for (int i = 0; i < viewBoxList.Count; i++)
            {
                if (HUD_Listbox.SelectedItem.ToString() == viewBoxList[i].Name)
                {
                    Viewbox tempViewBox = viewBoxList[i];

                    tempViewBox.Height = Text_height_slider.Value;
                }
            }
            Text_height_Box.Text = Text_height_slider.Value.ToString();
        }

        private void Text_height_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Text_height_Box.Text.ToString();
            decimal test = 0;

            bool succesfull = Decimal.TryParse(text, out test);
            Text_height_slider.Value = (double)test;
        }

        private void Text_opacity_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            

            Text_opacity_box.Text = Text_opacity_slider.Value.ToString();
            
        }

        private void Text_opacity_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Text_opacity_box.Text.ToString();
            decimal test = 0;

            bool succesfull = Decimal.TryParse(text, out test);
            Text_opacity_slider.Value = (double)test;
        }

        private void Add_text_Btn_Click(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < viewBoxList.Count; i++)
            {
                if (viewBoxList[i].Name == HudPart_Txtbox.Text)
                {
                    MessageBoxResult error = MessageBox.Show("Name already exist, please chose a different name", "Name Error", MessageBoxButton.OK);
                    return;
                }
            }
            Viewbox myViewBox = new Viewbox();
            
            TextBlock myTxtBlock = new TextBlock();
            myTxtBlock.Text = Add_txt_box.Text.ToString();
            myViewBox.Name = HudPart_Txtbox.Text;

            myViewBox.MouseLeftButtonDown += ViewBox_MouseLeftButtonDown;
            myViewBox.MouseLeftButtonUp += ViewBox_MouseLeftButtonUp;
            myViewBox.MouseMove += ViewBox_MouseMove;

            myViewBox.Width = Text_width_slider.Value;
            myViewBox.Height = Text_height_slider.Value;
            myViewBox.StretchDirection = StretchDirection.Both;
            myViewBox.Stretch = Stretch.Fill;
            myTxtBlock.Background = Brushes.Cyan;
            myTxtBlock.Opacity = Text_opacity_slider.Value;
            
            myTxtBlock.TextWrapping = TextWrapping.Wrap;

            myViewBox.Child = myTxtBlock;
            viewBoxList.Add(myViewBox);
            canvas.Children.Add(myViewBox);
            HUD_Listbox.Items.Add(myViewBox.Name);
        }

        private void Remove_Txt_Btn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < HUD_Listbox.SelectedItems.Count; i++)
            {

                for (int j = 0; j < viewBoxList.Count; j++)
                {
                    if (HUD_Listbox.SelectedItem.ToString() == viewBoxList[j].Name)
                    {
                        canvas.Children.Remove(viewBoxList[j]);
                        HUD_Listbox.Items.Remove(HUD_Listbox.SelectedItems[0]);
                        viewBoxList.RemoveAt(j);
                    }

                }


                
            }
        }

        private void ViewBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isRectDragInProg = true;
            Viewbox myViewBox = (Viewbox)sender;

            myViewBox.CaptureMouse();



            HUD_Listbox.UnselectAll();
            for (int i = 0; i < HUD_Listbox.Items.Count; i++)
            {

                if (HUD_Listbox.Items[i].ToString() == myViewBox.Name)
                {
                    HUD_Listbox.SelectedItem = HUD_Listbox.Items.GetItemAt(i);
                }
            }
           
            ListBoxUpdated();

        }

        private void ViewBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isRectDragInProg = false;
            Viewbox myViewbox = (Viewbox)sender;
            myViewbox.ReleaseMouseCapture();
        }

        private void ViewBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRectDragInProg) return;
            canvas.Height = CanvasWindow.ActualHeight;
            canvas.Width = CanvasWindow.ActualWidth;

            Viewbox myViewbox = (Viewbox)sender;

            var mousePos = e.GetPosition(canvas);

            double windowHeight = CanvasWindow.ActualHeight;
            double windowWidth = CanvasWindow.ActualWidth;

            double left = mousePos.X - (myViewbox.ActualWidth / 2);
            double top = mousePos.Y - (myViewbox.ActualHeight / 2);
            double right = mousePos.X + (myViewbox.ActualWidth / 2);
            double down = mousePos.Y + (myViewbox.ActualHeight / 2);

            stayInsideCanvasText(myViewbox, left, top, right, down, windowHeight, windowWidth);
        }

        private void Export_Btn_Click(object sender, RoutedEventArgs e)
        {
            
            nameWindow.Height = 300;
            nameWindow.Width = 300;
            Grid nameGrid = new Grid();
            nameWindow.Content = nameGrid;
            nameGrid.RowDefinitions.Add(new RowDefinition());
            Label enterLabel = new Label();
            enterLabel.Content = "Enter name";
            Grid.SetRow(enterLabel, 0);
            nameGrid.Children.Add(enterLabel);
            
            nameGrid.RowDefinitions.Add(new RowDefinition());
            
            TextBox nameBox = new TextBox();

            nameBox.TextChanged += NameBox_TextChanged;
            nameBox.Name = "nameBox";
            Grid.SetRow(nameBox, 1);
            nameGrid.Children.Add(nameBox);

            nameGrid.RowDefinitions.Add(new RowDefinition());
            Button ExportButton = new Button();
            ExportButton.Content = "Export";
            ExportButton.Click += ExportButton_Click;
            ExportButton.Width = 300;
            ExportButton.Height = 100;
            Grid.SetRow(ExportButton, 2);
            nameGrid.Children.Add(ExportButton);

            nameWindow.Show();
            
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            fileName = txt.Text;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (Package.Count>0)
            {
                Package.Clear();
            }
            if (textPackage.Count>0)
            {
                textPackage.Clear();

            }
            string directory = System.AppDomain.CurrentDomain.BaseDirectory;

            string path = directory;
            string temp = System.IO.Path.Combine(path, fileName);
            path = temp + ".HuD";
            

            if (File.Exists(path))
            {
                MessageBoxResult error = MessageBox.Show("File name already exist, please chose a different name", "Name Error", MessageBoxButton.OK);
                return;
            }

            

            Header fileHeader = new Header();
            fileHeader.NrOfRects = rectList.Count;
            fileHeader.NrOfTexts = viewBoxList.Count;

            for (int i = 0; i < rectList.Count; i++)
            {
                rectangleExport rectExp = new rectangleExport();
                rectExp.rectNameLength =  rectList[i].Name.Length;
                rectExp.rectName = rectList[i].Name;
                rectExp.Width = (int)rectList[i].Width;
                rectExp.Height = (int)rectList[i].Height;
                double opacity = rectList[i].Opacity;
                opacity = opacity * 100;
                rectExp.Opacity = (int)opacity;
                

                for (int j = 0; j < ElementInfoList.Count; j++)
                {
                    if (ElementInfoList[j].rectName == rectList[i].Name)
                    {
                        rectExp.textNameLength = ElementInfoList[j].textName.Length;
                        rectExp.textName = ElementInfoList[j].textName;
                        rectExp.layerDepth = ElementInfoList[j].layerDepth;
                        rectExp.PositionX = ElementInfoList[j].posX;
                        rectExp.PositionY = ElementInfoList[j].posY;
                        if (ElementInfoList[j].Btn)
                        {
                            rectExp.Btn = ElementInfoList[j].Btn;
                            rectExp.hoverTexLength = ElementInfoList[j].buttonTextures.HoverTexture.Length;
                            rectExp.hoverTex = ElementInfoList[j].buttonTextures.HoverTexture;
                            rectExp.pressTexLength = ElementInfoList[j].buttonTextures.PressTexture.Length;
                            rectExp.PressTex = ElementInfoList[j].buttonTextures.PressTexture;

                        }
                        else
                        {
                            rectExp.Btn = false;
                        }
                    }
                }

                Package.Add(rectExp);
            }
            for (int i = 0; i < viewBoxList.Count; i++)
            {
                TextExport package = new TextExport();
                package.TextName = viewBoxList[i].Name;
                TextBlock child = (TextBlock)viewBoxList[i].Child;
                package.TextContent = child.Text;
                package.Height = (int)viewBoxList[i].Height;
                package.Width = (int)viewBoxList[i].Width;
                Point position = viewBoxList[i].PointToScreen(new Point(0d, 0d));

                if (position.X > 1920)
                {
                    package.PosX = 1920;
                }
                else
                {
                    package.PosX = (int)position.X;
                }
                if (position.Y > 1040)
                {
                    package.PosY = 1080;
                }
                else
                {
                    package.PosY = (int)position.Y;
                }
                textPackage.Add(package);
            }



            FileStream writeStream = new FileStream(path, FileMode.Create);
            using (BinaryWriter file = new BinaryWriter(writeStream))
            {
                file.Write(Package.Count);
                file.Write(textPackage.Count);
                foreach (rectangleExport item in Package)
                {
                    if (item.rectName == null)
                    {
                        MessageBoxResult error = MessageBox.Show("Your Elements Require Names ", "Name Error", MessageBoxButton.OK);
                        return;
                    }
                    file.Write(item.rectNameLength);
                    file.Write(item.rectName);
                    if (item.textName == null)
                    {
                        MessageBoxResult error = MessageBox.Show("Your elements require atleast 1 texture", "Texture Error", MessageBoxButton.OK);
                        return;
                    }
                    file.Write(item.textNameLength);
                    file.Write(item.textName);

                    file.Write(item.Width);
                    file.Write(item.Height);
                    file.Write(item.PositionX);
                    file.Write(item.PositionY);

                    if (item.Btn)
                    {
                        file.Write(true);
                        if (item.hoverTex == null)
                        {
                            MessageBoxResult error = MessageBox.Show("Your buttons require hover textures", "Texture Error", MessageBoxButton.OK);
                            return;
                        }
                        file.Write(item.hoverTexLength);
                        file.Write(item.hoverTex);
                        if (item.PressTex == null) 
                        {
                            MessageBoxResult error = MessageBox.Show("Your buttons require pressed textures", "Texture Error", MessageBoxButton.OK);
                            return;
                        }
                        file.Write(item.pressTexLength);
                        file.Write(item.PressTex);

                    }
                    else
                    {
                        file.Write(false);
                    }

                    file.Write(item.Opacity);
                    file.Write(item.layerDepth);
                    
                }
                foreach (TextExport item in textPackage)
                {
                    if (item.TextName == null) 
                    {
                        MessageBoxResult error = MessageBox.Show("Your Texts need names", "Naming Error", MessageBoxButton.OK);
                        return;
                    }
                    file.Write(item.TextName.Length);
                    file.Write(item.TextName);
                    if (item.TextContent == null)
                    {
                        MessageBoxResult error = MessageBox.Show("Your Texts need content", "Content Error", MessageBoxButton.OK);
                        return;
                    }
                    file.Write(item.TextContent.Length);
                    file.Write(item.TextContent);
                    file.Write(item.Height);
                    file.Write(item.Width);
                    file.Write(item.PosX);
                    file.Write(item.PosY);
                    
                }
            }


            MessageBoxResult error2 = MessageBox.Show("Export Succesfull!", "Texture Error", MessageBoxButton.OK);
            
            Package.Clear();
            textPackage.Clear();

            nameWindow.Hide();
        }
    }
}
