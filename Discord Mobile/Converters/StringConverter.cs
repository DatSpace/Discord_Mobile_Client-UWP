using System;
using Windows.UI.Xaml.Data;

namespace Discord_Mobile.Converters
{
    class ChannelNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString().Length >= 18)
            {
                return string.Format(value.ToString().Substring(0, 17) + "...");
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class UserNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int temp = value.ToString().LastIndexOf('#');
            return value.ToString().Substring(0, temp);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class UserTypingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString().Length > 20)
            {
                while (value.ToString().Length > 20)
                {
                    int tempindex;
                    tempindex = value.ToString().LastIndexOf(",");
                    value.ToString().Substring(0, tempindex);
                }
                value += " and more are typing...";
            }
            else if (!value.ToString().Contains(","))
                value += " is typing...";
            else
                value += " are typing...";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class ChannelTopTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString() != "Discord Mobile Client")
            {
                if (value.ToString().Length >= 25)
                {
                    return string.Format("#" + value.ToString().Substring(0, 24) + "...");
                }
                else
                {
                    return string.Format("#" + value.ToString());
                }
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    //class MessageConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        if (value.ToString().Contains("<") && value.ToString().Contains(">"))
    //            return value;
    //        return value;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
