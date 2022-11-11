using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Labb3_NET22.DataModels;

namespace Labb3_NET22.Converters;

public class StringToBitmapImageConverter : IValueConverter
{

    //WPF standard converter låser bildfilerna efter att dom visats i ett bild-element. Denna convertern ser till att filerna inte blir låsta. Hemligheten är rad bitmap.CacheOption = BitmapCacheOption.OnLoad;
    //Det gör att vi kan ta bort Quiz och frågor direkt efter att deras bilder visats i GUI
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || value is not string) return DependencyProperty.UnsetValue;


        if (string.IsNullOrEmpty(value as string)) value = Question.NoImageFilePath;

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(value as string);
        bitmap.EndInit();

        return bitmap;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}