using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GTAVFix.Core;

namespace GTAVFix.App;

public partial class App : Application
{
    private Mutex? mutex;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if(e.Args.Length is 2 or 3&&e.Args[0]=="--render-preview")
        {
            if(e.Args.Length==3)System.Globalization.CultureInfo.CurrentUICulture=System.Globalization.CultureInfo.GetCultureInfo(e.Args[2]);
            var window=new MainWindow();var panel=(FrameworkElement)window.Content;
            if(panel is System.Windows.Controls.Panel container)container.Background=SystemColors.WindowBrush;
            panel.Margin=new Thickness(0);
            panel.Measure(new Size(692,428));panel.Arrange(new Rect(0,0,692,428));panel.UpdateLayout();
            var bitmap=new RenderTargetBitmap(692,428,96,96,PixelFormats.Pbgra32);bitmap.Render(panel);
            var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using(var file=File.Create(Path.GetFullPath(e.Args[1])))encoder.Save(file);
            Shutdown(0);return;
        }
        mutex=new Mutex(true,"Local\\MinamotoDev.GTAVFix",out bool owned);
        if(!owned){MessageBox.Show(Text.Get("AlreadyRunning"),"GTAVFix");Shutdown(1);return;}
        if(e.Args.Length==3&&e.Args[0]=="--prepare")
        {
            try
            {
                string output=Path.GetFullPath(e.Args[2]);Directory.CreateDirectory(output);
                using var log=new StreamWriter(Path.Combine(output,"prepare.log")){AutoFlush=true};
                new PatchService(log.WriteLine).Prepare(e.Args[1],output);Shutdown(0);
            }
            catch(Exception ex){File.WriteAllText(Path.Combine(Path.GetFullPath(e.Args[2]),"error.txt"),ex.ToString());Shutdown(1);}
            return;
        }
        MainWindow=new MainWindow();MainWindow.Show();
    }
    protected override void OnExit(ExitEventArgs e){mutex?.Dispose();base.OnExit(e);}
}
