﻿using System;
using System.Windows.Input;

namespace SixCloud.ViewModels
{
    /// <summary>
    /// 定义一个命令
    /// </summary>
    /// <typeparam name="T1">Command Parameter Type</typeparam>
    /// <typeparam name="T2">CanExecute Parameter Type</typeparam>
    public class DependencyCommand : ICommand
    {
        protected readonly Action<object> ExecuteAction;

        protected readonly Func<object, bool> CanExecuteAction;

        public DependencyCommand(Action<object> executeAction, Func<object, bool> canExecuteAction)
        {
            ExecuteAction = executeAction;
            CanExecuteAction = canExecuteAction;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            bool? x = CanExecuteAction?.Invoke(parameter);
            if (x != null)
            {
                return x.Value;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 一个预置方法，使Command总是可用
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool AlwaysCan(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {
            ExecuteAction?.Invoke(parameter);
        }

        public void OnCanExecutedChanged(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => CanExecuteChanged?.Invoke(sender, e));
        }
    }
}
