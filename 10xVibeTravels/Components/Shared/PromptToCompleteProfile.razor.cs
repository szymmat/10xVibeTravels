using Microsoft.AspNetCore.Components;

namespace _10xVibeTravels.Components.Shared
{
    public class PromptToCompleteProfileBase : ComponentBase
    {
        [Parameter] public bool Visible { get; set; }
        [Parameter] public EventCallback OnDismiss { get; set; }
        [Parameter] public EventCallback OnFillProfile { get; set; }

        protected async Task HandleDismissClick()
        {
            await OnDismiss.InvokeAsync();
        }

        protected async Task HandleFillProfileClick()
        {
            await OnFillProfile.InvokeAsync();
        }
    }
} 