using _10xVibeTravels.Components.Pages.ViewModels;
using _10xVibeTravels.Responses;
using _10xVibeTravels.Interfaces; // Added for IPlanService
using _10xVibeTravels.Dtos; // Added for UpdatePlanDto
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components.Authorization;
using _10xVibeTravels.Requests;
using _10xVibeTravels.Data; // Assuming IToastService is from Blazored.Toast
using System.Security.Claims; // Added for ClaimTypes

namespace _10xVibeTravels.Components.Pages;

[Authorize]
public partial class PlanProposalsView : ComponentBase
{
    // TODO: How to determine which proposals to load without NoteId? Assuming API handles this.

    [Inject] private IPlanService PlanService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!; // Inject ToastService

    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private List<ProposalViewModel> ProposalsList { get; set; } = new();
    private bool isLoading = true;
    private string? pageError = null;
    private bool hasInteracted = false; // To show the "Finish" button

    // Renamed to OnInitializedAsync
    protected override async Task OnInitializedAsync()
    {
        await LoadProposalsAsync();
    }

    private async Task LoadProposalsAsync()
    {
        isLoading = true;
        pageError = null;
        ProposalsList.Clear(); // Clear previous proposals if re-loading
        hasInteracted = false; // Reset interaction state on load/reload
        StateHasChanged(); // Update UI to show loading state

        try
        {
            // Assuming the API endpoint returns a list of PlanProposalResponse
            // Adjust the endpoint and response type if needed based on actual API implementation
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var userId = user.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = await PlanService.GetPlansAsync(userId!, new PlanListQueryParameters() { Status = _10xVibeTravels.Data.PlanStatus.Generated.ToString() });

            if (response != null)
            {
                ProposalsList = response.Items.Select(dto => new ProposalViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    OriginalTitle = dto.Title,
                    Content = dto.ContentPreview ?? string.Empty,
                    Status = dto.Status,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Budget = dto.Budget ?? 0m,
                    IsLoadingAction = false
                }).ToList();
            }
            else
            {
                 pageError = "Nie znaleziono propozycji.";
            }
        }
        catch (Exception ex)
        {
            // Log the exception details (e.g., using ILogger)
            pageError = $"Wystąpił błąd podczas ładowania propozycji."; // More user-friendly page error
            ToastService.ShowError($"Błąd ładowania: {ex.Message}"); // Show detailed error in toast
        }
        finally
        {
            isLoading = false;
            StateHasChanged(); // Update UI after loading finishes or fails
        }
    }

    private async Task HandleAcceptAsync(ProposalViewModel proposal)
    {
        Console.WriteLine("Accept");
        await UpdateProposalStatusAsync(proposal, PlanStatus.Accepted);
    }

    private async Task HandleRejectAsync(ProposalViewModel proposal)
    {
        Console.WriteLine("Reject");
        await UpdateProposalStatusAsync(proposal, PlanStatus.Rejected);
    }

    // Shared method for updating status
    private async Task UpdateProposalStatusAsync(ProposalViewModel proposal, PlanStatus newStatus)
    {
        proposal.IsLoadingAction = true;
        StateHasChanged(); 

        string? userId = null;
        string actionVerb = newStatus == PlanStatus.Accepted ? "akceptacji" : "odrzucenia";
        try
        {
            // Get userId
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User not authenticated.");
            }

            var updateDto = new UpdatePlanDto 
            { 
                Status = newStatus
            };
            
            // Pass userId to UpdatePlanAsync
            var (updatedPlanResult, updateError) = await PlanService.UpdatePlanAsync(userId, proposal.Id, updateDto); 

            if (!string.IsNullOrEmpty(updateError))
            {
                // Handle error returned from service
                ToastService.ShowError($"Błąd aktualizacji: {updateError}");
            }
            else if (updatedPlanResult != null)
            {
                // Update the proposal in the list using the returned plan data
                proposal.Status = updatedPlanResult.Status; 
                proposal.Title = updatedPlanResult.Title; 
                proposal.OriginalTitle = updatedPlanResult.Title;

                hasInteracted = true;
                ToastService.ShowSuccess($"Propozycja została {(newStatus == PlanStatus.Accepted ? "zaakceptowana" : "odrzucona")}.");
            }
            else
            {
                // Service returned success but no updated plan? Handle as needed.
                ToastService.ShowError($"Błąd przetwarzania odpowiedzi serwera po {actionVerb} propozycji (brak danych).");
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Wystąpił błąd podczas {actionVerb} propozycji: {ex.Message}");
            Console.WriteLine($"Exception updating status for {proposal.Id}: {ex.Message}");
        }
        finally
        {
            proposal.IsLoadingAction = false;
            StateHasChanged(); 
        }
    }

    private void NavigateToPlans()
    {
        NavigationManager.NavigateTo("/plans"); // Navigate to the list of saved plans
    }
} 