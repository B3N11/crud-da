   M S B A M L   `   `   `  ����A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:CarCRUD.ViewsCarCRUD.Views  5   	       CarCRUD.Views.MainView   5   	   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:CarCRUD.Views     5   	     � Title$  
MainWindow��5   	   $	��610��6   $
��1060��6)    � WindowStyle$
 None=�5	   	   ���#212c61 � 
ResizeMode$ NoResize=�5   	    � AllowsTransparency$
 True��5   	    � WindowStartupLocation$ CenterScreen=�5   	    � 	Resources 5       �� 
TargetType �� Property �� Value��%^   '��       �� 5   
   " ��6   .�5      ��  �@��CornerRadius$ 8��6-   5      5      .��5      �� 	 �� CornerRadius$	 8,8,8,8��6   ._�5   
   � 
 � RowDefinitions
 5      �� 5      $��20 �6    �� 5      �� 5      $��20 �6    .+�5      �� -   5        $��0��5      ���#2e2e2e$	 8,8,0,0��5       $��Stretch=�5!      $��Stretch=�5"      $��0,0,0,5q�5#      ._�5&      �  � ColumnDefinitions 5'      �� 5(      �� 5)      .+�5-      �� $��1��6!   $��Stretch=�5.   !   $��Right=�5/   !    �� Orientation$ 
Horizontal=�50   !   $��0,0,3,0q�51   !   .+�52      �� -   5         ��Name$ MinimizeApp��52   !      WindowControlButtonTheme#��[  53   !   ���#ff9d0054   !   �� 55      -   5        $ CloseApp��55   !   #��[  56   !   ���#eb050557   !   58      �� 5;      $��0��6    $��Stretch=�5<       $��5,3,0,0q�5=       $��CarCRUD��5>        �� TextAlignment$
 Left=�5?       $��13(�5@       $
��Bold"�5A       $��/Fonts/#Baloo Bhaijaan 2)�5B       $��Center=�5C       
�������5D       5E      5F      �� 5I      -   5         ��Name$ 
ActiveItem��5I   *   $��1��6   6*   �� 5L      $��2��6   $	 0,0,8,8��5M      ���#252e4a._�5O      �� -   5         ��Name$ ConnectionState��5O      $
��Left=�5P      $��Center=�5Q      $��5,0,0,0q�5R      $��Connecting to server...��5S      
��� ���5T      5U      5V      5W      5X      