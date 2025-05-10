using _10xVibeTravels.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;

namespace _10xVibeTravels.Components.Shared
{
    public class InterestsMultiSelectBase : InputBase<List<Guid>>
    {
        [Parameter] public List<InterestDto> Options { get; set; } = new();

        private string[]? _selectedStringValues;
        protected string[]? SelectedStringValues 
        {
            get => _selectedStringValues;
            set 
            {
                _selectedStringValues = value;
                CurrentValue = _selectedStringValues?.Select(Guid.Parse).ToList() ?? new List<Guid>();
                // EditContext is not directly available here, but InputBase handles EditContext.NotifyFieldChanged.
                // If explicit notification is needed for some reason beyond what InputBase provides with CurrentValue setter,
                // it would require passing EditContext as a CascadingParameter or similar.
                // For now, relying on InputBase's default behavior when CurrentValue changes.
            }
        }

        protected override void OnParametersSet()
        {
            // Convert List<Guid> to string[] for the select element when parameters are set
            // This ensures the visual selection matches the underlying CurrentValue
            if (CurrentValue != null)
            {
                _selectedStringValues = CurrentValue.Select(g => g.ToString()).ToArray();
            }
            else
            {
                _selectedStringValues = Array.Empty<string>();
            }
        }

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out List<Guid> result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            // Not used for select multiple, direct binding handles conversion.
            throw new NotSupportedException($"This component does not support parsing from a single string. Use @bind with {nameof(SelectedStringValues)}.");
        }

        protected override string? FormatValueAsString(List<Guid>? value)
        {
            return value != null ? string.Join(",", value.Select(g => g.ToString())) : null;
        }
    }
} 