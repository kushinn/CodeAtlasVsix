﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace CodeAtlasVSIX
{
    /// <summary>
    /// Interaction logic for FileList.xaml
    /// </summary>
    public partial class FileList : UserControl
    {
        Dictionary<string, Tuple<Color, FormattedText>> m_schemeNameDict =
            new Dictionary<string, Tuple<Color, FormattedText>>();
        double m_margin = 10.0;
        double m_lineHeight = 10.0;
        double m_lineSpace = 3.0;
        //double m_colorTextSpace = 5.0;
        double m_maxTextWidth = 0.0;
        double m_fontSize = 12.0;
        double m_rectThickness = 5.0;
        List<FormattedText> m_keyText = new List<FormattedText>();
        double m_formatWidth = 0.0;
        List<Button> m_buttonList = new List<Button>();

        public FileList()
        {
            InitializeComponent();

            ResourceSetter resMgr = new ResourceSetter(this);
            resMgr.SetStyle();

            CheckAndAddFormattedText(5);

            m_buttonList.Add(schemeButton0);
            m_buttonList.Add(schemeButton1);
            m_buttonList.Add(schemeButton2);
            m_buttonList.Add(schemeButton3);
            m_buttonList.Add(schemeButton4);

            m_buttonList.Add(schemeButton5);
            m_buttonList.Add(schemeButton6);
            m_buttonList.Add(schemeButton7);
            m_buttonList.Add(schemeButton8);
            m_buttonList.Add(schemeButton9);
        }

        void CheckAndAddFormattedText(int idx)
        {
            m_formatWidth = 0;
            for (int i = 0; i < idx; i++)
            {
                if (i >= m_keyText.Count)
                {
                    var formattedText = new FormattedText(string.Format("[{0}]", i + 1),
                                                            CultureInfo.CurrentUICulture,
                                                            FlowDirection.LeftToRight,
                                                            new Typeface("arial"),
                                                            m_fontSize,
                                                            Brushes.LightSalmon);
                    m_keyText.Add(formattedText);
                }
                m_formatWidth = Math.Max(m_formatWidth, m_keyText[i].Width);
            }
        }

        public static Color NameToColor(string name)
        {
            uint hashVal = (uint)name.GetHashCode();
            var h = ((hashVal) & 0xffff) / 65535.0;
            var s = ((hashVal >> 16) & 0xff) / 255.0;
            var l = ((hashVal >> 24) & 0xff) / 255.0;
            return CodeUIItem.HSLToRGB(h, 0.7 + s * 0.2, 0.75 + l * 0.15);
        }

        public void BuildFileListLegend()
        {
            var scene = UIManager.Instance().GetScene();
            var itemDict = scene.GetItemDict();
            var schemeNameList = scene.GetFileList();
            m_schemeNameDict.Clear();
            m_formatWidth = 0;
            CheckAndAddFormattedText(schemeNameList.Count);

            this.Dispatcher.BeginInvoke((ThreadStart)delegate
            {
                int maxDuration = 1;
                for (int i = 0; i < schemeNameList.Count; i++)
                {
                    maxDuration = Math.Max(maxDuration, schemeNameList[i].m_duration);
                }

                for (int i = 0; i < schemeNameList.Count; i++)
                {
                    var schemeName = schemeNameList[i].m_path;
                    int duration = schemeNameList[i].m_duration;
                    double ratio = Math.Pow((double)duration / (double)maxDuration, 0.25);

                    int idx = schemeName.LastIndexOf('/');
                    if (idx != -1)
                    {
                        schemeName = schemeName.Substring(idx + 1);
                    }
                    idx = schemeName.LastIndexOf('\\');
                    if (idx != -1)
                    {
                        schemeName = schemeName.Substring(idx + 1);
                    }

                    idx = schemeName.IndexOf('.');
                    string colorName = schemeName;
                    if(idx != -1)
                        colorName = schemeName.Substring(0, idx);
                    Color schemeColor = FileList.NameToColor(colorName);
                    //schemeColor.A = (byte)(50 * (1 - ratio) + 255 * ratio);

                    var formattedText = new FormattedText(schemeName,
                                                            CultureInfo.CurrentUICulture,
                                                            FlowDirection.LeftToRight,
                                                            new Typeface("arial"),
                                                            m_fontSize,
                                                            new SolidColorBrush(schemeColor));
                    m_schemeNameDict[schemeName] = new Tuple<Color, FormattedText>(schemeColor, formattedText);
                }

                var nScheme = m_schemeNameDict.Count;
                var maxWidth = 0.0;
                foreach (var item in m_schemeNameDict)
                {
                    var className = item.Key;
                    var textObj = item.Value.Item2;
                    var classSize = new Size(textObj.Width, textObj.Height);
                    maxWidth = Math.Max(maxWidth, textObj.Width);
                }

                m_maxTextWidth = maxWidth;
                //this.MinWidth = this.Width = m_lineHeight + m_colorTextSpace + maxWidth + m_margin * 2;
                //this.MinHeight = this.Height = m_classNameDict.Count * (m_lineHeight + m_lineSpace) - m_lineSpace + m_margin * 2;
                //InvalidateArrange();

                for (int i = 0; i < m_buttonList.Count; i++)
                {
                    var button = m_buttonList[i];
                    if (i < nScheme)
                    {
                        button.Visibility = Visibility.Visible;
                        //button.Content = schemeNameList[i];
                        //button.MinHeight = 0;
                        //button.FontSize = 12;
                        //button.Padding = new Thickness(1,-10,0,-10);
                        button.Margin = new Thickness(0, 1, 0, 1);
                        button.BorderThickness = new Thickness(6, 0, 0, 0);
                        button.Background = new SolidColorBrush();
                        //button.Foreground = Brushes.Moccasin;// new SolidColorBrush(Color.FromArgb(255,255,255,0));
                        button.Width = m_maxTextWidth + 13;//m_formatWidth + 
                        button.MaxWidth = button.Width;
                        button.MinWidth = button.Width;
                        Style style = button.TryFindResource("SchemeButtonStyle") as Style;
                        button.Style = style;
                        //button.BorderBrush = new SolidColorBrush();
                    }
                    else
                    {
                        button.Visibility = Visibility.Collapsed;
                    }
                }

                var rowDefinition = listGrid.RowDefinitions;
                for (int i = 0; i < 10; i++)
                {
                    rowDefinition[i].Height = new GridLength(i < schemeNameList.Count ? 13.0 : 0.0);
                }

                //this.MinHeight = 200;
                //this.MinWidth = 200;
                InvalidateVisual();
            });
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var scene = UIManager.Instance().GetScene();
            scene.AcquireLock();
            var itemDict = scene.GetItemDict();

            double x = m_margin;
            double y = m_margin;
            double contentHeight = 0;
            contentHeight = (m_lineHeight + m_lineSpace) * m_schemeNameDict.Count - m_lineSpace;
            var parent = this.Parent as Grid;
            if (parent != null)
            {
                //y = parent.ActualHeight - contentHeight - m_margin;
            }
            var colorSize = new Size(m_lineHeight * 2, 2);
            double contentWidth = m_maxTextWidth + m_lineSpace;// m_formatWidth + colorSize.Width

            if (m_schemeNameDict.Count > 0)
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)), new Pen(), new Rect(new Point(x - m_rectThickness, y - m_rectThickness), new Size(contentWidth + m_rectThickness * 2 + m_lineSpace * 1.5 + 6, contentHeight + m_rectThickness * 2)));
            }

            int i = 0;
            foreach (var item in m_schemeNameDict)
            {
                var className = item.Key;
                var color = item.Value.Item1;
                var textObj = item.Value.Item2;

                x = m_margin;

                //dc.DrawRectangle(new SolidColorBrush(color), new Pen(), new Rect(new Point(x, y + 4), colorSize));
                x += 6 + m_lineSpace * 1.3;//colorSize.Width + 

                //dc.DrawText(m_keyText[i], new Point(x, y - 2));
                //x += m_formatWidth + m_lineSpace * 0.5;

                dc.DrawText(textObj, new Point(x, y - 2));
                y += m_lineHeight + m_lineSpace;
                ++i;
            }
            scene.ReleaseLock();
        }

        public void ShowFile(int ithScheme)
        {
            var scene = UIManager.Instance().GetScene();
            scene.ShowIthFile(ithScheme);
        }

        private void schemeButton0_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(0);
        }
        private void schemeButton1_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(1);
        }
        private void schemeButton2_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(2);
        }
        private void schemeButton3_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(3);
        }
        private void schemeButton4_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(4);
        }
        private void schemeButton5_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(5);
        }
        private void schemeButton6_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(6);
        }
        private void schemeButton7_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(7);
        }
        private void schemeButton8_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(8);
        }
        private void schemeButton9_Click(object sender, RoutedEventArgs e)
        {
            ShowFile(9);
        }
    }
}
