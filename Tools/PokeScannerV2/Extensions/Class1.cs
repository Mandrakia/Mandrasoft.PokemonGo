using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PokeScannerV2.Extensions
{
    public static class AsyncWindowExtension
    {
        public static Task<bool?> ShowDialogAsync(this Window self)
        {
            if (self == null) throw new ArgumentNullException("self");

            TaskCompletionSource<bool?> completion = new TaskCompletionSource<bool?>();
            self.Dispatcher.BeginInvoke(new Action(() => completion.SetResult(self.ShowDialog())));

            return completion.Task;
        }
    }
}
