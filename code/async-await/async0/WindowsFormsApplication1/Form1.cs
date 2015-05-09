using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace WindowsFormsApplication1
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Logger.Info("(ダイアログ) 非同期操作 呼び出し開始");

			//重い処理の非同期呼び出し
			LongBlockA();
			
			//終わるまで待つことも可能
			//LongBlockA().GetAwaiter();
			
			Logger.Info("(ダイアログ) 非同期操作 呼び出し終了");
		}

		private void button2_Click(object sender, EventArgs e)
		{
			MessageBox.Show("こんにちは。この操作はウィンドウをブロックしています。");
		}

		private static async Task LongBlockA()
		{
			await Task.Run(() =>
			{
				Logger.Info("(非同期操作) 重い処理の開始");
				Thread.Sleep(1000 * 10);
				Logger.Info("(非同期操作) 重い処理の終了");
			});
		}
	}
}
