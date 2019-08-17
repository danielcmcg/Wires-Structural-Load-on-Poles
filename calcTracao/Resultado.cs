/*
 * Created by SharpDevelop.
 * User: Daniel
 * Date: 7/4/2016
 * Time: 23:53
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace calcTracao
{
	/// <summary>
	/// Description of Resultado.
	/// </summary>
	public class Resultado
	{
		public string id, resultante, fPosteAtual, nVaos;
		public double x, y, poleX, poleY;
		
		public Resultado()
		{
		}
		
		double divisor = 1f;
		public Pen pen = new Pen(Color.Blue, 4);
		
		public void Plot(Graphics g, Point posteZero, int delta, Point mouseDistance)
		{
			int resultadoX = (int)x*5;
			int resultadoY = (int)y*5;
			int posteIntX = (int)Convert.ToInt64(poleX*divisor);
			int posteIntY = (int)Convert.ToInt64(poleY*divisor);
			
			AdjustableArrowCap bigArrow = new AdjustableArrowCap(4, 4);
			pen.CustomEndCap = bigArrow;
			
			location1 = new Point(((posteIntX-posteZero.X)/delta)-mouseDistance.X,
			           			((posteIntY-posteZero.Y)/delta)-mouseDistance.Y);
			location2 = new Point((((posteIntX-posteZero.X)+(resultadoX))/delta)-mouseDistance.X,
			           			(((posteIntY-posteZero.Y)+(resultadoY))/delta)-mouseDistance.Y);
			
			g.DrawLine(pen, location1, location2);
			
			Font drawFont = new Font("Arial", 8, FontStyle.Bold);
    		SolidBrush drawBrush = new SolidBrush(Color.Black);
    		g.DrawString(Math.Round(double.Parse(resultante)).ToString(), drawFont, drawBrush, location1);
		}
		
		public Point location1, location2;
		
	}
}
