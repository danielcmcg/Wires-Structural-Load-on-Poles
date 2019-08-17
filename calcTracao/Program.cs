/*
 * Created using SharpDevelop.
 * By: Daniel Cavalcante de Menezes
 * Date: 7/4/2016
 * Time: 0:47
 */
using System;
using System.Windows.Forms;

namespace calcTracao
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
