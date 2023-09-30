﻿#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class FormCalibratePlane : Form
    {
        private CalibrationHelper calibrationHelper;
        private DrawingPlane drawingPlane;
        private QuadrilateralF quadImage;
        private QuadrilateralF quadPanel;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormCalibratePlane(CalibrationHelper calibrationHelper, DrawingPlane drawingPlane)
        {
            this.calibrationHelper = calibrationHelper;
            this.drawingPlane = drawingPlane;
            this.quadImage = drawingPlane.QuadImage;
            
            InitializeComponent();
            LocalizeForm();
            InitializeValues();
        }
        
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgCalibratePlane_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Calibration;
            
            // Combo Units (MUST be filled in the order of the enum)
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Millimeters + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Millimeters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Centimeters + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Centimeters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Meters + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Meters) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Inches + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Inches) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Feet + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Feet) + ")");
            cbUnit.Items.Add(ScreenManagerLang.LengthUnit_Yards + " (" + UnitHelper.LengthAbbreviation(LengthUnit.Yards) + ")");

            string customLengthUnit = PreferencesManager.PlayerPreferences.CustomLengthUnit;
            string customLengthAbbreviation = PreferencesManager.PlayerPreferences.CustomLengthAbbreviation;
            if (string.IsNullOrEmpty(customLengthUnit))
            {
                customLengthUnit = ScreenManagerLang.LengthUnit_Percentage;
                customLengthAbbreviation = "%";
            }

            cbUnit.Items.Add(customLengthUnit + " (" + customLengthAbbreviation + ")");
        }
        private void InitializeValues()
        {
            if (calibrationHelper.IsCalibrated && calibrationHelper.CalibratorType == CalibratorType.Plane)
            {
                SizeF size = calibrationHelper.CalibrationByPlane_GetRectangleSize();
                tbA.Text = String.Format("{0:0.00}", size.Width);
                tbB.Text = String.Format("{0:0.00}", size.Height);
                cbUnit.SelectedIndex = (int)calibrationHelper.LengthUnit;
            }
            else
            { 
                // Default values for perspective and flat grid.
                tbA.Text = "100";
                tbB.Text = "100";
                cbUnit.SelectedIndex = (int)LengthUnit.Centimeters;
            }

            PointF offset = calibrationHelper.GetWorldOffset();
            tbOffsetX.Text = String.Format("{0:0.00}", offset.X);
            tbOffsetY.Text = String.Format("{0:0.00}", offset.Y);
            lblOffsetUnit.Text = UnitHelper.LengthAbbreviation(calibrationHelper.LengthUnit);

            lblSeparator.Text = "×";
            lblHelpText.Text = ScreenManagerLang.dlgCalibratePlane_HelpPlane;
            
            // Prepare drawing.
            RectangleF bbox = quadImage.GetBoundingBox();
            SizeF usableSize = new SizeF(pnlQuadrilateral.Width * 0.8f, pnlQuadrilateral.Height * 0.8f);
            float ratioWidth = bbox.Width / usableSize.Width;
            float ratioHeight = bbox.Height / usableSize.Height;
            float ratio = Math.Max(ratioWidth, ratioHeight);
            
            float width = bbox.Width / ratio;
            float height = bbox.Height / ratio;
            float top = (pnlQuadrilateral.Height - height) / 2;
            float left = (pnlQuadrilateral.Width - width) / 2;
            
            quadPanel = new QuadrilateralF();
            for(int i = 0; i<4; i++)
            {
                PointF p = quadImage[i].Translate(-bbox.Left, -bbox.Top);
                p = p.Scale(1/ratio, 1/ratio);
                p = p.Translate(left, top);
                quadPanel[i] = p;
            }

            UpdateTheoreticalPrecision();
        }
        
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only accept numbers, decimal separator and backspace.
            // Note: when we pass here the text hasn't been updated yet.
            // TODO: move to a helper.
            
            NumberFormatInfo nfi = Thread.CurrentThread.CurrentCulture.NumberFormat;
            string decimalSeparator = nfi.NumberDecimalSeparator;
            
            char key = e.KeyChar;
            if (((key < '0') || (key > '9')) && (key != decimalSeparator[0]) && (key != '\b'))
            {
                e.Handled = true;
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            // This triggers after the text is changed.
            UpdateTheoreticalPrecision();
        }

        private void cbUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTheoreticalPrecision();

            LengthUnit unit = LengthUnit.Pixels;
            int selectedIndex = cbUnit.SelectedIndex;
            if (selectedIndex >= 0)
                unit = (LengthUnit)selectedIndex;

            lblOffsetUnit.Text = UnitHelper.LengthAbbreviation(unit);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if(tbA.Text.Length == 0 || tbB.Text.Length == 0)
                return;
            
            try
            {
                float width = float.Parse(tbA.Text);
                float height = float.Parse(tbB.Text);
                if (width <= 0 || height <= 0)
                {
                    log.Error(String.Format("The side length cannot be zero or negative. ({0}x{1}).", tbA.Text, tbB.Text));
                    return;
                }

                float offsetX = float.Parse(tbOffsetX.Text);
                float offsetY = float.Parse(tbOffsetY.Text);

                SizeF size = new SizeF(width, height);
                PointF offset = new PointF(offsetX, offsetY);
                
                drawingPlane.UpdateMapping(size);


                calibrationHelper.SetCalibratorFromType(CalibratorType.Plane);
                calibrationHelper.CalibrationByPlane_Initialize(drawingPlane.Id, size, drawingPlane.QuadImage);
                calibrationHelper.LengthUnit = (LengthUnit)cbUnit.SelectedIndex;
                calibrationHelper.SetOffset(offset);
            }
            catch
            {
                // Failed : do nothing.
                log.Error(String.Format("Error while parsing size or offset. size:{0}x{1}, offset:{2}×{3}.", 
                    tbA.Text, tbB.Text, tbOffsetX, tbOffsetY));
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
        }
        
        private void UpdateTheoreticalPrecision()
        {
            lblPrecision.Visible = false;
            
            try
            {
                float worldA = float.Parse(tbA.Text);
                float worldB = float.Parse(tbB.Text);

                if (worldA == 0 || worldB == 0)
                    return;

                // Average between the near and far side.
                float ab = GeometryHelper.GetDistance(quadImage.A, quadImage.B);
                float dc = GeometryHelper.GetDistance(quadImage.D, quadImage.C);
                float pixelA = (ab + dc) / 2;

                float ad = GeometryHelper.GetDistance(quadImage.A, quadImage.D);
                float bc = GeometryHelper.GetDistance(quadImage.B, quadImage.C);
                float pixelB = (ad + bc) / 2;

                LengthUnit unit = LengthUnit.Pixels;
                int selectedIndex = cbUnit.SelectedIndex;
                if (selectedIndex >= 0)
                    unit = (LengthUnit)selectedIndex;

                string pixelSize = UnitHelper.GetPixelSize(worldA, worldB, pixelA, pixelB, unit);

                lblPrecision.Text = string.Format(ScreenManagerLang.dlgCalibratePlane_AveragePixelSize, pixelSize);
                lblPrecision.Visible = true;
            }
            catch
            {

            }
        }

        private void pnlQuadrilateral_Paint(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;
            canvas.CompositingQuality = CompositingQuality.HighQuality;
            canvas.InterpolationMode = InterpolationMode.Bicubic;
            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Edges
            Pen p = new Pen(pnlQuadrilateral.ForeColor);
            canvas.DrawLine(p, quadPanel.A, quadPanel.B);
            canvas.DrawLine(p, quadPanel.B, quadPanel.C);
            canvas.DrawLine(p, quadPanel.C, quadPanel.D);
            canvas.DrawLine(p, quadPanel.D, quadPanel.A);

            // Origin
            canvas.DrawEllipse(p, quadPanel.D.Box(5));

            // Direction vectors
            PointF o = quadPanel.D;
            PointF dirX = quadPanel.C.Subtract(quadPanel.D);
            PointF dirY = quadPanel.A.Subtract(quadPanel.D);
            PointF x = o.Add(dirX.Scale(0.15f));
            PointF y = o.Add(dirY.Scale(0.15f));

            using (Pen pX = new Pen(Color.Tomato))
            using (Pen pY = new Pen(Color.YellowGreen))
            {
                ArrowHelper.Draw(canvas, Pens.Tomato, x, o);
                ArrowHelper.Draw(canvas, Pens.YellowGreen, y, o);
                
                pX.Width = 2.0f;
                pY.Width = 2.0f;
                canvas.DrawLine(pX, o, x);
                canvas.DrawLine(pY, o, y);
            }


            // Indicators to identify lengths or coordinates.
            //if (isDistanceGrid)
            //{
            //    DrawIndicator(canvas, " a ", quadPanel.D.Translate(0, 12));
            //    DrawIndicator(canvas, " b ", quadPanel.C.Translate(0, 12));
            //    p.DashStyle = DashStyle.Dash;
            //    PointF midTop = GeometryHelper.GetMiddlePoint(quadPanel.A, quadPanel.B);
            //    PointF midBot = GeometryHelper.GetMiddlePoint(quadPanel.D, quadPanel.C);
            //    canvas.DrawLine(p, midTop, midBot);
            //}
            //else
            //{
                DrawIndicator(canvas, " a ", GeometryHelper.GetMiddlePoint(quadPanel.A, quadPanel.B));
                DrawIndicator(canvas, " b ", GeometryHelper.GetMiddlePoint(quadPanel.B, quadPanel.C));
                DrawIndicator(canvas, " a ", GeometryHelper.GetMiddlePoint(quadPanel.C, quadPanel.D));
                DrawIndicator(canvas, " b ", GeometryHelper.GetMiddlePoint(quadPanel.D, quadPanel.A));
            //}

            p.Dispose();
        }
        
        /// <summary>
        /// Draw a label at the given point.
        /// </summary>
        private void DrawIndicator(Graphics canvas, string label, PointF point)
        {
            Font tempFont = new Font("Arial", 9, FontStyle.Regular);
            SolidBrush brushBack = new SolidBrush(pnlQuadrilateral.BackColor);
            SolidBrush brushFont = new SolidBrush(pnlQuadrilateral.ForeColor);
            
            SizeF labelSize = canvas.MeasureString(label, tempFont);
            PointF textOrigin = new PointF(point.X - labelSize.Width / 2, point.Y - labelSize.Height / 2);
            canvas.FillRectangle(brushBack, textOrigin.X, textOrigin.Y, labelSize.Width, labelSize.Height);
            canvas.DrawString(label, tempFont, brushFont, textOrigin);

            tempFont.Dispose();
            brushBack.Dispose();
            brushFont.Dispose();
        }
    }
}
