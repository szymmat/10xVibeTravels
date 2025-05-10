using _10xVibeTravels.Components.Pages.ViewModels;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Requests; // Added for Commands
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace _10xVibeTravels.Components.Pages
{
    public class ProfilePageBase : ComponentBase
    {
        [Inject] protected IUserProfileService UserProfileService { get; set; } = default!;
        [Inject] protected ITravelStyleService TravelStyleService { get; set; } = default!;
        [Inject] protected IIntensityService IntensityService { get; set; } = default!;
        [Inject] protected IInterestService InterestService { get; set; } = default!;
        [Inject] protected IToastService ToastService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected ProfileViewModel model = new ProfileViewModel();
        protected bool isLoading = true;
        protected bool isSavingPreferences; 
        protected bool isSavingInterests; 
        protected List<TravelStyleDto> travelStyles = new List<TravelStyleDto>();
        protected List<IntensityDto> intensities = new List<IntensityDto>();
        protected List<InterestDto> interestsOptions = new List<InterestDto>();
        protected bool showPrompt;
        protected string? userId; // Store userId after fetching

        protected override async Task OnInitializedAsync()
        {
            // TODO: Replace with actual userId retrieval logic from AuthenticationStateProvider
            // For now, using a placeholder and assuming it's fetched once.
            var authState = await AuthenticationStateTask; 
            userId = authState?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                ToastService.ShowError("Nie można zidentyfikować użytkownika. Nie można załadować ani zapisać profilu.");
                isLoading = false;
                return;
            }
            await LoadDataAsync();
        }

        protected async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(userId)) return; // Already handled in OnInitializedAsync, but as a safeguard

            isLoading = true;
            try
            {
                var travelStylesTask = TravelStyleService.GetAllAsync();
                var intensitiesTask = IntensityService.GetAllAsync();
                var interestsOptionsTask = InterestService.GetAllAsync();

                await Task.WhenAll(travelStylesTask, intensitiesTask, interestsOptionsTask);

                travelStyles = (await travelStylesTask).ToList();
                intensities = (await intensitiesTask).ToList();
                interestsOptions = (await interestsOptionsTask).ToList();

                var userProfileDto = await UserProfileService.GetUserProfileAsync(userId);

                if (userProfileDto != null)
                {
                    model.Budget = userProfileDto.Budget;
                    model.TravelStyleId = userProfileDto.TravelStyle?.Id;
                    model.IntensityId = userProfileDto.Intensity?.Id;
                    model.Interests = userProfileDto.Interests?.Select(i => i.Id).ToList() ?? new List<Guid>();

                    bool isProfileConsideredNew = userProfileDto.Budget == null &&
                                                  userProfileDto.TravelStyle == null &&
                                                  userProfileDto.Intensity == null &&
                                                  (userProfileDto.Interests == null || !userProfileDto.Interests.Any());
                    showPrompt = isProfileConsideredNew;
                }
                else
                {
                    showPrompt = true; 
                    model = new ProfileViewModel(); 
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError("Błąd ładowania danych profilu. Spróbuj ponownie później.");
                // Log exception ex
                showPrompt = false; 
            }
            finally
            {
                isLoading = false;
            }
        }

        protected async Task SavePreferencesAsync(ProfileViewModel updatedModel)
        {
            if (string.IsNullOrEmpty(userId)) 
            {
                ToastService.ShowError("Błąd zapisu: Nie można zidentyfikować użytkownika.");
                return;
            }

            isSavingPreferences = true;
            try
            {
                var command = new UpdateUserProfileCommand
                {
                    Budget = updatedModel.Budget,
                    TravelStyleId = updatedModel.TravelStyleId,
                    IntensityId = updatedModel.IntensityId
                };
                await UserProfileService.UpdateUserProfileAsync(userId, command);
                ToastService.ShowSuccess("Preferencje zostały zapisane.");
                // Update local model state after successful save
                model.Budget = updatedModel.Budget;
                model.TravelStyleId = updatedModel.TravelStyleId;
                model.IntensityId = updatedModel.IntensityId;
            }
            catch (Exception ex)
            {
                ToastService.ShowError("Błąd podczas zapisywania preferencji.");
                // Log ex
            }
            finally
            {
                isSavingPreferences = false;
            }
        }

        protected async Task SaveInterestsAsync(List<Guid> updatedInterestIds)
        {
            if (string.IsNullOrEmpty(userId))
            {
                ToastService.ShowError("Błąd zapisu: Nie można zidentyfikować użytkownika.");
                return;
            }

            isSavingInterests = true;
            try
            {
                var command = new SetUserInterestsCommand
                {
                    InterestIds = updatedInterestIds
                };
                await UserProfileService.SetUserInterestsAsync(userId, command);
                ToastService.ShowSuccess("Zainteresowania zostały zapisane.");
                // Update local model state after successful save
                model.Interests = updatedInterestIds;
            }
            catch (Exception ex)
            {
                ToastService.ShowError("Błąd podczas zapisywania zainteresowań.");
                // Log ex
            }
            finally
            {
                isSavingInterests = false;
            }
        }

        protected void HandlePromptDismiss()
        {
            showPrompt = false;
        }

        protected void HandlePromptFillProfile()
        {
            showPrompt = false;
            // Scroll to the form using NavigationManager
            NavigationManager.NavigateTo("/profile#profile-form-container", forceLoad: false);
        }
    }
} 