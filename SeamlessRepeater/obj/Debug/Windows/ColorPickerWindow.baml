   M S B A M L   `   `   `  ����K  GSeamlessRepeater, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:SeamlessRepeater.WindowsSeamlessRepeater.Windows  5   	   0    *SeamlessRepeater.Windows.ColorPickerWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   8local&clr-namespace:SeamlessRepeater.Windows     5   	   X TXceed.Wpf.Toolkit, Version=3.5.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	     � Title$  ColorPickerWindow��5	   	   $	��320��6#   $	��400��60    � 
ResizeMode$ NoResize=�6<   .��5
      � -   5         �Name$ 
WindowGrid��5
       � RowDefinitions 5   
   �� 5      $��64 �6   �� 5      $��* �6   �� 5      $��48 �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   .+�5   
   �� -   5         ��Name$ ColorPreviewOuterBorder��5      
�������$��1q�6^   $��1��6r   $��16, 0, 16, 16q�6    �� Padding$ 16q�6�   $	��140��6�   $	��140��6�    J� SnapsToDevicePixels . ._�5      �� -   5        $ ColorPreviewBackground��5      ._�5      �� -   5        $ ColorPreviewBorder��5      5      5      �� 5   
   $��Background Color��6   $��Center=�6+   $��Center=�6H   $��16(�6c   $��2��6q   6�   �� 5   
   -   5        $��OK��5      $��2��6   $��48��6+   $��0,0,0,16q�66   $��2��6Z   6p   �� 5   
   -   5         ��Name$ AlphaSlider��5      	 � Maximum$		 100Q�6G   $��1��6U   $��Stretch=�6e   $��1��6�   $	��Top=�6�   $��	0,32,16,0q�6�   �� 5   
   -   5        
 ��Name$
 
AlphaLabel��5      $��Alpha: 100%��6$   $��1��6:   $��1��6J   $��Center=�6W   $��Center=�6{   $��32��6�   $	��Top=�6�   $��0,0,16,0q�6�   �� 5   
   -   5         ��Name$ 
HexTextBox��5      $��16(�6&   
������ $��1��6N   $��Center=�6^   $��23��6{   $��	0,72,16,0q�6�   $��1��6�    �� TextWrapping$ NoWrap=�6�   $	��Top=�6�   5       5!      