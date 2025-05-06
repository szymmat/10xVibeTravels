using _10xVibeTravels.Components.Pages.ViewModels;
using _10xVibeTravels.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace _10xVibeTravels.Components.Shared
{
    public class ProfileFormBase : ComponentBase
    {
        [Parameter] public ProfileViewModel Model { get; set; } = new();
        [Parameter] public List<TravelStyleDto> TravelStyles { get; set; } = new();
        [Parameter] public List<IntensityDto> Intensities { get; set; } = new();
        [Parameter] public List<InterestDto> InterestsOptions { get; set; } = new();
        [Parameter] public EventCallback<ProfileViewModel> OnSavePreferences { get; set; }
        [Parameter] public EventCallback<List<Guid>> OnSaveInterests { get; set; }
        [Parameter] public bool IsSavingPreferences { get; set; }
        [Parameter] public bool IsSavingInterests { get; set; }

        protected EditContext? EditContext;

        protected override void OnInitialized()
        {
            EditContext = new EditContext(Model);
        }

        // Called when the Model parameter is set or changes
        public override Task SetParametersAsync(ParameterView parameters)
        {
            // Ensure EditContext is re-assigned if the Model instance changes
            parameters.TryGetValue<ProfileViewModel>(nameof(Model), out var newModel);
            if (newModel != null && newModel != Model)
            {
                Model = newModel;
                EditContext = new EditContext(Model);
            }
            return base.SetParametersAsync(parameters);
        }

        protected async Task HandleValidSavePreferences()
        {
            // This will be called by EditForm OnValidSubmit for preferences
            await OnSavePreferences.InvokeAsync(Model);
        }

        protected async Task HandleSaveInterests()
        {
            // For saving interests, we might not want to tie it to the form's overall validity
            // or have a separate EditContext if it's a truly independent save.
            // Plan states: "Zapisz zainteresowania (PUT /profile/interests)"
            // Plan states: "Formularz ProfileForm powinien blokować przycisk zapisz jeśli !EditContext.Validate()"
            // This implies the interests save button might also be disabled by the main form's validity.
            // If interests are saved independently of budget/style/intensity validation, 
            // then EditContext.Validate() shouldn't block it for *those* fields.
            // For now, assume if the form is generally valid, interests can be saved.
            // Or, we could have a separate button that doesn't use EditForm's submit if it has its own rules.
            if (EditContext != null && EditContext.Validate()) 
            {
                 await OnSaveInterests.InvokeAsync(Model.Interests);
            }
            else
            {
                // Optionally, notify user that the form is invalid if they try to save interests explicitly
                // However, the button should ideally be disabled.
            }
        }
    }
} 