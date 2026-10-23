using System.ComponentModel;
using System.IO;
using System.Windows;
using GTAVFix.Core;
using Microsoft.Win32;

namespace GTAVFix.App;

public partial class MainWindow : Window
{
    private bool busy;
    public MainWindow()
    {
        InitializeComponent();
        Language=System.Windows.Markup.XmlLanguage.GetLanguage(Text.Language);
        TitleText.Text=Text.Get("Title");SubtitleText.Text=Text.Get("Subtitle");HintText.Text=Text.Get("Hint");
        BrowseButton.Content=Text.Get("Browse");PatchButton.Content=Text.Get("Patch");RestoreButton.Content=Text.Get("Restore");
        FontFamily=new System.Windows.Media.FontFamily(Text.Language switch
        {"ja"=>"Yu Gothic UI, Segoe UI","ko"=>"Malgun Gothic, Segoe UI","zh-Hant"=>"Microsoft JhengHei UI, Segoe UI","zh-Hans"=>"Microsoft YaHei UI, Segoe UI",_=>"Segoe UI"});
        string? saved=SettingsPath();
        if(File.Exists(saved))GamePath.Text=File.ReadAllText(saved);
        else if(File.Exists(Path.Combine(AppContext.BaseDirectory,"GTA5_Enhanced.exe")))GamePath.Text=AppContext.BaseDirectory;
        Log(Text.Get("Ready"));
        Closing+=(_,e)=>{if(busy)e.Cancel=true;};
    }
    private static string SettingsPath()=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MinamotoDev","GTAVFix","game-path.txt");
    private void Browse_Click(object sender,RoutedEventArgs e)
    {var dialog=new OpenFolderDialog{Title=Text.Get("ChooseFolder")};if(dialog.ShowDialog()==true)GamePath.Text=dialog.FolderName;}
    private void Log(string text)=>Dispatcher.Invoke(()=>{LogBox.AppendText(text+Environment.NewLine);LogBox.ScrollToEnd();});
    private void SetBusy(bool value)
    {busy=value;BrowseButton.IsEnabled=PatchButton.IsEnabled=RestoreButton.IsEnabled=GamePath.IsEnabled=!value;Progress.Visibility=value?Visibility.Visible:Visibility.Collapsed;Progress.IsIndeterminate=value;}
    private async void Patch_Click(object sender,RoutedEventArgs e)
    {
        SetBusy(true);string work=Path.Combine(Path.GetTempPath(),"MinamotoDev.GTAVFix",Guid.NewGuid().ToString("N"));
        try
        {
            string game=PatchService.ValidateDirectory(GamePath.Text);
            string settings=SettingsPath();Directory.CreateDirectory(Path.GetDirectoryName(settings)!);File.WriteAllText(settings,game);
            var service=new PatchService(Log);
            await Task.Run(()=>{var plan=service.Prepare(game,work);service.Install(plan);});
            Log(Text.Get("Done"));
        }
        catch(Exception ex){string message=Text.Get("Failure",ex.Message);Log(message);MessageBox.Show(this,message,"GTAVFix",MessageBoxButton.OK,MessageBoxImage.Information);}
        finally
        {
            // Only this invocation's exact temporary directory is cleaned.
            try{if(Directory.Exists(work))Directory.Delete(work,true);}catch(IOException){}catch(UnauthorizedAccessException){}
            SetBusy(false);
        }
    }
    private async void Restore_Click(object sender,RoutedEventArgs e)
    {
        SetBusy(true);
        try{string game=GamePath.Text;await Task.Run(()=>new PatchService(Log).Restore(game));}
        catch(Exception ex){string message=Text.Get("Failure",ex.Message);Log(message);MessageBox.Show(this,message,"GTAVFix",MessageBoxButton.OK,MessageBoxImage.Information);}
        finally{SetBusy(false);}
    }
}
