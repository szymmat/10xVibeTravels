using _10xVibeTravels.Components.Pages.ViewModels;
using _10xVibeTravels.Dtos; // For UpdatePlanDto
using _10xVibeTravels.Responses; // For PlanProposalResponse
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using _10xVibeTravels.Data; // Assuming IToastService is from Blazored.Toast // For ValidationResult
using System.ComponentModel.DataAnnotations;

namespace _10xVibeTravels.Components.Pages;

public partial class ProposalCard : ComponentBase
{
    [Parameter, EditorRequired] public ProposalViewModel Proposal { get; set; } = default!;

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private string editedTitle = string.Empty;
    private string? titleValidationError = null;

    protected override void OnParametersSet()
    {
        // Initialize editedTitle when the component receives parameters
        // especially important if the parent component might re-render with new Proposal objects
        if (!Proposal.IsEditingTitle)
        {
            editedTitle = Proposal.Title;
        }
    }

    private void StartEditingTitle()
    {
        editedTitle = Proposal.Title; // Reset to current title on edit start
        Proposal.IsEditingTitle = true;
        titleValidationError = null; // Clear previous validation errors
    }

    private void CancelEditingTitle()
    {
        Proposal.IsEditingTitle = false;
        editedTitle = Proposal.Title; // Revert any changes made in the input
        titleValidationError = null;
    }

    private async Task SaveTitleAsync()
    {
        titleValidationError = null; // Clear previous error

        // Manual validation (can also use EditForm if preferred)
        if (string.IsNullOrWhiteSpace(editedTitle))
        {
            titleValidationError = "Tytuł nie może być pusty.";
            return;
        }
        if (editedTitle.Length > 100)
        {
            titleValidationError = "Tytuł nie może przekraczać 100 znaków.";
            return;
        }

        Proposal.IsLoadingAction = true; // Show loading state
        try
        {
            var updateDto = new UpdatePlanDto { Title = editedTitle };
            var response = await Http.PatchAsJsonAsync($"/api/plans/{Proposal.Id}", updateDto);

            if (response.IsSuccessStatusCode)
            {
                var updatedProposalDto = await response.Content.ReadFromJsonAsync<PlanProposalResponse>();
                if (updatedProposalDto != null)
                {
                    Proposal.Title = updatedProposalDto.Title;
                    Proposal.OriginalTitle = updatedProposalDto.Title; // Update original title as well
                    Proposal.IsEditingTitle = false;
                    ToastService.ShowSuccess("Tytuł zaktualizowany."); // Show success toast
                }
                else
                {
                    ToastService.ShowError("Błąd przetwarzania odpowiedzi serwera po aktualizacji tytułu."); // Show error toast
                }
            }
            else
            {
                 // Handle specific error codes if needed
                 var errorContent = await response.Content.ReadAsStringAsync(); // Read error details if available
                 ToastService.ShowError($"Nie udało się zaktualizować tytułu ({(int)response.StatusCode}). {errorContent?.Substring(0, Math.Min(errorContent.Length, 100))}");
                 Console.WriteLine($"Error updating title: {(int)response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            // Log exception
            ToastService.ShowError($"Wystąpił błąd podczas zapisywania tytułu: {ex.Message}");
             Console.WriteLine($"Exception saving title: {ex.Message}");
        }
        finally
        {
            Proposal.IsLoadingAction = false;
            // No need to call StateHasChanged() here if IsLoadingAction is part of Proposal VM and parent handles re-render
        }
    }

    // Method to handle card click
    private void HandleCardClick()
    {
        NavigationManager.NavigateTo($"/plan-proposals/{Proposal.Id}");
    }

    private string GetStatusBadgeClass() => Proposal.Status switch
    {
        PlanStatus.Accepted => "badge bg-success",
        PlanStatus.Rejected => "badge bg-secondary",
        PlanStatus.Generated => "badge bg-info text-dark",
        _ => "badge bg-light text-dark"
    };

    private string GetStatusText() => Proposal.Status switch
    {
        PlanStatus.Accepted => "Zaakceptowany",
        PlanStatus.Rejected => "Odrzucony",
        PlanStatus.Generated => "Wygenerowany",
        _ => "Nieznany"
    };

     private string GetCardClass()
    {
        var baseClass = "card h-100 proposal-card";
        return Proposal.Status switch
        {
            PlanStatus.Accepted => $"{baseClass} border-success",
            PlanStatus.Rejected => $"{baseClass} border-secondary opacity-75",
            _ => baseClass
        };
    }

} 