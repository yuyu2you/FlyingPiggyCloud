﻿using SixCloud.Controllers;
using SixCloudCustomControlLibrary.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SixCloud.Views
{
    /// <summary>
    /// ChangePasswordView.xaml 的交互逻辑
    /// </summary>
    public partial class ChangePasswordView : MetroWindow
    {
        private readonly Authentication authentication = new Authentication();

        public ChangePasswordView()
        {
            InitializeComponent();
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var x = await Task.Run(() => authentication.ChangePasswordByOldPassword(authentication.UserMd5(OldValue.Password), authentication.UserMd5(NewValue.Password)));
            if(x.Success)
            {
                Close();
            }
            else
            {
                MessageBox.Show($"修改失败，服务器返回{x.Message}", "失败", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
    }
}
