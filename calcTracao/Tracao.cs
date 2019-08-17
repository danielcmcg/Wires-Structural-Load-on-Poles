/*
 * Created using SharpDevelop.
 * By: Daniel Cavalcante de Menezes
 * Date: 10/19/2016
 * Time: 15:23
 */
using System;

namespace calcTracao
{
	/// <summary>
	/// Description of Tracao.
	/// </summary>
	public class Tracao
	{
		
		public string cpfl_arranjo, usage;
		public double nWires, angulo, forca, metros;
		public double distanceFromTop; // distância da primária para o topo, default = 0.2 m
		
		public Tracao()
		{
		}
	}
}
