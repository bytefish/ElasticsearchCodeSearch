// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System.Reflection.Metadata;

namespace ElasticsearchCodeSearch.Web.Client.Infrastructure
{
    /// <summary>
    /// Validation Error for a Property
    /// </summary>
    public record ValidationError
    {
        /// <summary>
        /// Gets or sets the PropertyName.
        /// </summary>
        public required string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the ErrorMessage.
        /// </summary>
        public required string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Provides a SimpleValidator, which takes a Validation function for the model to be validated.
    /// </summary>
    /// <typeparam name="TModel">Type of the Model in the <see cref="EditContext"/></typeparam>
    public class SimpleValidator<TModel> : ComponentBase, IDisposable
    {
        private IDisposable? _subscriptions;
        private EditContext? _originalEditContext;

        [CascadingParameter] EditContext? CurrentEditContext { get; set; }

        [Parameter]
        public Func<TModel?, IEnumerable<ValidationError>> ValidationFunc { get; set; } = null!;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException($"{nameof(SimpleValidator<TModel>)} requires a cascading " +
                    $"parameter of type {nameof(EditContext)}. For example, you can use {nameof(DataAnnotationsValidator)} " +
                    $"inside an EditForm.");
            }

            _subscriptions = CurrentEditContext.EnableSimpleValidation(ValidationFunc);
            _originalEditContext = CurrentEditContext;
        }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            if (CurrentEditContext != _originalEditContext)
            {
                // While we could support this, there's no known use case presently. Since InputBase doesn't support it,
                // it's more understandable to have the same restriction.
                throw new InvalidOperationException($"{GetType()} does not support changing the {nameof(EditContext)} dynamically.");
            }
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            Dispose(disposing: true);
        }
    }

    public static class EditContextSimpleValidationExtensions
    {
        /// <summary>
        /// Enables validation support for the <see cref="EditContext"/>.
        /// </summary>
        /// <param name="editContext">The <see cref="EditContext"/>.</param>
        /// <param name="validationFunc">Validation function to apply</param>
        /// <returns>A disposable object whose disposal will remove DataAnnotations validation support from the <see cref="EditContext"/>.</returns>
        public static IDisposable EnableSimpleValidation<TModel>(this EditContext editContext, Func<TModel?, IEnumerable<ValidationError>> validationFunc)
        {
            return new SimpleValidationEventSubscriptions<TModel>(editContext, validationFunc);
        }

        private sealed class SimpleValidationEventSubscriptions<TModel> : IDisposable
        {
            private readonly EditContext _editContext;
            private readonly Func<TModel?, IEnumerable<ValidationError>> _validationFunc;
            private readonly ValidationMessageStore _messages;

            public SimpleValidationEventSubscriptions(EditContext editContext, Func<TModel?, IEnumerable<ValidationError>> validationFunc)
            {
                _editContext = editContext ?? throw new ArgumentNullException(nameof(editContext));
                _validationFunc = validationFunc;
                _messages = new ValidationMessageStore(_editContext);

                _editContext.OnFieldChanged += OnFieldChanged;
                _editContext.OnValidationRequested += OnValidationRequested;
            }

            private void OnFieldChanged(object? sender, FieldChangedEventArgs eventArgs)
            {
                _messages.Clear();

                var validationErrors = _validationFunc((TModel)_editContext.Model);

                foreach (var validationError in validationErrors)
                {
                    _messages.Add(_editContext.Field(validationError.PropertyName), validationError.ErrorMessage);
                }

                _editContext.NotifyValidationStateChanged();
            }

            private void OnValidationRequested(object? sender, ValidationRequestedEventArgs eventArgs)
            {
                _messages.Clear();

                var validationErrors = _validationFunc((TModel)_editContext.Model);

                foreach (var validationError in validationErrors)
                {
                    _messages.Add(_editContext.Field(validationError.PropertyName), validationError.ErrorMessage);
                }

                _editContext.NotifyValidationStateChanged();
            }

            public void Dispose()
            {
                _messages.Clear();
                _editContext.OnFieldChanged -= OnFieldChanged;
                _editContext.OnValidationRequested -= OnValidationRequested;
                _editContext.NotifyValidationStateChanged();
            }
        }
    }
}