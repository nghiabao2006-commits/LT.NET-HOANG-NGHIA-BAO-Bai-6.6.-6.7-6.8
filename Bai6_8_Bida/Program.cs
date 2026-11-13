using System;
using System.Windows.Forms;
using Bai6_8_Bida.DAL;
using Bai6_8_Bida.GUI;

namespace Bai6_8_Bida
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Buoc 1: dam bao CSDL + bang + du lieu mau da san sang truoc khi mo giao dien
            try
            {
                DbInitializer.EnsureDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không kết nối được SQL Server hoặc không tạo được CSDL.\n\n" +
                    "Kiểm tra lại chuỗi kết nối trong App.config (mục Data Source).\n\n" +
                    "Chi tiết lỗi: " + ex.Message,
                    "Lỗi khởi tạo cơ sở dữ liệu",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Buoc 2: chay ung dung
            Application.Run(new MainForm());
        }
    }
}
