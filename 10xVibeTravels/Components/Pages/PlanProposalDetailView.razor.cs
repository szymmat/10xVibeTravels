using _10xVibeTravels.Interfaces; // For IPlanService
using _10xVibeTravels.Responses; // For PlanDetailDto
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using System.Security.Claims;

namespace _10xVibeTravels.Components.Pages;

[Authorize]
public partial class PlanProposalDetailView : ComponentBase
{
    [Parameter] public Guid Id { get; set; }

    [Inject] private IPlanService PlanService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!; 
    [Inject] private Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private PlanDetailDto? plan;
    private bool isLoading = true;
    private string? errorMessage;

    protected override async Task OnParametersSetAsync()
    {
        await LoadPlanDetails();
    }

    private async Task LoadPlanDetails()
    {
        isLoading = true;
        errorMessage = null;
        plan = null;
        StateHasChanged();

        try
        {
            // Assuming IPlanService has a method GetPlanAsync that returns PlanDetailDto
            // Adjust if the actual method name or return type is different
            var user = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userId = user.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            plan = await PlanService.GetPlanByIdAsync(userId!, Id);
            if (plan == null)
            {
                errorMessage = "Nie znaleziono planu o podanym ID.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = "Wystąpił błąd podczas ładowania szczegółów planu.";
            ToastService.ShowError($"Błąd ładowania: {ex.Message}");
            // Log the exception
            Console.WriteLine($"Error loading plan details for ID {Id}: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private void GoBack()
    {
        // Navigate back to the proposals list or potentially browser history
        NavigationManager.NavigateTo("/plan-proposals"); 
    }
} 