﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CodeAtlasVSIX
{
    public class OrderData
    {
        public int m_order;
        public Point m_point;
        public OrderData(int order, Point point)
        {
            m_order = order;
            m_point = point;
        }
    }

    public class CodeUIEdgeItem: Shape
    {
        public string m_srcUniqueName;
        public string m_tarUniqueName;
        PathGeometry m_geometry = new PathGeometry();
        bool m_isDirty = false;
        bool m_isSelected = false;
        DateTime m_mouseDownTime = new DateTime();
        public OrderData m_orderData = null;
        Point m_p0, m_p1, m_p2, m_p3;

        public CodeUIEdgeItem(string srcName, string tarName)
        {
            m_srcUniqueName = srcName;
            m_tarUniqueName = tarName;

            this.MouseDown += new MouseButtonEventHandler(MouseDownCallback);
            this.MouseUp += new MouseButtonEventHandler(MouseUpCallback);
            this.MouseMove += new MouseEventHandler(MouseMoveCallback);
            this.MouseEnter += new MouseEventHandler(MouseEnterCallback);
            this.MouseLeave += new MouseEventHandler(MouseLeaveCallback);

            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Color.FromArgb(255, 255, 255, 0);
            this.Fill = Brushes.Transparent;
            this.Stroke = brush;
            BuildGeometry();
        }

        Point CalculateBezierPoint(double t, Point p1, Point p2, Point p3, Point p4)
        {
            Point p = new Point();
            double tPower3 = t * t * t;
            double tPower2 = t * t;
            double oneMinusT = 1 - t;
            double oneMinusTPower3 = oneMinusT * oneMinusT * oneMinusT;
            double oneMinusTPower2 = oneMinusT * oneMinusT;
            p.X = oneMinusTPower3 * p1.X + (3 * oneMinusTPower2 * t * p2.X) + (3 * oneMinusT * tPower2 * p3.X) + tPower3 * p4.X;
            p.Y = oneMinusTPower3 * p1.Y + (3 * oneMinusTPower2 * t * p2.Y) + (3 * oneMinusT * tPower2 * p3.Y) + tPower3 * p4.Y;
            return p;
        }

        public Point PointAtPercent(double t)
        {
            return CalculateBezierPoint(t, m_p0, m_p1, m_p2, m_p3);
        }

        public Point GetMiddlePos()
        {
            var scene = UIManager.Instance().GetScene();
            var srcNode = scene.GetNode(m_srcUniqueName);
            var tarNode = scene.GetNode(m_tarUniqueName);
            if (srcNode == null || tarNode == null)
            {
                return new Point();
            }
            var srcPnt = srcNode.Pos;
            var tarPnt = tarNode.Pos;
            return srcPnt + (tarPnt - srcPnt) * 0.5;
        }

        public void GetNodePos(out Point srcPos, out Point tarPos)
        {
            var scene = UIManager.Instance().GetScene();
            var srcNode = scene.GetNode(m_srcUniqueName);
            var tarNode = scene.GetNode(m_tarUniqueName);
            if (srcNode == null || tarNode == null)
            {
                srcPos = tarPos = new Point();
            }
            srcPos = srcNode.Pos;
            tarPos = tarNode.Pos;
        }

        public double FindCurveYPos(double x)
        {
            var sign = 1.0;
            if (m_p3.X < m_p0.X)
            {
                sign = -1.0;
            }
            var minT = 0.0;
            var maxT = 1.0;
            var minPnt = PointAtPercent(minT);
            var maxPnt = PointAtPercent(maxT);
            for (int i = 0; i < 8; i++)
            {
                var midT = (minT + maxT) * 0.5;
                var midPnt = PointAtPercent(midT);
                if ((midPnt.X - x) * sign < 0.0)
                {
                    minT = midT;
                    minPnt = midPnt;
                }
                else
                {
                    maxT = midT;
                    maxPnt = midPnt;
                }
                if (Math.Abs(minPnt.Y - maxPnt.Y) < 0.01)
                {
                    break;
                }
            }
            return (minPnt.Y + maxPnt.Y) * 0.5;
        }

        public bool IsDirty
        {
            get { return m_isDirty; }
            set
            {
                m_isDirty = value;
                if (value == true)
                {
                    UIManager.Instance().GetScene().Invalidate();
                }
            }
        }

        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                m_isSelected = value;
                if (m_isSelected)
                {
                    StrokeThickness = 2.0;
                }
                else
                {
                    StrokeThickness = 1.0;
                }
            }
        }
        //public Point StartPoint
        //{
        //    set { SetValue(StartPointProperty, value); }
        //    get { return (Point)GetValue(StartPointProperty); }
        //}

        //public Point EndPoint
        //{
        //    set { SetValue(EndPointProperty, value); }
        //    get { return (Point)GetValue(EndPointProperty); }
        //}

        //public static readonly DependencyProperty StartPointProperty =
        //    DependencyProperty.Register("StartPoint", typeof(Point), typeof(CodeUIEdgeItem),
        //        new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));

        //public static readonly DependencyProperty EndPointProperty =
        //    DependencyProperty.Register("EndPoint", typeof(Point), typeof(CodeUIEdgeItem),
        //        new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));

        void MouseDownCallback(object sender, MouseEventArgs args)
        {
            var newDownTime = DateTime.Now;
            double duration = (newDownTime - m_mouseDownTime).TotalMilliseconds;
            m_mouseDownTime = newDownTime;
            if (duration > System.Windows.Forms.SystemInformation.DoubleClickTime)
            {
                MouseClickCallback(sender, args);
            }
            else
            {
                MouseDoubleClickCallback(sender, args);
            }
        }

        void MouseClickCallback(object sender, MouseEventArgs args)
        {
            IsSelected = true;
        }

        void MouseDoubleClickCallback(object sender, MouseEventArgs args)
        {
            IsSelected = true;
            System.Console.Out.WriteLine("double click");
        }

        void MouseMoveCallback(object sender, MouseEventArgs args)
        {
        }

        void MouseUpCallback(object sender, MouseEventArgs e)
        {
        }

        void MouseEnterCallback(object sender, MouseEventArgs e)
        {
            if(!IsSelected)
            {
                StrokeThickness = 2.0;
            }
        }

        void MouseLeaveCallback(object sender, MouseEventArgs e)
        {
            if (!IsSelected)
            {
                StrokeThickness = 1.0;
            }
        }

        void BuildGeometry()
        {
            var scene = UIManager.Instance().GetScene();
            var srcNode = scene.GetNode(m_srcUniqueName);
            var tarNode = scene.GetNode(m_tarUniqueName);
            //this.StartPoint = srcNode.Pos();
            //this.EndPoint = tarNode.Pos();
        }

        public int GetCallOrder()
        {
            return 0;
        }

        public void Invalidate()
        {
            var scene = UIManager.Instance().GetScene();
            var srcNode = scene.GetNode(m_srcUniqueName);
            var tarNode = scene.GetNode(m_tarUniqueName);
            if (IsDirty || srcNode.IsDirty || tarNode.IsDirty)
            {
                this.Dispatcher.Invoke((ThreadStart)delegate
                {
                    this._Invalidate();
                });
            }
        }

        void _Invalidate()
        {
            InvalidateVisual();
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                //EllipseGeometry circle0 = new EllipseGeometry(srcCtrlPnt, 20.0, 20.0);
                //EllipseGeometry circle1 = new EllipseGeometry(tarCtrlPnt, 20.0, 20.0);

                //var group = new GeometryGroup();
                //group.Children.Add(circle0);
                //group.Children.Add(circle1);
                //group.Children.Add(geometry);
                //return group;

                var scene = UIManager.Instance().GetScene();
                var srcNode = scene.GetNode(m_srcUniqueName);
                var tarNode = scene.GetNode(m_tarUniqueName);
                m_p0 = srcNode.Pos;
                m_p3 = tarNode.Pos;
                m_p1 = new Point(m_p0.X * 0.4 + m_p3.X * 0.6, m_p0.Y);
                m_p2 = new Point(m_p0.X * 0.6 + m_p3.X * 0.4, m_p3.Y);

                var segment = new BezierSegment(m_p1, m_p2, m_p3, true);
                var figure = new PathFigure();
                figure.StartPoint = m_p0;
                figure.Segments.Add(segment);
                figure.IsClosed = false;
                m_geometry = new PathGeometry();
                m_geometry.Figures.Add(figure);

                return m_geometry;
            }
        }
    }
}
