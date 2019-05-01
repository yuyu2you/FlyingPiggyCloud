﻿using SixCloud.Controllers;
using SixCloud.Models;
using SixCloud.Views;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SixCloud.ViewModels
{
    internal class AuthenticationViewModel : INotifyPropertyChanged
    {
        private readonly Authentication authentication = new Authentication();
        private string _phoneNumber;
        private string _username;
        private string _verificationCode;

        private string PhoneInfo { get; set; }

        private async void SignIn(object param)
        {
            LoginingElement = Visibility.Visible;
            OnPropertyChanged(nameof(LoginingElement));
            //如果允许自动登录，且保存了上一次的Token，则自动登录
            if (IsAutoSignIn && !string.IsNullOrEmpty(LocalProperties.Token))
            {
                GenericResult<UserInformation> x = await Task.Run(() => authentication.GetUserInformation());
                if (x.Success)
                {
                    new MainFrame(x.Result).Show();
                    Window.GetWindow(param as PasswordBox).Close();
                    return;
                }
            }
            //如果密码框中输入了信息，则使用密码框中的密码登陆
            if (param is PasswordBox passwordBox && !string.IsNullOrEmpty(passwordBox.Password))
            {
                string passwordMD5 = authentication.UserMd5(passwordBox.Password);
                GenericResult<UserInformation> x = await Task.Run(() =>
                {
                    return LoginOperate(passwordMD5);
                });
                if (x.Success)
                {
                    Window.GetWindow(passwordBox).Close();
                    new MainFrame(x.Result).Show();
                    if (IsRememberPassword)
                    {
                        LocalProperties.UserName = PhoneNumber;
                        LocalProperties.Password = authentication.UserMd5(passwordBox.Password);
                    }
                    else
                    {
                        LocalProperties.Password = "";
                    }
                }
                else
                {
                    MessageBox.Show(x.Message, "登陆失败");
                }
            }
            //如果允许保存密码，且保存了上次登录的密码，且密码框为空，则使用上次保存的密码的md5登陆
            else if (IsRememberPassword && !string.IsNullOrEmpty(LocalProperties.Password))
            {
                string passwordMD5 = LocalProperties.Password;
                GenericResult<UserInformation> x = await Task.Run(async () =>
                {
                    return await LoginOperate(passwordMD5);
                });

                if (x.Success)
                {
                    LocalProperties.UserName = PhoneNumber;
                    Window.GetWindow(param as PasswordBox).Close();
                    new MainFrame(x.Result).Show();
                }
                else
                {
                    MessageBox.Show(x.Message, "登陆失败");
                    LocalProperties.Password = "";
                    OnPropertyChanged("PasswordBoxHint");
                }
            }
            else
            {
                MessageBox.Show("要登陆，请输入密码");
            }
            LoginingElement = Visibility.Collapsed;
            OnPropertyChanged(nameof(LoginingElement));


            async Task<GenericResult<UserInformation>> LoginOperate(string passwordMD5)
            {
                try
                {
                    return authentication.LoginByPassword(PhoneNumber, passwordMD5);
                }
                catch (Authentication.LoginUserTooMuchException ex)
                {
                    GenericResult<OnlineDeviceList> getOnlineDeviceList = authentication.GetOnlineDeviceList(ex.Token, out string nextToken);
                    if (getOnlineDeviceList.Success)
                    {
                        string[] devicesSSID = await App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            LogoutOthersViewModels logoutOthersViewModels = new LogoutOthersViewModels(getOnlineDeviceList);
                            logoutOthersViewModels.ShowDialog();
                            string[] list = logoutOthersViewModels.DevicesSSID;
                            return list;
                        });
                        GenericResult<bool?> x = authentication.LogoutOnlineDevices(nextToken, devicesSSID);
                        if (x.Result == true)
                        {
                            return await LoginOperate(passwordMD5);
                        }
                    }
                    return ex.Response;
                }
            }
        }

        private bool CanSignIn(object paramObj)
        {
            if (string.IsNullOrEmpty(PhoneNumber))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async void SignUp(object param)
        {
            if (param is PasswordBox passwordBox)
            {
                GenericResult<UserInformation> x = await Task.Run(() =>
                            {
                                return authentication.Register(Username, authentication.UserMd5(passwordBox.Password), VerificationCode, PhoneInfo);
                            });
                if (x.Success)
                {
                    System.Windows.Window.GetWindow(passwordBox).Close();
                    new MainFrame(x.Result).Show();
                }
                else
                {
                    System.Windows.MessageBox.Show(x.Message, "注册失败");
                }
            }
        }

        private bool CanSignUp(object paramObj)
        {
            if (string.IsNullOrEmpty(PhoneNumber) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(VerificationCode) || string.IsNullOrEmpty(PhoneInfo))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async void SendVerificationCode(object paramObj)
        {
            GenericResult<string> x = await Task.Run(() =>
            {
                return authentication.SendingMessageToMobilePhoneNumber(PhoneNumber);
            });
            if (x.Success)
            {
                PhoneInfo = x.Result;
            }
            else
            {
                System.Windows.MessageBox.Show(x.Message, "发送验证码失败");
            }
        }

        private bool CanSendVerificationCode(object paramObj)
        {
            if (string.IsNullOrEmpty(PhoneNumber))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AuthenticationViewModel()
        {
            SignInCommand = new DependencyCommand(SignIn, CanSignIn);
            SignUpCommand = new DependencyCommand(SignUp, CanSignUp);
            SendVerificationCodeCommand = new DependencyCommand(SendVerificationCode, CanSendVerificationCode);
            if (IsRememberPassword && !string.IsNullOrEmpty(LocalProperties.UserName))
            {
                PhoneNumber = LocalProperties.UserName;
                OnPropertyChanged("PhoneNumber");
            }
        }

        public Visibility LoginingElement { get; private set; } = Visibility.Collapsed;

        public DependencyCommand SignInCommand { get; private set; }

        public DependencyCommand SignUpCommand { get; private set; }

        public DependencyCommand SendVerificationCodeCommand { get; private set; }

        public bool IsRememberPassword
        {
            get => LocalProperties.IsSavedPassword;
            set
            {
                LocalProperties.IsSavedPassword = value;
                if (!value)
                {
                    LocalProperties.Password = "";
                    OnPropertyChanged("PasswordBoxHint");
                }
            }
        }

        public string PasswordBoxHint
        {
            get
            {
                if (IsRememberPassword && !string.IsNullOrEmpty(LocalProperties.Password))
                {
                    return "[要更改已保存的密码，请点击这里]";
                }
                else
                {
                    return "密码";
                }
            }
        }

        public bool IsAutoSignIn
        {
            get => LocalProperties.IsAutoLogin;
            set => LocalProperties.IsAutoLogin = value;
        }

        public string PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; SendVerificationCodeCommand.OnCanExecutedChanged(this, null); SignUpCommand.OnCanExecutedChanged(this, null); SignInCommand.OnCanExecutedChanged(this, null); OnPropertyChanged("PhoneNumber"); } }

        public string Username { get => _username; set { _username = value; SignUpCommand.OnCanExecutedChanged(this, null); } }

        public string VerificationCode { get => _verificationCode; set { _verificationCode = value; SignUpCommand.OnCanExecutedChanged(this, null); } }

        public bool IsLocked { get; set; }

        public int RemainingTimeBasedSecond { get; set; }

    }

}
