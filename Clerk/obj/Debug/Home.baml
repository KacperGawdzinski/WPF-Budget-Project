   M S B A M L   `   `   `  ����@  <Clerk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ClerkClerk  5      U QLiveCharts.Wpf, Version=0.9.7.0, Culture=neutral, PublicKeyToken=0bc1f845d1ebb8dfG4clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.WpfLiveCharts.Wpf 5          
Clerk.Home   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      %localclr-namespace:Clerk     5      Dlvc4clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf     5      3fa$http://schemas.fontawesome.io/icons/     5      -   5          N� Title$
  Home��5
       N� 
FontFamily$
 Moon)�5       N� FontSize$ 16(�5       N� 
Foreground
 �����$
��Auto��5      $
��Auto��6   .��5      �  � ColumnDefinitions 5   
   �� 5      $	��40* �6#   �� 5      $��* �6#   �� 5      $	��600 �6#    � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   .+�5   
   �� $��Latest Transactions��6   $��0��6/   $��0��6?   $	��Top=�6L   $��Center=�6d   $��
120 20 0 0q�6�   � 5   
   -   5         �Name$ TransactionGrid��5      $��0��6(   $��0��68   $��150 50 30 20q�6E   .+�5      �� 
���   �$��2q�6)   ���#2e7d32._�5       �� -   5        $��LatestTransactions��5       $
��Left=�67   $	��Top=�6R   5!      5"      �� 5$   
   -   5         ��Name$ Hello��5$      $��Right=�6#   $��	0 20 30 0q�6?   $��2��6R   $��0��6b   �� 5%   
   -   5        $
 Date��5%      $��Right=�6"   $��	0 40 30 0q�6>   $��2��6Q   $��0��6a   � 5&   
   $
��Left=�6   $��2��6*   $��53 0 0 0q�6:   .+�5'      A� ���#a5d6a7$	��240��6%   $	��240��61   �� 5(      -   5        $ Balance��5(      $��34(�6)   $��Center=�67   $
��Bold"�6T   $��Center=�6f   $��0 0 0 20q�6�   �� 5)      -   5        $ BalanceText��5)      $
��Bold"�6-   $��Total Balance��6?   $��Center=�6T   $��Center=�6q   $��18(�6�   ���#424242$��0 50 0 0q�6�   5*        LiveCharts.Wpf.PieChart  5+   
   -   5          Name$ Chart��5+      &   LiveCharts.Wpf.Charts.Base.Chart	   LegendLocation$	 Right=�6A   
   InnerRadius$	
 100Q�6X      	Hoverable .  $��2��6�   $	��300��6�      Series 6&   �� *	Rounded+   DataTooltip 6|   Z�    ChartLegend 5,      "  LiveCharts.Wpf.DefaultLegend  5-         
BulletSize$ 20Q�6$   $	��240��64   5/      � 51   
   -	   5        $ Table��51      $��1��6   $��3��6+   $��130 20 40 30q�6?   .+�52      #  LiveCharts.Wpf.CartesianChart  $	 Right=�6:    .     DisableAnimations .  6!   �� *Basic+ 6R   Z�    AxisY 54        LiveCharts.Wpf.Axis     
FontFamily$
 Moon)�6      
Foreground
 �����   LabelFormatter 6D   �� *
YFormatter+6l      AxisX 57        $
 Moon)�6   
 �����   Labels 6D   �� *Labels+6`   59      5:      5;      5<      