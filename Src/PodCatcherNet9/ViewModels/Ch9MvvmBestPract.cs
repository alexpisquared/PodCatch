using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PodCatcherNet9.ViewModels //todo: https://youtu.be/ysWK4e2Mtco?t=1122 (Oct2017: for UWP mostly use DelegateCommand)
{
  public class Ch9MvvmBestPractViewModel : INotifyPropertyChanged //removes excessive CanExecite checking
  {
    public event PropertyChangedEventHandler PropertyChanged;

    Person _model;

    internal Person Model
    {
      get => _model;
      set
      {
        if (_model != null)
          _model.PropertyChanged -= Model_PropertyChanged;

        _model = value;

        if (_model != null)
          _model.PropertyChanged += Model_PropertyChanged;

        OnPropertyChanged();
      }
    }

    public DelegateCommand SomeCommand { get; set; }

    public Ch9MvvmBestPractViewModel()
    {
      Model = new Person();
      SomeCommand = new DelegateCommand/*<Person>*/(Execute, CanExecute);
    }

    void Execute(object obj) => MessageBox.Show("Sommand Executed"); // demo only!!!

    bool CanExecute(object obj) => !string.IsNullOrWhiteSpace(Model?.FirstName);

    void Model_PropertyChanged(object sender, PropertyChangedEventArgs e) => SomeCommand.RaiseCanExecuteChanged();

    void OnPropertyChanged([CallerMemberName] string propName = "")
    {
      var handler = PropertyChanged;
      if (handler != null)
        handler(this, new PropertyChangedEventArgs(propName));
    }
  }
  class Person : INotifyPropertyChanged
  {
    public string FirstName { get; internal set; }

    public event PropertyChangedEventHandler PropertyChanged;
  }

  //http://wpftutorial.net/DelegateCommand.html
  public class DelegateCommand : ICommand
  {
    readonly Predicate<object> _canExecute;
    readonly Action<object> _execute;

    public event EventHandler CanExecuteChanged;

    public DelegateCommand(Action<object> execute)
                   : this(execute, null)
    {
    }

    public DelegateCommand(Action<object> execute,                   Predicate<object> canExecute)
    {
      _execute = execute;
      _canExecute = canExecute;
    }

    public /*override */bool CanExecute(object parameter) => _canExecute == null ? true : _canExecute(parameter);

    public /*override */void Execute(object parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged()
    {
      if (CanExecuteChanged != null)
      {
        CanExecuteChanged(this, EventArgs.Empty);
      }
    }
  }
}