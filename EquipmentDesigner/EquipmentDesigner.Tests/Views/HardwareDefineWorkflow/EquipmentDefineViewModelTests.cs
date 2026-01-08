using System.Collections.ObjectModel;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class EquipmentDefineViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesWithEmptyName()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Name.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyEquipmentType()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.EquipmentType.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDisplayName()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.DisplayName.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptySubname()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Subname.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDescription()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Description.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyCustomer()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Customer.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyProcess()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Process.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyAttachedDocuments()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.AttachedDocuments.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void Name_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Name))
                    raised = true;
            };

            viewModel.Name = "TestEquipment";
            raised.Should().BeTrue();
        }

        [Fact]
        public void EquipmentType_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.EquipmentType))
                    raised = true;
            };

            viewModel.EquipmentType = "TypeA";
            raised.Should().BeTrue();
        }

        [Fact]
        public void DisplayName_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.DisplayName))
                    raised = true;
            };

            viewModel.DisplayName = "Display Name";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Subname_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Subname))
                    raised = true;
            };

            viewModel.Subname = "Subname";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Description))
                    raised = true;
            };

            viewModel.Description = "Test description";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Customer_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Customer))
                    raised = true;
            };

            viewModel.Customer = "Customer A";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Process_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Process))
                    raised = true;
            };

            viewModel.Process = "Process A";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForCanProceedToNext()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.CanProceedToNext))
                    raised = true;
            };

            viewModel.Name = "TestEquipment";
            raised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void CanProceedToNext_WhenNameIsNull_ReturnsFalse()
        {
            var viewModel = new EquipmentDefineViewModel { Name = null };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsEmptyString_ReturnsFalse()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "" };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "   " };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameHasValidValue_ReturnsTrue()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "ValidName" };
            viewModel.CanProceedToNext.Should().BeTrue();
        }

        #endregion

        #region Commands

        [Fact]
        public void LoadFromServerCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.LoadFromServerCommand.CanExecute(null).Should().BeTrue();
        }

        #endregion

        #region Data Conversion

        [Fact]
        public void ToDto_ReturnsEquipmentDtoWithAllPropertiesMapped()
        {
            var viewModel = new EquipmentDefineViewModel
            {
                EquipmentType = "TypeA",
                Name = "Equipment1",
                DisplayName = "Equipment One",
                Subname = "Sub",
                Description = "Description",
                Customer = "Customer A",
                Process = "Process A"
            };
            viewModel.AttachedDocuments.Add("doc1.pdf");
            viewModel.AttachedDocuments.Add("doc2.md");

            var dto = viewModel.ToDto();

            dto.EquipmentType.Should().Be("TypeA");
            dto.Name.Should().Be("Equipment1");
            dto.DisplayName.Should().Be("Equipment One");
            dto.Subname.Should().Be("Sub");
            dto.Description.Should().Be("Description");
            dto.Customer.Should().Be("Customer A");
            dto.Process.Should().Be("Process A");
            dto.AttachedDocuments.Should().HaveCount(2);
            dto.AttachedDocuments.Should().Contain("doc1.pdf");
            dto.AttachedDocuments.Should().Contain("doc2.md");
        }

        [Fact]
        public void FromDto_PopulatesAllPropertiesFromEquipmentDto()
        {
            var dto = new EquipmentDto
            {
                EquipmentType = "TypeB",
                Name = "Equipment2",
                DisplayName = "Equipment Two",
                Subname = "SubB",
                Description = "Description B",
                Customer = "Customer B",
                Process = "Process B",
                AttachedDocuments = new System.Collections.Generic.List<string> { "file1.ppt", "file2.drawio" }
            };

            var viewModel = EquipmentDefineViewModel.FromDto(dto);

            viewModel.EquipmentType.Should().Be("TypeB");
            viewModel.Name.Should().Be("Equipment2");
            viewModel.DisplayName.Should().Be("Equipment Two");
            viewModel.Subname.Should().Be("SubB");
            viewModel.Description.Should().Be("Description B");
            viewModel.Customer.Should().Be("Customer B");
            viewModel.Process.Should().Be("Process B");
            viewModel.AttachedDocuments.Should().HaveCount(2);
            viewModel.AttachedDocuments.Should().Contain("file1.ppt");
            viewModel.AttachedDocuments.Should().Contain("file2.drawio");
        }

        #endregion
    }
}
