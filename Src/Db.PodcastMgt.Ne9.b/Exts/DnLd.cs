using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.PodcastMgt.DbModel;

//[PropertyChanged.AddINotifyPropertyChangedInterface]
public partial class Feed //: INotifyPropertyChanged
{
  public event PropertyChangedEventHandler PropertyChanged;
  [NotMapped] public string SubFolderEx => string.IsNullOrEmpty(SubFolder) ? "_PlayER" : SubFolder;

  [NotMapped] public string SrchF { get; set; }

  [NotMapped] public string RunTimeNote { get; set; }

  [NotMapped] public int NewCastCount { get; set; }
}

///http://blog.magnusmontin.net/2013/08/26/data-validation-in-wpf/ - wpf [mvvm?] validation 
///
