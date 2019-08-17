/*
 * Created using SharpDevelop.
 * By: Daniel Cavalcante de Menezes
 * Date: 7/4/2016
 * Time: 23:53
 */
using System;
using System.Drawing;
using System.Collections.Generic;

namespace calcTracao
{
	/// <summary>
	/// Description of Poste.
	/// </summary>
	public class Poste
	{
		
		public string id, cpfl_formato, material, owner_type, owner_name, status;
		public double x, y, height, strength, result, resultX, resultY;
		public List<Vao> vaos = new List<Vao>();
		public List<Tracao> tracaoPrimario = new List<Tracao>();
		public List<Tracao> tracaoSecundario = new List<Tracao>();
		public List<Tracao> tracaoOcupante = new List<Tracao>();
		
		// usado nos cálculos da area pequena e antes de gravar
		public List<Tracao> tracaoPrimarioTemp = new List<Tracao>();
		public List<Tracao> tracaoSecundarioTemp = new List<Tracao>();
		
		public Poste()
		{
		}
	}
}
