/*
 * Created using SharpDevelop.
 * By: Daniel Cavalcante de Menezes
 * Date: 4/7/2016
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace calcTracao
{
	/// <summary>
	/// Main UI form.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
			
			this.WindowState = FormWindowState.Maximized;
			InitializeComponent();
			this.panel1.MouseWheel += Panel1MouseWheel;
			this.panel1.MouseMove += Panel1MouseMove;
			this.panel1.MouseDown += Panel1MouseDown;
			this.panel1.MouseUp += Panel1MouseUp;
			
			textBox1.Text = "";
		}
		
		double forcaVao;
		double hPoste;
		double hPlano;
		double xPlano;
		double fOnTop;
		double resultX, resultY;
		double result;
		int contVaosInPoste;
		double posteX;
		double posteY;
		double vaoX1;
		double vaoY1;
		double vaoX2;
		double vaoY2;
		
		bool rowsInsertDone = false;
		
		void DataGridView1CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			takenIndex = e.RowIndex;
			
			rowsInsertDone = false;
			dataGridView2.Rows.Clear();
			dataGridView2.Refresh();
			dataGridView3.Rows.Clear();
			dataGridView3.Refresh();
			dataGridView4.Rows.Clear();
			dataGridView4.Refresh();
			
			for (int j=0; j<postes.Count; j++)
			{
				if (dataGridView1.CurrentCell.Value == postes[j].id)
				{
					int index = dataGridView1.CurrentRow.Index;
					DrawOnSmall(index);
					
					for (int i=0; i<postes[index].tracaoPrimario.Count; i++)
					{
						dataGridView2.Rows.Add(postes[index].tracaoPrimario[i].nWires, null,
						                       postes[index].tracaoPrimario[i].forca*postes[index].tracaoPrimario[i].nWires,
						                       (postes[index].tracaoPrimario[i].angulo*(-180)/Math.PI).ToString("0.0"),
						                       postes[index].tracaoPrimario[i].metros.ToString("0.0"), "-",
						                       postes[index].tracaoPrimario[i].distanceFromTop.ToString("0.0"));
					}
					
					for (int i=0; i<postes[index].tracaoSecundario.Count; i++)
					{
						dataGridView3.Rows.Add(postes[index].tracaoSecundario[i].nWires, null,
						                       postes[index].tracaoSecundario[i].forca*postes[index].tracaoSecundario[i].nWires,
						                       (postes[index].tracaoSecundario[i].angulo*(-180)/Math.PI).ToString("0.0"),
						                       postes[index].tracaoSecundario[i].metros.ToString("0.0"), "-");
						
					}
					
					for (int i=0; i<postes[index].tracaoOcupante.Count; i++)
					{
						dataGridView4.Rows.Add(postes[index].tracaoOcupante[i].nWires, postes[index].tracaoOcupante[i].cpfl_arranjo,
						                       postes[index].tracaoOcupante[i].forca*postes[index].tracaoOcupante[i].nWires,
						                       (postes[index].tracaoOcupante[i].angulo*(-180)/Math.PI).ToString("0.0"),
						                       postes[index].tracaoOcupante[i].metros.ToString("0.0"), "-");
					}
					
					//adiciona lista de arranjos no combobox
					DataGridViewComboBoxColumn boxColumn2 = (DataGridViewComboBoxColumn)dataGridView2.Columns[1];
					DataGridViewComboBoxColumn boxColumn3 = (DataGridViewComboBoxColumn)dataGridView3.Columns[1];
					if(boxColumn2.Items.Count == 0){
						foreach(ListaMnemonicos lista in listaMnemonicos)
						{
							boxColumn2.Items.Add(lista.GetArranjo());
							boxColumn3.Items.Add(lista.GetArranjo());
						}
					}
					
					for (int i = 0; i < postes[index].tracaoPrimario.Count; i++)
					{
						string arr = postes[index].tracaoPrimario[i].cpfl_arranjo;
						
						if (listaMnemonicos.ToArray().Any(ind => ind.arranjo == arr))
				        {
				        	dataGridView2.Rows[i].Cells[1].Value = arr;
						}
					}
					
					for (int i = 0; i < postes[index].tracaoSecundario.Count; i++)
					{
						string arr = postes[index].tracaoSecundario[i].cpfl_arranjo;
						
						if (listaMnemonicos.ToArray().Any(ind => ind.arranjo == arr))
				        {
				        	dataGridView3.Rows[i].Cells[1].Value = arr;
				        }
					}
				}
			}
			rowsInsertDone = true;
			
			DrawOnBig();
			Pen pen = new Pen(Color.Orange, 4);
			Point location = new Point((int)((postes[e.RowIndex].x-center.X)/scaleOfBig)+(panel1.Width/2)-10,
			                     (int)((postes[e.RowIndex].y-center.Y)/scaleOfBig)+(panel1.Height/2)-10);
			Size size = new Size(20,20);
			g.DrawEllipse(pen, new Rectangle(new Point(location.X, location.Y), size));
		}
		
		List<Resultado> resultado = new List<Resultado>();
		List<Vao> vaos = new List<Vao>();
		List<Poste> postes = new List<Poste>();
		List<ListaMnemonicos> listaMnemonicos = new List<ListaMnemonicos>();
		List<Transformador> trafos = new List<Transformador>();
		
		Point center;
		int scaleOfBig;
		bool resetScale = true;
		
		void Button1Click(object sender, EventArgs e)
		{
			if(textBox1.Text != "")
			{	
				button2.Enabled = true;
				button3.Enabled = true;
				button4.Enabled = true;
				button5.Enabled = true;
				button6.Enabled = true;
				panel1.Enabled = true;
			}
			
			if(textBox1.Text != "")
			{
				g = panel1.CreateGraphics();
				g.Clear(Color.LightGray);
				dataGridView1.Rows.Clear();
				dataGridView1.Refresh();
				
				//importa dados 
				listaMnemonicos = ImportListaMnemonicos(); //could be used in a button to import new mnemonic/efforts list
				vaos = ImportCSVVao();
				postes = ImportCSVPoste();
				trafos = ImportTrafos();
				UpdateTracao();
				
				for (int i=0; i<postes.Count; i++)
				{
					StrengthCalc(postes[i]);
					
					dataGridView1.Rows.Add(postes[i].id, postes[i].result.ToString("0.0"), postes[i].height+" - "+postes[i].strength);
					
					if(postes[i].result > postes[i].strength+50)
						dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.LightCoral;
					if(postes[i].result > postes[i].strength && postes[i].result <= postes[i].strength+50)
						dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.Yellow;
					if(postes[i].result <= postes[i].strength)
						dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.White;
				}
				
				
				Point max = new Point((int)postes[0].x, (int)postes[0].y);
				Point min = new Point((int)postes[0].x, (int)postes[0].y);
				
				for (int i=0; i<postes.Count; i++)
				{
					if(max.X < postes[i].x)
						max.X = (int)postes[i].x;
					if(min.X > postes[i].x)
						min.X = (int)postes[i].x;
					if(max.Y < postes[i].y)
						max.Y = (int)postes[i].y;
					if(min.Y > postes[i].y)
						min.Y = (int)postes[i].y;
				}
				
				center = new Point((max.X+min.X)/2, (max.Y+min.Y)/2);
				
				double lengthX = (double)Math.Sqrt(Math.Pow((max.X - min.X),2));
				double lengthY = (double)Math.Sqrt(Math.Pow((max.Y - min.Y),2));
				
				
				if(lengthX > lengthY)
					scaleOfBig = (int)((lengthX/panel1.Width)*1.1);
				else 
					scaleOfBig = (int)((lengthY/panel1.Height)*1.1);
				
				DrawOnBig();
			}
		}
		
		void DrawOnSmall(int index)
		{
			g = panel2.CreateGraphics();
			g.Clear(Color.White);
			int scaleOfSmall = 20;
			Point location1, location2, location;
			Pen pen = new Pen(Color.Blue, 2);
			AdjustableArrowCap bigArrow = new AdjustableArrowCap(4, 4);
			pen.CustomEndCap = bigArrow;
			for (int i=0; i<postes[index].tracaoPrimario.Count; i++)
			{
				location1 = new Point((int)((0)/scaleOfSmall)+(panel2.Width/2),
				                      (int)((0)/scaleOfSmall)+(panel2.Height/2));
				
				double xT = Math.Cos(postes[index].tracaoPrimario[i].angulo);
				double yT = Math.Sin(postes[index].tracaoPrimario[i].angulo);
				location2 = new Point((int)((xT*2000)/scaleOfSmall)+(panel2.Width/2),
				                      (int)((yT*2000)/scaleOfSmall)+(panel2.Height/2));
				
				if(postes[index].tracaoPrimario[i].forca != 0) // caso esforço seja zero não desenhar
					g.DrawLine(pen, location1, location2);
			}
			
			pen.Color = Color.MediumSeaGreen;
			for (int i=0; i<postes[index].tracaoSecundario.Count; i++)
			{
				location1 = new Point((int)((0)/scaleOfSmall)+(panel2.Width/2),
				                      (int)((0)/scaleOfSmall)+(panel2.Height/2));
				
				double xT = Math.Cos(postes[index].tracaoSecundario[i].angulo);
				double yT = Math.Sin(postes[index].tracaoSecundario[i].angulo);
				location2 = new Point((int)((xT*1800)/scaleOfSmall)+(panel2.Width/2),
				                      (int)((yT*1800)/scaleOfSmall)+(panel2.Height/2));
				
				if(postes[index].tracaoSecundario[i].forca != 0) // caso esforço seja zero não desenhar
					g.DrawLine(pen, location1, location2);
			}
			
			pen.Color = Color.DarkGray;
			for (int i=0; i<postes[index].tracaoOcupante.Count; i++)
			{
				location1 = new Point((int)((0)/scaleOfSmall)+(panel2.Width/2),
				                      (int)((0)/scaleOfSmall)+(panel2.Height/2));
				
				double xT = Math.Cos(postes[index].tracaoOcupante[i].angulo);
				double yT = Math.Sin(postes[index].tracaoOcupante[i].angulo);
				location2 = new Point((int)((xT*1600)/scaleOfSmall)+(panel2.Width/2),
				                      (int)((yT*1600)/scaleOfSmall)+(panel2.Height/2));
				
				if(postes[index].tracaoOcupante[i].forca != 0) // caso esforço seja zero não desenhar
					g.DrawLine(pen, location1, location2);
			}
			
			pen.Width = 3;
			// draw resultante
			if (postes[index].result > postes[index].strength+50)
				pen.Color = Color.Red;
			if(postes[index].result > postes[index].strength && postes[index].result <= postes[index].strength+50)
				pen.Color = Color.Orange;
			if(postes[index].result <= postes[index].strength)
				pen.Color = Color.Black;
			
			location1 = new Point((int)((0)/scaleOfSmall)+(panel2.Width/2),
			                      (int)((0)/scaleOfSmall)+(panel2.Height/2));
			
			if ((int)postes[index].result > 0) // caso poste não tenha esforços (resultante=0) não desenha resultante
			{
				location2 = new Point((int)(((postes[index].resultX/postes[index].result)*1400)/scaleOfSmall)+(panel2.Width/2),
				                      (int)(((postes[index].resultY/postes[index].result)*1400)/scaleOfSmall)+(panel2.Height/2));
				
				g.DrawLine(pen, location1, location2);
			}
			// draw pole
			pen = new Pen(Color.DarkGray, 2);
			location = new Point((int)((0)/scaleOfSmall)+(panel2.Width/2)-10,
			                     (int)((0)/scaleOfSmall)+(panel2.Height/2)-10);
			Size size = new Size(20,20);
			
			g.DrawEllipse(pen, new Rectangle(new Point(location.X, location.Y), size));
			
			Font drawFont = new Font("Arial", 10, FontStyle.Regular);
			SolidBrush drawBrush = new SolidBrush(Color.Black);
			g.DrawString("Poste ID: " + postes[index].id, drawFont, drawBrush, 10, 10);
			double angulo = Math.Atan2(postes[index].resultY,postes[index].resultX)*(-180)/Math.PI;
			g.DrawString("Resultante: " + postes[index].result.ToString("0.0") + "  |    Ângulo: " + angulo.ToString("0.0")
			             , drawFont, drawBrush, 10, panel2.Height-30);
		
		}
		
		void DrawOnBig()
		{
			
			g = panel1.CreateGraphics();
			g.Clear(Color.White);
			
			Point location1, location2, location;
			Pen pen = new Pen(Color.LightGray, 2);
			pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
			
			for (int i=0; i<vaos.Count; i++)
			{
				// draw wires
				location1 = new Point((int)((vaos[i].x1-center.X)/scaleOfBig)+(panel1.Width/2),
						(int)((vaos[i].y1-center.Y)/scaleOfBig)+(panel1.Height/2));
				location2 = new Point((int)((vaos[i].x2-center.X)/scaleOfBig)+(panel1.Width/2),
						(int)((vaos[i].y2-center.Y)/scaleOfBig)+(panel1.Height/2));
				
				g.DrawLine(pen, location1, location2);
				
			}
			
			pen.Color = Color.Fuchsia;
			
			if (showTrafos == true)
			{
				for (int index=0; index<trafos.Count; index++)
				{
					location = new Point((int)((trafos[index].x-center.X)/scaleOfBig)+(panel1.Width/2)-10,
					                     (int)((trafos[index].y-center.Y)/scaleOfBig)+(panel1.Height/2)-20);
					
					g.DrawLine(pen, new Point(location.X, location.Y), new Point(location.X+20, location.Y));
					g.DrawLine(pen, new Point(location.X+20, location.Y), new Point(location.X+10, location.Y+20));
					g.DrawLine(pen, new Point(location.X+10, location.Y+20), new Point(location.X, location.Y));
					
				}
			}
			
			for (int index=0; index<postes.Count; index++)
			{
				
				// draw poles
				location = new Point((int)((postes[index].x-center.X)/scaleOfBig)+(panel1.Width/2)-10,
				                     (int)((postes[index].y-center.Y)/scaleOfBig)+(panel1.Height/2)-10);
				Size size = new Size(20,20);
				if(postes[index].material == "Eucalipto")
					pen.Color = Color.SaddleBrown;
				else
					pen.Color = Color.LightGray;
				
				g.DrawEllipse(pen, new Rectangle(new Point(location.X, location.Y), size));
				
			}
			
			pen.Color = Color.LightGray;
			// draw strength results
			AdjustableArrowCap bigArrow = new AdjustableArrowCap(4, 4);
			pen.CustomEndCap = bigArrow;
			for (int index=0; index<postes.Count; index++)
			{
				if (postes[index].result > postes[index].strength+50)
					pen.Color = Color.Red;
				if (postes[index].result > postes[index].strength && postes[index].result <= postes[index].strength+50)
					pen.Color = Color.Orange;
				if(postes[index].result <= postes[index].strength)
					pen.Color = Color.Black;
				
				location1 = new Point((int)((postes[index].x-center.X)/scaleOfBig)+(panel1.Width/2),
				                      (int)((postes[index].y-center.Y)/scaleOfBig)+(panel1.Height/2));
				
				if ((int)postes[index].result > 0) // caso poste não tenha esforços (resultante=0) não desenha resultante
				{
					location2 = new Point((int)(((postes[index].x+(postes[index].resultX/postes[index].result)*1000)
					                             -center.X)/scaleOfBig)+(panel1.Width/2),
					                      (int)(((postes[index].y+(postes[index].resultY/postes[index].result)*1000)
					                             -center.Y)/scaleOfBig)+(panel1.Height/2));
					
					g.DrawLine(pen, location1, location2);
				}
			}
			
			// draw pole ids
			if (showIDs == true)
			{
				Font drawFont = new Font("Arial", 8, FontStyle.Regular);
				SolidBrush drawBrush = new SolidBrush(Color.DarkSlateGray);
				for (int index=0; index<postes.Count; index++)
				{
					location = new Point((int)((postes[index].x-center.X)/scaleOfBig)+(panel1.Width/2)-20,
					                     (int)((postes[index].y-center.Y)/scaleOfBig)+(panel1.Height/2)-25);
					g.DrawString(postes[index].id, drawFont, drawBrush, location.X, location.Y);
				}
			}
		}
		
		bool showIDs = true;
		bool showTrafos = true;
		
		List<Vao> ImportCSVVao()
		{
			//importação de secundários
			var reader = new StreamReader(File.OpenRead(textBox1.Text + "/ed_oh_secondary_conductor.txt"),
			                              Encoding.Default);
			
			var list = new List<Vao>();
			reader.ReadLine();
			while (!reader.EndOfStream)
			{
				var line = reader.ReadLine();
				line = line.Replace("|NC|", "|1NC|");
				var values = line.Split('|');
				
				if (values[2] == "Secundário")
				{
					
					// Xs Ys
					vaoX1 = double.Parse(values[5].Split('.')[0]);
					vaoY1 = double.Parse("-"+values[6].Split('.')[0]);
					vaoX2 = double.Parse(values[7].Split('.')[0]);
					vaoY2 = double.Parse("-"+values[8].Split('.')[0]);
					
					// length
					double calcLength = (double)Math.Sqrt(Math.Pow((vaoX1 - vaoX2),2) + Math.Pow((vaoY1 - vaoY2),2));
					calcLength = calcLength/100;
					
					// arranjo
					var cabos = values[1].Split(new Char [] {'(', ')'});
					string tipoCabo = "";
					if (cabos[0] != "")
					{
						double times = double.Parse(cabos[0].Substring(0, 1));
						tipoCabo = cabos[0].Substring(1, 1);
						string arranjoCabo0 = cabos[0].Substring(1);
					
						// nWires						
						if(tipoCabo == "E" || tipoCabo == "P")
							times = 1;
						
						list.Add(new Vao(){id = values[0], cpfl_arranjo = arranjoCabo0,
						         	usage = values[2], anchor_id_1 = values[3], anchor_id_2 = values[4],
						         	x1 = vaoX1, y1 = vaoY1, x2 = vaoX2, y2 = vaoY2, forca = 0, length = calcLength,
						         	nWires = times});
					}
					
					if (cabos.Length > 1 && (tipoCabo != "E" && tipoCabo != "P"))
					{
						int index = 1;
						string arranjoCabo0 = cabos[index].Substring(0);
						
						list.Add(new Vao(){id = values[0], cpfl_arranjo = arranjoCabo0, usage = values[2],
					         	anchor_id_1 = values[3], anchor_id_2 = values[4], x1 = vaoX1, y1 = vaoY1,
					         	x2 = vaoX2, y2 = vaoY2, forca = 0, length = calcLength, nWires = 1});
						
					}
					
				}
			}
            
            //importação de primários
            reader = new StreamReader(File.OpenRead(textBox1.Text + "/ed_oh_primary_conductor.txt"),
				Encoding.Default);
            
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split('|');
 				
                // Xs Ys
                vaoX1 = double.Parse(values[4].Split('.')[0]);
				vaoY1 = double.Parse("-"+values[5].Split('.')[0]);
				vaoX2 = double.Parse(values[6].Split('.')[0]);
				vaoY2 = double.Parse("-"+values[7].Split('.')[0]);
                
				// length
                double calcLength = (double)Math.Sqrt(Math.Pow((vaoX1 - vaoX2),2) + Math.Pow((vaoY1 - vaoY2),2));
				calcLength = calcLength/100;
				
				// arranjo
				string tipoCabo = "";
				var cabos = values[1].Split(new Char [] {'(', ')'});
				if (cabos[0] != ""){
					double times = double.Parse(cabos[0].Substring(0, 1));
					tipoCabo = cabos[0].Substring(1, 1);
					string arranjoCabo0 = cabos[0].Substring(1);
					
					if(tipoCabo == "E" || tipoCabo == "P")
						times = 1;
					
					list.Add(new Vao(){id = values[0], cpfl_arranjo = arranjoCabo0,
					         	usage="Primário", anchor_id_1 = values[2], anchor_id_2 = values[3], x1 = vaoX1,
					         	y1 = vaoY1, x2 = vaoX2, y2 = vaoY2, forca = 0, length = calcLength, nWires = times});
					
				}
				if (cabos.Length > 1 && (tipoCabo != "E" && tipoCabo != "P")){
					int index = 1;
					string arranjoCabo0 = cabos[index].Substring(1);
					
					list.Add(new Vao(){id = values[0], cpfl_arranjo = arranjoCabo0, usage="Primário",
					         	anchor_id_1 = values[2], anchor_id_2 = values[3], x1 = vaoX1, y1 = vaoY1,
					         	x2 = vaoX2, y2 = vaoY2, forca = 0, length = calcLength, nWires = 1});
					
				}
                
            }
            
            return list;
		}
		
		// calcula o esforço dos vão em cada poste e a resultante
		void StrengthCalc(Poste poste)
		{
			double resultante = 0;
			double calcLength = 0;
			double hPoste = poste.height;
			resultX = 0;
			resultY = 0;
			
			for (int j=0; j<poste.tracaoPrimario.Count; j++){
				
				fOnTop = 0;
				double e = (hPoste/10)+0.6;
				xPlano = hPoste-e-0.2;
				double percent = ((xPlano+0.2-poste.tracaoPrimario[j].distanceFromTop)/(xPlano)); // dividido por 10 porque força está em daN
				fOnTop = (poste.tracaoPrimario[j].nWires*poste.tracaoPrimario[j].forca)*percent;
				
				resultX += fOnTop*Math.Cos(poste.tracaoPrimario[j].angulo);
				resultY += fOnTop*Math.Sin(poste.tracaoPrimario[j].angulo);
			}
			
			for (int j=0; j<poste.tracaoSecundario.Count; j++){
				
				fOnTop = 0;
				
				double e = (hPoste/10)+0.6;
				xPlano = hPoste-e-0.2;
				double percent = (7.2/(xPlano)); // dividido por 10 porque força está em daN
				// talvez alterar esse parte ([altura do plano]/(xPlano))
				
				fOnTop = (poste.tracaoSecundario[j].nWires*poste.tracaoSecundario[j].forca)*percent;
				
				resultX += fOnTop*Math.Cos(poste.tracaoSecundario[j].angulo);
				resultY += fOnTop*Math.Sin(poste.tracaoSecundario[j].angulo);
			}
			
			for (int j=0; j<poste.tracaoOcupante.Count; j++){
				
				fOnTop = 0;
				
				double e = (hPoste/10)+0.6;
				xPlano = hPoste-e-0.2;
				double percent = (5.5/(xPlano)); // dividido por 10 porque força está em daN
				// talvez alterar esse parte ([altura do plano]/(xPlano))
				
				fOnTop = (poste.tracaoOcupante[j].nWires*poste.tracaoOcupante[j].forca)*percent;
				
				resultX += fOnTop*Math.Cos(poste.tracaoOcupante[j].angulo);
				resultY += fOnTop*Math.Sin(poste.tracaoOcupante[j].angulo);
			}
			
			resultante = (double)Math.Sqrt(Math.Pow((resultX),2) + Math.Pow((resultY),2));
			
			poste.resultX = resultX;
			poste.resultY = resultY;
			poste.result = resultante;
			
		}
		
		List<Poste> ImportCSVPoste()
		{
			var reader = new StreamReader(File.OpenRead(textBox1.Text + "/ed_pole.txt"),
			                             Encoding.Default);
			
            var list = new List<Poste>();
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
            	
                var line = reader.ReadLine();
                var values = line.Split('|');
 				
                vaoX1 = double.Parse(values[7].Split('.')[0]);
            	vaoY1 = double.Parse("-"+values[8].Split('.')[0]);
                
                //=-=-=-=- pode-se usar os postes como "proposto remover" e "proposto" futuramente
                if(values[4] == "Próprio" && values[9] == "Existente")
                {
	                list.Add(new Poste(){id=values[0], height=double.Parse(values[1]), cpfl_formato=values[2],
	                         	material=values[3], owner_type=values[4], owner_name=values[5],
	                         	strength=double.Parse(values[6]), x=vaoX1, y=vaoY1, status=values[9]});
                }
            }
            return list;
		}
		
		List<Transformador> ImportTrafos()
		{
			var reader = new StreamReader(File.OpenRead(textBox1.Text + "/ed_oh_transformer.txt"),
			                             Encoding.Default);
            var list = new List<Transformador>();
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
            	
                var line = reader.ReadLine();
                var values = line.Split('|');
 				
                double TX = double.Parse(values[0].Split('.')[0]);
            	double TY = double.Parse("-"+values[1].Split('.')[0]);
                
                list.Add(new Transformador(){x = TX, y = TY});
            }
            return list;
		}
		
		// corrige a direção do vão que está conectado a algum poste mas não está no centro do poste
		void UpdateTracao()
		{
			
			foreach(Vao vao in vaos)
			{
				vao.x1calc = vao.x1;
				vao.y1calc = vao.y1;
				vao.x2calc = vao.x2;
				vao.y2calc = vao.y2;
			}
			
			foreach(Poste poste in postes)
			{
				foreach(Vao vao in vaos)
				{
					if (poste.id == vao.anchor_id_1 || poste.id == vao.anchor_id_2)
					{
						double distance1 = (double)Math.Sqrt(Math.Pow((vao.x1 - poste.x),2) + Math.Pow((vao.y1 - poste.y),2));
						double distance2 = (double)Math.Sqrt(Math.Pow((vao.x2 - poste.x),2) + Math.Pow((vao.y2 - poste.y),2));
						double x = 0;
						double y = 0;
						if (distance1 < distance2)
						{
							vao.x1calc = poste.x;
							vao.y1calc = poste.y;
						}
						else
						{
							vao.x2calc = poste.x;
							vao.y2calc = poste.y;
						}
					}
				}
			}
			
			foreach(Poste poste in postes)
			{
				// a partir da lista de vãos importada, procura os vãos que estão conectados em cada poste,
				// traduz para vetor e ângulo e adiciona a lista de trações do poste
                for(int j = 0; j < vaos.Count; j++)
                {
                	if (poste.id == vaos[j].anchor_id_1 || poste.id == vaos[j].anchor_id_2)
                	{
                		double f = FindStregth(vaos[j].cpfl_arranjo, vaos[j].length);
                		double a = FindAngle(poste, vaos[j]);
                		
                		if(vaos[j].usage == "Primário" && vaos[j].length > 3)
                		{
                			poste.tracaoPrimario.Add(new Tracao(){cpfl_arranjo=vaos[j].cpfl_arranjo, forca=f,
                			                 	nWires=vaos[j].nWires, usage=vaos[j].usage, angulo=a,
                			                 	metros=vaos[j].length, distanceFromTop=0.2d});
                		
                			// backup caso usuário deseje voltar a configuração inicial (ou antes de gravar);
                			poste.tracaoPrimarioTemp.Add(new Tracao(){cpfl_arranjo=vaos[j].cpfl_arranjo, forca=f,
                			                 	nWires=vaos[j].nWires, usage=vaos[j].usage, angulo=a,
                			                    metros=vaos[j].length, distanceFromTop=0.2d});
                		}
                		
                		if(vaos[j].usage == "Secundário" && vaos[j].length > 3)
                		{
                			poste.tracaoSecundario.Add(new Tracao(){cpfl_arranjo=vaos[j].cpfl_arranjo, forca=f,
                			                 	nWires=vaos[j].nWires, usage=vaos[j].usage, angulo=a,
                			                    metros=vaos[j].length});
                		
                			// backup caso usuário deseje voltar a configuração inicial (ou antes de gravar);
                			poste.tracaoSecundarioTemp.Add(new Tracao(){cpfl_arranjo=vaos[j].cpfl_arranjo, forca=f,
                			                 	nWires=vaos[j].nWires, usage=vaos[j].usage, angulo=a,
                			                 	metros=vaos[j].length});
                		}
                	}
                }
                
			}
		}
		
		double FindStregth(string arranjo, double calcLength)
		{
			
    		for (int i=0; i<listaMnemonicos.Count; i++)
			{
				if(listaMnemonicos[i].arranjo == arranjo)
				{
					if(calcLength >= 0 && calcLength <= 35)
					{
						forcaVao = double.Parse(listaMnemonicos[i].vao35);
					}
					if(calcLength > 35 && calcLength <= 46) //margem de segurança
					{
						forcaVao = double.Parse(listaMnemonicos[i].vao40);
					}
					if(calcLength > 46 && calcLength <= 80) //margem de segurança
					{
						forcaVao = double.Parse(listaMnemonicos[i].vao80);
					}
					if(calcLength > 80)
					{
						forcaVao = double.Parse(listaMnemonicos[i].vao120);
					}
				}
			}
    		
    		return forcaVao;
		}
		
		double FindAngle(Poste poste, Vao vao)
		{
			double angle = 0;
			double distance1 = (double)Math.Sqrt(Math.Pow((vao.x1 - poste.x),2) + Math.Pow((vao.y1 - poste.y),2));
			double distance2 = (double)Math.Sqrt(Math.Pow((vao.x2 - poste.x),2) + Math.Pow((vao.y2 - poste.y),2));
			double x = 0;
			double y = 0;
			if (distance1 > distance2)
			{
				x = vao.x1calc;
				y = vao.y1calc;
			}
			else
			{
				x = vao.x2calc;
				y = vao.y2calc;
			}
			
			angle = Math.Atan2(y-poste.y, x-poste.x);
			
			return angle;
		}
		
		List<ListaMnemonicos> ImportListaMnemonicos()
		{
			
			//Debug.Print(System.IO.Directory.GetCurrentDirectory());
			string projectDir = System.IO.Directory.GetCurrentDirectory();
			var reader = new StreamReader(File.OpenRead(projectDir + "/MnemonicoDeCabos.txt"),
			                             Encoding.Default);
            var list = new List<ListaMnemonicos>();
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split('	');
 
                list.Add(new ListaMnemonicos(){arranjo=values[0], vao35=values[1], vao40=values[2],
                         	vao80=values[3], vao120=values[4]});
            }
            
//            list.Add(new ListaMnemonicos(){arranjo="E35",vao35="187",vao40="187",vao80="448",vao120="448"});
//			list.Add(new ListaMnemonicos(){arranjo="E70",vao35="373",vao40="373",vao80="549",vao120="549"});
//			list.Add(new ListaMnemonicos(){arranjo="E70-1",vao35="438",vao40="438",vao80="601",vao120="601"});
//			list.Add(new ListaMnemonicos(){arranjo="E95",vao35="632",vao40="632",vao80="838",vao120="838"});	
//			list.Add(new ListaMnemonicos(){arranjo="E150",vao35="632",vao40="632",vao80="838",vao120="838"}); 	
//			list.Add(new ListaMnemonicos(){arranjo="E185",vao35="632",vao40="632",vao80="838",vao120="838"});
//			list.Add(new ListaMnemonicos(){arranjo="E185-1",vao35="659",vao40="659",vao80="837",vao120="837"});
//			list.Add(new ListaMnemonicos(){arranjo="P16",vao35="70",vao40="70",vao80="70",vao120="70"});
//			list.Add(new ListaMnemonicos(){arranjo="P25",vao35="96",vao40="96",vao80="96",vao120="96"});
//			list.Add(new ListaMnemonicos(){arranjo="P35",vao35="126",vao40="126",vao80="126",vao120="126"});
//			list.Add(new ListaMnemonicos(){arranjo="P50",vao35="156",vao40="156",vao80="156",vao120="156"});
//			list.Add(new ListaMnemonicos(){arranjo="P70",vao35="216",vao40="216",vao80="216",vao120="216"});
//			list.Add(new ListaMnemonicos(){arranjo="P120",vao35="336",vao40="336",vao80="336",vao120="336"});
//			list.Add(new ListaMnemonicos(){arranjo="P240-1",vao35="723",vao40="723",vao80="723",vao120="723"});
//			list.Add(new ListaMnemonicos(){arranjo="P240-2",vao35="825",vao40="825",vao80="825",vao120="825"});
//			list.Add(new ListaMnemonicos(){arranjo="P240-3",vao35="966",vao40="966",vao80="966",vao120="966"});
//			list.Add(new ListaMnemonicos(){arranjo="A16",vao35="60",vao40="60",vao80="133",vao120="133"});
//			list.Add(new ListaMnemonicos(){arranjo="A25",vao35="60",vao40="60",vao80="133",vao120="133"});
//			list.Add(new ListaMnemonicos(){arranjo="A50",vao35="60",vao40="60",vao80="133",vao120="133"});
//			list.Add(new ListaMnemonicos(){arranjo="A04",vao35="60",vao40="60",vao80="133",vao120="133"});
//			list.Add(new ListaMnemonicos(){arranjo="A02",vao35="60",vao40="60",vao80="133",vao120="133"});
//			list.Add(new ListaMnemonicos(){arranjo="A1/0",vao35="96",vao40="96",vao80="186",vao120="186"});
//			list.Add(new ListaMnemonicos(){arranjo="A2/0",vao35="121",vao40="121",vao80="220",vao120="220"});
//			list.Add(new ListaMnemonicos(){arranjo="A3/0",vao35="194",vao40="194",vao80="315",vao120="315"});
//			list.Add(new ListaMnemonicos(){arranjo="A4/0",vao35="194",vao40="194",vao80="315",vao120="315"});
//			list.Add(new ListaMnemonicos(){arranjo="A336",vao35="308",vao40="308",vao80="483",vao120="706"});
//			list.Add(new ListaMnemonicos(){arranjo="A477",vao35="435",vao40="435",vao80="683",vao120="998"});
//			list.Add(new ListaMnemonicos(){arranjo="S04",vao35="63",vao40="63",vao80="135",vao120="175"});
//			list.Add(new ListaMnemonicos(){arranjo="S02",vao35="100",vao40="100",vao80="191",vao120="247"});
//			list.Add(new ListaMnemonicos(){arranjo="S1/0",vao35="160",vao40="160",vao80="275",vao120="335"});
//			list.Add(new ListaMnemonicos(){arranjo="S2/0",vao35="201",vao40="201",vao80="346",vao120="428"});
//			list.Add(new ListaMnemonicos(){arranjo="S4/0",vao35="320",vao40="320",vao80="551",vao120="652"});
//			list.Add(new ListaMnemonicos(){arranjo="S336",vao35="876",vao40="876",vao80="876",vao120="1035"});
//			list.Add(new ListaMnemonicos(){arranjo="C06",vao35="56",vao40="56",vao80="56",vao120="56"});
//			list.Add(new ListaMnemonicos(){arranjo="C04",vao35="141",vao40="141",vao80="141",vao120="141"});	
//			list.Add(new ListaMnemonicos(){arranjo="C02",vao35="141",vao40="141",vao80="141",vao120="141"});
//			list.Add(new ListaMnemonicos(){arranjo="C1/0",vao35="208",vao40="208",vao80="208",vao120="208"});
//			list.Add(new ListaMnemonicos(){arranjo="NC",vao35="0",vao40="0",vao80="0",vao120="0"});
            
            return list;
		}
		
		double FtoTop(double hPoste_, double forcaVao_)
		{
			double e = (hPoste_ * 0.1f) + 0.6d;
			double hUtil = hPoste_ - e - 0.2d;
			double res = forcaVao_ * 7.2f/hUtil;
			return res;
		}
		
		Point posteZero;
		Graphics g;
		Graphics g2;
		double divisor = 1d;
		int delta = 1;
		
		void Panel1MouseWheel(object sender, MouseEventArgs e)
		{
			
			scaleOfBig -= e.Delta/25;
			
			if(scaleOfBig <= 0)
				scaleOfBig = 1;
			
			dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
			DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
			
			panel1.Focus();
		}
		
		Point mousePos;
		Point mouseDistance = new Point(0, 0);
		bool enableMove = false;
		string tipoObj = "";
		int takenIndex = 0;
		
		void Panel1MouseDown(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
			{
				takenIndex = 0;
				double distTaken = 1000000000000;

				//check click on pole
				for(int i = 0; i < postes.Count; i++)
				{
					if(postes[i].status != "Proposto Remover")
					{
						double distTakenTemp =(double)Math.Sqrt(
						    Math.Pow(
								e.Location.X -(((postes[i].x-center.X)/scaleOfBig)+(panel1.Width/2)-10),2
							) +
							Math.Pow(
								e.Location.Y -(((postes[i].y-center.Y)/scaleOfBig)+(panel1.Height/2)-10),2
							)
						);
						
						if(distTakenTemp < distTaken)
						{
							distTaken = distTakenTemp;
							takenIndex = i;
						}
					}
				}
				
				dataGridView1.CurrentCell = dataGridView1[0,takenIndex];
				DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
				
			}
			
			if(e.Button == MouseButtons.Right)
			{
				enableMove = true;
				mousePos = new Point(e.Location.X, e.Location.Y);
			}
			
			panel1.Focus();
		}
		
		void Panel1MouseUp(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				enableMove = false;
				mouseDistance = new Point(mousePos.X - e.Location.X, mousePos.Y - e.Location.Y);
				mouseDistance = new Point(mouseDistance.X*(scaleOfBig/2), mouseDistance.Y*(scaleOfBig/2));
				center = new Point(center.X + mouseDistance.X, center.Y + mouseDistance.Y);
				
				dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
				DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
			}
		}
		
		void Panel1MouseMove(object sender, MouseEventArgs e)
		{
			
		}
		
		private void dataGridView2_KeyPress(object sender, DataGridViewCellEventArgs e)
        {
			
        }
		
		void Panel1Paint(object sender, PaintEventArgs e)
		{
			
		}
		
		void DataGridView2CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			var senderGrid = (DataGridView)sender;
			// clique no botão da tabela
			if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
		        e.RowIndex >= 0)
		    {
				Poste pol = postes[dataGridView1.CurrentRow.Index];
				if (dataGridView2.Rows[e.RowIndex].Cells[5].Value.ToString() == "-")
				{
					dataGridView2.Rows.RemoveAt(e.RowIndex);
					pol.tracaoPrimario.RemoveAt(e.RowIndex);
				}
				
				StrengthCalc(pol);
				
				dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
				DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
		    }
		}
		
		void dataGridView2MouseEnter(object sender, DataGridViewCellEventArgs e)
        {
			if (e.RowIndex >= 0 && e.ColumnIndex == 1)
		    {
				dataGridView2.CurrentCell = dataGridView2[1, e.RowIndex];
		    }
			
        }
		
		void DataGridView3CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			var senderGrid = (DataGridView)sender;
			// clique no botão da tabela
		    if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
		        e.RowIndex >= 0)
		    {
				Poste pol = postes[dataGridView1.CurrentRow.Index];
				if (dataGridView3.Rows[e.RowIndex].Cells[5].Value.ToString() == "-")
				{
					dataGridView3.Rows.RemoveAt(e.RowIndex);
					pol.tracaoSecundario.RemoveAt(e.RowIndex);
				}
				
				StrengthCalc(pol);
				
				dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
				DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
		    }
		}
		
		void DataGridView4CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			var senderGrid = (DataGridView)sender;
			// clique no botão da tabela
		    if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
		        e.RowIndex >= 0)
		    {
				Poste pol = postes[dataGridView1.CurrentRow.Index];
				if (dataGridView4.Rows[e.RowIndex].Cells[5].Value.ToString() == "-")
				{
					dataGridView4.Rows.RemoveAt(e.RowIndex);
					pol.tracaoOcupante.RemoveAt(e.RowIndex);
				}
				
				StrengthCalc(pol);
				
				dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
				DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
		    }
		}
		
		void dataGridView3MouseEnter(object sender, DataGridViewCellEventArgs e)
        {
			if (e.RowIndex >= 0 && e.ColumnIndex == 1)
		    {
				dataGridView3.CurrentCell = dataGridView3[1, e.RowIndex];
		    }
        }
		
		void DataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && rowsInsertDone == true)
		    {
				Poste pol = postes[dataGridView1.CurrentRow.Index];
				
				DataGridViewRow r = dataGridView2.Rows[e.RowIndex];
				
				
				// to implement: allow only numerical inputs
				try
				{
					r.Cells[e.ColumnIndex].Style.BackColor = Color.White;
					if(r.Cells[0] != null)
						pol.tracaoPrimario[r.Index].nWires = double.Parse(r.Cells[0].Value.ToString());
					else
						pol.tracaoPrimario[r.Index].nWires = 1;
					
					if(r.Cells[1].Value != null)
						pol.tracaoPrimario[r.Index].cpfl_arranjo = r.Cells[1].Value.ToString();
					else
						pol.tracaoPrimario[r.Index].cpfl_arranjo = "";
					
					if(r.Cells[2] != null)
						pol.tracaoPrimario[r.Index].forca = double.Parse(r.Cells[2].Value.ToString())/double.Parse(r.Cells[0].Value.ToString());
					else
						pol.tracaoPrimario[r.Index].forca = 0;
					
					if(r.Cells[3] != null && r.Cells[3].Value.ToString() != "-")
						pol.tracaoPrimario[r.Index].angulo = double.Parse(r.Cells[3].Value.ToString())*Math.PI/(-180);
					else
						pol.tracaoPrimario[r.Index].angulo = 0;
					
					if(r.Cells[4] != null && r.Cells[4].Value.ToString() != "-")
						pol.tracaoPrimario[r.Index].metros = double.Parse(r.Cells[4].Value.ToString());
					else
						pol.tracaoPrimario[r.Index].metros = 0;
					
					if(r.Cells[6] != null)
						pol.tracaoPrimario[r.Index].distanceFromTop = double.Parse(r.Cells[6].Value.ToString());
					else
						pol.tracaoPrimario[r.Index].distanceFromTop = 0.2;
				
				}
				catch(Exception)
				{
					r.Cells[e.ColumnIndex].Style.BackColor = Color.Yellow;
					MessageBox.Show("Valor inválido \n\nEm caso de dúvidas, consultar a documentação");
					goto done;
				}
				
				StrengthCalc(pol);
				DrawOnSmall(dataGridView1.CurrentRow.Index);
			
				// se escolher um arranjo de condutor ou mudar o número de cabos
				if (e.ColumnIndex == 1 || e.ColumnIndex == 0 || e.ColumnIndex == 4)
				{
					foreach(ListaMnemonicos lista in listaMnemonicos)
					{
						if(dataGridView2.CurrentRow.Cells[1].Value != null)
						{
							if(lista.arranjo == dataGridView2.CurrentRow.Cells[1].Value.ToString())
							{
								double f = FindStregth(dataGridView2.CurrentRow.Cells[1].Value.ToString(),
								                       double.Parse(dataGridView2.CurrentRow.Cells[4].Value.ToString()));
								dataGridView2.Rows[e.RowIndex].Cells[2].Value = (f * 
									double.Parse(dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString())).ToString();
								dataGridView2.Refresh();
							}
						}
					}
				}
			done:;
			}
		}
		
		void DataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && rowsInsertDone == true)
		    {
				Poste pol = postes[dataGridView1.CurrentRow.Index];
				
				DataGridViewRow r = dataGridView3.Rows[e.RowIndex];
				
				// to implement: allow only numerical inputs 
				try
				{
					r.Cells[e.ColumnIndex].Style.BackColor = Color.White;
					if(r.Cells[0] != null)
						pol.tracaoSecundario[r.Index].nWires = double.Parse(r.Cells[0].Value.ToString());
					else
						pol.tracaoSecundario[r.Index].nWires = 1;
					
					if(r.Cells[1].Value != null)
						pol.tracaoSecundario[r.Index].cpfl_arranjo = r.Cells[1].Value.ToString();
					else
						pol.tracaoSecundario[r.Index].cpfl_arranjo = "";
					
					if(r.Cells[2] != null)
						pol.tracaoSecundario[r.Index].forca = double.Parse(r.Cells[2].Value.ToString())/double.Parse(r.Cells[0].Value.ToString());
					else
						pol.tracaoSecundario[r.Index].forca = 0;
					
					if(r.Cells[3] != null && r.Cells[3].Value.ToString() != "-")
						pol.tracaoSecundario[r.Index].angulo = double.Parse(r.Cells[3].Value.ToString())*Math.PI/(-180);
					else
						pol.tracaoSecundario[r.Index].angulo = 0;
					
					if(r.Cells[4] != null && r.Cells[4].Value.ToString() != "-")
						pol.tracaoSecundario[r.Index].metros = double.Parse(r.Cells[4].Value.ToString());
					else
						pol.tracaoSecundario[r.Index].metros = 0;
				}
				catch(Exception)
				{
					r.Cells[e.ColumnIndex].Style.BackColor = Color.Yellow;
					MessageBox.Show("Valor inválido \n\nEm caso de dúvidas, consultar a documentação");
					goto done;
				}
				
				StrengthCalc(pol);
				DrawOnSmall(dataGridView1.CurrentRow.Index);
			
				// se escolher um arranjo de condutor ou mudar o número de cabos
				if (e.ColumnIndex == 1 || e.ColumnIndex == 0 || e.ColumnIndex == 4)
				{
					foreach(ListaMnemonicos lista in listaMnemonicos)
					{
						if(dataGridView3.CurrentRow.Cells[1].Value != null)
						{
							if(lista.arranjo == dataGridView3.CurrentRow.Cells[1].Value.ToString())
							{
								double f = FindStregth(dataGridView3.CurrentRow.Cells[1].Value.ToString(),
								                       double.Parse(dataGridView3.CurrentRow.Cells[4].Value.ToString()));
								dataGridView3.Rows[e.RowIndex].Cells[2].Value = (f * 
									double.Parse(dataGridView3.Rows[e.RowIndex].Cells[0].Value.ToString())).ToString();
								dataGridView3.Refresh();
							}
						}
					}
				}
			done:;
			}
		}
		
		void DataGridView4_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && rowsInsertDone == true)
		    {
				Poste pol = postes[dataGridView1.CurrentRow.Index];
				
				DataGridViewRow r = dataGridView4.Rows[e.RowIndex];
				
				//to implement: allow only numerical inputs
				try
				{
					r.Cells[e.ColumnIndex].Style.BackColor = Color.White;
					if(r.Cells[0] != null)
						pol.tracaoOcupante[r.Index].nWires = double.Parse(r.Cells[0].Value.ToString());
					else
						pol.tracaoOcupante[r.Index].nWires = 1;
					
					if(r.Cells[1].Value != null)
						pol.tracaoOcupante[r.Index].cpfl_arranjo = r.Cells[1].Value.ToString();
					else
						pol.tracaoOcupante[r.Index].cpfl_arranjo = "";
					
					if(r.Cells[2] != null)
						pol.tracaoOcupante[r.Index].forca = double.Parse(r.Cells[2].Value.ToString())/double.Parse(r.Cells[0].Value.ToString());
					else
						pol.tracaoOcupante[r.Index].forca = 0;
					
					if(r.Cells[3] != null && r.Cells[3].Value.ToString() != "-")
						pol.tracaoOcupante[r.Index].angulo = double.Parse(r.Cells[3].Value.ToString())*Math.PI/(-180);
					else
						pol.tracaoOcupante[r.Index].angulo = 0;
					
					if(r.Cells[4] != null && r.Cells[4].Value.ToString() != "-")
						pol.tracaoOcupante[r.Index].metros = double.Parse(r.Cells[4].Value.ToString());
					else
						pol.tracaoOcupante[r.Index].metros = 0;
				}
				catch(Exception ex)
				{
					r.Cells[e.ColumnIndex].Style.BackColor = Color.Yellow;
					MessageBox.Show("Valor inválido \n\nEm caso de dúvidas, consultar a documentação");
					goto done;
				}
				
				StrengthCalc(pol);
				DrawOnSmall(dataGridView1.CurrentRow.Index);
			
			done:;
			}
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			g.Clear(Color.LightGray);
			for (int i=0; i<postes.Count; i++)
			{
				StrengthCalc(postes[i]);
			}
			
			// update datagrid 1 values
			dataGridView1.Rows.Clear();
			dataGridView1.Refresh();
			for (int i=0; i<postes.Count; i++)
			{
				StrengthCalc(postes[i]);
				
				dataGridView1.Rows.Add(postes[i].id, (int)postes[i].result, postes[i].height+" - "+postes[i].strength);
				
				if(postes[i].result > postes[i].strength+50)
					dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.LightCoral;
				if(postes[i].result > postes[i].strength && postes[i].result <= postes[i].strength+50)
					dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.Yellow;
				if(postes[i].result <= postes[i].strength)
					dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.White;
			}
			//--
			
			dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
			DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
		}
		
		void Button4Click(object sender, EventArgs e)
		{	
			MessageBox.Show ("Em desenvolvimento. \n\nPara ajuda, entre em contato com o suporte\n\n" +
			                 "ou consulte a documentação", "Ajuda",
			MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		
		void Button5Click(object sender, EventArgs e)
		{
			dataGridView2.Rows.Insert(dataGridView2.Rows.Count, 1, null, null, null, null, "-");
			
			Poste pol = postes[dataGridView1.CurrentRow.Index];
			pol.tracaoPrimario.Add(new Tracao(){cpfl_arranjo="", forca=0, nWires=1, usage="Primário", angulo=0, distanceFromTop=0.2});
			
			dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
			DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
		}
		
		void Button6Click(object sender, EventArgs e)
		{
			dataGridView3.Rows.Insert(dataGridView3.Rows.Count, 1, null, null, null, null, "-");
			
			Poste pol = postes[dataGridView1.CurrentRow.Index];
			pol.tracaoSecundario.Add(new Tracao(){cpfl_arranjo="", forca=0, nWires=1, usage="Secundário", angulo=0});
			
			dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
			DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
		}
		
		void Button3Click(object sender, EventArgs e)
		{
			dataGridView4.Rows.Insert(dataGridView4.Rows.Count, 1, null, null, null, null, "-");
			
			Poste pol = postes[dataGridView1.CurrentRow.Index];
			pol.tracaoOcupante.Add(new Tracao(){cpfl_arranjo="", forca=0, nWires=1, usage="Ocupante", angulo=0});
			
			dataGridView1.CurrentCell = dataGridView1[0, takenIndex];
			DataGridView1CellContentClick(dataGridView1, new DataGridViewCellEventArgs(0, takenIndex));
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
			
		}
		
		void Button7Click(object sender, EventArgs e)
		{
			folderBrowserDialog1.ShowDialog();
			textBox1.Text = folderBrowserDialog1.SelectedPath;
		}
		
		void CheckBox1CheckedChanged(object sender, EventArgs e)
		{
			showIDs = !showIDs;
		}
		
		void CheckBox2CheckedChanged(object sender, EventArgs e)
		{
			showTrafos = !showTrafos;
		}
		
		void Button8Click(object sender, EventArgs e)
		{
			
			Help help_ = new Help();
			help_.Show();
		}
		
		void Panel2Paint(object sender, PaintEventArgs e)
		{
			
		}
		
		void SplitContainer2SplitterMoved(object sender, SplitterEventArgs e)
		{
			
		}
	}
}
