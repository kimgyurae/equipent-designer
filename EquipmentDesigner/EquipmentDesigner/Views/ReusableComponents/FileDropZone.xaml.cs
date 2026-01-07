using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace EquipmentDesigner.Views.ReusableComponents
{
    /// <summary>
    /// A reusable file drop zone component that supports drag-and-drop and click-to-upload.
    /// </summary>
    public partial class FileDropZone : UserControl
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".ppt", ".pptx", ".md", ".drawio" };
        private Brush _originalBorderBrush;

        #region Dependency Properties

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register(
                nameof(LabelText),
                typeof(string),
                typeof(FileDropZone),
                new PropertyMetadata("Design Documents"));

        public static readonly DependencyProperty DocumentsProperty =
            DependencyProperty.Register(
                nameof(Documents),
                typeof(ObservableCollection<string>),
                typeof(FileDropZone),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(
                nameof(IsEditable),
                typeof(bool),
                typeof(FileDropZone),
                new PropertyMetadata(true, OnIsEditableChanged));

        public string LabelText
        {
            get => (string)GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
        }

        public ObservableCollection<string> Documents
        {
            get => (ObservableCollection<string>)GetValue(DocumentsProperty);
            set => SetValue(DocumentsProperty, value);
        }

        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        #endregion

        public FileDropZone()
        {
            InitializeComponent();
            _originalBorderBrush = (Brush)FindResource("Brush.Border.Primary");
        }

        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileDropZone dropZone)
            {
                dropZone.DropZoneBorder.AllowDrop = (bool)e.NewValue;
                dropZone.DropZoneBorder.Cursor = (bool)e.NewValue ? Cursors.Hand : Cursors.Arrow;
            }
        }

        private void DropZoneBorder_DragEnter(object sender, DragEventArgs e)
        {
            if (!IsEditable) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                _originalBorderBrush = DropZoneBorder.BorderBrush;
                DropZoneBorder.BorderBrush = (Brush)FindResource("Brush.Border.Focus");
                DropZoneBorder.Background = (Brush)FindResource("Brush.Background.Tertiary");
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void DropZoneBorder_DragLeave(object sender, DragEventArgs e)
        {
            ResetDropZoneVisual();
            e.Handled = true;
        }

        private void DropZoneBorder_DragOver(object sender, DragEventArgs e)
        {
            if (!IsEditable)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void DropZoneBorder_Drop(object sender, DragEventArgs e)
        {
            ResetDropZoneVisual();

            if (!IsEditable) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddFiles(files);
            }
            e.Handled = true;
        }

        private void DropZoneBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsEditable) return;

            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Design Documents|*.pdf;*.ppt;*.pptx;*.md;*.drawio|All Files|*.*",
                Title = "Select Design Documents"
            };

            if (dialog.ShowDialog() == true)
            {
                AddFiles(dialog.FileNames);
            }
        }

        private void RemoveDocument_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditable) return;

            if (sender is Button button && button.Tag is string fileName)
            {
                Documents?.Remove(fileName);
            }
        }

        private void ResetDropZoneVisual()
        {
            DropZoneBorder.BorderBrush = _originalBorderBrush ?? (Brush)FindResource("Brush.Border.Primary");
            DropZoneBorder.Background = Brushes.Transparent;
        }

        private void AddFiles(string[] files)
        {
            if (Documents == null) return;

            foreach (var file in files)
            {
                var extension = System.IO.Path.GetExtension(file).ToLowerInvariant();
                if (AllowedExtensions.Contains(extension))
                {
                    var fileName = System.IO.Path.GetFileName(file);
                    if (!Documents.Contains(fileName))
                    {
                        Documents.Add(fileName);
                    }
                }
            }
        }
    }
}
