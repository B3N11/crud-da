   M S B A M L   `   `   `  ���� A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:CarCRUD.ViewsCarCRUD.Views      CarCRUD.Views.CareditView   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     -localclr-namespace:CarCRUD.Views     .���   � RowDefinitions  �� $	��100 ��� $	��300 ��� $	��100 �.+��� $��0��._��� $��
CREATE CAR��$��50(�$��0,5,0,0q�   
TitleTheme#��[  � $��1��  �� $��40 ��� $��40 ��� $��40 ���  � ColumnDefinitions �� �� .+��� $��0��$��0��._��� $��
Car Brand:��$��20(�$��
0,20,200,0q�  SubtitleTheme#��[ �� $��1��$��0��$��30��$	��300��._���  �� DisplayMemberPath$
 name������ *	CarBrands+ �� Mode$ TwoWay=� �� SelectedItem �� *SelectedBrand+$ OneWayToSource=��� $��2��$��0��._��� $��	Car Type:��$��20(�$��
0,20,200,0q�#��[ � $��3��$��0��  �� $��50 ��� .+��� $
 name��$��30��$	��300������ *
CarTypes+$ TwoWay=� �� *Type+$ OneWayToSource=��� -    ��Name$ CarTypeName��$��1��$��30��$	��300���� $��0��$��1��._��� $��Color:��$��20(�$��
0,20,250,0q�#��[ �� $��1��$��1��$��30��$	��300��._��� -   $ Color���� $��2��$��1��._��� $��Year (number only)��$��20(�$��
0,20,120,0q�#��[ � $��3��$��1��  �� �� $��40 ��� .+��� -   $
 Year��$��0��$��30��$	��300���� $��1��$��1��._��� $
��Fuel��$��20(�$��
0,20,270,0q�#��[ �� -   $
 Fuel��$��2��$��30��$	��300��� $��4��.+���  �� Orientation$ 
Horizontal=�$��Center=�.+��� -    ��Name$ Create��$��CREATE��$��30(�$	��150��$��70��$��0,0,10,0q�  LoginButtonTheme#��[ �� -   $ Cancel��$��Cancel��$��30(�$	��150��$��70��$��10,0,0,0q�  LoginButtonInverseTheme#��[ 